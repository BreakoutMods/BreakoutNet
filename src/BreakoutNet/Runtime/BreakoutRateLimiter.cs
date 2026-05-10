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
            return Allow(key, now, capacity, refillPerSecond);
        }

        internal bool Allow(string key, float now, float bucketCapacity, float bucketRefillPerSecond)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = "unknown";
            }

            bucketCapacity = Math.Max(1f, bucketCapacity);
            bucketRefillPerSecond = Math.Max(0f, bucketRefillPerSecond);

            Bucket bucket;
            if (!buckets.TryGetValue(key, out bucket))
            {
                bucket = new Bucket { Tokens = bucketCapacity, LastUpdate = now };
            }

            float elapsed = Math.Max(0f, now - bucket.LastUpdate);
            bucket.Tokens = Math.Min(bucketCapacity, bucket.Tokens + elapsed * bucketRefillPerSecond);
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
