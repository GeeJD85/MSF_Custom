using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace GW.Master
{
    public class Friendplate_Function : MonoBehaviour
    {
        public string _username;

        TMP_Text myText => GetComponentInChildren<TMP_Text>();
        Friendlist friendList;
        Button myButton => GetComponentInChildren<Button>();

        private void Start()
        {
            friendList = FindObjectOfType<Friendlist>();
            myButton.onClick.AddListener(friendList.IdentifyActiveNameplate);
            myText.text = _username;
        }
    }
}