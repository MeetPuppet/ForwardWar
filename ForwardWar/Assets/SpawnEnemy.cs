using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject enemy;

    // Start is called before the first frame update
    void Start()
    {
        enemy = Instantiate<GameObject>(enemyPrefab, transform);
        enemy.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if(enemy == null)
        {
            enemy = Instantiate<GameObject>(enemyPrefab, transform);
            enemy.transform.localPosition = Vector3.zero;

            //스포너 밖인 경우
            //enemy = Instantiate<GameObject>(enemyPrefab, transform);
            //Vector3 pos = new Vector3(-778.057129f, 366.935852f, 1335.54895f);
            //enemy.transform.localPosition = pos;
        }
    }
}
