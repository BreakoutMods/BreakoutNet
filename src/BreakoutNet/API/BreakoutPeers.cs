using System.Collections.Generic;

namespace BreakoutMods.BreakoutNet
{
    public static class BreakoutPeers
    {
        private static readonly IReadOnlyList<ZNetPeer> EmptyPeers = new List<ZNetPeer>();

        public static ZNetPeer ServerPeer
        {
            get
            {
                if (ZNet.instance == null || ZNet.instance.IsServer())
                {
                    return null;
                }

                return ZNet.instance.GetServerPeer();
            }
        }

        public static IReadOnlyList<ZNetPeer> ConnectedPeers
        {
            get
            {
                if (ZNet.instance == null)
                {
                    return EmptyPeers;
                }

                return ZNet.instance.GetConnectedPeers();
            }
        }

        public static bool TryGetPeer(long peerId, out ZNetPeer peer)
        {
            peer = null;

            if (ZNet.instance == null)
            {
                return false;
            }

            ZNetPeer serverPeer = ServerPeer;
            if (serverPeer != null && serverPeer.m_uid == peerId)
            {
                peer = serverPeer;
                return true;
            }

            foreach (ZNetPeer connectedPeer in ZNet.instance.GetConnectedPeers())
            {
                if (connectedPeer != null && connectedPeer.m_uid == peerId)
                {
                    peer = connectedPeer;
                    return true;
                }
            }

            return false;
        }
    }
}
