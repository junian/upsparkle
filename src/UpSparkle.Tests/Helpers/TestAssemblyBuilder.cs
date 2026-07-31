using System.Reflection;
using System.Reflection.Emit;

namespace UpSparkle.Tests.Helpers;

/// <summary>
/// Builds lightweight in-memory assemblies decorated with the standard
/// <see cref="System.Reflection.AssemblyCompanyAttribute"/>,
/// <see cref="System.Reflection.AssemblyProductAttribute"/>,
/// <see cref="System.Reflection.AssemblyInformationalVersionAttribute"/>, and
/// <see cref="System.Reflection.AssemblyMetadataAttribute"/> attributes so that
/// <see cref="UpSparkle.UpSparkleUpdater"/> can resolve its configuration from them
/// without requiring a real application assembly.
/// </summary>
internal static class TestAssemblyBuilder
{
    /// <summary>
    /// Builds and returns a dynamic assembly with the specified attribute values.
    /// Any parameter left <see langword="null"/> causes the corresponding attribute to be
    /// omitted, which is useful for testing error-path behaviour.
    /// </summary>
    public static Assembly Build(
        string? companyName = "Test Company",
        string? appName = "Test App",
        string? appVersion = "1.2.3",
        string? appcastUrl = "https://example.com/appcast.xml",
        string? edDSAPublicKey = "dGVzdGtleQ==",
        string? assemblyName = null)
    {
        var name = new AssemblyName(assemblyName ?? $"TestAssembly_{Guid.NewGuid():N}");
        var ab = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);

        if (companyName is not null)
            ab.SetCustomAttribute(BuildAttr<AssemblyCompanyAttribute>(companyName));

        if (appName is not null)
            ab.SetCustomAttribute(BuildAttr<AssemblyProductAttribute>(appName));

        if (appVersion is not null)
            ab.SetCustomAttribute(BuildAttr<AssemblyInformationalVersionAttribute>(appVersion));

        if (appcastUrl is not null)
            ab.SetCustomAttribute(BuildMetadataAttr(UpSparkle.UpSparkleSettings.SUFeedURL, appcastUrl));

        if (edDSAPublicKey is not null)
            ab.SetCustomAttribute(BuildMetadataAttr(UpSparkle.UpSparkleSettings.SUPublicEDKey, edDSAPublicKey));

        return ab;
    }

    // -------------------------------------------------------------------------
    // helpers
    // -------------------------------------------------------------------------

    private static CustomAttributeBuilder BuildAttr<T>(string value) where T : Attribute
    {
        var ctor = typeof(T).GetConstructor([typeof(string)])!;
        return new CustomAttributeBuilder(ctor, [value]);
    }

    private static CustomAttributeBuilder BuildMetadataAttr(string key, string value)
    {
        var ctor = typeof(AssemblyMetadataAttribute).GetConstructor([typeof(string), typeof(string)])!;
        return new CustomAttributeBuilder(ctor, [key, value]);
    }
}
