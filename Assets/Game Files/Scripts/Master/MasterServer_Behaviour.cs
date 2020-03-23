﻿using Barebones.Logging;
using Barebones.Networking;
using Barebones.MasterServer;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace GW.Master
{
    /// <summary>
    /// Starts the master server
    /// </summary>
    public class MasterServer_Behaviour : ServerBehaviour
    {
        /// <summary>
        /// Singleton instance of the master server behaviour
        /// </summary>
        public static MasterServer_Behaviour Instance { get; private set; }

        /// <summary>
        /// Invoked when master server started
        /// </summary>
        public static event Action<MasterServer_Behaviour> OnMasterStartedEvent;

        /// <summary>
        /// Invoked when master server stopped
        /// </summary>
        public static event Action<MasterServer_Behaviour> OnMasterStoppedEvent;

        protected override void Awake()
        {
            base.Awake();

            // If instance of the server is already running
            if (Instance != null)
            {
                // Destroy, if this is not the first instance
                Destroy(gameObject);
                return;
            }

            // Create new instance
            Instance = this;

            // Move to root, so that it won't be destroyed
            // In case this MSF instance is a child of another gameobject
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }

            // Set server behaviour to be able to use in all levels
            DontDestroyOnLoad(gameObject);

            // Check is command line argument '-msfMasterPort' is defined
            if (Msf.Args.IsProvided(Msf.Args.Names.MasterIp))
            {
                ip = Msf.Args.MasterIp;
            }

            // Check is command line argument '-msfMasterPort' is defined
            if (Msf.Args.IsProvided(Msf.Args.Names.MasterPort))
            {
                port = Msf.Args.MasterPort;
            }
        }

        protected override void Start()
        {
            base.Start();

            // If master is allready running then return function
            if (IsRunning)
            {
                return;
            }

            // Start the server on next frame
            MsfTimer.WaitForEndOfFrame(() =>
            {
                StartServer();
            });
        }

        /// <summary>
        /// Start master server with given port
        /// </summary>
        /// <param name="port"></param>
        public override void StartServer(int port)
        {
            // If master is allready running then return function
            if (IsRunning)
            {
                return;
            }

            logger.Info($"Starting Master Server... {Msf.Version}");

            base.StartServer(port);
        }

        protected override void OnStartedServer()
        {
            logger.Info($"Master Server is started and listening to: {ip}:{port}");

            base.OnStartedServer();

            OnMasterStartedEvent?.Invoke(this);
        }

        protected override void OnStoppedServer()
        {
            logger.Info("Master Server is stopped");

            OnMasterStoppedEvent?.Invoke(this);
        }
    }
}