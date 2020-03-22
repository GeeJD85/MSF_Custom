using Aevien.UI;
using TMPro;

namespace GW.Master
{
    public class EmailConfirmation_View : UIView
    {
        TMP_InputField confirmationCodeField;

        public string ConfirmationCode
        {
            get
            {
                return confirmationCodeField != null ? confirmationCodeField.text : string.Empty;
            }
        }

        protected override void Start()
        {
            base.Start();

            confirmationCodeField = ChildComponent<TMP_InputField>("ConfirmationCodeInput");
        }
    }
}