using UnityEngine;

namespace BreakoutMods.BreakoutNet
{
    internal sealed class BreakoutNetRunner : MonoBehaviour
    {
        internal const string RoutedRpcName = "BreakoutNet.RPC";

        private ZRoutedRpc registeredInstance;
        private float nextSettingsBroadcast;
        private bool broadcastedThisSession;

        private void Update()
        {
            if (ZRoutedRpc.instance == null)
            {
                registeredInstance = null;
                broadcastedThisSession = false;
                return;
            }

            if (registeredInstance != ZRoutedRpc.instance)
            {
                registeredInstance = ZRoutedRpc.instance;
                registeredInstance.Register<ZPackage>(RoutedRpcName, OnRoutedRpc);
                broadcastedThisSession = false;
                BreakoutLog.Info("Registered routed RPC endpoint '{0}'.", RoutedRpcName);
            }

            if (!BreakoutSide.IsServer)
            {
                return;
            }

            if (!broadcastedThisSession)
            {
                BreakoutSettingsSyncRegistry.BroadcastServerSettings();
                broadcastedThisSession = true;
                nextSettingsBroadcast = Time.time + BreakoutSettingsSyncRegistry.BroadcastIntervalSeconds;
            }

            if (Time.time >= nextSettingsBroadcast)
            {
                BreakoutSettingsSyncRegistry.BroadcastServerSettings();
                nextSettingsBroadcast = Time.time + BreakoutSettingsSyncRegistry.BroadcastIntervalSeconds;
            }
        }

        private static void OnRoutedRpc(long senderPeerId, ZPackage package)
        {
            BreakoutRpcRegistry.Dispatch(senderPeerId, package);
        }
    }
}
