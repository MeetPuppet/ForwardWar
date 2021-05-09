using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform target;

    // Update is called once per frame
    void Update()
    {
        if(target != null)
            transform.forward = target.forward;
    }

    public void SetPlayerPos(Transform player)
    {
        target = player;
    }
}
