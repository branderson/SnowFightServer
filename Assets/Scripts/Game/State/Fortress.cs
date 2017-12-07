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
        }

        public void SetVisible(bool visible)
        {
            _renderer.enabled = visible;
        }
    }
}