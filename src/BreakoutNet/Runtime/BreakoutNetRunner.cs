using UnityEngine;
using System.Collections.Generic;

namespace BreakoutMods.BreakoutNet
{
    internal sealed class BreakoutNetRunner : MonoBehaviour
    {
        internal const string RoutedRpcName = "BreakoutNet.RPC";

        private ZRoutedRpc registeredInstance;
        private float nextSettingsBroadcast;
        private bool broadcastedThisSession;
        private readonly HashSet<long> knownPeers = new HashSet<long>();

        private void Update()
        {
            if (ZRoutedRpc.instance == null)
            {
                if (registeredInstance != null)
                {
                    BreakoutCoreHookRegistry.PublishWorldLeft();
                    knownPeers.Clear();
                }

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
                BreakoutCoreHookRegistry.PublishNetworkReady();
            }

            UpdatePeerHooks();

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

        private void UpdatePeerHooks()
        {
            if (ZNet.instance == null)
            {
                return;
            }

            HashSet<long> currentPeers = new HashSet<long>();

            foreach (ZNetPeer peer in ZNet.instance.GetConnectedPeers())
            {
                if (peer == null)
                {
                    continue;
                }

                currentPeers.Add(peer.m_uid);
                if (!knownPeers.Contains(peer.m_uid))
                {
                    BreakoutCoreHookRegistry.PublishPeerJoined(CreatePeerEvent(peer, false));
                }
            }

            ZNetPeer serverPeer = !ZNet.instance.IsServer() ? ZNet.instance.GetServerPeer() : null;
            if (serverPeer != null)
            {
                currentPeers.Add(serverPeer.m_uid);
                if (!knownPeers.Contains(serverPeer.m_uid))
                {
                    BreakoutCoreHookRegistry.PublishPeerJoined(CreatePeerEvent(serverPeer, true));
                }
            }

            foreach (long peerId in knownPeers)
            {
                if (!currentPeers.Contains(peerId))
                {
                    BreakoutCoreHookRegistry.PublishPeerLeft(new BreakoutPeerChangedEvent(peerId, string.Empty, false, peerId == ZNet.GetUID()));
                }
            }

            knownPeers.Clear();
            foreach (long peerId in currentPeers)
            {
                knownPeers.Add(peerId);
            }
        }

        private static BreakoutPeerChangedEvent CreatePeerEvent(ZNetPeer peer, bool isServerPeer)
        {
            string playerName = string.Empty;
            try
            {
                playerName = peer.m_playerName;
            }
            catch
            {
                playerName = string.Empty;
            }

            return new BreakoutPeerChangedEvent(peer.m_uid, playerName, isServerPeer, peer.m_uid == ZNet.GetUID());
        }
    }
}
