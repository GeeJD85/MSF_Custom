using Aevien.UI;
using TMPro;

namespace GW.Master
{
    public class Login_View : UIView
    {
        private TMP_InputField usernameInputField;
        private TMP_InputField passwordInputField;

        public string Username
        {
            get
            {
                return usernameInputField != null ? usernameInputField.text : string.Empty;
            }
        }

        public string Password
        {
            get
            {
                return passwordInputField != null ? passwordInputField.text : string.Empty;
            }
        }

        protected override void Start()
        {
            base.Start();

            usernameInputField = ChildComponent<TMP_InputField>("UsernameInput");
            passwordInputField = ChildComponent<TMP_InputField>("PasswordInput");
        }

        public void SetInputFieldsValues(string username, string password)
        {
            usernameInputField.gameObject.SetActive(false);
            usernameInputField.text = username;
            usernameInputField.gameObject.SetActive(true);
            passwordInputField.text = password;
        }
    }
}