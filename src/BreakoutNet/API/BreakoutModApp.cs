using System;
using System.Collections.Generic;

namespace BreakoutMods.BreakoutNet
{
    public sealed class BreakoutModApp : IDisposable
    {
        private readonly List<IBreakoutModule> modules = new List<IBreakoutModule>();
        private readonly List<BreakoutSubscription> subscriptions = new List<BreakoutSubscription>();
        private bool disposed;

        internal BreakoutModApp(string modGuid)
        {
            Context = new BreakoutModuleContext(modGuid, this);
        }

        public BreakoutModuleContext Context { get; }

        public string ModGuid
        {
            get { return Context.ModGuid; }
        }

        internal void AddModule(IBreakoutModule module)
        {
            modules.Add(module);
        }

        internal void Track(BreakoutSubscription subscription)
        {
            if (subscription != null)
            {
                subscriptions.Add(subscription);
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            foreach (BreakoutSubscription subscription in subscriptions.ToArray())
            {
                subscription.Dispose();
            }

            foreach (IBreakoutModule module in modules.ToArray())
            {
                IDisposable disposable = module as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            subscriptions.Clear();
            modules.Clear();
        }
    }
}
