using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitch : ActSwitchObject
{
    public int ItemNum;
    // Update is called once per frame
    void Update()
    {

    }
    //상호작용 작동 시
    public override void ActivateObject(PlayerMove player)
    {
        player.RefreshItem(ItemNum);
    }
}
