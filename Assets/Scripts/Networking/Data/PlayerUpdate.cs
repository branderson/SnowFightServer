using System;
using UnityEngine;

namespace Networking.Data
{
    public class PlayerUpdate
    {
        public DateTime Time = DateTime.UtcNow;
        public int ClientID = 0;
        public float MoveX = 0;
        public float MoveY = 0;
        public bool Fire = false;
    }
}