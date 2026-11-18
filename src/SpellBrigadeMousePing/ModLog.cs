namespace SpellBrigadeMousePing;

using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;

/// <summary>
/// Thin wrapper around the BepInEx logger with "log this only once" helpers.
///
/// Everything is also written to BepInEx/SpellBrigadeMousePing.log and flushed after every line. BepInEx's own
/// LogOutput.log is buffered, so after a hard game crash its last lines are usually missing. This file always
/// ends with the last thing the mod did (see <see cref="Step"/>).
/// </summary>
internal static class ModLog
{
    private static ManualLogSource? _log;
    private static StreamWriter? _file;
    private static readonly HashSet<string> Once = new();
    private static readonly Dictionary<string, float> Throttle = new();

    public static string FilePath => Path.Combine(Paths.BepInExRootPath, "SpellBrigadeMousePing.log");

    public static string PreviousFilePath => Path.Combine(Paths.BepInExRootPath, "SpellBrigadeMousePing.prev.log");

    /// <summary>
    /// The last breadcrumb of the previous session if it did NOT end with a clean exit (i.e. the game crashed or was
    /// killed), else null.
    /// </summary>
    public static string? PreviousCrashStep { get; private set; }

    public static void Init(ManualLogSource log)
    {
        _log = log;
        try
        {
            // Keep the previous session's log (a crash log is no longer lost by restarting) and see how it ended.
            if (File.Exists(FilePath))
            {
                File.Copy(FilePath, PreviousFilePath, true);
                string? lastStep = null;
                foreach (var line in File.ReadAllLines(FilePath))
                {
                    var i = line.IndexOf(" STEP  ", StringComparison.Ordinal);
                    if (i >= 0) lastStep = line[(i + 7)..].Trim();
                }

                if (lastStep != null && lastStep != CleanExit) PreviousCrashStep = lastStep;
            }
        }
        catch (Exception ex)
        {
            log.LogWarning($"Could not read the previous {FilePath}: {ex.Message}");
        }

        try
        {
            _file = new StreamWriter(new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) { AutoFlush = true };
            _file.WriteLine($"SpellBrigadeMousePing {Plugin.Version} log, started {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            if (PreviousCrashStep != null)
                _file.WriteLine($"Previous session did not exit cleanly; its last step was: {PreviousCrashStep} (log kept as SpellBrigadeMousePing.prev.log)");
        }
        catch (Exception ex)
        {
            log.LogWarning($"Could not open {FilePath}: {ex.Message}");
        }
    }

    public static void Info(string msg)
    {
        _log?.LogInfo(msg);
        ToFile("INFO ", msg);
    }

    public static void Warn(string msg)
    {
        _log?.LogWarning(msg);
        ToFile("WARN ", msg);
    }

    public static void Error(string msg)
    {
        _log?.LogError(msg);
        ToFile("ERROR", msg);
    }

    public static void Debug(string msg)
    {
        _log?.LogDebug(msg);
        ToFile("DEBUG", msg);
    }

    /// <summary>
    /// Breadcrumb written ONLY to SpellBrigadeMousePing.log, right before something that could crash the game natively
    /// (Harmony patches, first use of a Unity API). If the game dies, the last "STEP" line names the culprit.
    /// </summary>
    public static void Step(string msg) => ToFile("STEP ", msg);

    public const string CleanExit = "clean exit";

    /// <summary><see cref="Step"/>, but only the first time for a given key (for things that run every frame).</summary>
    public static void StepOnce(string key, string msg)
    {
        if (Once.Add("step:" + key)) Step(msg);
    }

    public static void InfoOnce(string key, string msg)
    {
        if (Once.Add(key)) Info(msg);
    }

    public static void WarnOnce(string key, string msg)
    {
        if (Once.Add(key)) Warn(msg);
    }

    /// <summary>Log an exception at most once every <paramref name="seconds"/> per key, so a per-frame failure can't flood the log.</summary>
    public static void ErrorThrottled(string key, Exception ex, float now, float seconds = 10f)
    {
        if (Throttle.TryGetValue(key, out var last) && now - last < seconds) return;
        Throttle[key] = now;
        Error($"[{key}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
    }

    private static void ToFile(string level, string msg)
    {
        if (_file == null) return;
        try
        {
            lock (_file)
            {
                _file.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {level} {msg}");
            }
        }
        catch
        {
            // never let logging break the game
        }
    }
}
