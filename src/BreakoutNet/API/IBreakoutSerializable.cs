namespace BreakoutMods.BreakoutNet
{
    public interface IBreakoutSerializable
    {
        void Write(ZPackage package);

        void Read(ZPackage package);
    }
}
