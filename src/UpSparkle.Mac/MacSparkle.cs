namespace UpSparkle;

public class MacSparkle: IUpSparkle
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public bool IsInitialized { get; }
    public string? AppCastUrl { get; }
    public string? PublicKey { get; }
    public string? CompanyName { get; }
    public string? AppName { get; }
    public string? AppVersion { get; }
    public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
    {
        throw new NotImplementedException();
    }

    public void CheckUpdateWithUI()
    {
        throw new NotImplementedException();
    }
}