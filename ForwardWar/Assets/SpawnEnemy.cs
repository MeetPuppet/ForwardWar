using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject enemyPrefab;
    GameObject enemy;
    public GameObject Sample;

    void Start()
    {
        Destroy(Sample);
    }

    public void Spawn()
    {
        if (enemyPrefab == null)
            return;
        enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        enemyPrefab = null;
    }
    public bool isDead()
    {
        if (enemy)
            return false;
        return true;
    }
}
