using Aevien.UI;
using Barebones.MasterServer;
using TMPro;

namespace GW.Master
{
    public class Profile_View : UIView
    {
        private Profile_Manager profileManager;

        private TMP_Text displayName;

        public string DisplayName
        {
            get
            {
                return displayName != null ? displayName.text : string.Empty;
            }
            set
            {
                if (displayName)
                    displayName.text = value;
            }
        }

        protected override void Start()
        {
            base.Start();

            if(!profileManager)
            {
                profileManager = FindObjectOfType<Profile_Manager>();
            }
            profileManager.OnPropertyUpdatedEvent += ProfileManager_OnPropertyUpdatedEvent;

            displayName = ChildComponent<TMP_Text>("DisplayName");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(profileManager)
            {
                profileManager.OnPropertyUpdatedEvent -= ProfileManager_OnPropertyUpdatedEvent;
            }
        }

        private void ProfileManager_OnPropertyUpdatedEvent(short key, IObservableProperty property)
        {
            if(key == (short)ObservablePropertyCodes.DisplayName)
            {
                DisplayName = $"{ property.Serialize()}";
            }
        }
    }
}