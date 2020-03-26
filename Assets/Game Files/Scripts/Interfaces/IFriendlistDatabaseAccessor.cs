
namespace GW.Master
{
    public interface IFriendlistDatabaseAccessor
    {
        void RestoreFriendlist(ObservableServerFriendlist friendlist);

        void UpdateFriendlist(ObservableServerFriendlist friendlist);
    }
}
