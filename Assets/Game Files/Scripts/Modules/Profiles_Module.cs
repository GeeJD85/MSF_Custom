using Barebones.Networking;
using Barebones.MasterServer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GW.Master
{
    public enum ObservablePropertyCodes { DisplayName }

    public class Profiles_Module : ProfilesModule
    {
        public HelpBox _header = new HelpBox()
        {
            Text = "This script is a custom module, which sets up profiles values for new users"
        };

        public override void Initialize(IServer server)
        {
            base.Initialize(server);
            
            //Set the new factory in ProfilesModule
            ProfileFactory = CreateProfileInServer;

            server.SetHandler((short)MsfMessageCodes.UpdateDisplayNameRequest, UpdateDisplayNameRequestHandler);
        }

        private ObservableServerProfile CreateProfileInServer(string username, IPeer clientPeer)
        {
            Debug.Log("Called it");
            return new ObservableServerProfile(username, clientPeer)
            {
                new ObservableString((short)ObservablePropertyCodes.DisplayName, username)
            };
        }

        #region Handlers

        private void UpdateDisplayNameRequestHandler(IIncommingMessage message)
        {
            var userExtension = message.Peer.GetExtension<IUserPeerExtension>();

            if(userExtension == null || userExtension.Account == null)
            {
                message.Respond("Invalid session", ResponseStatus.Unauthorized);
                return;
            }

            var newProfileData = new Dictionary<string, string>().FromBytes(message.AsBytes());

            try
            {
                if(ProfilesList.TryGetValue(userExtension.Username, out ObservableServerProfile profile))
                {
                    profile.GetProperty<ObservableString>((short)ObservablePropertyCodes.DisplayName).Set(newProfileData["displayName"]);

                    message.Respond(ResponseStatus.Success);
                }
                else
                {
                    message.Respond("Invalid session", ResponseStatus.Unauthorized);
                }
            }
            catch (Exception e)
            {
                message.Respond($"Internal server error: {e}", ResponseStatus.Error);
            }
        }

        #endregion Handlers
    }
}