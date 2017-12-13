using UnityEngine;

namespace Game.State
{
    public class Snowball : MonoBehaviour
    {
        [SerializeField] public static float Speed = 20f;
        public string OwnerID;
        [SerializeField] private float _lifespan = 3f;

        private int _objectID;

        public void Initialize(int id, string ownerID)
        {
            _objectID = id;
            OwnerID = ownerID;
        }

        public void Update()
        {
            _lifespan -= Time.deltaTime;
            if (_lifespan <= 0)
            {
                Destroy();
            }
        }

        public void Destroy()
        {
            World.Instance.DestroyObject(_objectID);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            Player player = col.gameObject.GetComponent<Player>();
            if (player.UserID != OwnerID)
            {
                player.GetHit(OwnerID);
                Destroy();
            }
        }
    }
}