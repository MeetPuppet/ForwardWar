using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileAction : MonoBehaviour
{
    
    public GameObject Missile_prefab;
    public GameObject Missile_spawn;
    public GameObject target;
    float speed = 1f;


    // ���� ����Ʈ ������ ����
    public GameObject bombEffect;

    // �浹���� ���� ó��
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
       
        // ����Ʈ �������� �����Ѵ�.
        GameObject eff = Instantiate(bombEffect);

        // ����Ʈ �������� ��ġ�� ����ź ������Ʈ �ڽ��� ��ġ�� �����ϴ�.
        eff.transform.position = target.transform.position;


        Destroy(missile);

    }
}