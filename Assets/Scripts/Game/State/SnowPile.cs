using JetBrains.Annotations;
using UnityEngine;

namespace Game.State
{
    public class SnowPile : SnowSource
    {
        private int _objectID;
        [SerializeField] private float _lifespan = 10f;

        public void Initialize(int id)
        {
            _objectID = id;
//            if (transform.position.sqrMagnitude < 5) World.Instance.DestroyObject(_objectID);
        }

        private void Start()
        {
            Collider2D[] results = new Collider2D[10];
            Physics2D.OverlapCollider(GetComponent<Collider2D>(), default(ContactFilter2D), results);
            int count = Physics2D.OverlapCollider(GetComponent<Collider2D>(), default(ContactFilter2D), results);
            Debug.Log(count);
            for (int i = 0; i < count; i++)
            {
                SnowMountain mountain = results[i].GetComponent<SnowMountain>();
                if (mountain)
                {
                    World.Instance.DestroyObject(_objectID);
                }
            }
        }

        public void Update()
        {
            _lifespan -= Time.deltaTime;
            if (_lifespan <= 0)
            {
                World.Instance.DestroyObject(_objectID);
            }
        }

        public override void PickUp()
        {
            World.Instance.DestroyObject(_objectID);
        }
    }
}