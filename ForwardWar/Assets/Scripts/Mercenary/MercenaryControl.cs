using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryControl : MonoBehaviour
{
    public GameObject Units;
    MercenaryMovement[] Moves;

    public int guideLine
    {
        get
        {
            return operationGuide;
        }
        set
        {
            operationGuide = value;
        }
    }
    int operationGuide = 0;

    // Start is called before the first frame update
    void Start()
    {
        operationGuide = 0;
        Moves = Units.transform.GetComponentsInChildren<MercenaryMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(operationGuide == 0)
            {
                operationGuide = 1;
            }
            else
            {
                operationGuide = 0;
            }
        }


        for (int i =0; i < Moves.Length; ++i)
        {
            Moves[i].guideLine = operationGuide;
        }
    }

}
