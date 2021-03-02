using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GameData
{
    /*
     모델이나 기타 등등 불러둘 게임정보
     */
    private List<EnemyFSM> enemyList;
    public List<EnemyFSM> Enemy
    {
        get { return enemyList; }
    }
    public void AddEnemyList(EnemyFSM enemy)
    {
        enemyList.Add(enemy);
    }


    private int score;
    public int Score {
        get { return score; }
        set { score = value; }
    }

    public void Init()
    {
        score = 0;
        enemyList = new List<EnemyFSM>();
    }
}
