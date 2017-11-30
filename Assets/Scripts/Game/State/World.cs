using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Utility;
using Networking;
using Networking.Data;
using UnityEngine;

namespace Game.State
{
    public class World : Singleton<World>
    {
        [SerializeField] private GameObject _playerPrefab;

        // Dictionary<userID, Player>
        private Dictionary<string, Player> _players;

        // Dictionary<objectID, GameObject>
        private Dictionary<int, GameObject> _worldObjects;

        protected World() { }

        public void Awake()
        {
            _players = new Dictionary<string, Player>();
            _worldObjects = new Dictionary<int, GameObject>();
        }

        /// <summary>
        /// Instantiate a player with the given ID and return a reference to it
        /// </summary>
        /// <param name="userID">
        /// ID to assign to instantiated player
        /// </param>
        /// <returns>
        /// Instantiated player
        /// </returns>
        public Player AddPlayer(string userID)
        {
            GameObject playerObject = Instantiate(_playerPrefab, this.transform);
            Player player = playerObject.GetComponent<Player>();
            player.Initialize(userID);

            _players[userID] = player;

            return player;
        }

        /// <summary>
        /// Remove the player with the given ID
        /// </summary>
        /// <param name="userID">
        /// ID of player to remove
        /// </param>
        public void RemovePlayer(string userID)
        {
            Player player = GetPlayer(userID);
            if (player == null) return;

            _players.Remove(userID);
            Destroy(player.gameObject);
        }

        /// <summary>
        /// Get the player with the given ID or null
        /// </summary>
        /// <param name="userID">
        /// ID of player to get
        /// </param>
        /// <returns>
        /// Player with the given ID or null if not found
        /// </returns>
        public Player GetPlayer(string userID)
        {
            Player player;
            if (_players.TryGetValue(userID, out player))
            {
                return player;
            }
            return null;
        }

        public List<Player> GetPlayers()
        {
            return _players.Values.ToList();
        }

        public void LoadPlayers(SerializablePlayer[] players)
        {
            foreach (Player player in _players.Values)
            {
                Destroy(player.gameObject);
            }
            _players.Clear();
            foreach (SerializablePlayer player in players)
            {
                GameObject playerObject = Instantiate(_playerPrefab, this.transform);
                Player newPlayer = playerObject.GetComponent<Player>();
                newPlayer.Initialize(player.UserID);
                newPlayer.Score = player.Score;
                newPlayer.gameObject.SetActive(false);

                _players[player.UserID] = newPlayer;
            }
        }

        /// <summary>
        /// Send state of each player to each player
        /// </summary>
        public void SyncPlayers()
        {
            foreach (string aboutID in _players.Keys)
            {
                Player player = _players[aboutID];
                if (!player.gameObject.activeInHierarchy) continue;
                PlayerSync sync = new PlayerSync
                {
                    Time = DateTime.UtcNow,
                    UserID = aboutID,
                    Team = TeamManager.Instance.GetUserTeam(aboutID).Name,
                    PosX = player.transform.position.x,
                    PosY = player.transform.position.y,
                    Facing = player.Facing,
                    Health = player.Health,
                    Carrying = player.Carrying,
                    WasHit = player.WasHit,
                };
                Socket.Instance.SendPacket(sync, Packets.PlayerSync);
                player.WasHit = false;
            }
        }

        /// <summary>
        /// Add the given GameObject to the world and return its assigned ObjectID
        /// </summary>
        /// <param name="obj">
        /// GameObject to add to the world
        /// </param>
        /// <returns>
        /// ObjectID assigned to object
        /// </returns>
        public int AddObject(GameObject obj)
        {
            while (true)
            {
                int id = RNG.Instance.Random.Next();
                if (_worldObjects.ContainsKey(id)) continue;
                _worldObjects[id] = obj;
                return id;
            }
        }

        public void DestroyObject(int objectID)
        {
            GameObject obj = GetObject(objectID);
            if (obj == null) return;
            _worldObjects.Remove(objectID);
            Destroy(obj);
            DestroyObject destroy = new DestroyObject
            {
                ObjectID = objectID,
            };
            Socket.Instance.SendPacket(destroy, Packets.DestroyObject);
        }

        public GameObject GetObject(int objectID)
        {
            GameObject obj;
            if (_worldObjects.TryGetValue(objectID, out obj))
            {
                return obj;
            }
            return null;
        }
    }
}