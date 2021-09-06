using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyFSM : EnemyBase
{
    void Start()
    {
        Initialize();
        //agent.updateRotation = false;
    }
    
    //아래 업데이트 함수를 통해서 행동 조정
    protected override void EnemyUpdateNone()
    {
        //Debug.Log("FSM");
        Idle();
    }
    protected override void EnemyUpdateMove()
    {
        // 만일 현재 위치가 초기 위치에서 이동 가능 범위를 넘어간다면...
        if (Vector3.Distance(transform.position, originPos) > moveDistance)
        {
            // 현재 상태를 Return 상태로 전환한다.
            m_State = EnemyState.Move;
            //print("상태 전환: Move -> Return");
            Return();
        }
        else
        {
            Move();
        }
    }
    protected override void EnemyUpdateAttack()
    {
        Attack();
    }
    protected override void EnemyUpdateDead()
    {
        Die();
    }

    void Idle()
    {
        // 만일, 플레이어와의 거리가 액션 시작 범위 이내라면 Move 상태로 전환한다.
        if (Vector3.Distance(transform.position, target.position) < findDistance)
        {
            m_State = EnemyState.Move;
            print("상태 전환: Idle -> Move");

            anim.SetTrigger("IdleToMove");
        }
    }

    void Move()
    {
        // 만일, 플레이어와의 거리가 공격 범위 밖이라면 플레이어를 향해 이동한다.
        if (Vector3.Distance(transform.position, target.position) > attackDistance)
        {
            // 이동 방향 설정
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0;

            // 캐릭터 콘트롤러를 이용하여 이동하기
            //cc.Move(dir * moveSpeed * Time.deltaTime);
            //플레이어를 향해 방향을 전환한다.
            transform.forward = dir;

            agent.SetDestination(target.transform.position);
        }
        // 그렇지 않다면, 현재 상태를 Attack 상태로 전환한다.
        else
        {
            m_State = EnemyState.Attack;
            print("상태 전환: Move -> Attack");

            // 누적 시간을 공격 딜레이 시간만큼 미리 진행시켜놓는다.
            currentTime = attackDelay;

            // 공격 대기 애니메이션 플레이
            anim.SetTrigger("MoveToAttackDelay");
        }
    }

    void Attack()
    {
        agent.SetDestination(transform.position);
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;
        transform.forward = dir;
        // 만일, 플레이어가 공격 범위 이내에 있다면 플레이어를 공격한다.
        if (Vector3.Distance(transform.position, target.position) < attackDistance)
        {
            // 일정한 시간마다 플레이어를 공격한다.
            currentTime += Time.deltaTime;
            if (currentTime > attackDelay)
            {
                PlayerMove pm = target.GetComponent<PlayerMove>();
                if(pm == null)
                {
                    VillagerComp vc = target.GetComponent<VillagerComp>();
                    vc.HitVillager(attackPower);
                }
                else
                {
                    pm.DamageAction(attackPower);
                }
                //print("공격");
                currentTime = 0;

                // 공격 애니메이션 플레이
                anim.SetTrigger("StartAttack");
            }
        }
        // 그렇지 않다면, 현재 상태를 Move 상태로 전환한다(재 추격 실시).
        else
        {
            m_State = EnemyState.Move;
            print("상태 전환: Attack -> Move");
            currentTime = 0;
            
            // 이동 애니메이션 플레이
            anim.SetTrigger("AttackToMove");
        }
    }

    void Return()
    {
        // 만일, 초기 위치에서의 거리가 0.1f 이상이라면 초기 위치 쪽으로 이동한다.
        if (Vector3.Distance(transform.position, originPos) > 0.1f)
        {
            //Vector3 dir = (originPos - transform.position).normalized;
            //cc.Move(dir * moveSpeed * Time.deltaTime);

            //원래 자리를 향해 방향을 전환한다.
            //transform.forward = dir;

            agent.SetDestination(originPos);
        }
        // 그렇지 않다면, 자신의 위치를 초기 위치로 조정하고 현재 상태를 대기 상태로 전환한다.
        else
        {
            transform.position = originPos;
            transform.rotation = originRot;

            m_State = EnemyState.None;
            print("상태 전환: Return -> Idle");

            anim.SetTrigger("MoveToIdle");
        }
    }

    //// 데미지 실행 함수
    public override void HitEnemy(int hitPower)
    {
        Debug.Log("FSM");
        // 만일, 이미 피격 상태이거나 사망 상태 또는 복귀 상태라면 아무런 처리도 하지 않고 함수를 종료한다.
        if (m_State == EnemyState.Dead)
        {
            //return;
        }

        // 플레이어의 공격력만큼 에너미의 체력을 감소시킨다.
        base.HitEnemy(hitPower);

        // 에너미의 체력이 0보다 크면 피격 상태로 전환한다.
        if (HP > 0)
        {
            print("상태 전환: Any state -> Damaged");

            Damaged();
        }
        // 그렇지 않다면, 죽음 상태로 전환한다.
        else
        {
            m_State = EnemyState.Dead;
            print("상태 전환: Any state -> Die");

            Die();
        }
    }

    void Damaged()
    {
        // 피격 상태를 처리하기 위한 코루틴을 실행한다.
        StartCoroutine(DamageProcess());
    }

    //// 데미지 처리용 코루틴 함수
    IEnumerator DamageProcess()
    {
        // 피격 모션 시간만큼 기다린다.
        yield return new WaitForSeconds(0.5f);

        // 현재 상태를 이동 상태로 전환한다.
        m_State = EnemyState.Move;
        print("상태 전환: Damaged -> Move");
        yield break;
    }

    // 죽음 상태 함수
    void Die()
    {
        // 진행중인 피격 코루틴을 중지한다.
        StopAllCoroutines();

        // 죽음 상태를 처리하기 위한 코루틴을 실행한다.
        StartCoroutine(DieProcess());
    }

    IEnumerator DieProcess()
    {
        // 캐릭터 콘트롤러 컴포넌트를 비활성화한다.
        //cc.enabled = false;

        // 2초 동안 기다린 뒤에 자기 자신을 제거한다.
        //yield return new WaitForSeconds(2f);
        //yield return null;
        if (DeadCheck)
            yield break;

        GameObject go = Instantiate(DropItem, transform.position, transform.rotation);
        DeadCheck = true;
        agent.SetDestination(transform.position);
        anim.Play("Dead");
        go.GetComponent<Rigidbody>().AddForce(Vector3.up * 100);
        Destroy(gameObject, 2);
        yield break;
    }

}
