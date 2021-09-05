using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EventFlag : MonoBehaviour
{
    public bool SpawnFirst;
    public PlayableDirector[] Timelines;
    public SpawnEnemy[] ActiveSpawners;
    public SpawnEnemy[] AfterSpawn;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7 && SpawnFirst)
        {
            GameManager.Updater.Add(SpawnFirstAct());
        }
        else if (other.gameObject.layer == 7)
        {
            GameManager.Updater.Add(TimelinesFirstAct());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 7)
            Destroy(gameObject);
    }

    IEnumerator SpawnFirstAct()
    {
        for (int i = 0; i < ActiveSpawners.Length; ++i)
        {
            ActiveSpawners[i].Spawn();
            yield return null;
        }
        int deadCheck = 0;
        while (deadCheck < ActiveSpawners.Length)
        {
            for (int i = 0; i < ActiveSpawners.Length; ++i)
            {
                if (ActiveSpawners[i] != null)
                {
                    if (ActiveSpawners[i].isDead())
                    {
                        Destroy(ActiveSpawners[i].gameObject);
                        ActiveSpawners[i] = null;
                        ++deadCheck;
                    }
                }
                yield return null;
            }
        };

        for (int i = 0; i < Timelines.Length; ++i)
        {
            Timelines[i].gameObject.SetActive(true);
            Timelines[i].Play();
            while (Timelines[i].state == PlayState.Playing)
            {
                yield return null;
            }
        }

        for (int i = 0; i < AfterSpawn.Length; ++i)
        {
            AfterSpawn[i].Spawn();
            yield return null;
        }
        yield break;
    }
    IEnumerator TimelinesFirstAct()
    {
        for (int i = 0; i < Timelines.Length; ++i)
        {
            Timelines[i].gameObject.SetActive(true);
            Timelines[i].Play();
            while (Timelines[i].state == PlayState.Playing)
            {
                yield return null;
            }
        }

        for (int i = 0; i < ActiveSpawners.Length; ++i)
        {
            ActiveSpawners[i].Spawn();
            yield return null;
        }
        yield break;
    }
}
