using Barebones.MasterServer;
using Barebones.Networking;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace GW.Master
{
    public class Friendlist : MonoBehaviour
    {
        GameObject friendsList => gameObject;
        public GameObject friendPlatePrefab;

        public Transform prefabParent;        
        public Vector2 offScreenPos;
        public Vector2 onScreenPos;

        GameObject selectedFriendPlate;
        GameObject searchBar => GameObject.Find("SearchBar");
        TMP_InputField searchInput => GameObject.Find("SearchInput").GetComponent<TMP_InputField>();

        bool isOpen = false;
        bool searchOpen = false;

        public void OpenCloseList()
        {
            if (!isOpen)
            {
                LeanTween.scale(friendsList, onScreenPos, 0.1f);
                MsfTimer.WaitForSeconds(0.2f, () =>
                {
                    isOpen = true;
                    return;
                });
            }

            if (isOpen)
            {
                LeanTween.scale(friendsList, offScreenPos, 0.1f);
                MsfTimer.WaitForSeconds(0.2f, () =>
                {
                    isOpen = false;
                    return;
                });
            }
        }

        public void OpenSearch()
        {
            if (!searchOpen)
            {
                LeanTween.scale(searchBar, new Vector2(1, 1), 0.1f);
                MsfTimer.WaitForSeconds(0.2f, () =>
                {
                    searchOpen = true;
                    return;
                });
            }
            else
                SearchFriends(searchInput.text);
        }

        public void CloseSearch()
        {
            if (searchOpen)
            {
                LeanTween.scale(searchBar, new Vector2(0, 1), 0.1f);
                MsfTimer.WaitForSeconds(0.2f, () =>
                {
                    searchOpen = false;
                    return;
                });
            }
            searchInput.text = "";
        }

        public void SearchFriends(string username)
        {
            var authDbAccessor = Msf.Server.DbAccessors.GetAccessor<IAccountsDatabaseAccessor>();

            var userData = authDbAccessor.SearchUsers(username);

            if (userData == null)
            {
                Msf.Events.Invoke(Event_Keys.showOkDialogBox, "User not found. Check spelling and try again!");
                return;
            }

            if (userData.Username == username)
            {
                AddFriendPlate(username);
            }
        }

        public void AddFriendPlate(string username)
        {
            GameObject go = Instantiate(friendPlatePrefab, prefabParent);
            go.GetComponentInChildren<TMP_Text>().text = username;
        }

        public void IdentifyActiveNameplate()
        {
            selectedFriendPlate = (EventSystem.current.currentSelectedGameObject);
        }

        public void DeleteFriend()
        {
            if (selectedFriendPlate == null)
                return;

            Destroy(selectedFriendPlate);
        }
    }

}