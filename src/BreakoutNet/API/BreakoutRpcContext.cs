namespace BreakoutMods.BreakoutNet
{
    public sealed class BreakoutRpcContext
    {
        internal BreakoutRpcContext(
            long senderPeerId,
            bool isFromServer,
            bool isServerSide,
            string rpcName,
            string senderModGuid,
            int sequence)
        {
            SenderPeerId = senderPeerId;
            IsFromServer = isFromServer;
            IsServerSide = isServerSide;
            RpcName = rpcName;
            SenderModGuid = senderModGuid;
            Sequence = sequence;
        }

        public long SenderPeerId { get; }

        public bool IsFromServer { get; }

        public bool IsServerSide { get; }

        public string RpcName { get; }

        public string SenderModGuid { get; }

        public int Sequence { get; }

        public bool IsRejected { get; private set; }

        public string RejectionReason { get; private set; }

        public void Reject(string reason)
        {
            IsRejected = true;
            RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Rejected by handler." : reason;
            BreakoutLog.Warning(
                "Rejected RPC '{0}' from peer {1}: {2}",
                RpcName,
                SenderPeerId,
                RejectionReason);
        }
    }
}
