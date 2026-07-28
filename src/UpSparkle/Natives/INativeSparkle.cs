using System;

namespace UpSparkle.Natives
{
    internal interface INativeSparkle : IDisposable
    {
        /// <summary>
        /// Initialize native Sparkle updater
        /// </summary>
        /// <param name="appCastUrl"></param>
        /// <param name="publicKey"></param>
        /// <param name="companyName"></param>
        /// <param name="appName"></param>
        /// <param name="appVersion"></param>
        void Init(string appCastUrl, string publicKey, string companyName, string appName, string appVersion);

        /// <summary>
        /// Check Update with UI
        /// </summary>
        void CheckUpdateWithUI();
    }
}
