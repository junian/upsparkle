using UpSparkle.Natives;

namespace UpSparkle.Tests.Fakes;

/// <summary>
/// Hand-written test double for <see cref="INativeSparkle"/>.
/// Records every call so tests can assert on behaviour without touching native DLLs.
/// </summary>
internal sealed class FakeNativeSparkle : INativeSparkle
{
    // --- call counts -----------------------------------------------------------
    public int SetAppDetailsCallCount { get; private set; }
    public int SetAppcastUrlCallCount { get; private set; }
    public int SetEdDSAPublicKeyCallCount { get; private set; }
    public int InitializeCallCount { get; private set; }
    public int CheckUpdateWithUICallCount { get; private set; }
    public int CheckUpdateWithoutUICallCount { get; private set; }
    public int SetHttpHeaderCallCount { get; private set; }
    public int ClearHttpHeadersCallCount { get; private set; }
    public int SetErrorCallbackCallCount { get; private set; }
    public int DisposeCallCount { get; private set; }

    // --- last values supplied --------------------------------------------------
    public string? LastCompanyName { get; private set; }
    public string? LastAppName { get; private set; }
    public string? LastAppVersion { get; private set; }
    public string? LastAppcastUrl { get; private set; }
    public string? LastEdDSAPublicKey { get; private set; }
    public string? LastHttpHeaderName { get; private set; }
    public string? LastHttpHeaderValue { get; private set; }

    public NativeSparkleCallback.NativeSparkleErrorCallback? ErrorCallback { get; private set; }

    // --- INativeSparkle --------------------------------------------------------
    public void SetAppDetails(string companyName, string appName, string appVersion)
    {
        SetAppDetailsCallCount++;
        LastCompanyName = companyName;
        LastAppName = appName;
        LastAppVersion = appVersion;
    }

    public void SetAppcastUrl(string appcastUrl)
    {
        SetAppcastUrlCallCount++;
        LastAppcastUrl = appcastUrl;
    }

    public void SetEdDSAPublicKey(string edDSAPublicKey)
    {
        SetEdDSAPublicKeyCallCount++;
        LastEdDSAPublicKey = edDSAPublicKey;
    }

    public void Initialize()
    {
        InitializeCallCount++;
    }

    public void CheckUpdateWithUI()
    {
        CheckUpdateWithUICallCount++;
    }

    public void CheckUpdateWithoutUI()
    {
        CheckUpdateWithoutUICallCount++;
    }

    public void SetHttpHeader(string name, string value)
    {
        SetHttpHeaderCallCount++;
        LastHttpHeaderName = name;
        LastHttpHeaderValue = value;
    }

    public void ClearHttpHeaders()
    {
        ClearHttpHeadersCallCount++;
    }

    public void SetErrorCallback(NativeSparkleCallback.NativeSparkleErrorCallback callback)
    {
        SetErrorCallbackCallCount++;
        ErrorCallback = callback;
    }

    public bool IsAutomaticCheckForUpdates { get; set; }

    public int UpdateCheckInterval { get; set; }

    public DateTime? LastCheckTime { get; set; }

    public void Dispose()
    {
        DisposeCallCount++;
    }
}
