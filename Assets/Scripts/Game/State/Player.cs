using UnityEngine;

namespace Game.State
{
    public class Player : MonoBehaviour
    {
        [SerializeField] public int ClientID { get; private set; }
        [SerializeField] private float _posX;
        [SerializeField] private float _posY;

        public void Initialize(int clientID)
        {
            ClientID = clientID;
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
}