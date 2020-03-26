using Barebones.MasterServer;
using Barebones.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System;


//SORT THIS SHIT OUT AND MOVE DB STUFF TO A FRIENDLIST MANAGER!!!!!!



namespace GW.Master
{
    public class Friendlist : BaseClientModule
    {
        public ObservableFriendList ClientFriendList { get; private set; }

        public List<string> myFriendsList;
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
        public event Action<short, IObservableProperty> OnPropertyUpdatedEvent;

        protected override void Initialize()
        {
            myFriendsList = new List<string>();
            friendPlatesInList = new List<GameObject>();

            ClientFriendList = new ObservableFriendList
            {
                new ObservableString((short)ObservableFriendCodes.Username)
            };

            ClientFriendList.OnPropertyUpdatedEvent += OnPropertyUpdatedEventHandler;       
        }
        private void OnPropertyUpdatedEventHandler(short key, IObservableProperty property)
        {
            OnPropertyUpdatedEvent?.Invoke(key, property);
            logger.Debug($"Property with code: {key} were updated: {property.Serialize()}");
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
            Connection.SendMessage((short)MsfMessageCodes.SearchForUserByName, username.ToBytes(), OnSearchResultReceivedFromServer);
        }

        private void OnSearchResultReceivedFromServer(ResponseStatus status, IIncommingMessage response)
        {
            if (status == ResponseStatus.Success)
            {
                UpdateFriendsList(response.AsString());
                searchInput.text = "";
            }
            else
            {
                Msf.Events.Invoke(Event_Keys.showOkDialogBox, response.AsString());
                logger.Error(response.AsString());
                searchInput.text = "";
            }
        }

        void UpdateFriendsList(string friendName)
        {
            for(int i=0; i<myFriendsList.Count; i++)
            {
                if (friendName == myFriendsList[i])
                    return;
            }

            myFriendsList.Add(friendName);
            AddFriendPlate(friendName);
        }

        void AddFriendPlate(string friendName)
        {
            for(int i=0; i < myFriendsList.Count; i++)
            {
                GameObject go = Instantiate(friendPlatePrefab, prefabParent);
                go.GetComponent<Friendplate_Function>()._username = friendName;
                go.name = myFriendsList[i];
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
                if(selectedFriendPlate.GetComponent<Friendplate_Function>()._username == myFriendsList[i])
                {
                    friendPlatesInList.Remove(selectedFriendPlate);                    
                    myFriendsList.Remove(myFriendsList[i]);
                    Debug.Log("Friends: " + myFriendsList.Count + " after deletion");
                    Destroy(selectedFriendPlate);
                }
            }
        }

        public void LoadFriendlist()
        {
            Msf.Events.Invoke(Event_Keys.showLoadingInfo, "Loading profile... please wait!");

            MsfTimer.WaitForSeconds(1, () =>
            {
                Msf.Client.Friendlist.GetFriendlistValues(ClientFriendList, (successful, error) =>
                {
                    if (successful)
                    {
                        Msf.Events.Invoke(Event_Keys.hideLoadingInfo);
                    }
                    else
                    {
                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, $"An error has occured whilst retrieving your profile: " + error);
                    }
                });
            });
        }
    }

}