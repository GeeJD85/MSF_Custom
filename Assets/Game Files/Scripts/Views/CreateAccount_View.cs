using Aevien.UI;
using TMPro;

namespace GW.Master
{
    public class CreateAccount_View : UIView
    {
        private TMP_InputField usernameInputField;
        private TMP_InputField emailAddressInputField;
        private TMP_InputField passwordInputField;
        private TMP_InputField reenterInputField;

        public string Username
        {
            get
            {
                return usernameInputField != null ? usernameInputField.text : string.Empty;
            }
        }

        public string Email
        {
            get
            {
                return emailAddressInputField != null ? emailAddressInputField.text : string.Empty;
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
            emailAddressInputField = ChildComponent<TMP_InputField>("EmailAddressInput");
            passwordInputField = ChildComponent<TMP_InputField>("PasswordInput");
            reenterInputField = ChildComponent<TMP_InputField>("ReenterInput");
        }

        public void SetInputFieldsValues(string username, string email, string password)
        {
            usernameInputField.text = username;
            emailAddressInputField.text = email;
            passwordInputField.text = password;
            reenterInputField.text = password;
        }
    }
}