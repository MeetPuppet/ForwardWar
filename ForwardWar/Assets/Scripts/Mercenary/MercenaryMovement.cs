using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryMovement : MonoBehaviour
{
    enum MercenaryState // 경계, 이동, 공격, 이탈
    {
        Alert = 0,
        Move,
        Attack,
        Breakaway
    }

    // 작전지침
    enum OperationGuide
    {
        Combat = 0,
        Snick
    }

    public GameObject Commander;

    public int guideLine
    {
        get
        {
            return (int)guideLine;
        }
        set
        {
            operationGuide = (OperationGuide)value;
        }
    }
    OperationGuide operationGuide;
    MercenaryState mercenaryState;

    public float fightRange = 30;
    public float moveSpeed = 7f;
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        EnemyParent = GameObject.Find("Enemys");
        anim = GetComponentInChildren<Animator>();
        BulletFirePS = BulletFire.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hitInfo = new RaycastHit();

            if (Physics.Raycast(ray, out hitInfo))
            {
                StopAllCoroutines();
                StartCoroutine("MoveTranslate", hitInfo.point);
            }
        }
    }

    public GameObject EnemyParent;
    public float radius = 10f;

    public GameObject[] GetSightEnemys()
    {
        List<GameObject> enemyList = new List<GameObject>();
        
        for(int i=0;i< EnemyParent.transform.childCount; ++i)
        {
            float dist = Vector3.Distance(transform.position, EnemyParent.transform.GetChild(i).position);
            if (dist < radius)
            {
                enemyList.Add(EnemyParent.transform.GetChild(i).gameObject);
            }
        }
        if (enemyList.Count <= 0)
            return null;

        return enemyList.ToArray();
    }

    IEnumerator MoveTranslate(Vector3 wayPoint)
    {
        Vector3 dir = default;
        GameObject[] insightEnemys = GetSightEnemys();

        dir.x = wayPoint.x - transform.position.x;
        dir.z = wayPoint.z - transform.position.z;

        float dist = Vector3.Distance(transform.position, wayPoint);

        anim.SetFloat("MoveBlend", dir.magnitude);

        while (dist >= moveSpeed * moveSpeed * Time.deltaTime)
        {
            transform.Translate(dir.normalized * moveSpeed * Time.deltaTime, Space.World);

            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * moveSpeed).eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

            insightEnemys = GetSightEnemys();
            dist = Vector3.Distance(transform.position, wayPoint);
            yield return null;

            //적발견 예외처리
            if(insightEnemys != null && dist < fightRange)
            {
                anim.SetFloat("MoveBlend", 0f);
                StartCoroutine("FightEnemys", insightEnemys);
                yield break;
            }
        }
        anim.SetFloat("MoveBlend", 0f);
    }

    IEnumerator FightEnemys(GameObject[] insightEnemysArr)
    {
        int targetStep = 0;
        EnemyFSM enemyDatas = insightEnemysArr[targetStep].GetComponent<EnemyFSM>();

        Vector3 dir = default;
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo = new RaycastHit();

        dir.x = insightEnemysArr[targetStep].transform.position.x - transform.position.x;
        dir.z = insightEnemysArr[targetStep].transform.position.z - transform.position.z;

        while(targetStep < insightEnemysArr.Length)
        {
            dir.x = insightEnemysArr[targetStep].transform.position.x - transform.position.x;
            dir.z = insightEnemysArr[targetStep].transform.position.z - transform.position.z;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * moveSpeed * 2).eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

            if(!BulletFirePS.isPlaying)
            {
                ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, out hitInfo))
                {
                    BulletFire.transform.position = hitInfo.point;
                    BulletFire.transform.forward = hitInfo.normal;

                    StartCoroutine("MercenaryHitEffect");
                }
            }

            yield return null;
        }

    }

    public GameObject BulletFire;
    ParticleSystem BulletFirePS;

    IEnumerator MercenaryHitEffect()
    {
        BulletFire.SetActive(true);
        BulletFirePS.Play();

        yield return new WaitForSeconds(0.3f);

        BulletFirePS.Stop();
        BulletFire.SetActive(false);
    }
}
