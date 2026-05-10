using System;
using System.Collections.Generic;

namespace BreakoutMods.BreakoutNet
{
    internal static class BreakoutEventRegistry
    {
        private static readonly Dictionary<EventKey, List<ISubscription>> Subscriptions = new Dictionary<EventKey, List<ISubscription>>();

        public static BreakoutSubscription SubscribeScoped<TEvent>(string modGuid, Action<TEvent> handler)
            where TEvent : class, IBreakoutEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            EventKey key = EventKey.Scoped(modGuid, typeof(TEvent));
            return AddSubscription(key, new Subscription<TEvent>(modGuid, key.ToString(), handler));
        }

        public static BreakoutSubscription SubscribeNamed<TEvent>(string modGuid, string eventName, Action<TEvent> handler)
            where TEvent : class, IBreakoutEvent
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException("Event name cannot be empty.", nameof(eventName));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            EventKey key = EventKey.Named(eventName, typeof(TEvent));
            return AddSubscription(key, new Subscription<TEvent>(modGuid, eventName, handler));
        }

        public static void PublishScoped<TEvent>(string modGuid, TEvent evt)
            where TEvent : class, IBreakoutEvent
        {
            Publish(EventKey.Scoped(modGuid, typeof(TEvent)), modGuid, evt);
        }

        public static void PublishNamed<TEvent>(string modGuid, string eventName, TEvent evt)
            where TEvent : class, IBreakoutEvent
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException("Event name cannot be empty.", nameof(eventName));
            }

            Publish(EventKey.Named(eventName, typeof(TEvent)), modGuid, evt);
        }

        private static BreakoutSubscription AddSubscription(EventKey key, ISubscription subscription)
        {
            List<ISubscription> list;
            if (!Subscriptions.TryGetValue(key, out list))
            {
                list = new List<ISubscription>();
                Subscriptions[key] = list;
            }

            list.Add(subscription);
            return new BreakoutSubscription(() => RemoveSubscription(key, subscription));
        }

        private static void RemoveSubscription(EventKey key, ISubscription subscription)
        {
            List<ISubscription> list;
            if (!Subscriptions.TryGetValue(key, out list))
            {
                return;
            }

            list.Remove(subscription);
            if (list.Count == 0)
            {
                Subscriptions.Remove(key);
            }
        }

        private static void Publish<TEvent>(EventKey key, string publisherModGuid, TEvent evt)
            where TEvent : class, IBreakoutEvent
        {
            if (evt == null)
            {
                BreakoutLog.Warning("Ignored null event '{0}' from mod '{1}'.", key, publisherModGuid);
                return;
            }

            List<ISubscription> list;
            if (!Subscriptions.TryGetValue(key, out list))
            {
                return;
            }

            foreach (ISubscription subscription in list.ToArray())
            {
                subscription.Invoke(evt, publisherModGuid);
            }
        }

        internal static bool EventWithMultipleSubscribersIsDeliveredForTest()
        {
            int count = 0;
            BreakoutSubscription first = SubscribeScoped<TestEvent>("test.mod", e => count++);
            BreakoutSubscription second = SubscribeScoped<TestEvent>("test.mod", e => count++);
            PublishScoped("test.mod", new TestEvent());
            first.Dispose();
            second.Dispose();
            return count == 2;
        }

        internal static bool DisposedSubscriptionStopsDeliveryForTest()
        {
            int count = 0;
            BreakoutSubscription subscription = SubscribeScoped<TestEvent>("test.mod", e => count++);
            subscription.Dispose();
            PublishScoped("test.mod", new TestEvent());
            return count == 0;
        }

        internal static bool SubscriberExceptionDoesNotStopDispatchForTest()
        {
            int count = 0;
            BreakoutSubscription first = SubscribeScoped<TestEvent>("test.mod", e => { throw new InvalidOperationException("test"); });
            BreakoutSubscription second = SubscribeScoped<TestEvent>("test.mod", e => count++);
            PublishScoped("test.mod", new TestEvent());
            first.Dispose();
            second.Dispose();
            return count == 1;
        }

        private interface ISubscription
        {
            void Invoke(object evt, string publisherModGuid);
        }

        private sealed class Subscription<TEvent> : ISubscription
            where TEvent : class, IBreakoutEvent
        {
            private readonly string subscriberModGuid;
            private readonly string eventName;
            private readonly Action<TEvent> handler;

            public Subscription(string subscriberModGuid, string eventName, Action<TEvent> handler)
            {
                this.subscriberModGuid = subscriberModGuid;
                this.eventName = eventName;
                this.handler = handler;
            }

            public void Invoke(object evt, string publisherModGuid)
            {
                TEvent typedEvent = evt as TEvent;
                if (typedEvent == null)
                {
                    BreakoutLog.Warning(
                        "Rejected event '{0}' for subscriber '{1}' because payload type '{2}' does not match '{3}'.",
                        eventName,
                        subscriberModGuid,
                        evt != null ? evt.GetType().FullName : "null",
                        typeof(TEvent).FullName);
                    return;
                }

                try
                {
                    handler(typedEvent);
                }
                catch (Exception ex)
                {
                    BreakoutLog.Error(
                        "Event '{0}' handler for subscriber '{1}' failed. Publisher='{2}', payload='{3}': {4}",
                        eventName,
                        subscriberModGuid,
                        publisherModGuid,
                        typeof(TEvent).FullName,
                        ex);
                }
            }
        }

        private struct EventKey
        {
            private readonly string scope;
            private readonly string typeName;

            private EventKey(string scope, Type type)
            {
                this.scope = scope ?? string.Empty;
                typeName = type.FullName;
            }

            public static EventKey Scoped(string modGuid, Type type)
            {
                return new EventKey("scope:" + modGuid, type);
            }

            public static EventKey Named(string eventName, Type type)
            {
                return new EventKey("name:" + eventName, type);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((scope != null ? scope.GetHashCode() : 0) * 397) ^ (typeName != null ? typeName.GetHashCode() : 0);
                }
            }

            public override bool Equals(object obj)
            {
                if (!(obj is EventKey))
                {
                    return false;
                }

                EventKey other = (EventKey)obj;
                return string.Equals(scope, other.scope, StringComparison.Ordinal)
                       && string.Equals(typeName, other.typeName, StringComparison.Ordinal);
            }

            public override string ToString()
            {
                return scope + ":" + typeName;
            }
        }

        private sealed class TestEvent : IBreakoutEvent
        {
        }
    }
}
