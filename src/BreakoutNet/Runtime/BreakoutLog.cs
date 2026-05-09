using BepInEx.Logging;

namespace BreakoutMods.BreakoutNet
{
    internal static class BreakoutLog
    {
        private static readonly BreakoutRateLimiter MalformedLogLimiter = new BreakoutRateLimiter(4, 0.5f);

        internal static ManualLogSource Logger { get; set; }

        public static void Info(string message, params object[] args)
        {
            Logger?.LogInfo(Format(message, args));
        }

        public static void Warning(string message, params object[] args)
        {
            Logger?.LogWarning(Format(message, args));
        }

        public static void Error(string message, params object[] args)
        {
            Logger?.LogError(Format(message, args));
        }

        public static void Malformed(long peerId, string message, params object[] args)
        {
            string key = "malformed:" + peerId;
            if (MalformedLogLimiter.Allow(key))
            {
                Warning(message, args);
            }
        }

        private static string Format(string message, object[] args)
        {
            return args == null || args.Length == 0 ? message : string.Format(message, args);
        }
    }
}
