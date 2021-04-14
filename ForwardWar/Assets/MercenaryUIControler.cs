using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MercenaryUIControler : MonoBehaviour
{
    public GameObject MercenaryParent;
    MercenaryMovement[] MercInfo;

    void Start()
    {
        MercInfo = MercenaryParent.GetComponentsInChildren<MercenaryMovement>();
        
        for (int i=0;i< MercenaryParent.transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }
    
    void Update()
    {
        for (int i = 0; i < MercenaryParent.transform.childCount; ++i)
        {
            float MaxHP = MercInfo[i].MaxHP;
            MaxHP = MercInfo[i].HP / MaxHP;

            transform.GetChild(i).GetComponentInChildren<Slider>().value = MaxHP;
        }
    }
}
