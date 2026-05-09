using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreakoutMods.BreakoutNet
{
    internal sealed class BreakoutRateLimiter
    {
        private readonly Dictionary<string, Bucket> buckets = new Dictionary<string, Bucket>();
        private readonly float capacity;
        private readonly float refillPerSecond;

        public BreakoutRateLimiter(float capacity, float refillPerSecond)
        {
            this.capacity = Math.Max(1f, capacity);
            this.refillPerSecond = Math.Max(0f, refillPerSecond);
        }

        public bool Allow(string key)
        {
            return Allow(key, Time.realtimeSinceStartup);
        }

        internal bool Allow(string key, float now)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = "unknown";
            }

            Bucket bucket;
            if (!buckets.TryGetValue(key, out bucket))
            {
                bucket = new Bucket { Tokens = capacity, LastUpdate = now };
            }

            float elapsed = Math.Max(0f, now - bucket.LastUpdate);
            bucket.Tokens = Math.Min(capacity, bucket.Tokens + elapsed * refillPerSecond);
            bucket.LastUpdate = now;

            if (bucket.Tokens < 1f)
            {
                buckets[key] = bucket;
                return false;
            }

            bucket.Tokens -= 1f;
            buckets[key] = bucket;
            return true;
        }

        private struct Bucket
        {
            public float Tokens;

            public float LastUpdate;
        }
    }
}
