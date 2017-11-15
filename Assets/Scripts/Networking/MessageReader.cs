using Game.State;
using Networking.Data;
using UnityEngine;

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
                case Packets.PlayerUpdate:
                    HandlePlayerUpdate(SerializationHandler.Deserialize<PlayerUpdate>(envelope.Packet));
                    break;
                default:
                    break;
            }
        }

        private static void HandlePlayerUpdate(PlayerUpdate update)
        {
            if (update == null) throw new WrongPacketTypeException();
            Player player = World.Instance.GetPlayer(update.ClientID);
            player.Move(update.MoveX, update.MoveY);
        }
    }
}