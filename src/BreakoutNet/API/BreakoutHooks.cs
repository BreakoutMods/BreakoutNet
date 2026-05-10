using System;

namespace BreakoutMods.BreakoutNet
{
    public sealed class BreakoutHooks
    {
        private readonly BreakoutModApp owner;

        internal BreakoutHooks(string modGuid, BreakoutModApp owner)
        {
            this.owner = owner;
        }

        public BreakoutSubscription OnNetworkReady(Action<BreakoutNetworkReadyEvent> handler)
        {
            return Track(BreakoutCoreHookRegistry.Subscribe(handler));
        }

        public BreakoutSubscription OnWorldLeft(Action<BreakoutWorldLeftEvent> handler)
        {
            return Track(BreakoutCoreHookRegistry.Subscribe(handler));
        }

        public BreakoutSubscription OnPeerJoined(Action<BreakoutPeerChangedEvent> handler)
        {
            return Track(BreakoutCoreHookRegistry.Subscribe(BreakoutCoreHookRegistry.PeerJoinedHookName, handler));
        }

        public BreakoutSubscription OnPeerLeft(Action<BreakoutPeerChangedEvent> handler)
        {
            return Track(BreakoutCoreHookRegistry.Subscribe(BreakoutCoreHookRegistry.PeerLeftHookName, handler));
        }

        public BreakoutSubscription OnRpcReceived(Action<BreakoutRpcObservedEvent> handler)
        {
            return Track(BreakoutCoreHookRegistry.Subscribe(handler));
        }

        public BreakoutSubscription OnRpcRejected(Action<BreakoutRpcRejectedEvent> handler)
        {
            return Track(BreakoutCoreHookRegistry.Subscribe(handler));
        }

        private BreakoutSubscription Track(BreakoutSubscription subscription)
        {
            if (owner != null)
            {
                owner.Track(subscription);
            }

            return subscription;
        }
    }
}
