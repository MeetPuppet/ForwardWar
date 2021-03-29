using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryControl : MonoBehaviour
{
    public GameObject unit;
    MercenaryMovement unitMovement;

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
        unitMovement = unit.GetComponent<MercenaryMovement>();
        unitMovement.guideLine = operationGuide;
    }

    // Update is called once per frame
    void Update()
    {
        unitMovement.guideLine = operationGuide;
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
    }

}
