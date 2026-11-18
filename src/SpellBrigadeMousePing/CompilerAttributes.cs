// Why this file exists
// --------------------
// Nullable annotations (string?, Foo?) make the C# compiler stamp NullableAttribute / NullableContextAttribute
// on our code. Normally it generates its own copy of these attributes. But some of the game's BepInEx interop
// assemblies contain PUBLIC copies of them (the game's compiler embedded them as internal types and Il2CppInterop
// makes everything public). Those copies have interop-style constructors, so the compiler picks them up and
// fails with CS0656 "Missing compiler required member NullableAttribute..ctor".
//
// Declaring correct copies here fixes it: types from the project's own source always win over referenced ones.

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.GenericParameter |
                    AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    internal sealed class NullableAttribute : Attribute
    {
        public readonly byte[] NullableFlags;

        public NullableAttribute(byte flag) => NullableFlags = new[] { flag };

        public NullableAttribute(byte[] flags) => NullableFlags = flags;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Method |
                    AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class NullableContextAttribute : Attribute
    {
        public readonly byte Flag;

        public NullableContextAttribute(byte flag) => Flag = flag;
    }
}
