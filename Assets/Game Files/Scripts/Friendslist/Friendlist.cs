using Barebones.MasterServer;
using Barebones.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

namespace GW.Master
{
    public class Friendlist : MonoBehaviour
    {
        List<IProfileData> myFriendsList;
        List<GameObject> friendPlatesInList; //So we can sort alphabetically etc
                
        public GameObject friendPlatePrefab;
        public Transform prefabParent;
        public Button searchAddButton;

        GameObject friendsList => gameObject;
        GameObject selectedFriendPlate;        
        GameObject searchBar => GameObject.Find("SearchBar");
        TMP_InputField searchInput => GameObject.Find("SearchInput").GetComponent<TMP_InputField>();

        bool isOpen = false;
        bool searchOpen = false;

        private void Start()
        {
            myFriendsList = new List<IProfileData>();
            friendPlatesInList = new List<GameObject>();
        }

        #region Open/Close Windows
        public void OpenCloseList()
        {
            if (!isOpen)
            {
                LeanTween.scale(friendsList, new Vector2(1,1), 0.1f);
                MsfTimer.WaitForSeconds(0.2f, () =>
                {
                    isOpen = true;
                    return;
                });
            }

            if (isOpen)
            {
                LeanTween.scale(friendsList, new Vector2(0,0), 0.1f);
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
                searchAddButton.GetComponentInChildren<TMP_Text>().text = "Add";
                LeanTween.scale(searchBar, new Vector2(1, 1), 0.1f);
                MsfTimer.WaitForSeconds(0.2f, () =>
                {
                    searchOpen = true;
                    searchInput.ActivateInputField(); //So we can type right after opening searchbar
                    return;
                });
            }
            else
            {
                SearchFriends(searchInput.text);
            }                
        }

        public void CloseSearch()
        {
            if (searchOpen)
            {
                searchAddButton.GetComponentInChildren<TMP_Text>().text = "Find friends";
                LeanTween.scale(searchBar, new Vector2(0, 1), 0.1f);
                MsfTimer.WaitForSeconds(0.2f, () =>
                {
                    searchOpen = false;                    
                    return;
                });
            }
            searchInput.text = "";
        }
        #endregion Open/Close Windows

        public void SearchFriends(string username)
        {
            var profilesDBAccessor = Msf.Server.DbAccessors.GetAccessor<IProfilesDatabaseAccessor>();

            var userData = profilesDBAccessor.GetProfileByUsername(username);

            if (userData == null) //Friend wasnt found
            {
                Msf.Events.Invoke(Event_Keys.showOkDialogBox, "User not found. Check spelling and try again!");
                searchInput.ActivateInputField();
                return;
            }

            if (userData.Username == username) //Friend found, add to friend list
            {
                UpdateFriendsList(userData);
                searchInput.text = "";
            }
        }

        void UpdateFriendsList(IProfileData friendData)
        {
            for(int i=0; i<myFriendsList.Count; i++)
            {
                if (friendData.Username == myFriendsList[i].Username)
                    return;
            }

            myFriendsList.Add(friendData);
            AddFriendPlate();
        }

        void AddFriendPlate()
        {
            for(int i=0; i < myFriendsList.Count; i++)
            {
                GameObject go = Instantiate(friendPlatePrefab, prefabParent);
                go.GetComponent<Friendplate_Function>().myData = myFriendsList[i];
                go.name = myFriendsList[i].Username;
                friendPlatesInList.Add(go);
            }
        }

        public void IdentifyActiveNameplate()
        {
            selectedFriendPlate = (EventSystem.current.currentSelectedGameObject);
        }

        public void DeleteFriend()
        {
            if (selectedFriendPlate == null)
                return;
            Debug.Log("Friends: " + myFriendsList.Count + " before deletion");
            for(int i=0; i < myFriendsList.Count; i++)
            {
                if(selectedFriendPlate.GetComponent<Friendplate_Function>()._username == myFriendsList[i].Username)
                {
                    friendPlatesInList.Remove(selectedFriendPlate);                    
                    myFriendsList.Remove(myFriendsList[i]);
                    Debug.Log("Friends: " + myFriendsList.Count + " after deletion");
                    Destroy(selectedFriendPlate);
                }
            }
        }
    }

}