using System;

namespace Networking.Data
{
    public class PlayerSync
    {
        public DateTime Time = DateTime.UtcNow;
        public string UserID;
        public string Team;
        public float PosX = 0f;
        public float PosY = 0f;
        public float Facing = 0f;
        public bool WasHit = false;
        public bool PickedUp = false;
    }
}