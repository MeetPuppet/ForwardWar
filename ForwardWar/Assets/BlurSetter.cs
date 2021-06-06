using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurSetter : MonoBehaviour
{
    public GameObject BlurObject;
    public GameObject[] BlurList;
    public GameObject[] ClawList;

    void Start()
    {
        List<GameObject> LG = new List<GameObject>();

        for (int i = 0; i < ClawList.Length; ++i)
        {
            LG.Add(Instantiate(BlurObject));
        }

        BlurList = LG.ToArray();
        for (int i = 0; i < BlurList.Length; ++i)
        {
            BlurList[i].SetActive(false);
        }
    }

    void Update()
    {
        for(int i=0;i< BlurList.Length; ++i)
        {
            BlurList[i].transform.position = ClawList[i].transform.position;
        }
    }

    public void SwitchBlur(bool onoff)
    {
        for (int i = 0; i < BlurList.Length; ++i)
        {
            BlurList[i].SetActive(onoff);
        }
    }
}
