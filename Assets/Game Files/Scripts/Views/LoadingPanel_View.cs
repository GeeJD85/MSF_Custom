using Aevien.UI;
using Barebones.MasterServer;

namespace GW.Master
{
    public class LoadingPanel_View : PopupViewComponent
    {
        Loading_Panel myPanel;

        public override void OnOwnerStart()
        {
            myPanel = GetComponentInChildren<Loading_Panel>();

            Msf.Events.AddEventListener(Event_Keys.showLoadingInfo, OnShowLoadingInfoEventHandler);
            Msf.Events.AddEventListener(Event_Keys.hideLoadingInfo, OnHideLoadingInfoEventHandler);
        }

        private void OnShowLoadingInfoEventHandler(EventMessage message)
        {
            SetLables(message.GetData<string>());
            Owner.Show();
        }

        private void OnHideLoadingInfoEventHandler(EventMessage message)
        {
            Owner.Hide();
        }
    }
}