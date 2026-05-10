using System;

namespace BreakoutMods.BreakoutNet
{
    public sealed class BreakoutRpcRateLimit
    {
        private static readonly BreakoutRpcRateLimit DefaultPolicy = new BreakoutRpcRateLimit(true, 12f, 8f);
        private static readonly BreakoutRpcRateLimit UnlimitedPolicy = new BreakoutRpcRateLimit(false, 0f, 0f);

        private BreakoutRpcRateLimit(bool enabled, float capacity, float refillPerSecond)
        {
            Enabled = enabled;
            Capacity = Math.Max(1f, capacity);
            RefillPerSecond = Math.Max(0f, refillPerSecond);
        }

        public bool Enabled { get; private set; }

        public float Capacity { get; private set; }

        public float RefillPerSecond { get; private set; }

        public static BreakoutRpcRateLimit Default
        {
            get { return DefaultPolicy; }
        }

        public static BreakoutRpcRateLimit Unlimited
        {
            get { return UnlimitedPolicy; }
        }

        public static BreakoutRpcRateLimit TokenBucket(float capacity, float refillPerSecond)
        {
            return new BreakoutRpcRateLimit(true, capacity, refillPerSecond);
        }

        public static BreakoutRpcRateLimit ForMessagesPerSecond(float messagesPerSecond, float burstSeconds)
        {
            float refill = Math.Max(1f, messagesPerSecond);
            float burst = Math.Max(1f, burstSeconds);
            return TokenBucket(refill * burst, refill);
        }
    }
}
