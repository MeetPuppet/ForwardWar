using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienMovement : EnemyBase
{
    float chargeTime = 0;

    void Start()
    {
        Initialize();
    }

    protected override void EnemyUpdateNone()
    {
        DeadCheck();
        transform.Rotate(Vector3.up, 1f);


        if(Vector3.Distance(target.transform.position, transform.position) < findDistance)
        {
            m_State = EnemyState.Move;
            anim.SetBool("isMove", true);
        }
    }
    protected override void EnemyUpdateMove()
    {
        DeadCheck();
        agent.destination = target.transform.position;
        if (Vector3.Distance(agent.destination, transform.position) < findDistance)
        {
            m_State = EnemyState.Attack;
            agent.destination = transform.position;
            anim.SetBool("isFight", true);
        }
    }
    protected override void EnemyUpdateAttack()
    {
        DeadCheck();
        Debug.Log(Vector3.Distance(target.transform.position, transform.position));
        if (Vector3.Distance(target.transform.position, transform.position) > attackDistance)
        {
            m_State = EnemyState.Move;
            anim.SetBool("isMove", true);
            anim.SetBool("isFight", false);
        }
        else
        {
            chargeTime += Time.deltaTime;
            if(chargeTime > 2f)//AttackSpeed
            {
                chargeTime -= 2f;
                anim.SetTrigger("isCharge");
                MakeEnergyBall();
            }
        }
    }
    protected override void EnemyUpdateDead()
    {
        m_State = EnemyState.Dead;
    }
    void DeadCheck()
    {
        if (HP <= 0)
        {
            m_State = EnemyState.Dead;
            anim.SetBool("isDead", true);
        }
        Vector3 dir = (target.position - transform.position).normalized;
        transform.forward = dir;
    }

    public GameObject EnergyBall;
    void MakeEnergyBall()
    {
        if (m_State == EnemyState.Dead)
            return;


        Instantiate(EnergyBall,transform.position, transform.rotation).GetComponent<EnergyBall>().InfoSet(2, target.gameObject);
    }
}
