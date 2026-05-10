namespace BreakoutMods.BreakoutNet
{
    public sealed class BreakoutNetworkReadyEvent : IBreakoutEvent
    {
        public BreakoutNetworkReadyEvent(bool isServer, bool isClient, bool isDedicatedServer, bool isListenServer)
        {
            IsServer = isServer;
            IsClient = isClient;
            IsDedicatedServer = isDedicatedServer;
            IsListenServer = isListenServer;
        }

        public bool IsServer { get; }

        public bool IsClient { get; }

        public bool IsDedicatedServer { get; }

        public bool IsListenServer { get; }
    }

    public sealed class BreakoutWorldLeftEvent : IBreakoutEvent
    {
        public BreakoutWorldLeftEvent()
        {
        }
    }

    public sealed class BreakoutPeerChangedEvent : IBreakoutEvent
    {
        public BreakoutPeerChangedEvent(long peerId, string playerName, bool isServerPeer, bool isLocalPeer)
        {
            PeerId = peerId;
            PlayerName = playerName ?? string.Empty;
            IsServerPeer = isServerPeer;
            IsLocalPeer = isLocalPeer;
        }

        public long PeerId { get; }

        public string PlayerName { get; }

        public bool IsServerPeer { get; }

        public bool IsLocalPeer { get; }
    }

    public sealed class BreakoutRpcObservedEvent : IBreakoutEvent
    {
        public BreakoutRpcObservedEvent(long senderPeerId, bool isFromServer, bool isServerSide, string rpcName, string senderModGuid, string messageTypeName, int sequence)
        {
            SenderPeerId = senderPeerId;
            IsFromServer = isFromServer;
            IsServerSide = isServerSide;
            RpcName = rpcName ?? string.Empty;
            SenderModGuid = senderModGuid ?? string.Empty;
            MessageTypeName = messageTypeName ?? string.Empty;
            Sequence = sequence;
        }

        public long SenderPeerId { get; }

        public bool IsFromServer { get; }

        public bool IsServerSide { get; }

        public string RpcName { get; }

        public string SenderModGuid { get; }

        public string MessageTypeName { get; }

        public int Sequence { get; }
    }

    public sealed class BreakoutRpcRejectedEvent : IBreakoutEvent
    {
        public BreakoutRpcRejectedEvent(long senderPeerId, string rpcName, string reason, string category)
        {
            SenderPeerId = senderPeerId;
            RpcName = rpcName ?? string.Empty;
            Reason = reason ?? string.Empty;
            Category = category ?? "unknown";
        }

        public long SenderPeerId { get; }

        public string RpcName { get; }

        public string Reason { get; }

        public string Category { get; }
    }
}
