using System;

namespace UpSparkle.Natives
{
    /// <summary>
    /// Attribute used to mark methods that are called back from unmanaged code.
    /// Mono's Ahead-of-Time (AOT) compiler and Unity's IL2CPP require callbacks
    /// from native code to target a static method annotated with an attribute of
    /// this name so the native-to-managed trampoline can be generated.
    /// </summary>
    /// <remarks>
    /// This is a local placeholder: netstandard2.0 does not expose
    /// <c>Mono.AOT.MonoPInvokeCallbackAttribute</c>, which only ships with the
    /// Mono / Xamarin / Unity platform BCLs. The AOT toolchains identify the
    /// attribute by its simple name, so a same-named type compiled into this
    /// assembly is recognized when the library is consumed by an AOT-compiled app.
    /// In JIT scenarios the attribute is a no-op.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute(Type delegateType)
        {
        }
    }
}
