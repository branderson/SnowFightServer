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
                default:
                    break;
            }
        }

        private static void HandleLogin(Login login)
        {
            if (login == null) throw new WrongPacketTypeException();
            bool success = ConnectionManager.Instance.Login(login.UserID, login.ClientID);
            AckLogin ack = new AckLogin
            {
                UserID = login.UserID,
                Success = success,
            };
            Socket.Instance.SendPacket(ack, Packets.AckLogin, login.ClientID);
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
    }
}