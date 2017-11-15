using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets.Utility;
using Game.State;
using Networking.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    public class Socket : Singleton<Socket>
    {
        [SerializeField] private int _maxConnections = 140;
        [SerializeField] private int _socketPort = 30993;
        [SerializeField] private int _websocketPort = 30992;
        [SerializeField] private int _maxPacketsPerFrame = 500;
        private static int _bufferSize = 1024;
        private int _channelID;
        private int _socketID;
        private int _websocketID;

        // Dictionary<socketID, port>
        private readonly Dictionary<int, int> _sockets = new Dictionary<int, int>();

        private ConnectionManager _connectionManager;

        protected Socket() { }

        private void Start()
        {
            Time.fixedDeltaTime = 1f / 30f;
            InitializeSocket();

            _connectionManager = GetComponent<ConnectionManager>();
        }
	
        private void FixedUpdate()
        {
            PollSocket();
            UpdateClients();
        }

        private void OnApplicationQuit()
        {
            NetworkTransport.Shutdown();
        }

        private void InitializeSocket()
        {
            // Initialize socket
            NetworkTransport.Init();
            ConnectionConfig config = new ConnectionConfig();

            // Configure socket
            _channelID = config.AddChannel(QosType.Reliable);

            HostTopology topology = new HostTopology(config, _maxConnections);

            _socketID = NetworkTransport.AddHost(topology, _socketPort);
            _websocketID = NetworkTransport.AddWebsocketHost(topology, _websocketPort, null);

            // Map ports
            _sockets.Add(_socketID, _socketPort);
            _sockets.Add(_websocketID, _websocketPort);
        }

        private void PollSocket()
        {
            int packetLimit = _maxPacketsPerFrame;
            while (packetLimit-- >= 0)
            {
                // Poll for messages
                int recHostId;
                int recConnectionId;
                int recChannelId;
                byte[] recBuffer = new byte[_bufferSize];
                int bufferSize = _bufferSize;
                int dataSize;
                byte error;
                NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);
                if (recNetworkEvent == NetworkEventType.Nothing) break;
                HandleNetworkEvent(recNetworkEvent, recHostId, recConnectionId, recChannelId, recBuffer, dataSize, error);
            }
        }

        private void HandleNetworkEvent(NetworkEventType networkEvent, int hostID, int connectionID, int channelID, byte[] buffer, int dataSize, byte error)
        {
            switch (networkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("Event: Connected, ClientID: " + connectionID);
                    _connectionManager.AddClient(connectionID, _sockets[hostID]);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("Event: Disconnected, ClientID: " + connectionID);
                    _connectionManager.RemoveClient(connectionID);
                    break;
                case NetworkEventType.DataEvent:
                    MessageReader.ReadMessage(buffer);
                    break;
                case NetworkEventType.BroadcastEvent:
                    break;
            }
        }

        private void UpdateClients()
        {
            World.Instance.SyncPlayers();
        }

        public void SendPacket<T>(T packet, Packets packetType, int clientID) where T : class
        {
            Envelope envelope = new Envelope
            {
                PacketType = packetType,
                Packet = SerializationHandler.Serialize(packet, _bufferSize - 512)
            };
            byte error;
            byte[] message = SerializationHandler.Serialize(envelope, _bufferSize);
            int socketID = _sockets.First(item => item.Value ==  _connectionManager.GetClientPort(clientID)).Key;
//            Thread.Sleep(200);
            NetworkTransport.Send(socketID, clientID, _channelID, message, _bufferSize, out error);
        }

        public void SendJSONMessage(int clientID, PlayerUpdate message)
        {
            Debug.Log(string.Format("Event: Send Message, ClientID: {0}, Message: {1}", clientID, message));
            SendPacket(message, Packets.PlayerUpdate, clientID);
        }

        public void SendSocketMessage()
        {
            PlayerUpdate update = new PlayerUpdate
            {
            };
            foreach (int clientID in _connectionManager.GetClientIDs())
            {
                SendJSONMessage(clientID, update);
            }
        }
    }
}
