using System.Runtime.InteropServices;

namespace UpSparkle.Win;

public class WinSparkle: IDisposable
{
    #region WinSparkle.dll
    
    [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_init();

    [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_cleanup();

    [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_set_appcast_url(string url);

    [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_set_eddsa_public_key(string publicKey);

    [DllImport("WinSparkle.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_set_app_details(
        string companyName,
        string appName,
        string appVersion);

    [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void win_sparkle_check_update_with_ui();
    
    #endregion

    public bool IsInitialized { get; private set; } = false;
    public string? AppCastUrl { get; private set; }
    public string? PublicKey { get; private set; }
    public string? CompanyName { get; private set; }
    public string? AppName { get; private set; }
    public string? AppVersion { get; private set; }
    
    public WinSparkle()
    {
    }

    public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
    {
        AppCastUrl = appCastUrl;
        win_sparkle_set_appcast_url(AppCastUrl);
        
        PublicKey = publicKey;
        win_sparkle_set_eddsa_public_key(PublicKey);
        
        CompanyName = companyName;
        AppName = appName;
        AppVersion = appVersion;
        win_sparkle_set_app_details(CompanyName, AppName, AppVersion);

        // Start automatic update checks.
        win_sparkle_init();
        IsInitialized = true;
    }
    
    public void CheckUpdateWithUI()
    {
        if( !IsInitialized)
            throw new InvalidOperationException($"{nameof(UpSparkle)} is not initialized");
        
        win_sparkle_check_update_with_ui();
    }
    
    public void Dispose()
    {
        win_sparkle_cleanup();
    }
}
