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
    int state = 0;

    protected override void initialize()
    {
        ActivateButton = GameObject.Find("InteractiveButton");
        //OffActivateButton();

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
        switch (state)
        {
            case 1:
                agent.destination = EscPosition.position;
                break;
        }
    }

    //��ȣ�ۿ� �۵� ��
    public override void ActivateObject()
    {
        Debug.Log("Activate Villager");
        //OffActivateButton();
        anim.SetBool("Run", true);
        anim.SetBool("UnderAttack", false);
        anim.SetBool("Safe", false);
        state = 1;
    }


    public void HitVillager(int power)
    {
        HP -= power;
        if (HP <= 0)
        {
            //�÷��� �߰�
            Destroy(gameObject);
            Debug.Log("Dead");
        }
    }

    bool score = false;
    private void SafeCheck()
    {
        //Debug.Log(Vector3.Distance(transform.position, agent.destination));

        Vector2 villPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 EscPos = new Vector2(EscPosition.position.x, EscPosition.position.z);

        if (Vector2.Distance(villPos, EscPos) <= 0.1f)
        {
            if (!score)
            {
                GameManager.Score.editScore(100 * HP);
                score = true;
            }
            anim.SetBool("Safe", true);
            Destroy(gameObject, 2);
        }
    }
}
