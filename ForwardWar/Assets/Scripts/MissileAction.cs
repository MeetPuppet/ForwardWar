using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileAction : MonoBehaviour
{
    
    public GameObject Missile_prefab;
    public GameObject Missile_spawn;
    public GameObject target;
    float speed = 1f;


    // 폭발 이펙트 프리팹 변수
    public GameObject bombEffect;

    // 충돌했을 때의 처리
    private void OnCollisionEnter(Collision collision)
    {
        target.transform.position = transform.position;

        GameObject missile = Instantiate(Missile_prefab);

        missile.transform.position = Missile_spawn.transform.position;
        missile.transform.LookAt(target.transform.position);

        Destroy(gameObject);
        StartCoroutine(Launch(missile));
    }

    public IEnumerator Launch(GameObject missile)
    {
        while(Vector3.Distance(target.transform.position, missile.transform.position) > 0.3f)
        {
            missile.transform.position += (target.transform.position - missile.transform.position).normalized * speed * Time.deltaTime;
            yield return null;
        }
       
        // 이펙트 프리팹을 생성한다.
        GameObject eff = Instantiate(bombEffect);

        // 이펙트 프리팹의 위치는 수류탄 오브젝트 자신의 위치와 동일하다.
        eff.transform.position = target.transform.position;


        Destroy(missile);

    }
}