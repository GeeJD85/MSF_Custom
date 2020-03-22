using Aevien.UI;
using TMPro;

namespace GW.Master
{
    public class ResetPasswordRequest_View : UIView
    {
        private TMP_InputField emailAddressInput;

        public string Email
        {
            get
            {
                return emailAddressInput != null ? emailAddressInput.text : string.Empty;
            }
        }

        protected override void Start()
        {
            base.Start();

            emailAddressInput = ChildComponent<TMP_InputField>("EmailAddressInput");
        }

        public void SetInputFieldValue(string email)
        {
            emailAddressInput.text = email;
        }
    }
}