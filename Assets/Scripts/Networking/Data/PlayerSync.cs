using System;

namespace Networking.Data
{
    public class PlayerSync
    {
        public DateTime Time = DateTime.UtcNow;
        public int ClientID = 0;
        public float PosX = 0f;
        public float PosY = 0f;
    }
}