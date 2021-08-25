using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyBase : MonoBehaviour
{

    #region public
    public Animator anim;
    //��ã��
    public NavMeshAgent agent;

    // �÷��̾� �߰� ����
    public float findDistance = 8f;

    // ���� ���� ����
    public float attackDistance = 2f;

    // �̵� �ӵ�
    public float moveSpeed = 5f;

    // ���ʹ� ���ݷ�
    public int attackPower = 3;

    // �̵� ���� ����
    public float moveDistance = 20f;

    // ���ʹ��� ü��
    public int MaxHP = 10;
    public int HP = 10;
    public float Speed = 10f;
    public Slider hpSlider;

    //����ǥ�� FleX ������Ʈ
    public GameObject Blood;
    //��� ������
    public GameObject DropItem;


    #endregion
    #region protected
    // ���ʹ� ���� ���
    protected bool DeadCheck = false;
    protected enum EnemyState
    {
        None, // ���
        Move, // �̵�
        Attack, // ����
        Dead // ���
    }
    // ���ʹ� ���� ����
    protected EnemyState m_State;
    // �÷��̾� Ʈ������
    protected Transform target;

    // ĳ���� ��Ʈ�ѷ� ������Ʈ
    //protected CharacterController cc;

    // ���� �ð�
    protected float currentTime = 0;

    // ���� ������ �ð�
    protected float attackDelay = 2f;
    // �ʱ� ��ġ ����� ����
    protected Vector3 originPos;
    protected Quaternion originRot;
    #endregion

    protected void Initialize()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        //agent.path

        // ������ ���ʹ� ���´� ��� ����(Idle)�� �Ѵ�.
        m_State = EnemyState.None;

        // �÷��̾��� Ʈ������ ������Ʈ �޾ƿ���
        target = GameObject.Find("Player").transform;

        // ĳ���� ��Ʈ�ѷ� ������Ʈ �޾ƿ���
        //cc = GetComponent<CharacterController>();

        // �ڽ��� �ʱ� ��ġ �����ϱ�
        originPos = transform.position;
        originRot = transform.rotation;
        anim = transform.GetComponentInChildren<Animator>();
        agent.speed = Speed;
        agent.updateRotation = false;

        isBlood = false;
        bloodTime = 0f;
    }

    void Update()
    {
        EnemyUpdate();
        hpSlider.value = (float)HP / (float)MaxHP;
    }
    void bloodoff()
    {
        if (!isBlood || bloodTime > 0f)
            return;

        FlexSourceActor act = Blood.GetComponent<FlexSourceActor>();
        act.isActive = false;
        isBlood = false;
    }
    void EnemyUpdate()
    {
        if (bloodTime > 0f)
        {
            bloodTime -= Time.deltaTime;
        }
        else
        {
            bloodoff();
        }


        if (target == null)
        {
            target = GameObject.Find("Player").transform;
        }
        else
        {
            Collider[] collider = Physics.OverlapSphere(transform.position, 10);
            for (int i = 0; i < collider.Length; ++i)
            {
                VillagerComp villager = collider[i].GetComponent<VillagerComp>();
                if(villager)
                    target = villager.transform;
            }
        }
            switch (m_State)
        {
            case EnemyState.None:
                EnemyUpdateNone();
                break;
            case EnemyState.Move:
                EnemyUpdateMove();
                break;
            case EnemyState.Attack:
                EnemyUpdateAttack();
                break;
            case EnemyState.Dead:
                EnemyUpdateDead();
                break;
        }
    }

    //�Ʒ� ������Ʈ �Լ��� ���ؼ� �ൿ ����
    protected virtual void EnemyUpdateNone()
    {
        //Debug.Log("Base");
    }
    protected virtual void EnemyUpdateMove()
    {
    }
    protected virtual void EnemyUpdateAttack()
    {
    }
    protected virtual void EnemyUpdateDead()
    {
    }

    // ������ ���� �Լ�
    public virtual void HitEnemy(int hitPower)
    {
        Debug.Log("Base");
        // �÷��̾��� ���ݷ¸�ŭ ���ʹ��� ü���� ���ҽ�Ų��.
        HP -= hitPower;
        if(HP <= 0)
        {
            m_State = EnemyState.Dead;
            GameManager.Score.editScore(100);
            Destroy(gameObject, 3);
            bloodTime = 0;
            Destroy(Blood, 3);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (HP <= 0)
        {
            //collision.gameObject.GetComponent
        }
    }

    public void BloodActive(RaycastHit ray)
    {
        StartCoroutine("FlowBlood", ray);
    }

    bool isBlood;
    float bloodTime;
    void FlowBlood(RaycastHit ray)
    {
        Blood.transform.position = ray.point;
        Blood.transform.eulerAngles = -ray.normal;
        FlexSourceActor act = Blood.GetComponent<FlexSourceActor>();
        act.isActive = true;
        isBlood = true;
        bloodTime = Time.deltaTime;
    }
}
