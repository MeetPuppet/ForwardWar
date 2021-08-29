using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSwitchComp : ActSwitchObject
{
    bool isOpen = false;
    public Vector3 RotValue;
    Animator animator;
    Coroutine coroutine;
    WaitForSeconds WFsecond = new WaitForSeconds(1f);

    protected override void initialize()
    {
        base.initialize();
        RotValue = transform.localEulerAngles;
        animator = GetComponent<Animator>();
    }

    public override void ActivateObject(PlayerMove player)
    {
        if (coroutine == null)
           coroutine = StartCoroutine("DoorControl");
    }

    IEnumerator DoorControl()
    {
        if (isOpen)
        {
            animator.Play("Close");
        }
        else
        {
            animator.Play("Open");
        }

        isOpen = !isOpen;
        yield return WFsecond;
        coroutine = null;
        yield break;
    }
}
