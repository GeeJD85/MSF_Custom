using Aevien.UI;
using TMPro;

public class PasswordResetEntry_View : UIView
{
    private TMP_InputField enterCodeField;
    private TMP_InputField enterNewPasswordField;
    private TMP_InputField reenterNewPasswordField;

    public string Code
    {
        get
        {
            return enterCodeField != null ? enterCodeField.text : string.Empty;
        }
    }

    public string NewPassword
    {
        get
        {
            return enterNewPasswordField != null ? enterNewPasswordField.text : string.Empty;
        }
    }

    public string NewPasswordConfirm
    {
        get
        {
            return reenterNewPasswordField != null ? reenterNewPasswordField.text : string.Empty;
        }
    }

    protected override void Start()
    {
        base.Start();

        enterCodeField = ChildComponent<TMP_InputField>("EnterCode");
        enterNewPasswordField = ChildComponent<TMP_InputField>("PasswordInput");
        reenterNewPasswordField = ChildComponent<TMP_InputField>("ReenterPassword");
    }
}
