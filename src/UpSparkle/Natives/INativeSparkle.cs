using System;

namespace UpSparkle.Natives
{
    internal interface INativeSparkle : IDisposable
    {
        /// <summary>
        /// Set App Details to the native updater
        /// </summary>
        /// <param name="companyName"></param>
        /// <param name="appName"></param>
        /// <param name="appVersion"></param>
        void SetAppDetails(string companyName, string appName, string appVersion);

        /// <summary>
        /// Set Appcast URL to the native updater
        /// </summary>
        /// <param name="appcastUrl"></param>
        void SetAppcastUrl(string appcastUrl);

        /// <summary>
        /// Sets the EdDSA public key to the native updater
        /// </summary>
        /// <param name="edDSAPublicKey"></param>
        void SetEdDSAPublicKey(string edDSAPublicKey);

        /// <summary>
        /// Initializes and Starts the native updater
        /// </summary>
        void Initialize();

        /// <summary>
        /// Check Update with UI
        /// </summary>
        void CheckUpdateWithUI();
    }
}
