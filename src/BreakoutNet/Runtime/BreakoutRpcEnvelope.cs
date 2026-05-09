using System;

namespace BreakoutMods.BreakoutNet
{
    internal sealed class BreakoutRpcEnvelope
    {
        public const int CurrentProtocolVersion = 1;

        public int ProtocolVersion { get; private set; }

        public string RpcName { get; private set; }

        public string SenderModGuid { get; private set; }

        public string MessageTypeName { get; private set; }

        public int Sequence { get; private set; }

        public static void WriteHeader(ZPackage package, string rpcName, string senderModGuid, string messageTypeName, int sequence)
        {
            package.Write(CurrentProtocolVersion);
            package.Write(rpcName ?? string.Empty);
            package.Write(string.IsNullOrWhiteSpace(senderModGuid) ? BreakoutNetPlugin.PluginGuid : senderModGuid);
            package.Write(messageTypeName ?? string.Empty);
            package.Write(sequence);
        }

        public static bool TryReadHeader(ZPackage package, out BreakoutRpcEnvelope envelope, out string reason)
        {
            envelope = null;
            reason = null;

            if (package == null)
            {
                reason = "Package is null.";
                return false;
            }

            try
            {
                envelope = new BreakoutRpcEnvelope
                {
                    ProtocolVersion = package.ReadInt(),
                    RpcName = package.ReadString(),
                    SenderModGuid = package.ReadString(),
                    MessageTypeName = package.ReadString(),
                    Sequence = package.ReadInt()
                };
            }
            catch (Exception ex)
            {
                reason = "Malformed BreakoutNet envelope: " + ex.Message;
                return false;
            }

            if (envelope.ProtocolVersion != CurrentProtocolVersion)
            {
                reason = "Unsupported protocol version " + envelope.ProtocolVersion + ".";
                return false;
            }

            if (string.IsNullOrWhiteSpace(envelope.RpcName))
            {
                reason = "RPC name is empty.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(envelope.MessageTypeName))
            {
                reason = "Message type name is empty.";
                return false;
            }

            return true;
        }
    }
}
