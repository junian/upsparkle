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
        return new WindowsUpSparkleImplementation();
        return new MacUpSparkleImplementation();
#if UPSPARKLE_WINDOWS
        return new WindowsUpSparkleImplementation();
#elif UPSPARKLE_MACOS
        return new MacUpSparkleImplementation();
#elif UPSPARKLE_GENERIC
        if (OperatingSystem.IsWindows())
        {
            return new WindowsUpSparkleImplementation();
        }

        if (OperatingSystem.IsMacOS())
        {
            return new MacUpSparkleImplementation();
        }

        throw new PlatformNotSupportedException("UpSparkle is only supported on Windows and macOS.");
#else
        throw new PlatformNotSupportedException("UpSparkle is only supported on Windows and macOS.");
#endif
    }
}

internal interface IUpSparklePlatformImplementation
{
    void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion);
    void CheckUpdateWithUI();
    void Dispose();
}
