using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreakoutMods.BreakoutNet
{
    internal static class BreakoutRpcRegistry
    {
        private static readonly Dictionary<string, IHandler> ServerHandlers = new Dictionary<string, IHandler>();
        private static readonly Dictionary<string, IHandler> ClientHandlers = new Dictionary<string, IHandler>();
        private static readonly BreakoutRateLimiter InboundClientLimiter = new BreakoutRateLimiter(12f, 8f);
        private static int sequence;

        public static void RegisterServer<TMessage>(string rpcName, BreakoutRpcHandler<TMessage> handler, BreakoutRpcRateLimit rateLimit)
            where TMessage : IBreakoutSerializable, new()
        {
            Register(ServerHandlers, rpcName, new Handler<TMessage>(handler, rateLimit));
            BreakoutLog.Info(
                "Registered server RPC '{0}' for {1} with {2}.",
                rpcName,
                typeof(TMessage).FullName,
                DescribeRateLimit(rateLimit));
        }

        public static void RegisterClient<TMessage>(string rpcName, BreakoutRpcHandler<TMessage> handler)
            where TMessage : IBreakoutSerializable, new()
        {
            Register(ClientHandlers, rpcName, new Handler<TMessage>(handler, BreakoutRpcRateLimit.Unlimited));
            BreakoutLog.Info("Registered client RPC '{0}' for {1}.", rpcName, typeof(TMessage).FullName);
        }

        public static bool SendToServer<TMessage>(string rpcName, TMessage message, string senderModGuid)
            where TMessage : IBreakoutSerializable, new()
        {
            if (!CanSend(rpcName, message))
            {
                return false;
            }

            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                return DispatchLocalAsServer(rpcName, message, senderModGuid);
            }

            ZNetPeer serverPeer = BreakoutPeers.ServerPeer;
            if (serverPeer == null)
            {
                BreakoutLog.Warning("Cannot send RPC '{0}' to server because no server peer is available.", rpcName);
                return false;
            }

            SendPackage(serverPeer.m_uid, rpcName, message, senderModGuid);
            return true;
        }

        public static bool SendToPeer<TMessage>(long peerId, string rpcName, TMessage message, string senderModGuid)
            where TMessage : IBreakoutSerializable, new()
        {
            if (!BreakoutSide.IsServer)
            {
                BreakoutLog.Warning("Cannot send server RPC '{0}' to peer {1}; this side is not server-authoritative.", rpcName, peerId);
                return false;
            }

            if (!CanSend(rpcName, message))
            {
                return false;
            }

            if (BreakoutSide.IsListenServer && peerId == ZNet.GetUID())
            {
                return DispatchLocalAsClient(rpcName, message, senderModGuid);
            }

            SendPackage(peerId, rpcName, message, senderModGuid);
            return true;
        }

        public static bool Broadcast<TMessage>(string rpcName, TMessage message, string senderModGuid)
            where TMessage : IBreakoutSerializable, new()
        {
            if (!BreakoutSide.IsServer)
            {
                BreakoutLog.Warning("Cannot broadcast RPC '{0}'; this side is not server-authoritative.", rpcName);
                return false;
            }

            if (!CanSend(rpcName, message))
            {
                return false;
            }

            foreach (ZNetPeer peer in BreakoutPeers.ConnectedPeers)
            {
                if (peer != null)
                {
                    SendPackage(peer.m_uid, rpcName, message, senderModGuid);
                }
            }

            if (BreakoutSide.IsListenServer)
            {
                DispatchLocalAsClient(rpcName, message, senderModGuid);
            }

            return true;
        }

        public static int BroadcastExcept<TMessage>(long senderPeerId, string rpcName, TMessage message, string senderModGuid)
            where TMessage : IBreakoutSerializable, new()
        {
            if (!BreakoutSide.IsServer || !CanSend(rpcName, message))
            {
                return 0;
            }

            int sent = 0;
            foreach (ZNetPeer peer in BreakoutPeers.ConnectedPeers)
            {
                if (peer == null || peer.m_uid == senderPeerId)
                {
                    continue;
                }

                SendPackage(peer.m_uid, rpcName, message, senderModGuid);
                sent++;
            }

            if (BreakoutSide.IsListenServer && senderPeerId != ZNet.GetUID())
            {
                if (DispatchLocalAsClient(rpcName, message, senderModGuid))
                {
                    sent++;
                }
            }

            return sent;
        }

        public static int BroadcastNear<TMessage>(Vector3 position, float radius, string rpcName, TMessage message, string senderModGuid)
            where TMessage : IBreakoutSerializable, new()
        {
            if (!BreakoutSide.IsServer || !CanSend(rpcName, message))
            {
                return 0;
            }

            float radiusSquared = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            int sent = 0;

            foreach (ZNetPeer peer in BreakoutPeers.ConnectedPeers)
            {
                if (peer == null)
                {
                    continue;
                }

                if ((peer.GetRefPos() - position).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                SendPackage(peer.m_uid, rpcName, message, senderModGuid);
                sent++;
            }

            if (BreakoutSide.IsListenServer && Player.m_localPlayer != null)
            {
                Vector3 localPosition = Player.m_localPlayer.transform.position;
                if ((localPosition - position).sqrMagnitude <= radiusSquared)
                {
                    if (DispatchLocalAsClient(rpcName, message, senderModGuid))
                    {
                        sent++;
                    }
                }
            }

            return sent;
        }

        public static void Dispatch(long senderPeerId, ZPackage package)
        {
            bool isServerSide = BreakoutSide.IsServer;
            bool isFromServer = IsSenderServer(senderPeerId);
            DispatchToHandlers(senderPeerId, package, isServerSide, isFromServer);
        }

        internal static bool TryCreateDispatchPlanForTest(bool isServerSide, bool isFromServer, string rpcName, out string reason)
        {
            reason = null;

            if (string.IsNullOrWhiteSpace(rpcName))
            {
                reason = "RPC name is empty.";
                return false;
            }

            if (!isServerSide && !isFromServer)
            {
                reason = "Client-side RPC must come from server.";
                return false;
            }

            return true;
        }

        private static void DispatchToHandlers(long senderPeerId, ZPackage package, bool isServerSide, bool isFromServer)
        {
            BreakoutRpcEnvelope envelope;
            string reason;
            if (!BreakoutRpcEnvelope.TryReadHeader(package, out envelope, out reason))
            {
                BreakoutLog.Malformed(senderPeerId, "Rejected malformed BreakoutNet packet from peer {0}: {1}", senderPeerId, reason);
                BreakoutCoreHookRegistry.PublishRpcRejected(senderPeerId, string.Empty, reason, "malformed");
                return;
            }

            BreakoutCoreHookRegistry.PublishRpcReceived(new BreakoutRpcObservedEvent(
                senderPeerId,
                isFromServer,
                isServerSide,
                envelope.RpcName,
                envelope.SenderModGuid,
                envelope.MessageTypeName,
                envelope.Sequence));

            Dictionary<string, IHandler> handlers = isServerSide ? ServerHandlers : ClientHandlers;
            IHandler handler;
            if (!handlers.TryGetValue(envelope.RpcName, out handler))
            {
                BreakoutLog.Malformed(senderPeerId, "Rejected unregistered RPC '{0}' from peer {1}.", envelope.RpcName, senderPeerId);
                BreakoutCoreHookRegistry.PublishRpcRejected(senderPeerId, envelope.RpcName, "Unregistered RPC.", "unregistered");
                return;
            }

            if (!isServerSide && !isFromServer)
            {
                BreakoutLog.Malformed(senderPeerId, "Rejected client-side RPC '{0}' from non-server peer {1}.", envelope.RpcName, senderPeerId);
                BreakoutCoreHookRegistry.PublishRpcRejected(senderPeerId, envelope.RpcName, "Client-side RPC came from a non-server peer.", "unauthorized");
                return;
            }

            if (!string.Equals(handler.MessageTypeName, envelope.MessageTypeName, StringComparison.Ordinal))
            {
                BreakoutLog.Malformed(
                    senderPeerId,
                    "Rejected RPC '{0}' from peer {1}; expected message type '{2}' but received '{3}'.",
                    envelope.RpcName,
                    senderPeerId,
                    handler.MessageTypeName,
                    envelope.MessageTypeName);
                BreakoutCoreHookRegistry.PublishRpcRejected(senderPeerId, envelope.RpcName, "Message type mismatch.", "type-mismatch");
                return;
            }

            if (isServerSide && !isFromServer)
            {
                BreakoutRpcRateLimit rateLimit = handler.RateLimit ?? BreakoutRpcRateLimit.Default;
                if (rateLimit.Enabled)
                {
                    string limitKey = senderPeerId + ":" + envelope.RpcName;
                    if (!InboundClientLimiter.Allow(limitKey, Time.realtimeSinceStartup, rateLimit.Capacity, rateLimit.RefillPerSecond))
                    {
                        BreakoutLog.Malformed(
                            senderPeerId,
                            "Rate-limited inbound RPC '{0}' from peer {1}; capacity={2:0.##}, refill={3:0.##}/s.",
                            envelope.RpcName,
                            senderPeerId,
                            rateLimit.Capacity,
                            rateLimit.RefillPerSecond);
                        BreakoutCoreHookRegistry.PublishRpcRejected(senderPeerId, envelope.RpcName, "Rate-limited inbound RPC.", "rate-limit");
                        return;
                    }
                }
            }

            BreakoutRpcContext context = new BreakoutRpcContext(
                senderPeerId,
                isFromServer,
                isServerSide,
                envelope.RpcName,
                envelope.SenderModGuid,
                envelope.Sequence);

            handler.Invoke(context, package);
        }

        private static void Register(Dictionary<string, IHandler> handlers, string rpcName, IHandler handler)
        {
            if (string.IsNullOrWhiteSpace(rpcName))
            {
                throw new ArgumentException("RPC name cannot be empty.", nameof(rpcName));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            handlers[rpcName] = handler;
        }

        private static bool CanSend<TMessage>(string rpcName, TMessage message)
            where TMessage : IBreakoutSerializable, new()
        {
            if (ZRoutedRpc.instance == null)
            {
                BreakoutLog.Warning("Cannot send RPC '{0}' because ZRoutedRpc is not ready.", rpcName);
                return false;
            }

            if (string.IsNullOrWhiteSpace(rpcName))
            {
                BreakoutLog.Warning("Cannot send RPC with an empty name.");
                return false;
            }

            if (message == null)
            {
                BreakoutLog.Warning("Cannot send RPC '{0}' because message is null.", rpcName);
                return false;
            }

            return true;
        }

        private static void SendPackage<TMessage>(long targetPeerId, string rpcName, TMessage message, string senderModGuid)
            where TMessage : IBreakoutSerializable, new()
        {
            ZPackage package = CreatePackage(rpcName, message, senderModGuid);
            ZRoutedRpc.instance.InvokeRoutedRPC(targetPeerId, BreakoutNetRunner.RoutedRpcName, package);
        }

        private static bool DispatchLocalAsServer<TMessage>(string rpcName, TMessage message, string senderModGuid)
            where TMessage : IBreakoutSerializable, new()
        {
            ZPackage package = CreatePackage(rpcName, message, senderModGuid);
            long localPeerId = ZNet.instance != null ? ZNet.GetUID() : 0L;
            Dispatch(localPeerId, package);
            return true;
        }

        private static bool DispatchLocalAsClient<TMessage>(string rpcName, TMessage message, string senderModGuid)
            where TMessage : IBreakoutSerializable, new()
        {
            ZPackage package = CreatePackage(rpcName, message, senderModGuid);
            long serverPeerId = ZNet.instance != null ? ZNet.GetUID() : 0L;
            DispatchToHandlers(serverPeerId, package, false, true);
            return true;
        }

        private static ZPackage CreatePackage<TMessage>(string rpcName, TMessage message, string senderModGuid)
            where TMessage : IBreakoutSerializable, new()
        {
            ZPackage package = new ZPackage();
            int nextSequence = System.Threading.Interlocked.Increment(ref sequence);
            BreakoutRpcEnvelope.WriteHeader(package, rpcName, senderModGuid, typeof(TMessage).FullName, nextSequence);
            message.Write(package);
            package.SetPos(0);
            return package;
        }

        private static string DescribeRateLimit(BreakoutRpcRateLimit rateLimit)
        {
            rateLimit = rateLimit ?? BreakoutRpcRateLimit.Default;
            if (!rateLimit.Enabled)
            {
                return "no inbound client rate limit";
            }

            return string.Format(
                "inbound client rate limit capacity={0:0.##}, refill={1:0.##}/s",
                rateLimit.Capacity,
                rateLimit.RefillPerSecond);
        }

        private static bool IsSenderServer(long senderPeerId)
        {
            if (ZNet.instance == null)
            {
                return false;
            }

            if (ZNet.instance.IsServer())
            {
                return senderPeerId == ZNet.GetUID();
            }

            ZNetPeer serverPeer = ZNet.instance.GetServerPeer();
            return serverPeer != null && serverPeer.m_uid == senderPeerId;
        }

        private interface IHandler
        {
            string MessageTypeName { get; }

            BreakoutRpcRateLimit RateLimit { get; }

            void Invoke(BreakoutRpcContext context, ZPackage package);
        }

        private sealed class Handler<TMessage> : IHandler
            where TMessage : IBreakoutSerializable, new()
        {
            private readonly BreakoutRpcHandler<TMessage> handler;
            private readonly BreakoutRpcRateLimit rateLimit;

            public Handler(BreakoutRpcHandler<TMessage> handler, BreakoutRpcRateLimit rateLimit)
            {
                this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
                this.rateLimit = rateLimit ?? BreakoutRpcRateLimit.Default;
            }

            public string MessageTypeName
            {
                get { return typeof(TMessage).FullName; }
            }

            public BreakoutRpcRateLimit RateLimit
            {
                get { return rateLimit; }
            }

            public void Invoke(BreakoutRpcContext context, ZPackage package)
            {
                try
                {
                    TMessage message = new TMessage();
                    message.Read(package);
                    handler(context, message);
                }
                catch (Exception ex)
                {
                    BreakoutLog.Error("RPC '{0}' handler failed for peer {1}: {2}", context.RpcName, context.SenderPeerId, ex);
                }
            }
        }
    }
}
