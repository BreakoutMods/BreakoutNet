using System;

namespace BreakoutMods.BreakoutNet
{
    internal static class BreakoutCoreHookRegistry
    {
        public const string PeerJoinedHookName = "breakoutnet.core.peer.joined";
        public const string PeerLeftHookName = "breakoutnet.core.peer.left";

        private const string CoreModGuid = "com.breakoutmods.valheim.breakoutnet";

        public static BreakoutSubscription Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : class, IBreakoutEvent
        {
            return BreakoutEventRegistry.SubscribeNamed(CoreModGuid, typeof(TEvent).FullName, handler);
        }

        public static BreakoutSubscription Subscribe<TEvent>(string hookName, Action<TEvent> handler)
            where TEvent : class, IBreakoutEvent
        {
            return BreakoutEventRegistry.SubscribeNamed(CoreModGuid, hookName, handler);
        }

        public static void PublishNetworkReady()
        {
            BreakoutEventRegistry.PublishNamed(
                CoreModGuid,
                typeof(BreakoutNetworkReadyEvent).FullName,
                new BreakoutNetworkReadyEvent(
                    BreakoutSide.IsServer,
                    BreakoutSide.IsClient,
                    BreakoutSide.IsDedicatedServer,
                    BreakoutSide.IsListenServer));
        }

        public static void PublishWorldLeft()
        {
            BreakoutEventRegistry.PublishNamed(CoreModGuid, typeof(BreakoutWorldLeftEvent).FullName, new BreakoutWorldLeftEvent());
        }

        public static void PublishPeerJoined(BreakoutPeerChangedEvent evt)
        {
            BreakoutEventRegistry.PublishNamed(CoreModGuid, PeerJoinedHookName, evt);
        }

        public static void PublishPeerLeft(BreakoutPeerChangedEvent evt)
        {
            BreakoutEventRegistry.PublishNamed(CoreModGuid, PeerLeftHookName, evt);
        }

        public static void PublishRpcReceived(BreakoutRpcObservedEvent evt)
        {
            BreakoutEventRegistry.PublishNamed(CoreModGuid, typeof(BreakoutRpcObservedEvent).FullName, evt);
        }

        public static void PublishRpcRejected(long senderPeerId, string rpcName, string reason, string category)
        {
            BreakoutEventRegistry.PublishNamed(
                CoreModGuid,
                typeof(BreakoutRpcRejectedEvent).FullName,
                new BreakoutRpcRejectedEvent(senderPeerId, rpcName, reason, category));
        }
    }
}
