using Barebones.MasterServer;
using Barebones.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GW.Master
{
    public delegate ObservableServerFriendlist FriendlistFactory(string username, List<string> friendName);

    //Handles players friendlists within master server. Listen to changes in player friendlist
    //and sends updates to the client.
    public class BaseFriendlist_Module : BaseServerModule
    {
        public Auth_Module authModule;
        public Profiles_Module profilesModule;
        private HashSet<string> debouncedSaves;
        private HashSet<string> debouncedClientUpdates;

        public float unloadFriendlistDataAfter = 20f;
        public float saveFriendllistInterval = 1f;
        public float clientUpdateInterval = 0f;
        public IFriendlistDatabaseAccessor friendlistDatabaseAccessor;

        public FriendlistFactory FriendlistFactory { get; set; }
        public Dictionary<string, ObservableServerFriendlist> Friendslists { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            if (DestroyIfExists())
                return;

            //Profile module is required
            AddDependency<Profiles_Module>();

            //Auth module may not be used so possible to get rid of later? Use to check online status of players?
            AddOptionalDependency<Auth_Module>();

            Friendslists = new Dictionary<string, ObservableServerFriendlist>();
            debouncedSaves = new HashSet<string>();
            debouncedClientUpdates = new HashSet<string>();
        }

        public override void Initialize(IServer server)
        {
            authModule = server.GetModule<Auth_Module>();

            if(authModule != null)
            {
                authModule.OnUserLoggedInEvent += OnUserLoggedInEventHandler;
            }

            profilesModule = server.GetModule<Profiles_Module>();
            friendlistDatabaseAccessor = Msf.Server.DbAccessors.GetAccessor<IFriendlistDatabaseAccessor>();

            if (friendlistDatabaseAccessor == null)
                logger.Error("Friendlist database implementation was not found");

            server.SetHandler((short)MsfMessageCodes.ClientFriendlistRequest, ClientFriendlistRequestHandler);            
        }

        private void OnUserLoggedInEventHandler(IUserPeerExtension user)
        {
            user.Peer.OnPeerDisconnectedEvent += OnPlayerDisconnectedEventHandler;

            //Create a friendlist
            ObservableServerFriendlist friendlist;

            if(Friendslists.ContainsKey(user.Username))
            {
                //Found a friendlist from earlier so use this
                friendlist = Friendslists[user.Username];
                friendlist.ClientPeer = user.Peer;
            }
            else
            {
                //Create a new one if one wasnt found
                friendlist = CreateFriendlist(user.Username, user.Peer);
                Friendslists.Add(user.Username, friendlist);
            }

            //Save friendlist property
            friendlistDatabaseAccessor.RestoreFriendlist(friendlist);

            //Listen to friendlist events
            user.Peer.AddExtension(new FriendlistPeerExtension(friendlist, user.Peer));
        }

        //Create an observable friendlist for a client. Override if you want to customize creation
        protected virtual ObservableServerFriendlist CreateFriendlist(string username, IPeer clientPeer)
        {
            if(Friendslists != null)
            {
                var friendlist = FriendlistFactory(username, new List<string>());
                friendlist.ClientPeer = clientPeer;
                return friendlist;
            }

            return new ObservableServerFriendlist(username)
            {
                AddedFriends = new List<string>(),
                ClientPeer = clientPeer
            };
        }

        //Invoked when friendlist is changed
        private void OnFriendlistChangedEventHandler(ObservableServerFriendlist friendlist)
        {
            //Debouncing used to reduce number of updates per interval to one
            if(!debouncedSaves.Contains(friendlist.Username))
            {
                //If friendlist is not already waiting to be saved
                debouncedSaves.Add(friendlist.Username);
                StartCoroutine(SaveFriendlist(friendlist, saveFriendllistInterval));
            }

            if(!debouncedClientUpdates.Contains(friendlist.Username))
            {
                //If its a master server
                debouncedClientUpdates.Add(friendlist.Username);
                StartCoroutine(SendUpdatesToClient(friendlist, clientUpdateInterval));
            }
        }

        //Invoked when user logs out
        private void OnPlayerDisconnectedEventHandler(IPeer peer)
        {
            peer.OnPeerDisconnectedEvent -= OnPlayerDisconnectedEventHandler;

            var friendlistExtension = peer.GetExtension<FriendlistPeerExtension>();

            if (friendlistExtension == null)
                return;

            //Unload friendlist
            StartCoroutine(UnloadFriendlist(friendlistExtension.Username, unloadFriendlistDataAfter));
        }

        private IEnumerator SaveFriendlist(ObservableServerFriendlist friendlist, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            //Remove values from debounced updates
            debouncedSaves.Remove(friendlist.Username);

            friendlistDatabaseAccessor.UpdateFriendlist(friendlist);

            friendlist.UnsavedProperties.Clear();
        }

        private IEnumerator SendUpdatesToClient(ObservableServerFriendlist friendlist, float delay)
        {
            if (delay > 0.01f)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return null; //Wait one frame so we dont send multiple packets

            debouncedClientUpdates.Remove(friendlist.Username);

            if(friendlist.ClientPeer == null || !friendlist.ClientPeer.IsConnected)
            {
                //Client isnt connected so we dont need to send updates
                friendlist.ClearUpdates();
                yield break;
            }

            using (var ms = new MemoryStream())
            {
                using (var writer = new EndianBinaryWriter(EndianBitConverter.Big, ms))
                {
                    friendlist.GetUpdates(writer);
                    friendlist.ClearUpdates(); //Clear after sending
                }

                friendlist.ClientPeer.SendMessage(MessageHelper.Create((short)MsfMessageCodes.UpdateClientFriendlist,
                    ms.ToArray()), DeliveryMethod.ReliableSequenced);
            }
        }

        private IEnumerator UnloadFriendlist(string username, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            //If user is logged in, dont remove friendlist
            if(authModule.IsUserLoggedIn(username))
            {
                yield break;
            }

            Friendslists.TryGetValue(username, out ObservableServerFriendlist friendlist);

            if(friendlist == null)
            {
                yield break;
            }

            Friendslists.Remove(username);

            friendlist.OnModifiedInServerEvent -= OnFriendlistChangedEventHandler;
        }

        #region Handlers
        //Handles a request from the client to get their friendlist
        protected virtual void ClientFriendlistRequestHandler(IIncommingMessage message)
        {
            var clientPropCount = message.AsInt();

            var friendlistExt = message.Peer.GetExtension<FriendlistPeerExtension>();

            if(friendlistExt == null)
            {
                message.Respond("Friendlist not found", ResponseStatus.Failed);
                return;
            }

            friendlistExt.Friendlist.ClientPeer = message.Peer;

            if(clientPropCount != friendlistExt.Friendlist.PropertyCount)
            {
                logger.Error(string.Format($"Client requested a profile with {clientPropCount} properties but server "
                    + $"constructed a profile with {friendlistExt.Friendlist.PropertyCount}."));
            }

            message.Respond(friendlistExt.Friendlist.ToBytes(), ResponseStatus.Success);
        }
        #endregion Handlers
    }
}