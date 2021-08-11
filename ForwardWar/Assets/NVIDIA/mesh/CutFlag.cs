using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutFlag : MonoBehaviour
{
    //public EnemyBase command;
    public GameObject colliders;
    public bool isWorked;

    private void Start()
    {
        isWorked = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (colliders == null)
            return;
        colliders.transform.position = transform.position;

        //if (Input.GetMouseButtonDown(0))
        //{
        //    HitEnemy(10, colliders.transform.position);
        //}
    }

    public void HitEnemy(int damage, Vector3 hitpoint)
    {
        if(isWorked)
            return;
        OnFlag();
        //command.HitEnemy(damage);
        //if(command.HP <= 0)
        //{
        //    OnFlag(hitpoint);
        //}
    }

    public void OnFlag()
    {
        if (isWorked)
            return;

        colliders.transform.position = transform.position;
        PartCutter cutter = colliders.GetComponent<PartCutter>();

        cutter.ActivateBlades();

        //command.FlagOff();
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (isWorked == true)
    //        return;
    //    OnFlag();
    //}
    //
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (isWorked == true)
    //        return;
    //    OnFlag();
    //}

    public void SetCommand(EnemyBase eneymBase)
    {
        //command = eneymBase;
    }
}
