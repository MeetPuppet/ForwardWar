using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlFence : MonoBehaviour
{
    public GameObject[] Fense;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OpenDoor()
    {
        for(int i = 0; i < Fense.Length; ++i)
        {
            Fense[i].SetActive(false);
        }
    }
    public void CloseDoor()
    {
        for (int i = 0; i < Fense.Length; ++i)
        {
            Fense[i].SetActive(true);
        }
    }
}
