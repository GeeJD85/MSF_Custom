using Aevien.Utilities;
using Barebones.Logging;
using Barebones.Networking;
using Barebones.MasterServer;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GW.Master
{
    /// <summary>
    /// Automatically connects to master server
    /// </summary>
    public class ConnectionTo_Master : Singleton<ConnectionTo_Master>
    {
        protected int currentAttemptToConnect = 0;
        protected Barebones.Logging.Logger logger;

        [SerializeField]
        protected HelpBox header = new HelpBox()
        {
            Text = "This script automatically connects to master server. Is is just a helper",
            Type = HelpBoxType.Info
        };

        [Tooltip("Log level of this script"), SerializeField]
        protected LogLevel logLevel = LogLevel.Info;

        [Tooltip("If true, ip and port will be read from cmd args"), SerializeField]
        protected bool readMasterServerAddressFromCmd = true;

        [Tooltip("Address to the server"), SerializeField]
        protected string masterIp = "127.0.0.1";

        [Tooltip("Port of the server"), SerializeField]
        protected int masterPort = 5000;

        [Header("Automation"), Tooltip("If true, will try to connect on the Start()"), SerializeField]
        protected bool connectOnStart = false;

        [Header("Advanced"), SerializeField]
        protected float minTimeToConnect = 0.5f;
        [SerializeField]
        protected float maxTimeToConnect = 4f;
        [SerializeField]
        protected float timeToConnect = 0.5f;
        [SerializeField]
        protected int maxAttemptsToConnect = 5;

        public GameObject tryAgain;

        [Header("Events")]
        /// <summary>
        /// Triggers when connected to master server
        /// </summary>
        public UnityEvent OnConnectedEvent;

        /// <summary>
        /// triggers when disconnected from master server
        /// </summary>
        public UnityEvent OnDisconnectedEvent;

        /// <summary>
        /// Main connection to master server
        /// </summary>
        public IClientSocket Connection => Msf.Connection;

        protected override void Awake()
        {
            base.Awake();

            logger = Msf.Create.Logger(typeof(ConnectionTo_Master).Name);
            logger.LogLevel = logLevel;

            // In case this object is not at the root level of hierarchy
            // move it there, so that it won't be destroyed
            if (transform.parent != null)
            {
                transform.SetParent(null, false);
            }

            if (readMasterServerAddressFromCmd)
            {
                // If master IP is provided via cmd arguments
                if (Msf.Args.IsProvided(Msf.Args.Names.MasterIp))
                {
                    masterIp = Msf.Args.MasterIp;
                }

                // If master port is provided via cmd arguments
                if (Msf.Args.IsProvided(Msf.Args.Names.MasterPort))
                {
                    masterPort = Msf.Args.MasterPort;
                }
            }

            if (Msf.Args.AutoConnectClient)
            {
                connectOnStart = true;
            }
        }

        protected virtual void OnValidate()
        {
            if (maxAttemptsToConnect <= 0) maxAttemptsToConnect = 1;
        }

        protected virtual void Start()
        {
            if (connectOnStart)
            {
                StartConnection();
            }
        }

        /// <summary>
        /// Sets the master server IP
        /// </summary>
        /// <param name="masterIp"></param>
        public void SetIpAddress(string masterIp)
        {
            this.masterIp = masterIp;
        }

        /// <summary>
        /// Sets the master server port
        /// </summary>
        /// <param name="masterPort"></param>
        public void SetPort(int masterPort)
        {
            this.masterPort = masterPort;
        }

        /// <summary>
        /// Starts connection to master server
        /// </summary>
        public void StartConnection()
        {
            StartCoroutine(StartConnectionProcess(masterIp, masterPort, maxAttemptsToConnect));
        }

        public void StartConnection(int numberOfAttempts)
        {
            StartCoroutine(StartConnectionProcess(masterIp, masterPort, numberOfAttempts));
        }

        public void StartConnection(string serverIp, int serverPort, int numberOfAttempts = 5)
        {
            StartCoroutine(StartConnectionProcess(serverIp, serverPort, numberOfAttempts));
        }

        protected virtual IEnumerator StartConnectionProcess(string serverIp, int serverPort, int numberOfAttempts)
        {
            currentAttemptToConnect = 0;
            maxAttemptsToConnect = numberOfAttempts;

            // Wait a fraction of a second, in case we're also starting a master server at the same time
            yield return new WaitForSeconds(0.2f);

            if (!Connection.IsConnected)
            {
                Msf.Events.Invoke(Event_Keys.showLoadingInfo, "Connecting to server...");
                logger.Info($"Starting MSF Client... {Msf.Version}. Multithreading is: {(Msf.Runtime.SupportsThreads ? "On" : "Off")}");
            }

            Connection.AddConnectionListener(OnConnectedEventHandler);

            while (true)
            {
                // If is already connected break cycle
                if (Connection.IsConnected)
                {
                    yield break;
                }

                // If currentAttemptToConnect of attemts equals maxAttemptsToConnect stop connection
                if (currentAttemptToConnect == maxAttemptsToConnect)
                {
                    Msf.Events.Invoke(Event_Keys.hideLoadingInfo);
                    Msf.Events.Invoke(Event_Keys.showOkDialogBox, "Failed to connect to server. Check your connection or check service status");
                    logger.Info($"Client cannot to connect to MSF server at: {serverIp}:{serverPort}");
                    Connection.Disconnect();

                    tryAgain.SetActive(true);
                    yield break;
                }

                // If we got here, we're not connected
                if (Connection.IsConnecting)
                {
                    if (maxAttemptsToConnect > 0)
                    {
                        currentAttemptToConnect++;
                    }

                    Msf.Events.Invoke(Event_Keys.showLoadingInfo, "Connecting to server... " + currentAttemptToConnect + "/" + maxAttemptsToConnect);
                    logger.Info($"Retrying to connect to MSF server at: {serverIp}:{serverPort}");
                }
                else
                {
                    logger.Info($"Connecting to MSF server at: {serverIp}:{serverPort}");
                }

                if (!Connection.IsConnected)
                {
                    Connection.Connect(serverIp, serverPort);
                }

                // Give a few seconds to try and connect
                yield return new WaitForSeconds(timeToConnect);

                // If we're still not connected
                if (!Connection.IsConnected)
                {
                    timeToConnect = Mathf.Min(timeToConnect * 2, maxTimeToConnect);
                }
            }
        }

        protected virtual void OnDisconnectedEventHandler()
        {
            logger.Info($"Disconnected from MSF server");

            timeToConnect = minTimeToConnect;

            Connection.RemoveDisconnectionListener(OnDisconnectedEventHandler);

            OnDisconnectedEvent?.Invoke();
        }

        protected virtual void OnConnectedEventHandler()
        {
            logger.Info($"Connected to MSF server at: {masterIp}:{masterPort}");

            timeToConnect = minTimeToConnect;

            Connection.RemoveConnectionListener(OnConnectedEventHandler);
            Connection.AddDisconnectionListener(OnDisconnectedEventHandler);

            OnConnectedEvent?.Invoke();
        }

        protected virtual void OnApplicationQuit()
        {
            if (Connection != null)
            {
                Connection.Disconnect();
            }
        }
    }
}