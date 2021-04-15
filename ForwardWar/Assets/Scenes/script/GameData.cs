using UnityEngine;
using System.Collections;

public class GameData
{
    /*
     모델이나 기타 등등 불러둘 게임정보
     */
    public class MercFunc
    {
        //need nothing
        public static string MercAlertOrder = "AlertMovement";

        //need position
        public static string MercMoveOrder = "MoveTranslate";
        public static string MercFightOreder = "FightEnemys";
        public static string MercMoveFightOrder = "FightMoveTranslate";
    }

    public void Init()
    {

    }
}
