namespace BreakoutMods.BreakoutNet
{
    public abstract class BreakoutModuleBase : IBreakoutModule
    {
        protected BreakoutModuleContext Context { get; private set; }

        public virtual void Initialize(BreakoutModuleContext context)
        {
            Context = context;
        }
    }

    public abstract class BreakoutSharedModule : BreakoutModuleBase
    {
    }

    public abstract class BreakoutServerModule : BreakoutModuleBase
    {
    }

    public abstract class BreakoutClientModule : BreakoutModuleBase
    {
    }
}
