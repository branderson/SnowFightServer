using System;

namespace Networking.Data
{
    public class Envelope
    {
        public Packets PacketType = Packets.None;
        public byte[] Packet = null;
    }

    public class NotAnEnvelopeException : Exception
    {
        public NotAnEnvelopeException() :
            base("The message received is not a valid Envelope")
        {
        }
    }

    public class WrongPacketTypeException : Exception
    {
        public WrongPacketTypeException() :
            base("The packet received is not of the specified type")
        {
        }
    }
}