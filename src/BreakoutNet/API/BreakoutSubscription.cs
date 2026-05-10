using System;

namespace BreakoutMods.BreakoutNet
{
    public sealed class BreakoutSubscription : IDisposable
    {
        private readonly Action unsubscribe;
        private bool disposed;

        internal BreakoutSubscription(Action unsubscribe)
        {
            this.unsubscribe = unsubscribe ?? (() => { });
        }

        public bool IsDisposed
        {
            get { return disposed; }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            unsubscribe();
        }
    }
}
