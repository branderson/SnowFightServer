using System.Collections.Generic;
using System.Linq;
using Game.State;
using UnityEngine;

namespace Networking
{
    public class ConnectionManager : MonoBehaviour
    {
        // Dictionary<connectionID, port>
        private readonly Dictionary<int, int> _connections = new Dictionary<int, int>();

        private World _world;

        private void Start()
        {
            _world = GameObject.FindObjectOfType<World>();
        }

        public void AddClient(int clientID, int port)
        {
            _connections.Add(clientID, port);
            _world.AddPlayer(clientID);
        }

        public void RemoveClient(int clientID)
        {
            _connections.Remove(clientID);
            _world.RemovePlayer(clientID);
        }

        public int GetClientPort(int clientID)
        {
            return _connections[clientID];
        }

        public int[] GetClientIDs()
        {
            return _connections.Keys.ToArray();
        }
    }
}