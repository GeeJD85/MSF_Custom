using Barebones.MasterServer;
using Barebones.Networking;
using System;
using System.Collections.Generic;

namespace GW.Master
{
    public enum ObservableFriendCodes { Username, FriendName }

    public class Friendlist_Module : BaseFriendlist_Module
    {
        public HelpBox _header = new HelpBox()
        {
            Text = "This script is a custom module, which sets up friendlist values for new users"
        };

        public override void Initialize(IServer server)
        {
            base.Initialize(server);

            //Set the new factory in FriendlistModule
            FriendlistFactory = CreateFriendlistInServer;

            server.SetHandler((short)MsfMessageCodes.SearchForUserByName, SearchForUserByName);
            server.SetHandler((short)MsfMessageCodes.UpdateClientFriendlist, UpdateFriendlistHandler);
        }

        private ObservableServerFriendlist CreateFriendlistInServer(string username, List<string> friendNames)
        {
            return new ObservableServerFriendlist(username, friendNames)
            {
                new ObservableString((short)ObservableFriendCodes.Username, username)
            };
        }

        private void UpdateFriendlistHandler(IIncommingMessage message)
        {
            var userExtension = message.Peer.GetExtension<IUserPeerExtension>();

            if(userExtension == null || userExtension.Account == null)
            {
                message.Respond("Invalid session", ResponseStatus.Unauthorized);
            }

            var newFriendlistData = new List<string>().FromBytes(message.AsBytes());

            try
            {
                if(Friendslists.TryGetValue(userExtension.Username, out ObservableServerFriendlist friendlist))
                {
                    friendlist.AddedFriends = newFriendlistData;

                    message.Respond(ResponseStatus.Success);
                }
                else
                {
                    message.Respond("Friendlist couldnt be updated", ResponseStatus.Failed);
                }                
            }
            catch (Exception e)
            {
                message.Respond($"Internal server error: {e}", ResponseStatus.Error);
            }
        }

        private void SearchForUserByName(IIncommingMessage message)
        {
            var userExtension = message.Peer.GetExtension<IUserPeerExtension>();

            if (userExtension == null || userExtension.Account == null)
            {
                message.Respond("Invalid session", ResponseStatus.Unauthorized);
                return;
            }

            string username = message.AsString();

            //Check stored profile data - theres no need to create more data for the same person
            var userData = profilesModule.profileDatabaseAccessor.GetProfileByUsername(username);
            try
            {
                if (userData == null)
                {
                    message.Respond("User was not found. Please check spelling and try again!", ResponseStatus.Failed);
                    return;
                }
                else if (userData.Username == username)
                {
                    //TODO? Send relevant data back to be used clientside for friendPlate construction
                    message.Respond(username, ResponseStatus.Success);
                }                
            }
            catch (Exception e)
            {
                message.Respond($"Internal Server Fault: {e}", ResponseStatus.Error);
            }
        }
    }
}