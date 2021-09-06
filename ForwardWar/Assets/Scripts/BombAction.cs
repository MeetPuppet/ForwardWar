using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombAction : MonoBehaviour
{
    // ���� ����Ʈ ������ ����
    public GameObject bombEffect;


    private void Start()
    {
    }
    // �浹���� ���� ó��
    private void OnCollisionEnter(Collision collision)
    {
        // ����Ʈ �������� �����Ѵ�.
        GameObject eff = Instantiate(bombEffect);
        // ����Ʈ �������� ��ġ�� ����ź ������Ʈ �ڽ��� ��ġ�� �����ϴ�.
        eff.transform.position = transform.position;


        //�Ÿ� 10���� ��� �ݶ��̴� Ȯ��
        Collider[] collider = Physics.OverlapSphere(transform.position, 10);
        for (int i = 0; i < collider.Length; ++i)
        {
            EnemyBase enemy = collider[i].GetComponent<EnemyBase>();
            if (enemy)
            {
                Vector3 vec3 = collider[i].transform.position - transform.position;
                float dist = 10 - Vector3.Distance(collider[i].transform.position, transform.position);
                enemy.ExploreDamage(vec3 * dist);
            }
        }

        // �ڱ� �ڽ��� �����Ѵ�.
        Destroy(gameObject);
    }
    void CutPartSet()
    {
        //�Ÿ� 10���� ��� �ݶ��̴� Ȯ��
        Collider[] collider = Physics.OverlapSphere(transform.position, 10);
        for (int i = 0; i < collider.Length; ++i)
        {
            Transform parent = collider[i].transform.parent;
            //�θ� �ִ� ���
            if (parent)
            {
                //CutPart�� �ҷ��� �Լ� �۵�
                CutPart cp = parent.GetComponent<CutPart>();
                if (cp)
                    cp.Activate();
                else
                {
                    CutPart ccp = collider[i].GetComponent<CutPart>();
                    ccp?.Activate();
                }
            }

            //rigidBody�� �ִ� ���
            Rigidbody rd = collider[i].GetComponent<Rigidbody>();
            if (rd)
            {
                //��� ���� Ȯ��
                Vector3 dir = default;
                dir.x = rd.transform.position.x - transform.position.x;
                dir.z = rd.transform.position.z - transform.position.z;
                dir = dir.normalized;

                rd.AddForce(dir * 1000);
            }
        }

    }
}

