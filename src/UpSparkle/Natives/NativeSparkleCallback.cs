namespace UpSparkle.Natives
{
    /// <summary>
    /// Holds callback delegate types shared by the native Sparkle implementations.
    /// </summary>
    internal static class NativeSparkleCallback
    {
        /// <summary>
        /// Represents a method that is called when the native updater encounters an error.
        /// The callback is invoked on the main thread with no arguments.
        /// </summary>
        public delegate void NativeSparkleErrorCallback();
    }
}
