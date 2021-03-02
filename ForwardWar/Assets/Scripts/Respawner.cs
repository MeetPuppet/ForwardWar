using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spawner.Data
{
    public class Respawner : MonoBehaviour
    {
        public GameObject obj;
        private int count = 5;
        private bool isSpawn = false;

        private void Awake()
        {
            Updater.Add(SpawnObj());
        }

        public IEnumerator SpawnObj()
        {
            while (count > 0)
            {
                if (isSpawn == false)
                {
                    Transform[] childs = GetComponentsInChildren<Transform>();
                    count = childs.Length;
                    for (int i=0;i< childs.Length; ++i)
                    {
                        Instantiate(obj, childs[i].transform.position, childs[i].transform.rotation);
                    }

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
