using System;
using Networking;
using Networking.Data;
using UnityEngine;
using UnityEngine.WSA;

namespace Game.State
{
    public class Player : MonoBehaviour
    {
        public const int KillBonus = 1;
        public const int BuildBonus = 3;
        public const float MaxPositionRadius = 120f;

        [SerializeField] private string _userID;
        private float _posX;
        private float _posY;
        public bool Active = false;
        public Skins Skin = Skins.WhiteBlue;
        public Fortress Fortress;
        public float Facing;
        public int Health = 3;
        public int Score;
        public bool Carrying;

        public bool WasHit;

        public string UserID
        {
            get { return _userID; }
        }

        private void Awake()
        {
        }

        public void Initialize(string userID, Fortress fortress)
        {
            _userID = userID;
            Fortress  = fortress;
            Despawn();
        }

        public void SetSkin(Skins skin)
        {
            Skin = skin;
        }

        public void Spawn()
        {
            Health = 3;
            SetPosition(Fortress.transform.position);
            gameObject.SetActive(true);
            Active = true;
        }

        public void Despawn()
        {
            SetPosition(Fortress.transform.position);
            gameObject.SetActive(false);
            Active = false;
        }

        public void Respawn()
        {
            Health = 3;
            SetPosition(Fortress.transform.position);
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
            Vector2 newPos = new Vector2(_posX, _posY);
            if (newPos.sqrMagnitude > MaxPositionRadius * MaxPositionRadius) newPos = newPos.normalized * MaxPositionRadius;
            SetPosition(newPos);
        }

        public void PickUp()
        {
            if (Carrying)
            {
                Drop();
            }
            else
            {
                Collider2D[] results = new Collider2D[10];
                int count = Physics2D.OverlapCollider(GetComponent<Collider2D>(), default(ContactFilter2D), results);
                for (int i = 0; i < count; i++)
                {
                    SnowSource snow = results[i].GetComponent<SnowSource>();
                    if (snow)
                    {
                        snow.PickUp();
                        Carrying = true;
                    }
                }
            }
        }

        private void Drop()
        {
            GameObject pile = GameObject.Instantiate(Prefabs.Instance.SnowPilePrefab);
            pile.transform.position = new Vector2(transform.position.x, transform.position.y);

            int id = World.Instance.AddObject(pile);
            pile.GetComponent<SnowPile>().Initialize(id);
            Carrying = false;

            SnowPileSync sync = new SnowPileSync()
            {
                ObjectID = id,
                PosX = transform.position.x,
                PosY = transform.position.y,
            };

            Socket.Instance.SendPacket(sync, Packets.SnowPileSync);
        }

        private void Die()
        {
            Debug.Log(string.Format("Player {0} has died", UserID));
            Respawn();
        }

        public void GetHit(string hitBy)
        {
            Health--;
            Drop();
            if (Health <= 0)
            {
                Die();
                World.Instance.GetPlayer(hitBy).Score += KillBonus;
            }
            else WasHit = true;
        }

        public void Build()
        {
            Score += BuildBonus;
            Carrying = false;
        }
    }

    [Serializable]
    public class SerializablePlayer
    {
        public string UserID;
        public int FortressID;
        public int Score;
        public Skins Skin;

        public SerializablePlayer(Player player)
        {
            UserID = player.UserID;
            FortressID = player.Fortress.ID;
            Score = player.Score;
            Skin = player.Skin;
        }
    }
}