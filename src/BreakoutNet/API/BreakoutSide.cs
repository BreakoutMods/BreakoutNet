using UnityEngine;

namespace BreakoutMods.BreakoutNet
{
    public static class BreakoutSide
    {
        public static bool IsServer
        {
            get { return ZNet.instance != null && ZNet.instance.IsServer(); }
        }

        public static bool IsClient
        {
            get { return !Application.isBatchMode; }
        }

        public static bool IsDedicatedServer
        {
            get { return Application.isBatchMode && IsServer; }
        }

        public static bool IsListenServer
        {
            get { return IsServer && !Application.isBatchMode; }
        }

        public static bool IsInWorld
        {
            get { return ZNet.instance != null && ZRoutedRpc.instance != null; }
        }
    }
}
