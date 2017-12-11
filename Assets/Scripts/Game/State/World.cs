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

        private Dictionary<Fortress, Player> _fortresses;

        protected World() { }

        public void Awake()
        {
            _players = new Dictionary<string, Player>();
            _worldObjects = new Dictionary<int, GameObject>();
            _fortresses = new Dictionary<Fortress, Player>();
            LoadFortresses();
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
            // TODO: Prevent adding too many players
            GameObject playerObject = Instantiate(_playerPrefab, this.transform);
            Player player = playerObject.GetComponent<Player>();
            Fortress fortress = GetUnassignedFortress();
            player.Initialize(userID, fortress);
            AssignFortress(fortress, player);

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
                Fortress fortress = GetFortress(player.FortressID);
                newPlayer.Initialize(player.UserID, fortress);
                newPlayer.SetSkin(player.Skin);
                newPlayer.Score = player.Score;
                newPlayer.gameObject.SetActive(false);
                AssignFortress(fortress, newPlayer);

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
                PlayerSync sync = new PlayerSync
                {
                    Time = DateTime.UtcNow,
                    UserID = aboutID,
                    Team = TeamManager.Instance.GetUserTeam(aboutID).Name,
                    Score = player.Score,
                    Active = player.Active,
                    Skin = player.Skin,
                    PosX = player.transform.position.x,
                    PosY = player.transform.position.y,
                    Facing = player.Facing,
                    FortressID = player.Fortress.ID,
                    Health = player.Health,
                    Carrying = player.Carrying,
                    WasHit = player.WasHit,
                };
                Socket.Instance.SendPacket(sync, Packets.PlayerSync);
                player.WasHit = false;
            }
        }

        /// <summary>
        /// Load fortresses into the world
        /// </summary>
        private void LoadFortresses()
        {
            foreach (Fortress fortress in FindObjectsOfType<Fortress>())
            {
                _fortresses[fortress] = null;
            }
        }

        /// <summary>
        /// Get a fortress which has not yet been assigned to a player
        /// </summary>
        /// <returns>
        /// Fortress not yet assigned to a player
        /// </returns>
        private Fortress GetUnassignedFortress()
        {
            List<Fortress> unused = _fortresses.Where(item => item.Value == null).Select(item => item.Key).ToList();
            // TODO: Handle case where no unused fortress
            return unused[RNG.Instance.Random.Next(unused.Count)];
        }

        private void AssignFortress(Fortress fortress, Player owner)
        {
            // TODO: Prevent reassignment of fortresses
            _fortresses[fortress] = owner;
            fortress.OwnerID = owner.UserID;
            fortress.SetVisible(true);
        }

        /// <summary>
        /// Get the Fortress with the given ID
        /// </summary>
        /// <param name="fortressID">
        /// ID of Fortress to get
        /// </param>
        /// <returns>
        /// Fortress with the given ID
        /// </returns>
        public Fortress GetFortress(int fortressID)
        {
            Fortress fortress = _fortresses.Keys.FirstOrDefault(item => item.ID == fortressID);
            return fortress;
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