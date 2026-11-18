namespace SpellBrigadeMousePing.Game;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime.Runtime;

/// <summary>What <see cref="NativeCode.Inspect"/> found out about one game method's machine code.</summary>
internal sealed class NativeInfo
{
    public NativeInfo(string label) => Label = label;

    public string Label { get; }
    public IntPtr Code { get; set; }
    public uint Size { get; set; }

    /// <summary>Why the method must not be patched, or null when its own code looks safe to detour.</summary>
    public string? Refusal { get; set; }

    public override string ToString() => Refusal ?? $"native 0x{Code.ToInt64():X}, {Size} bytes";
}

/// <summary>
/// Where an interop method's real machine code lives, and whether it is safe to Harmony-patch. Patches on IL2CPP
/// detour that native code, so:
///  * pointer 0 = no code (interface / abstract method): patching it detours address 0;
///  * the game's native DLL merges functions with identical machine code ("identical code folding"): every empty
///    method is one shared <c>ret</c>, small getters are shared too. Patching one of them patches all of them, with
///    the wrong object and arguments. Both crashed the game (0.1.0 at start, 0.3.0 at level-up).
///
/// <see cref="CanPatch"/> (ported from the 17 Nov PC session, branch pc-backup) refuses: no code, abstract/interface
/// methods (their pointer is a stub shared by every abstract method with that signature), leaf functions (no x64
/// unwind entry: empty or trivial, exactly the code that gets folded), functions under <see cref="MinBytes"/>, and
/// code addresses shared with another inspected method. Every new patch must go through it.
/// </summary>
internal static class NativeCode
{
    /// <summary>Non-leaf functions below this size are refused too: forwarding stubs get folded, and the detour needs room.</summary>
    private const uint MinBytes = 16;

    private static readonly Dictionary<MethodBase, NativeInfo> Cache = new();
    private static readonly Dictionary<IntPtr, List<string>> Owners = new();

    [DllImport("kernel32.dll")]
    private static extern IntPtr RtlLookupFunctionEntry(ulong controlPc, out ulong imageBase, IntPtr historyTable);

    /// <summary>The method's native code address, or zero (no code / not an interop method).</summary>
    public static IntPtr Pointer(MethodBase method)
    {
        try
        {
            var field = Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method);
            if (field == null) return IntPtr.Zero;
            var methodInfo = (IntPtr)field.GetValue(null)!;
            // Il2CppMethodInfo starts with the method pointer
            return methodInfo == IntPtr.Zero ? IntPtr.Zero : Marshal.ReadIntPtr(methodInfo);
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    /// <summary>Find the method's native code and check it (cached). For a batch, inspect all of them before patching any.</summary>
    public static NativeInfo Inspect(MethodBase method, string label)
    {
        if (Cache.TryGetValue(method, out var known)) return known;
        var info = new NativeInfo(label);
        Cache[method] = info;

        bool isAbstract;
        try
        {
            info.Code = MethodPointer(method, out isAbstract);
        }
        catch (Exception ex)
        {
            info.Refusal = $"native code not found: {ex.GetType().Name}: {ex.Message}";
            return info;
        }

        if (info.Code == IntPtr.Zero)
        {
            info.Refusal = "no native code";
            return info;
        }

        // Interop assemblies turn abstract and interface methods into ordinary wrappers (MethodInfo.IsAbstract is false),
        // but natively they have no body: their code pointer is a stub shared by every abstract method with the same signature.
        if (isAbstract)
        {
            info.Refusal = $"abstract or interface method: 0x{info.Code.ToInt64():X} is a stub shared by all abstract methods with this signature";
            return info;
        }

        if (!Owners.TryGetValue(info.Code, out var owners)) Owners[info.Code] = owners = new List<string>();
        if (!owners.Contains(label)) owners.Add(label);

        var address = (ulong)info.Code.ToInt64();
        var entry = RtlLookupFunctionEntry(address, out var imageBase, IntPtr.Zero);
        if (entry == IntPtr.Zero)
        {
            info.Refusal = $"leaf function at 0x{address:X} [{FirstBytes(info.Code)}]: empty or trivial, the compiler shares such code between methods";
            return info;
        }

        var begin = imageBase + (uint)Marshal.ReadInt32(entry);
        var end = imageBase + (uint)Marshal.ReadInt32(entry, 4);
        info.Size = (uint)(end - begin);
        if (begin != address)
            info.Refusal = $"0x{address:X} is not the start of a function (it starts at 0x{begin:X})";
        else if (info.Size < MinBytes)
            info.Refusal = $"tiny function at 0x{address:X} ({info.Size} bytes [{FirstBytes(info.Code)}])";
        return info;
    }

    /// <summary>
    /// True when the method can be patched. <paramref name="why"/> is the native address and size, or the reason for
    /// refusing; log it before patching so a crash can be traced to the patch.
    /// </summary>
    public static bool CanPatch(MethodBase method, string label, out string why)
    {
        var info = Inspect(method, label);
        if (info.Refusal == null && Owners.TryGetValue(info.Code, out var owners) && owners.Count > 1)
        {
            why = $"native code 0x{info.Code.ToInt64():X} is shared with {string.Join(", ", owners.Where(o => o != label))}";
            return false;
        }

        why = info.ToString();
        return info.Refusal == null;
    }

    private static unsafe IntPtr MethodPointer(MethodBase method, out bool isAbstract)
    {
        isAbstract = false;
        var field = Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method)
                    ?? throw new InvalidOperationException("not a generated interop method");
        var methodInfo = (IntPtr)field.GetValue(null)!;
        if (methodInfo == IntPtr.Zero) return IntPtr.Zero;
        var native = UnityVersionHandler.Wrap((Il2CppMethodInfo*)methodInfo);
        isAbstract = (native.Flags & Il2CppMethodFlags.METHOD_ATTRIBUTE_ABSTRACT) != 0;
        return native.MethodPointer;
    }

    private static string FirstBytes(IntPtr code)
    {
        try
        {
            var bytes = new byte[8];
            Marshal.Copy(code, bytes, 0, bytes.Length);
            return BitConverter.ToString(bytes).Replace('-', ' ');
        }
        catch
        {
            return "?";
        }
    }
}
