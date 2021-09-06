using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombAction : MonoBehaviour
{
    // 폭발 이펙트 프리팹 변수
    public GameObject bombEffect;


    private void Start()
    {
    }
    // 충돌했을 때의 처리
    private void OnCollisionEnter(Collision collision)
    {
        // 이펙트 프리팹을 생성한다.
        GameObject eff = Instantiate(bombEffect);
        // 이펙트 프리팹의 위치는 수류탄 오브젝트 자신의 위치와 동일하다.
        eff.transform.position = transform.position;


        //거리 10내의 모든 콜라이더 확인
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

        // 자기 자신을 제거한다.
        Destroy(gameObject);
    }
    void CutPartSet()
    {
        //거리 10내의 모든 콜라이더 확인
        Collider[] collider = Physics.OverlapSphere(transform.position, 10);
        for (int i = 0; i < collider.Length; ++i)
        {
            Transform parent = collider[i].transform.parent;
            //부모가 있는 경우
            if (parent)
            {
                //CutPart를 불러서 함수 작동
                CutPart cp = parent.GetComponent<CutPart>();
                if (cp)
                    cp.Activate();
                else
                {
                    CutPart ccp = collider[i].GetComponent<CutPart>();
                    ccp?.Activate();
                }
            }

            //rigidBody가 있는 경우
            Rigidbody rd = collider[i].GetComponent<Rigidbody>();
            if (rd)
            {
                //대상 방향 확인
                Vector3 dir = default;
                dir.x = rd.transform.position.x - transform.position.x;
                dir.z = rd.transform.position.z - transform.position.z;
                dir = dir.normalized;

                rd.AddForce(dir * 1000);
            }
        }

    }
}

