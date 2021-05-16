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


        if (Input.GetKeyDown(KeyCode.F))
        {
            StopAllCoroutines();
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hitInfo = new RaycastHit();

            if (Physics.Raycast(ray, out hitInfo))
            {
                if (Moves.Length == 1)
                {
                    Moves[0].ActiveStateMovement(GameData.MercFunc.MercMoveOrder, hitInfo.point);
                }
                else
                {
                    Vector3[] vecs = new Vector3[Moves.Length];

                    Vector3 dir = transform.position - hitInfo.point;
                    dir.y = 0;
                    dir = Vector3.Normalize(dir);
                    Quaternion quat = Quaternion.Euler(dir);

                    float xSin = default;
                    float xCos = default;
                    float zSin = default;
                    float zCos = default;

                    for (int i = 0; i < Moves.Length; ++i,
                        dir.x = xCos - zSin, dir.z = xSin + zCos,
                    dir = Vector3.Normalize(dir))
                    {
                        xSin = Mathf.Sin(360 / (i + 1));
                        xCos = Mathf.Cos(360 / (i+1));
                        zSin = Mathf.Sin(360 / (i+1));
                        zCos = Mathf.Cos(360 / (i+1));

                        vecs[i] = hitInfo.point + (dir * 5);
                        Moves[i].ActiveStateMovement(GameData.MercFunc.MercMoveOrder, vecs[i]);

                    }
                }
            }
        }
    }

}
