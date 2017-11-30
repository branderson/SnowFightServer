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
        public int Health = 3;
        public int Score;
        public bool Carrying;

        public bool WasHit;

        private Rigidbody2D _rigidbody;

        public string UserID
        {
            get { return _userID; }
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
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

        public void PickUp()
        {
            // TODO: Implement this
            Carrying = true;
        }

        private void Die()
        {
            Debug.Log(string.Format("Player {0} has died", UserID));
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Snowball snowball = collision.gameObject.GetComponent<Snowball>();
            if (snowball)
            {
                Health--;
                // TODO: Reward snowball's owner
                snowball.Destroy();
                if (Health <= 0) Die();
                else WasHit = true;
            }
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