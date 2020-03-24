using GW.Master;

namespace Barebones.MasterServer
{
    /// <summary>
    /// Represents generic database for profiles
    /// </summary>
    public interface IProfilesDatabaseAccessor
    {
        IProfileData GetProfileByUsername(string username);
        /// <summary>
        /// Should restore all values of the given profile, 
        /// or not change them, if there's no entry in the database
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        void RestoreProfile(ObservableServerProfile profile);

        /// <summary>
        /// Should save updated profile into database
        /// </summary>
        /// <param name="profile"></param>
        void UpdateProfile(ObservableServerProfile profile);
    }
}