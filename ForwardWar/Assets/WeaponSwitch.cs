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
    //��ȣ�ۿ� �۵� ��
    public override void ActivateObject(PlayerMove player)
    {
        player.RefreshItem(ItemNum);
    }
}
