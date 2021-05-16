using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public int maxSpawn = 1;

    public GameObject enemyPrefab;
    public Transform EnemyParent;

    GameObject enemy;
    void Start()
    {
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        if(enemy == null && maxSpawn > 0)
        {
            //enemy = Instantiate<GameObject>(enemyPrefab, transform);
            //enemy.transform.localPosition = Vector3.zero;
            --maxSpawn;

            //스포너 밖인 경우
            Spawn();
        }
    }

    void Spawn()
    {
        enemy = Instantiate<GameObject>(enemyPrefab, EnemyParent);
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        enemy.transform.position = pos;
    }
}
