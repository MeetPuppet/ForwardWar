using UnityEngine;
using UnityEditor;


namespace Spawner
{
    [CustomEditor(typeof(Respawner))]
    public class RespawnerEditor : Editor
    {
        Respawner window = null;
        public GameObject obj;
        public int count;

        void OnEnable()
        {
            window = (Respawner)target;
        }

    }
}