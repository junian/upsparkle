using System.Runtime.InteropServices;

namespace UpSparkle;

internal sealed class WindowsUpSparkleImplementation : IUpSparklePlatformImplementation
{
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

    public void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion)
    {
        win_sparkle_set_appcast_url(appCastUrl);
        win_sparkle_set_eddsa_public_key(publicKey);
        win_sparkle_set_app_details(companyName, appName, appVersion);
        win_sparkle_init();
    }

    public void CheckUpdateWithUI()
    {
        win_sparkle_check_update_with_ui();
    }

    public void Dispose()
    {
        win_sparkle_cleanup();
    }
}
