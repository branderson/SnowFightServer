using System.Collections.Generic;
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

        // Dictionary<port, socketID>
        private readonly Dictionary<int, int> _ports = new Dictionary<int, int>();

        protected Socket() { }

        private void Start()
        {
            Time.fixedDeltaTime = 1f / 30f;
            InitializeSocket();
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
            _ports.Add(_socketPort, _socketID);
            _ports.Add(_websocketPort, _websocketID);
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
                    ConnectionManager.Instance.AddClient(connectionID, _sockets[hostID]);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("Event: Disconnected, ClientID: " + connectionID);
                    ConnectionManager.Instance.RemoveClient(connectionID);
                    break;
                case NetworkEventType.DataEvent:
                    MessageReader.ReadMessage(buffer, connectionID);
                    break;
                case NetworkEventType.BroadcastEvent:
                    break;
            }
        }

        private void UpdateClients()
        {
            World.Instance.SyncPlayers();
        }

        public void SendPacket<T>(T packet, Packets packetType) where T : class
        {
            foreach (int clientID in ConnectionManager.Instance.GetClientIDs())
            {
                SendPacket(packet, packetType, clientID);
            }
        }

        public void SendPacket<T>(T packet, Packets packetType, string userID) where T : class
        {
            int clientID = ConnectionManager.Instance.GetClientID(userID);
            SendPacket(packet, packetType, clientID);
        }

        public void SendPacket<T>(T packet, Packets packetType, int clientID) where T : class
        {
            if (!ConnectionManager.Instance.GetClientConnected(clientID)) return;
            Envelope envelope = new Envelope
            {
                PacketType = packetType,
                Packet = SerializationHandler.Serialize(packet, _bufferSize - 512)
            };
            byte error;
            byte[] message = SerializationHandler.Serialize(envelope, _bufferSize);
            int socketID = _ports[ConnectionManager.Instance.GetClientPort(clientID)];
//            Thread.Sleep(200);
            NetworkTransport.Send(socketID, clientID, _channelID, message, _bufferSize, out error);
        }
    }
}
