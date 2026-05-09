namespace BreakoutMods.BreakoutNet
{
    internal static class BreakoutValidationHarness
    {
        public static bool UnknownProtocolVersionFails()
        {
            ZPackage package = new ZPackage();
            package.Write(999);
            package.Write("test.rpc");
            package.Write(BreakoutNetPlugin.PluginGuid);
            package.Write(typeof(EmptyMessage).FullName);
            package.Write(1);
            package.SetPos(0);

            BreakoutRpcEnvelope envelope;
            string reason;
            return !BreakoutRpcEnvelope.TryReadHeader(package, out envelope, out reason)
                   && reason.Contains("Unsupported protocol version");
        }

        public static bool MalformedPackageFails()
        {
            ZPackage package = new ZPackage();
            package.Write(1);
            package.SetPos(0);

            BreakoutRpcEnvelope envelope;
            string reason;
            return !BreakoutRpcEnvelope.TryReadHeader(package, out envelope, out reason);
        }

        public static bool ClientSideNonServerMessageFails()
        {
            string reason;
            return !BreakoutRpcRegistry.TryCreateDispatchPlanForTest(false, false, "test.rpc", out reason);
        }

        public static bool RateLimiterDropsExcessiveMessages()
        {
            BreakoutRateLimiter limiter = new BreakoutRateLimiter(1f, 0f);
            return limiter.Allow("peer:rpc", 0f) && !limiter.Allow("peer:rpc", 0.01f);
        }

        private sealed class EmptyMessage : IBreakoutSerializable
        {
            public void Write(ZPackage package)
            {
            }

            public void Read(ZPackage package)
            {
            }
        }
    }
}
