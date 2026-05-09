using UnityEngine;

namespace BreakoutMods.BreakoutNet
{
    public static class BreakoutRpc
    {
        public static class Server
        {
            public static void Register<TRequest>(string rpcName, BreakoutRpcHandler<TRequest> handler)
                where TRequest : IBreakoutSerializable, new()
            {
                BreakoutRpcRegistry.RegisterServer(rpcName, handler);
            }

            public static bool SendToClient<TMessage>(long peerId, string rpcName, TMessage message, string senderModGuid = null)
                where TMessage : IBreakoutSerializable, new()
            {
                return BreakoutRpcRegistry.SendToPeer(peerId, rpcName, message, senderModGuid);
            }

            public static bool Broadcast<TMessage>(string rpcName, TMessage message, string senderModGuid = null)
                where TMessage : IBreakoutSerializable, new()
            {
                return BreakoutRpcRegistry.Broadcast(rpcName, message, senderModGuid);
            }

            public static bool BroadcastToAll<TMessage>(string rpcName, TMessage message, string senderModGuid = null)
                where TMessage : IBreakoutSerializable, new()
            {
                return Broadcast(rpcName, message, senderModGuid);
            }

            public static int BroadcastExcept<TMessage>(long senderPeerId, string rpcName, TMessage message, string senderModGuid = null)
                where TMessage : IBreakoutSerializable, new()
            {
                return BreakoutRpcRegistry.BroadcastExcept(senderPeerId, rpcName, message, senderModGuid);
            }

            public static int BroadcastNear<TMessage>(Vector3 position, float radius, string rpcName, TMessage message, string senderModGuid = null)
                where TMessage : IBreakoutSerializable, new()
            {
                return BreakoutRpcRegistry.BroadcastNear(position, radius, rpcName, message, senderModGuid);
            }
        }

        public static class Client
        {
            public static void Register<TMessage>(string rpcName, BreakoutRpcHandler<TMessage> handler)
                where TMessage : IBreakoutSerializable, new()
            {
                BreakoutRpcRegistry.RegisterClient(rpcName, handler);
            }

            public static bool SendToServer<TRequest>(string rpcName, TRequest request, string senderModGuid = null)
                where TRequest : IBreakoutSerializable, new()
            {
                return BreakoutRpcRegistry.SendToServer(rpcName, request, senderModGuid);
            }
        }
    }
}
