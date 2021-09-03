using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitch : ActSwitchObject
{
    public int ItemNum;
    public int Power;
    public int Ammor;

    //상호작용 작동 시
    public override void ActivateObject(PlayerMove player)
    {
        player.RefreshItem(ItemNum);
        player.pf.gun = player.Weapons[ItemNum].GetComponent<AudioSource>();
        player.pf.GunSetting(ItemNum, Power, Ammor);
    }
}
