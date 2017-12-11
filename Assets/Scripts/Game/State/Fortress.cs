using UnityEngine;

namespace Game.State
{
    public class Fortress : MonoBehaviour
    {
        [SerializeField] public int ID;
        public string OwnerID;
        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            _renderer.enabled = visible;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            Player player = col.gameObject.GetComponent<Player>();
            if (player && player.UserID == OwnerID && player.Carrying)
            {
                player.Build();
            }
        }
    }
}