using Barebones.MasterServer;
using UnityEngine;

namespace GW.Master
{
    public class DialoguePanel_Control : MonoBehaviour
    {
        public void HideDialogueView()
        {
            Msf.Events.Invoke(Event_Keys.hideOkDialogBox);
        }
    }
}