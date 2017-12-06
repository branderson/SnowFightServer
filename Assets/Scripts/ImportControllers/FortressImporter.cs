using Game.State;
using TiledLoader;
using UnityEngine;

namespace ImportControllers
{
    [ExecuteInEditMode]
    public class FortressImporter : MonoBehaviour
    {
        private void HandleInstanceProperties()
        {
            Fortress fortress = GetComponent<Fortress>();
            fortress.ID = Mathf.FloorToInt(transform.position.x + transform.position.y * 1000);
            DestroyImmediate(GetComponent<TiledLoaderProperties>());
            DestroyImmediate(this, true);
        }
    }
}