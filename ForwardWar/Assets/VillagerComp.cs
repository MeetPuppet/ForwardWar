using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class VillagerComp : ActSwitchObject
{
    float MaxHP = 10;
    public int HP = 10;
    public Slider hpSlider;

    public Animator anim;

    public Transform EscPosition;
    public float Speed = 10f;
    public NavMeshAgent agent;

    protected override void initialize()
    {
        ActivateButton = GameObject.Find("InteractiveButton");
        OffActivateButton();

        MaxHP = HP;

        if (hpSlider == null)
            hpSlider = GetComponentInChildren<Slider>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        if (EscPosition == null)
            EscPosition = transform;
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        anim.SetBool("Run", false);
        anim.SetBool("UnderAttack", false);
        anim.SetBool("Safe", false);
    }

    private void Update()
    {
        hpSlider.value = HP / MaxHP;
        SafeCheck();
    }

    //상호작용 작동 시
    public override void ActivateObject()
    {
        Debug.Log("Activate Villager");
        OffActivateButton();
        anim.SetBool("Run", true);
        anim.SetBool("UnderAttack", false);
        anim.SetBool("Safe", false);
        agent.destination = EscPosition.position;
    }


    public void HitVillager(int power)
    {
        agent.destination = transform.position;
        anim.SetBool("UnderAttack", true);

        HP -= power;
        if (HP <= 0)
        {
            //플렉스 추가
            Destroy(gameObject);
            Debug.Log("Dead");
        }
    }

    private void SafeCheck()
    {
        //Debug.Log(Vector3.Distance(transform.position, agent.destination));

        Vector2 villPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 EscPos = new Vector2(EscPosition.position.x, EscPosition.position.z);

        if (Vector2.Distance(villPos, EscPos) <= 0.1f)
        {
            anim.SetBool("Safe", true);
            Destroy(gameObject, 2);
        }
    }
}
