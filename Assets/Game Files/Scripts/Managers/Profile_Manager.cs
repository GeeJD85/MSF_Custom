using Aevien.UI;
using Barebones.MasterServer;
using Barebones.Networking;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace GW.Master
{
    public class Profile_Manager : BaseClientModule
    {
        public ObservableProfile Profile { get; private set; }

        private Profile_View profileView;
        private ProfileSettings_View profileSettingsView;

        public event Action<short, IObservableProperty> OnPropertyUpdatedEvent;
        public UnityEvent OnProfileLoadedEvent;
        public UnityEvent OnProfileSavedEvent;

        protected override void Initialize()
        {
            profileView = ViewsManager.GetView<Profile_View>("ProfileView");
            profileSettingsView = ViewsManager.GetView<ProfileSettings_View>("ProfileSettingsView");

            Profile = new ObservableProfile
            {                
                new ObservableString((short)ObservablePropertyCodes.DisplayName)
            };

            Profile.OnPropertyUpdatedEvent += OnPropertyUpdatedEventHandler;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Profile.OnPropertyUpdatedEvent -= OnPropertyUpdatedEventHandler;
        }

        private void OnPropertyUpdatedEventHandler(short key, IObservableProperty property)
        {
            OnPropertyUpdatedEvent?.Invoke(key, property);
        }

        public void LoadProfile()
        {
            logger.Info("LoadProfile called");
            Msf.Events.Invoke(Event_Keys.showLoadingInfo, "Loading profile... please wait!");

            MsfTimer.WaitForSeconds(1, () =>
            {
                Msf.Client.Profiles.GetProfileValues(Profile, (successful, error) =>
                {
                    if (successful)
                    {
                        Msf.Events.Invoke(Event_Keys.hideLoadingInfo);
                        OnProfileLoadedEvent?.Invoke();
                    }
                    else
                    {
                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, $"An error has occured whilst retrieving your profile: " + error);
                    }
                });
            });
        }

        public void UpdateProfile()
        {

        }
    }
}