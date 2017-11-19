using System;
using UnityEngine;

namespace Game.State
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private string _userID;
        private float _posX;
        private float _posY;
        public float Facing;
        public int Score;

        public string UserID
        {
            get { return _userID; }
        }

        public void Initialize(string userID)
        {
            _userID = userID;
        }

        public void SetPosition(Vector2 position)
        {
            _posX = position.x;
            _posY = position.y;
            transform.position = position;
        }

        public void Move(float x, float y)
        {
            _posX += x;
            _posY += y;
            SetPosition(new Vector2(_posX, _posY));
        }
    }

    [Serializable]
    public class SerializablePlayer
    {
        public string UserID;
        public int Score;

        public SerializablePlayer(Player player)
        {
            UserID = player.UserID;
            Score = player.Score;
        }
    }
}