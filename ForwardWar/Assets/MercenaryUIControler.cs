using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MercenaryUIControler : MonoBehaviour
{
    public GameObject MercenaryParent;
    MercenaryMovement[] MercInfo;
    GameObject[] Combat;
    GameObject[] Run;


    void Start()
    {
        MercInfo = MercenaryParent.GetComponentsInChildren<MercenaryMovement>();
        Run = new GameObject[MercenaryParent.transform.childCount];
        Combat = new GameObject[MercenaryParent.transform.childCount];

        for (int i=0;i< MercenaryParent.transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(true);
            Combat[i] = transform.GetChild(i).gameObject.transform.Find("Portrait").transform.Find("Combat").gameObject;
            Run[i] = transform.GetChild(i).gameObject.transform.Find("Portrait").transform.Find("Run").gameObject;
            Run[i].SetActive(false);
        }
    }
    
    void Update()
    {
        for (int i = 0; i < MercenaryParent.transform.childCount; ++i)
        {
            float MaxHP = MercInfo[i].MaxHP;
            MaxHP = MercInfo[i].HP / MaxHP;

            transform.GetChild(i).GetComponentInChildren<Slider>().value = MaxHP;

            if (MercInfo[i].guideLine == 0)
            {
                Combat[i].SetActive(true);
                Run[i].SetActive(false);
            }
            else
            {
                Combat[i].SetActive(false);
                Run[i].SetActive(true);
            }
        }
    }
}
