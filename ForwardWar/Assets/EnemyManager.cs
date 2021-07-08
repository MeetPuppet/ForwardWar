using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject Player;
    public GameObject EnemyList;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HitEnemy(int arr, int power)
    {
        EnemyFSM enemy = EnemyList.transform.GetChild(arr).gameObject.GetComponent<EnemyFSM>();

        enemy.HP -= power;


        for (int i = 0; i < EnemyList.transform.childCount; ++i)
        {
        }
    }
}
