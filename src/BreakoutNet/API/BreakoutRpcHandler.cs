namespace BreakoutMods.BreakoutNet
{
    public delegate void BreakoutRpcHandler<TMessage>(BreakoutRpcContext context, TMessage message)
        where TMessage : IBreakoutSerializable, new();
}
