using System;
using UnityEngine;

namespace Networking.Data
{
    public class PlayerUpdate
    {
        public DateTime Time = DateTime.UtcNow;
        public string UserID;
        public float MoveX = 0;
        public float MoveY = 0;
        public float Facing = 0f;
        public bool PickUp = false;
    }
}