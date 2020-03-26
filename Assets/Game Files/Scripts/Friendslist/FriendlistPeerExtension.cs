using Barebones.MasterServer;
using Barebones.Networking;

namespace GW.Master
{
    public class FriendlistPeerExtension : IPeerExtension
    {
        public string Username { get; private set; }

        public ObservableServerFriendlist Friendlist { get; private set; }

        public IPeer Peer { get; private set; }

        public FriendlistPeerExtension(ObservableServerFriendlist friendlist, IPeer peer)
        {
            Username = friendlist.Username;
            Friendlist = friendlist;
            Peer = peer;
        }
    }
}