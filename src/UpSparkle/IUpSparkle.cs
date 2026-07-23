namespace UpSparkle;

public interface IUpSparkle: IDisposable
{
    public bool IsInitialized { get; }
    public string? AppCastUrl { get; }
    public string? PublicKey { get; }
    public string? CompanyName { get; }
    public string? AppName { get; }
    public string? AppVersion { get; }

    public void Init(string appCastUrl, string publicKey, System.Reflection.Assembly assemblyInfo);
    public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion);

    public void CheckUpdateWithUI();
}
