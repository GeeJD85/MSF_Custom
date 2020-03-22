using Aevien.UI;
using Barebones.MasterServer;

namespace GW.Master
{    public class DialoguePanel_View : PopupViewComponent
    {
        public override void OnOwnerStart()
        {
            Msf.Events.AddEventListener(Event_Keys.showOkDialogBox, OnShowEventDialogueEventHandler);
            Msf.Events.AddEventListener(Event_Keys.hideOkDialogBox, OnHideEventDialogueEventHandler);
        }

        private void OnShowEventDialogueEventHandler(EventMessage message)
        {
            SetLables(message.GetData<string>());
            Owner.Show();
        }

        private void OnHideEventDialogueEventHandler(EventMessage message)
        {
            Owner.Hide();
        }
    }
}