using System;

namespace BreakoutMods.BreakoutNet
{
    public sealed class BreakoutEvents
    {
        private readonly string modGuid;
        private readonly BreakoutModApp owner;

        internal BreakoutEvents(string modGuid, BreakoutModApp owner)
        {
            this.modGuid = modGuid;
            this.owner = owner;
        }

        public BreakoutSubscription Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : class, IBreakoutEvent
        {
            return Track(BreakoutEventRegistry.SubscribeScoped(modGuid, handler));
        }

        public BreakoutSubscription Subscribe<TEvent>(string eventName, Action<TEvent> handler)
            where TEvent : class, IBreakoutEvent
        {
            return Track(BreakoutEventRegistry.SubscribeNamed(modGuid, eventName, handler));
        }

        public void Publish<TEvent>(TEvent evt)
            where TEvent : class, IBreakoutEvent
        {
            BreakoutEventRegistry.PublishScoped(modGuid, evt);
        }

        public void Publish<TEvent>(string eventName, TEvent evt)
            where TEvent : class, IBreakoutEvent
        {
            BreakoutEventRegistry.PublishNamed(modGuid, eventName, evt);
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
