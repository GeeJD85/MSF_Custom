#if (!UNITY_WEBGL && !UNITY_IOS) || UNITY_EDITOR
using Barebones.MasterServer;
using LiteDB;

namespace GW.Master
{
    public class ProfilesDatabase_Accessor : IProfilesDatabaseAccessor
    {
        private readonly LiteCollection<ProfileInfoData> profiles;
        private readonly LiteDatabase database;

        public ProfilesDatabase_Accessor(LiteDatabase database)
        {
            this.database = database;

            profiles = this.database.GetCollection<ProfileInfoData>("profiles");
            profiles.EnsureIndex(a => a.Username, true); //Ensure true means value must be unique
        }

        public IProfileData GetProfileByUsername(string username)
        {
            return profiles.FindOne(a => a.Username == username);
        }

        //Get profile info from database
        public void RestoreProfile(ObservableServerProfile profile)
        {
            var data = FindOrCreateData(profile);
            profile.FromBytes(data.Data);
        }

        //Update profile info in database
        public void UpdateProfile(ObservableServerProfile profile)
        {
            var data = FindOrCreateData(profile);
            data.Data = profile.ToBytes();
            profiles.Update(data);
        }

        //Find profile in database or create new data and insert into database
        private ProfileInfoData FindOrCreateData(ObservableServerProfile profile)
        {
            var data = profiles.FindOne(a => a.Username == profile.Username);

            if(data == null)
            {
                data = new ProfileInfoData()
                {
                    Username = profile.Username,
                    Data = profile.ToBytes()
                };
                profiles.Insert(data);
            }
            return data;
        }

        private class ProfileInfoData : IProfileData
        {
            [BsonId]
            public string Username { get; set; }
            public byte[] Data { get; set; }
        }
    }
}
#endif