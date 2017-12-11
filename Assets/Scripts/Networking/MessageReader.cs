using System.Collections.Generic;
using System.Linq;
using Game;
using Game.State;
using Networking.Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Networking
{
    public static class MessageReader
    {
        public static void ReadMessage(byte[] message)
        {
            Envelope envelope = SerializationHandler.Deserialize<Envelope>(message);
            if (envelope == null) throw new NotAnEnvelopeException();
            switch (envelope.PacketType)
            {
                case Packets.None:
                    Debug.Log("None");
                    break;
                case Packets.String:
                    string value = SerializationHandler.Deserialize<string>(envelope.Packet);
                    if (value == null) throw new WrongPacketTypeException();
                    Debug.Log(value);
                    break;
                case Packets.Login:
                    HandleLogin(SerializationHandler.Deserialize<Login>(envelope.Packet));
                    break;
                case Packets.JoinTeam:
                    HandleJoinTeam(SerializationHandler.Deserialize<JoinTeam>(envelope.Packet));
                    break;
                case Packets.PlayerUpdate:
                    HandlePlayerUpdate(SerializationHandler.Deserialize<PlayerUpdate>(envelope.Packet));
                    break;
                case Packets.SpawnSnowball:
                    HandleSpawnSnowball(SerializationHandler.Deserialize<SpawnSnowball>(envelope.Packet));
                    break;
                case Packets.SetSkin:
                    HandleSetSkin(SerializationHandler.Deserialize<SetSkin>(envelope.Packet));
                    break;
                case Packets.RequestLeaderboardData:
                    HandleRequestLeaderboardData(SerializationHandler.Deserialize<RequestLeaderboardData>(envelope.Packet));
                    break;
                default:
                    break;
            }
        }

        private static void HandleLogin(Login login)
        {
            if (login == null) throw new WrongPacketTypeException();
            ConnectionManager.Instance.Login(login.UserID, login.ClientID);
        }

        private static void HandleJoinTeam(JoinTeam joinTeam)
        {
            if (joinTeam == null) throw new WrongPacketTypeException();
            bool success = TeamManager.Instance.AddUserToTeam(joinTeam.UserID, joinTeam.TeamName);
            AckJoinTeam ack = new AckJoinTeam
            {
                UserID = joinTeam.UserID,
                TeamName = joinTeam.TeamName,
                Success = success,
            };
            Socket.Instance.SendPacket(ack, Packets.AckJoinTeam, ConnectionManager.Instance.GetClientID(joinTeam.UserID));
        }

        private static void HandlePlayerUpdate(PlayerUpdate update)
        {
            if (update == null) throw new WrongPacketTypeException();
            Player player = World.Instance.GetPlayer(update.UserID);
            if (player == null)
            {
                Debug.LogError("No player exists for PlayerUpdate received!");
                return;
            }
            if (!player.Active) return;
            player.Move(update.MoveX, update.MoveY);
            player.Facing = update.Facing;
            if (update.PickUp)
            {
                player.PickUp();
            }
        }

        private static void HandleSpawnSnowball(SpawnSnowball spawn)
        {
            if (spawn == null) throw new WrongPacketTypeException();
            Player player = World.Instance.GetPlayer(spawn.UserID);
            if (player.Carrying) return;
            GameObject snowball = GameObject.Instantiate(Prefabs.Instance.SnowballPrefab);
            snowball.transform.position = new Vector2(spawn.PosX, spawn.PosY);
            Vector2 angle = Quaternion.AngleAxis(spawn.Direction, Vector3.forward) * Vector3.down;
            Rigidbody2D rigidbody = snowball.GetComponent<Rigidbody2D>();
            // Ignore collisions with player who threw it
            Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();
            // TODO: Make this a helper function in utilities
            Collider2D[] playerCols = new Collider2D[playerRigidbody.attachedColliderCount];
            Collider2D[] snowCols = new Collider2D[rigidbody.attachedColliderCount];
            rigidbody.GetAttachedColliders(snowCols);
            foreach (Collider2D snowCol in snowCols)
            {
                foreach (Collider2D playerCol in playerCols)
                {
                    Physics2D.IgnoreCollision(snowCol, playerCol);
                }
            }

            rigidbody.velocity = angle * Snowball.Speed;

            int id = World.Instance.AddObject(snowball);
            snowball.GetComponent<Snowball>().Initialize(id, spawn.UserID);

            SnowballSync sync = new SnowballSync
            {
                ObjectID = id,
                PosX = spawn.PosX,
                PosY = spawn.PosY,
                Direction = spawn.Direction,
                Velocity = Snowball.Speed,
            };

            Socket.Instance.SendPacket(sync, Packets.SnowballSync);
        }

        private static void HandleSetSkin(SetSkin setSkin)
        {
            if (setSkin == null) throw new WrongPacketTypeException();
            Player player = World.Instance.GetPlayer(setSkin.UserID);
            player.SetSkin(setSkin.Skin);
        }

        private static void HandleRequestLeaderboardData(RequestLeaderboardData request)
        {
            if (request == null) throw new WrongPacketTypeException();
            LeaderboardDataEntry[] playerData = World.Instance.GetPlayers()
                .Select(item => new LeaderboardDataEntry { Name = item.UserID, Score = item.Score })
                .OrderByDescending(item => item.Score).ToArray();
            int rank = 1;
            foreach (LeaderboardDataEntry entry in playerData)
            {
                entry.Type = LeaderboardDataType.Player;
                entry.Rank = rank++;
                Socket.Instance.SendPacket(entry, Packets.LeaderboardData, request.UserID);
            }
            LeaderboardDataEntry[] teamData = TeamManager.Instance.GetTeams()
                .Select(item => new LeaderboardDataEntry { Name = item.Name, Score = item.Score })
                .OrderByDescending(item => item.Score).ToArray();
            rank = 1;
            foreach (LeaderboardDataEntry entry in teamData)
            {
                entry.Type = LeaderboardDataType.Team;
                entry.Rank = rank++;
                Socket.Instance.SendPacket(entry, Packets.LeaderboardData, request.UserID);
            }
            Socket.Instance.SendPacket(new EndLeaderboardResponse(), Packets.EndLeaderboardResponse, request.UserID);
        }
    }
}