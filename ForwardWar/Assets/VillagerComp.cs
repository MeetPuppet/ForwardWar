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
    villagerManager manager;
    int state = 0;

    protected override void initialize()
    {
        ActivateButton = GameManager.Button;
        EscPosition = GameObject.Find("escape").transform;
        manager = GameObject.Find("VillagerManager").GetComponent<villagerManager>();
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
                agent.SetDestination(EscPosition.position);
                break;
        }
        if (Input.GetKeyDown("l"))
        {
            HP = 0;
            HitVillager(1);
        }
    }

    //��ȣ�ۿ� �۵� ��
    public override void ActivateObject(PlayerMove player)
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
            manager.VillagerRemove(this);
            Debug.Log("Dead");
        }
    }

    public bool score = false;
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
                manager.VillagerRemove(this);
                score = true;
            }
            anim.SetBool("Safe", true);
            Destroy(gameObject, 2);
        }
    }
}
