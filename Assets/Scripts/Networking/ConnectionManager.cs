using System.Collections.Generic;
using System.Linq;
using Assets.Utility;
using Game.State;
using UnityEngine;

namespace Networking
{
    public class ConnectionManager : Singleton<ConnectionManager>
    {
        // Dictionary<clientID, port>
        private readonly Dictionary<int, int> _connections = new Dictionary<int, int>();

        // Dictionary<userID, loggedIn>
        private readonly Dictionary<string, bool> _loggedIn = new Dictionary<string, bool>();

        // Dictionary<userID, clientID>
        private readonly Dictionary<string, int> _users = new Dictionary<string, int>();

        // Dictionary<clientID, userID>
        private readonly Dictionary<int, string> _userClientIDs = new Dictionary<int, string>();

        protected ConnectionManager() { }

        public void AddClient(int clientID, int port)
        {
            _connections.Add(clientID, port);
        }

        public void RemoveClient(int clientID)
        {
            _connections.Remove(clientID);
            string userID;
            if (_userClientIDs.TryGetValue(clientID, out userID))
            {
                if (_loggedIn[userID]) Logout(userID);
            }
        }

        public bool GetClientConnected(int clientID)
        {
            return _connections.ContainsKey(clientID);
        }

        public int GetClientPort(int clientID)
        {
            int port;
            if (_connections.TryGetValue(clientID, out port))
            {
                return _connections[clientID];
            }
            return 0;
        }

        public bool Login(string userID, int clientID)
        {
            bool loggedIn;
            if (_loggedIn.TryGetValue(userID, out loggedIn))
            {
                if (loggedIn)
                {
                    Debug.Log(string.Format("User attempted to login as {0} but {0} is already logged in", userID));
                    return false;
                }
            }
            Player player = World.Instance.GetPlayer(userID);
            if (player == null)
            {
                player = World.Instance.AddPlayer(userID);
            }
            player.Spawn();
            _users[userID] = clientID;
            _userClientIDs[clientID] = userID;
            _loggedIn[userID] = true;
            Debug.Log(string.Format("Event: Log In, UserID: {0}, ClientID: {1}", userID, clientID));
            return true;
        }

        public void Logout(string userID)
        {
            if (!_loggedIn.ContainsKey(userID) || !_loggedIn[userID])
            {
                Debug.Log(string.Format("User {0} attempted to logout but was not logged in", userID));
                return;
            }
            Player player = World.Instance.GetPlayer(userID);
            if (player == null)
            {
                Debug.Log(string.Format("User {0} tried to logout but does not exist in the game", userID));
                return;
            }
            player.Despawn();
            Debug.Log("Event: Log Out, UserID: " + userID);
            player.gameObject.SetActive(false);
            _loggedIn[userID] = false;
        }

        public int GetClientID(string userID)
        {
            return _users[userID];
        }

        public int[] GetClientIDs()
        {
            return _connections.Keys.ToArray();
        }

        public string[] GetUserIDs()
        {
            return _users.Keys.ToArray();
        }
    }
}