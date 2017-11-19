using UnityEngine;

namespace Game.State
{
    public class Snowball : MonoBehaviour
    {
        [SerializeField] public static float Speed = 10f;
        [SerializeField] private float _lifespan = 3f;

        private int _objectID;

        public void Initialize(int id)
        {
            _objectID = id;
        }

        public void Update()
        {
            _lifespan -= Time.deltaTime;
            if (_lifespan <= 0)
            {
                World.Instance.DestroyObject(_objectID);
            }
        }
    }
}