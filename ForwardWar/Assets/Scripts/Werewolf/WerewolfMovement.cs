using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Werewolf;

public class WerewolfMovement : EnemyBase
{
    void Start()
    {
        Initialize();
        //agent.updateRotation = false;
    }

    //�Ʒ� ������Ʈ �Լ��� ���ؼ� �ൿ ����
    protected override void EnemyUpdateNone()
    {
        //Debug.Log("FSM");
        Idle();
    }
    protected override void EnemyUpdateMove()
    {
        // ���� ���� ��ġ�� �ʱ� ��ġ���� �̵� ���� ������ �Ѿ�ٸ�...
        if (Vector3.Distance(transform.position, originPos) > moveDistance)
        {
            // ���� ���¸� Return ���·� ��ȯ�Ѵ�.
            m_State = EnemyState.Move;
            //print("���� ��ȯ: Move -> Return");
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
        // ����, �÷��̾���� �Ÿ��� �׼� ���� ���� �̳���� Move ���·� ��ȯ�Ѵ�.
        if (Vector3.Distance(transform.position, target.position) < findDistance)
        {
            m_State = EnemyState.Move;
            print("���� ��ȯ: Idle -> Move");

            anim.SetTrigger("IdleToMove");
        }
    }

    void Move()
    {
        // ����, �÷��̾���� �Ÿ��� ���� ���� ���̶�� �÷��̾ ���� �̵��Ѵ�.
        if (Vector3.Distance(transform.position, target.position) > attackDistance)
        {
            // �̵� ���� ����
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0;

            // ĳ���� ��Ʈ�ѷ��� �̿��Ͽ� �̵��ϱ�
            //cc.Move(dir * moveSpeed * Time.deltaTime);
            //�÷��̾ ���� ������ ��ȯ�Ѵ�.
            transform.forward = dir;

            agent.destination = target.transform.position;
        }
        // �׷��� �ʴٸ�, ���� ���¸� Attack ���·� ��ȯ�Ѵ�.
        else
        {
            m_State = EnemyState.Attack;
            print("���� ��ȯ: Move -> Attack");

            // ���� �ð��� ���� ������ �ð���ŭ �̸� ������ѳ��´�.
            currentTime = attackDelay;

            // ���� ��� �ִϸ��̼� �÷���
            anim.SetTrigger("MoveToAttackDelay");
        }
    }

    void Attack()
    {
        agent.destination = transform.position;
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;
        transform.forward = dir;
        // ����, �÷��̾ ���� ���� �̳��� �ִٸ� �÷��̾ �����Ѵ�.
        if (Vector3.Distance(transform.position, target.position) < attackDistance)
        {
            // ������ �ð����� �÷��̾ �����Ѵ�.
            currentTime += Time.deltaTime;
            if (currentTime > attackDelay)
            {
                PlayerMove pm = target.GetComponent<PlayerMove>();
                if (pm == null)
                {
                    VillagerComp vc = target.GetComponent<VillagerComp>();
                    vc.HitVillager(attackPower);
                }
                else
                {
                    pm.DamageAction(attackPower);
                }
                currentTime = 0;

                // ���� �ִϸ��̼� �÷���
                anim.SetTrigger("StartAttack");
            }
        }
        // �׷��� �ʴٸ�, ���� ���¸� Move ���·� ��ȯ�Ѵ�(�� �߰� �ǽ�).
        else
        {
            m_State = EnemyState.Move;
            print("���� ��ȯ: Attack -> Move");
            currentTime = 0;

            // �̵� �ִϸ��̼� �÷���
            anim.SetTrigger("AttackToMove");
        }
    }

    void Return()
    {
        // ����, �ʱ� ��ġ������ �Ÿ��� 0.1f �̻��̶�� �ʱ� ��ġ ������ �̵��Ѵ�.
        if (Vector3.Distance(transform.position, originPos) > 0.1f)
        {
            //Vector3 dir = (originPos - transform.position).normalized;
            //cc.Move(dir * moveSpeed * Time.deltaTime);

            //���� �ڸ��� ���� ������ ��ȯ�Ѵ�.
            //transform.forward = dir;

            agent.destination = originPos;
        }
        // �׷��� �ʴٸ�, �ڽ��� ��ġ�� �ʱ� ��ġ�� �����ϰ� ���� ���¸� ��� ���·� ��ȯ�Ѵ�.
        else
        {
            transform.position = originPos;
            transform.rotation = originRot;

            m_State = EnemyState.None;
            print("���� ��ȯ: Return -> Idle");

            anim.SetTrigger("MoveToIdle");
        }
    }

    //// ������ ���� �Լ�
    public override void HitEnemy(int hitPower)
    {
        Debug.Log("FSM");
        // ����, �̹� �ǰ� �����̰ų� ��� ���� �Ǵ� ���� ���¶�� �ƹ��� ó���� ���� �ʰ� �Լ��� �����Ѵ�.
        if (m_State == EnemyState.Dead)
        {
            //return;
        }

        // �÷��̾��� ���ݷ¸�ŭ ���ʹ��� ü���� ���ҽ�Ų��.
        base.HitEnemy(hitPower);

        // ���ʹ��� ü���� 0���� ũ�� �ǰ� ���·� ��ȯ�Ѵ�.
        if (HP > 0)
        {
            print("���� ��ȯ: Any state -> Damaged");

            Damaged();
        }
        // �׷��� �ʴٸ�, ���� ���·� ��ȯ�Ѵ�.
        else
        {
            m_State = EnemyState.Dead;
            print("���� ��ȯ: Any state -> Die");

            Die();
        }
    }

    void Damaged()
    {
        // �ǰ� ���¸� ó���ϱ� ���� �ڷ�ƾ�� �����Ѵ�.
        StartCoroutine(DamageProcess());
    }

    //// ������ ó���� �ڷ�ƾ �Լ�
    IEnumerator DamageProcess()
    {
        // �ǰ� ��� �ð���ŭ ��ٸ���.
        yield return new WaitForSeconds(0.5f);

        // ���� ���¸� �̵� ���·� ��ȯ�Ѵ�.
        m_State = EnemyState.Move;
        print("���� ��ȯ: Damaged -> Move");
        yield break;
    }

    // ���� ���� �Լ�
    void Die()
    {
        // �������� �ǰ� �ڷ�ƾ�� �����Ѵ�.
        StopAllCoroutines();

        // ���� ���¸� ó���ϱ� ���� �ڷ�ƾ�� �����Ѵ�.
        StartCoroutine(DieProcess());
    }

    IEnumerator DieProcess()
    {
        // ĳ���� ��Ʈ�ѷ� ������Ʈ�� ��Ȱ��ȭ�Ѵ�.
        //cc.enabled = false;

        // 2�� ���� ��ٸ� �ڿ� �ڱ� �ڽ��� �����Ѵ�.
        //yield return new WaitForSeconds(2f);
        //yield return null;
        if (DeadCheck)
            yield break;
        GameObject go = Instantiate(DropItem, transform.position, transform.rotation);
        DeadCheck = true;
        agent.destination = transform.position;

        go.GetComponent<Rigidbody>().AddForce(Vector3.up * 100);
        anim.Play("Dead");
        Destroy(gameObject,2);
        yield break;
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
