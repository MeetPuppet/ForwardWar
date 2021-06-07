using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemComp : MonoBehaviour
{
    float StartPointY;
    bool check = true;
    public float MoveRange;
    float MoveSpeed;

    public float RotSpeed = 4;
    private Vector3 rot;


    void Start()
    {
        rot = new Vector3(0, RotSpeed, 0);
        StartPointY = transform.localPosition.y;
        MoveSpeed = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rot);

        if (transform.localPosition.y > StartPointY + MoveRange)
        {
            check = true;
        }
        else if (transform.localPosition.y < StartPointY - MoveRange)
        {
            check = false;
        }

        if (check)
        {
            MoveSpeed += Time.deltaTime;
        }
        else
        {
            MoveSpeed -= Time.deltaTime;
        }


        transform.localPosition = transform.localPosition - (Vector3.up * Time.deltaTime * MoveSpeed);
    }
}
