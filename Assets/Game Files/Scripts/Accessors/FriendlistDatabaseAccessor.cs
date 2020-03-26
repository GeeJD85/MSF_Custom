using LiteDB;

namespace GW.Master
{
    public class FriendlistDatabaseAccessor : IFriendlistDatabaseAccessor
    {
        private LiteCollection<FriendlistData> friendlistData;
        private LiteDatabase database;

        public FriendlistDatabaseAccessor(LiteDatabase database)
        {
            this.database = database;

            friendlistData = this.database.GetCollection<FriendlistData>("frinedlist");
            friendlistData.EnsureIndex(a => a.Username, true);
        }

        public void RestoreFriendlist(ObservableServerFriendlist friendlist)
        {
            var data = FindOrCreateData(friendlist);
            friendlist.FromBytes(data.AddedFriends);
        }

        public void UpdateFriendlist(ObservableServerFriendlist friendlist)
        {
            var data = FindOrCreateData(friendlist);
            data.AddedFriends = friendlist.ToBytes();
            friendlistData.Update(data);
        }

        private FriendlistData FindOrCreateData(ObservableServerFriendlist friendlist)
        {
            var data = friendlistData.FindOne(a => a.Username == friendlist.Username);

            if(data == null)
            {
                data = new FriendlistData()
                {
                    Username = friendlist.Username,
                    AddedFriends = friendlist.ToBytes()
                };
                friendlistData.Insert(data);
            }
            return data;
        }

        private class FriendlistData
        {
            [BsonId]
            public string Username { get; set; }
            public byte[] AddedFriends { get; set; }
        }
    }
}