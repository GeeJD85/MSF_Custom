using Barebones.MasterServer;
using Barebones.Networking;
using UnityEngine;

namespace GW.Master
{
    public class MsfFriendClient : MsfBaseClient
    {
        public MsfFriendClient(IClientSocket connection) : base(connection) { }

        //Send a request to the server to retrieve friendlist values and apply to a provided friendlist
        public void GetFriendlistValues(ObservableFriendList friendlist, SuccessCallback callback)
        {
            GetFriendlistValues(friendlist, callback, Connection);
        }

        public void GetFriendlistValues(ObservableFriendList friendlist, SuccessCallback callback, IClientSocket connection)
        {
            if(!connection.IsConnected)
            {
                callback.Invoke(false, "Not connected");
                return;
            }

            if (friendlist == null)
                Debug.Log("No friendlist found");

            connection.SendMessage((short)MsfMessageCodes.ClientFriendlistRequest, friendlist.PropertyCount, (status, response) =>
            {
                if(status != ResponseStatus.Success)
                {
                    callback.Invoke(false, response.AsString("Uknown error retrieving friendlist"));
                    return;
                }

                //Use the bytes received to replicate friendlist
                connection.SetHandler((short)MsfMessageCodes.UpdateClientFriendlist, message =>
                {
                    friendlist.ApplyUpdates(message.AsBytes());
                });

                callback.Invoke(true, null);
            });
        }
    }
}