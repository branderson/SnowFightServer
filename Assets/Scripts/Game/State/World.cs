using System;
using System.Collections.Generic;
using Assets.Utility;
using Networking;
using Networking.Data;
using UnityEngine;

namespace Game.State
{
    public class World : Singleton<World>
    {
        [SerializeField] private GameObject _playerPrefab;

        // Dictionary<clientID, Player>
        private Dictionary<int, Player> _players;

        protected World() { }

        public void Awake()
        {
            _players = new Dictionary<int, Player>();
        }

        /// <summary>
        /// Instantiate a player with the given ID and return a reference to it
        /// </summary>
        /// <param name="clientID">
        /// ID to assign to instantiated player
        /// </param>
        /// <returns>
        /// Instantiated player
        /// </returns>
        public Player AddPlayer(int clientID)
        {
            GameObject playerObject = Instantiate(_playerPrefab, this.transform);
            Player player = playerObject.GetComponent<Player>();
            player.Initialize(clientID);

            _players[clientID] = player;

            return player;
        }

        /// <summary>
        /// Remove the player with the given ID
        /// </summary>
        /// <param name="clientID">
        /// ID of player to remove
        /// </param>
        public void RemovePlayer(int clientID)
        {
            Player player = GetPlayer(clientID);
            if (player == null) return;

            _players.Remove(clientID);
            Destroy(player.gameObject);
        }

        /// <summary>
        /// Get the player with the given ID or null
        /// </summary>
        /// <param name="clientID">
        /// ID of player to get
        /// </param>
        /// <returns>
        /// Player with the given ID or null if not found
        /// </returns>
        public Player GetPlayer(int clientID)
        {
            Player player;
            if (_players.TryGetValue(clientID, out player))
            {
                return player;
            }
            return null;
        }

        public void SyncPlayers()
        {
            foreach (int clientID in _players.Keys)
            {
                Player player = _players[clientID];
                PlayerSync sync = new PlayerSync
                {
                    Time = DateTime.UtcNow,
                    ClientID = clientID,
                    PosX = player.transform.position.x,
                    PosY = player.transform.position.y,
                };
                Socket.Instance.SendPacket(sync, Packets.PlayerSync, clientID);
            }
        }
    }
}