using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spawner
{
    using Data;
    public class Respawner : MonoBehaviour
    {
        public GameObject obj;
        public int count = 5;
        private bool isSpawn = false;

        public IEnumerator SpawnObj()
        {
            while (count > 0)
            {
                if (isSpawn == false)
                {
                    Instantiate(obj, transform.position, transform.rotation);
                    isSpawn = true;
                }
                yield return null;
            }
            yield break;
        }

        public IEnumerator SpawnReset()
        {
            isSpawn = false;
            yield break;
        }

    }
}
