using Barebones.MasterServer;
using Barebones.Networking;
using System;
using System.Collections.Generic;

namespace GW.Master
{
    public class ObservableServerFriendlist : ObservableFriendList
    {
        public string Username { get; private set; }

        public List<string> AddedFriends { get; set; }

        public IPeer ClientPeer { get; set; }

        public event Action<ObservableServerFriendlist> OnModifiedInServerEvent;
        public event Action<ObservableServerFriendlist> OnDisposedEvent;

        public ObservableServerFriendlist(string username)
        {
            Username = username;
        }

        public ObservableServerFriendlist(string username, List<string> friends)
        {
            Username = username;
            if (friends == null)
                AddedFriends = new List<string>();
            else
                AddedFriends = friends;
        }

        protected override void OnDirtyProperty(IObservableProperty property)
        {
            base.OnDirtyProperty(property);

            if(OnModifiedInServerEvent != null)
            {
                OnModifiedInServerEvent.Invoke(this);
            }
        }

        protected void Dispose()
        {
            if(OnDisposedEvent != null)
            {
                Dispose();
            }

            OnModifiedInServerEvent = null;
            OnDisposedEvent = null;
            UnsavedProperties.Clear();
            ClearUpdates();
        }
    }
}