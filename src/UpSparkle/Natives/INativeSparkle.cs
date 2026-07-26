namespace UpSparkle.Natives;

public interface INativeSparkle: IDisposable
{
    public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion);

    public void CheckUpdateWithUI();
}
