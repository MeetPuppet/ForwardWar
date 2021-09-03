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
    //길찾기
    public NavMeshAgent agent;

    // 플레이어 발견 범위
    public float findDistance = 8f;

    // 공격 가능 범위
    public float attackDistance = 2f;

    // 이동 속도
    public float moveSpeed = 5f;

    // 에너미 공격력
    public int attackPower = 3;

    // 이동 가능 범위
    public float moveDistance = 20f;

    // 에너미의 체력
    public int MaxHP = 10;
    public int HP = 10;
    public float Speed = 10f;
    public Slider hpSlider;

    //유혈표현 FleX 오브젝트
    public GameObject Blood;
    //드랍 아이템
    public GameObject DropItem;


    #endregion
    #region protected
    // 에너미 상태 상수
    protected bool DeadCheck = false;
    protected enum EnemyState
    {
        None, // 대기
        Move, // 이동
        Attack, // 전투
        Dead // 사망
    }
    // 에너미 상태 변수
    protected EnemyState m_State;
    // 플레이어 트랜스폼
    protected Transform target;

    // 캐릭터 콘트롤러 컴포넌트
    //protected CharacterController cc;

    // 누적 시간
    protected float currentTime = 0;

    // 공격 딜레이 시간
    protected float attackDelay = 2f;
    // 초기 위치 저장용 변수
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

        // 최초의 에너미 상태는 대기 상태(Idle)로 한다.
        m_State = EnemyState.None;

        // 플레이어의 트랜스폼 컴포넌트 받아오기
        target = GameObject.Find("Player").transform;

        // 캐릭터 콘트롤러 컴포넌트 받아오기
        //cc = GetComponent<CharacterController>();

        // 자신의 초기 위치 저장하기
        originPos = transform.position;
        originRot = transform.rotation;
        anim = transform.GetComponentInChildren<Animator>();
        agent.speed = Speed;
        agent.updateRotation = false;
    }

    void Update()
    {
        EnemyUpdate();
        hpSlider.value = (float)HP / (float)MaxHP;
    }
    void EnemyUpdate()
    {
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

    //아래 업데이트 함수를 통해서 행동 조정
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

    // 데미지 실행 함수
    public virtual void HitEnemy(int hitPower)
    {
        Debug.Log("Base");
        // 플레이어의 공격력만큼 에너미의 체력을 감소시킨다.
        HP -= hitPower;
        if(HP <= 0)
        {
            m_State = EnemyState.Dead;
            GameManager.Score.editScore(100);
            Destroy(gameObject, 3);
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
        GameManager.Updater.Add(FlowBlood(ray));
    }

    IEnumerator FlowBlood(RaycastHit ray)
    {
        Blood.transform.position = ray.point;
        Blood.transform.eulerAngles = ray.normal;

        yield return new WaitForSeconds(0.1f);

        yield break;
    }

}
