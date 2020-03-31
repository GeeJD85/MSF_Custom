using Barebones.Logging;
using Barebones.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barebones.MasterServer
{
    public abstract class BaseClientModule : MonoBehaviour
    {
        /// <summary>
        /// Logger connected to this module
        /// </summary>
        protected Logging.Logger logger;

        [Header("Base Module Settings"), SerializeField]
        protected LogLevel logLevel = LogLevel.Info;

        /// <summary>
        /// Current module connection
        /// </summary>
        public IClientSocket Connection { get; protected set; }

        /// <summary>
        /// Check if current module connection isconnected to server
        /// </summary>
        public bool IsConnected => Connection != null && Connection.IsConnected;

        protected virtual void Awake()
        {
            Connection = Msf.Connection;

            logger = Msf.Create.Logger(GetType().Name);
            logger.LogLevel = logLevel;

            Connection.AddConnectionListener(ConnectedToMaster);
            Connection.OnStatusChangedEvent += OnConnectionStatusChanged;
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void OnDestroy()
        {
            Connection.OnStatusChangedEvent -= OnConnectionStatusChanged;
            Connection.RemoveConnectionListener(ConnectedToMaster);
        }

        private void ConnectedToMaster()
        {
            Connection.RemoveConnectionListener(ConnectedToMaster);
            Connection.AddDisconnectionListener(DisconnectedToMaster);

            OnConnectedToMaster();
        }

        private void DisconnectedToMaster()
        {
            Connection.AddConnectionListener(ConnectedToMaster);
            Connection.RemoveDisconnectionListener(DisconnectedToMaster);

            OnDisconnectedFromMaster();
        }

        protected virtual void Initialize() { }

        protected virtual void OnConnectionStatusChanged(ConnectionStatus status) { }

        protected virtual void OnConnectedToMaster() { }

        protected virtual void OnDisconnectedFromMaster() { }
    }
}
