using System.Reflection;

namespace UpSparkle;

public class UpSparkleUpdater : IUpSparkle
{
    private readonly IUpSparklePlatformImplementation implementation = CreateImplementation();

    public bool IsInitialized { get; private set; }
    public string? AppCastUrl { get; private set; }
    public string? PublicKey { get; private set; }
    public string? CompanyName { get; private set; }
    public string? AppName { get; private set; }
    public string? AppVersion { get; private set; }

    public virtual void Init(string appCastUrl, string publicKey, Assembly assemblyInfo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appCastUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicKey);
        ArgumentNullException.ThrowIfNull(assemblyInfo);

        var companyName = assemblyInfo.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company
            ?? throw new ArgumentException("Assembly is missing AssemblyCompanyAttribute.", nameof(assemblyInfo));

        var appName = assemblyInfo.GetCustomAttribute<AssemblyProductAttribute>()?.Product
            ?? assemblyInfo.GetName().Name
            ?? throw new ArgumentException("Assembly has no product name or assembly name.", nameof(assemblyInfo));

        var appVersion = assemblyInfo.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assemblyInfo.GetName().Version?.ToString()
            ?? throw new ArgumentException("Assembly has no version information.", nameof(assemblyInfo));

        // Strip any build metadata suffix (e.g. "1.0.0+abc123" -> "1.0.0")
        var plusIndex = appVersion.IndexOf('+');
        if (plusIndex >= 0)
            appVersion = appVersion[..plusIndex];

        Init(appCastUrl, publicKey, companyName, appName, appVersion);
    }

    public virtual void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appCastUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentException.ThrowIfNullOrWhiteSpace(appVersion);

        AppCastUrl = appCastUrl;
        PublicKey = publicKey;
        CompanyName = companyName;
        AppName = appName;
        AppVersion = appVersion;

        implementation.Init(appCastUrl, publicKey, companyName, appName, appVersion);
        IsInitialized = true;
    }

    public virtual void CheckUpdateWithUI()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException($"{nameof(UpSparkle)} is not initialized");
        }

        implementation.CheckUpdateWithUI();
    }

    public virtual void Dispose()
    {
        implementation.Dispose();
        IsInitialized = false;
    }

    private static IUpSparklePlatformImplementation CreateImplementation()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsUpSparkleImplementation();
        }

        if (OperatingSystem.IsMacOS())
        {
            return new MacUpSparkleImplementation();
        }

        throw new PlatformNotSupportedException("UpSparkle is only supported on Windows and macOS.");
    }
}

internal interface IUpSparklePlatformImplementation
{
    void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion);
    void CheckUpdateWithUI();
    void Dispose();
}
