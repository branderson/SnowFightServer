using Assets.Utility;
using UnityEngine;

namespace Game
{
    public class Prefabs : Singleton<Prefabs>
    {
        [SerializeField] public GameObject SnowballPrefab;

        protected Prefabs() { }
    }
}