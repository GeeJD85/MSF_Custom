using Aevien.Utilities;
using Barebones.Logging;
using Barebones.Networking;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Barebones.MasterServer
{
    /// <summary>
    /// Automatically connects to master server
    /// </summary>
    public class ClientToMasterConnector : ConnectionHelper
    {
        [Header("sater Server Settings")]
        [Tooltip("If true, ip and port will be read from cmd args"), SerializeField]
        protected bool readMasterServerAddressFromCmd = true;

        protected override void Awake()
        {
            if (readMasterServerAddressFromCmd)
            {
                // If master IP is provided via cmd arguments
                if (Msf.Args.IsProvided(Msf.Args.Names.MasterIp))
                {
                    serverIp = Msf.Args.MasterIp;
                }

                // If master port is provided via cmd arguments
                if (Msf.Args.IsProvided(Msf.Args.Names.MasterPort))
                {
                    serverPort = Msf.Args.MasterPort;
                }
            }

            if (Msf.Args.AutoConnectClient)
            {
                connectOnStart = true;
            }

            Connection = Msf.Client.Connection;

            base.Awake();
        }
    }
}