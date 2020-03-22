using Aevien.UI;
using Barebones.MasterServer;
using Barebones.Networking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GW.Master
{
    public class Account_Manager : BaseClientModule
    {
        Login_View loginView;
        CreateAccount_View createAccountView;
        ResetPasswordRequest_View resetPassword_View;
        PasswordResetEntry_View passwordResetEntry_View;
        EmailConfirmation_View emailConfirmationView;

        Toggle rememberToggle;

        public UnityEvent OnSignedInEvent;
        public UnityEvent OnSignedOutEvent;
        public UnityEvent OnPasswordChangedEvent;
        public UnityEvent OnEmailConfirmedEvent;

        protected override void Initialize()
        {
            loginView = ViewsManager.GetView<Login_View>("LoginView");
            createAccountView = ViewsManager.GetView<CreateAccount_View>("CreateAccount");
            resetPassword_View = ViewsManager.GetView<ResetPasswordRequest_View>("ResetPasswordRequest");
            passwordResetEntry_View = ViewsManager.GetView<PasswordResetEntry_View>("PasswordResetEntry");
            emailConfirmationView = ViewsManager.GetView<EmailConfirmation_View>("EmailConfirmationView");

            rememberToggle = GameObject.Find("RememberToggle").GetComponent<Toggle>();
            //CheckPlayerPrefs();

            MsfTimer.WaitForEndOfFrame(() => 
            { 
                if(IsConnected)
                {
                    Msf.Events.Invoke(Event_Keys.hideLoadingInfo);                    
                    loginView.Show();
                }
            });            
        }

        public void SignIn()
        {
            Msf.Events.Invoke(Event_Keys.showLoadingInfo, "Signing in.. please wait!");

            MsfTimer.WaitForSeconds(1, () =>
            {
                Msf.Client.Auth.SignIn(loginView.Username, loginView.Password, (accountInfo, error) =>
                {
                    Msf.Events.Invoke(Event_Keys.hideLoadingInfo);

                    if (accountInfo != null)
                    {
                        loginView.Hide();
                        SetPlayerPrefs();

                        //If we want email verification
                        if (accountInfo.IsEmailConfirmed)
                        {
                            OnSignedInEvent?.Invoke();

                            //Use this to create a message when a user logs in
                            //Msf.Events.Invoke(Event_Keys.showOkDialogBox,
                            //new OkDialogBox_ViewEventMessage($"You have successfuly signed in as {Msf.Client.Auth.AccountInfo.Username} and now you can create another part of your cool game!"));

                            logger.Debug($"You are successfully logged in as {Msf.Client.Auth.AccountInfo.Username}");
                        }
                        else
                        {
                            RequestEmailConfirmationCode();
                        }
                    }
                    else
                    {
                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, "An error has occured whilst signing in: " + error);
                    }
                });
            });            
        }

        public void SignUp()
        {
            Msf.Events.Invoke(Event_Keys.showLoadingInfo, "Creating account.. please wait!");

            MsfTimer.WaitForSeconds(1, () =>
            {
                string username = createAccountView.Username;
                string email = createAccountView.Email;
                string password = createAccountView.Password;

                var credentials = new Dictionary<string, string>
                {
                    { "username", username },
                    { "email", email },
                    { "password", password }
                };

                Msf.Client.Auth.SignUp(credentials, (successful, error) =>
                {
                    Msf.Events.Invoke(Event_Keys.hideLoadingInfo);

                    if(successful)
                    {
                        createAccountView.Hide();
                        loginView.SetInputFieldsValues(username, password);
                        loginView.Show();
                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, "Account created successfully!");
                    }
                    else
                    {
                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, "An error has occured whilst creating account: " + error);
                    }
                });
            });
        }        

        public void RequestPasswordResetCode()
        {
            Msf.Events.Invoke(Event_Keys.showLoadingInfo, "Requesting reset code from master");

            MsfTimer.WaitForSeconds(1, () =>
            {
                Msf.Client.Auth.RequestPasswordReset(resetPassword_View.Email, (success, error) =>
                {
                    Msf.Events.Invoke(Event_Keys.hideLoadingInfo);

                    if (success)
                    {
                        resetPassword_View.Hide();
                        passwordResetEntry_View.Show();

                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, "We have sent a reset code to your email address at: " + resetPassword_View.Email);
                    }
                    else
                    {
                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, "There was an error retrieving a reset code: " + error);
                    }
                });
            });            
        }

        public void PasswordResetEntry()
        {
            Msf.Events.Invoke(Event_Keys.showLoadingInfo, "Resetting your password.. please wait!");

            MsfTimer.WaitForSeconds(1, () =>
            {
                Msf.Client.Auth.ChangePassword(new PasswordChangeData()
                {
                    Email = resetPassword_View.Email,
                    Code = passwordResetEntry_View.Code,
                    NewPassword = passwordResetEntry_View.NewPassword
                }, (successful, error) =>
                {
                    Msf.Events.Invoke(Event_Keys.hideLoadingInfo);

                    if (successful)
                    {                        
                        passwordResetEntry_View.Hide();
                        loginView.Show();

                        OnPasswordChangedEvent?.Invoke();

                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, "Password changed successfully!");
                    }
                    else
                    {
                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, "There was an error changing your password: " + error);
                    }
                });
            });
        }

        public void RequestEmailConfirmationCode()
        {
            Msf.Events.Invoke(Event_Keys.showLoadingInfo, "Sending confirmation code... please wait!");

            MsfTimer.WaitForSeconds(1, () =>
            {
                Msf.Client.Auth.RequestEmailConfirmationCode((successful, error) =>
                {
                    Msf.Events.Invoke(Event_Keys.hideLoadingInfo);

                    if(successful)
                    {
                        emailConfirmationView.Show();
                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, "We have sent and email with your confirmation code to your address: " + Msf.Client.Auth.AccountInfo.Email);
                    }
                    else
                    {
                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, "An error occured whilst requesting confirmation code: " + error);
                    }
                });
            });
        }

        public void ConfirmAccount()
        {
            Msf.Events.Invoke(Event_Keys.showLoadingInfo, "Confirming your account.. please wait!");

            MsfTimer.WaitForSeconds(1, () =>
            {
                string confirmationCode = emailConfirmationView.ConfirmationCode;

                Msf.Client.Auth.ConfirmEmail(confirmationCode, (successful, error) =>
                {
                    Msf.Events.Invoke(Event_Keys.hideLoadingInfo);

                    if(successful)
                    {
                        emailConfirmationView.Hide();
                        OnEmailConfirmedEvent?.Invoke();
                    }
                    else
                    {
                        Msf.Events.Invoke(Event_Keys.showOkDialogBox, "An error occured whilst confirming your account: " + error);
                    }
                });
            });
        }

        public void SignOut()
        {
            OnSignedOutEvent?.Invoke();
            Msf.Client.Auth.SignOut();

            ViewsManager.HideAllViews();
            Initialize();
        }

        public void Quit()
        {
            Msf.Runtime.Quit();
        }

        protected override void OnConnectedToMaster()
        {
            Msf.Events.Invoke(Event_Keys.hideLoadingInfo);

            if (Msf.Client.Auth.IsSignedIn)
            {
                logger.Info("Connected and signed in!");
                OnSignedInEvent?.Invoke();
            }
            else
            {
                logger.Info("Failed to sign in");
                Msf.Events.Invoke(Event_Keys.hideLoadingInfo);
                loginView.Show();
            }
        }

        protected override void OnDisconnectedFromMaster()
        {
            // Logout after diconnection
            Msf.Client.Auth.SignOut();

            Msf.Events.Invoke(Event_Keys.showOkDialogBox, "The connection to the server has been lost. " +
                "Please try again or contact the developers of the game or your internet provider.");

            ViewsManager.HideAllViews();
            Initialize();
            ConnectionToMaster.Instance.StartConnection();
        }

        #region PlayerPrefs
        void CheckPlayerPrefs()
        {
            rememberToggle.isOn = PlayerPrefs.GetInt("rememberMe", -1) > 0;
            loginView.SetInputFieldsValues(PlayerPrefs.GetString("username"), string.Empty);
        }

        void SetPlayerPrefs()
        {
            if(!rememberToggle.isOn)
            {
                PlayerPrefs.DeleteKey("username");
                PlayerPrefs.DeleteKey("rememberMe");
                return;
            }

            PlayerPrefs.SetString("username", loginView.Username);
            PlayerPrefs.SetInt("rememberMe", 1);
        }
        #endregion PlayerPrefs
    }
}