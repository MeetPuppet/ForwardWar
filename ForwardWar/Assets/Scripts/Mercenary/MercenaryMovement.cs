using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    private NavMeshAgent agent = null;
    public GameObject CoverList;

    public int guideLine
    {
        get
        {
            return (int)operationGuide;
        }
        set
        {
            operationGuide = (OperationGuide)value;
        }
    }
    OperationGuide operationGuide;
    MercenaryState mercenaryState = MercenaryState.Alert;

    public int MaxHP = 10;
    public int HP = 10;

    public float fightRange = 30;
    public float moveSpeed = 7f;
    Animator anim;

    GameObject[] insightEnemys = null;
    System.Random rnd = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        HP = MaxHP;
        EnemyParent = GameObject.Find("Enemys");
        anim = GetComponentInChildren<Animator>();
        BulletFirePS = BulletFire.GetComponent<ParticleSystem>();

        agent = GetComponent<NavMeshAgent>();
        if (agent)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;
        }

        StartCoroutine("AlertMovement");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActiveStateMovement(string orderStr)
    {
        switch (mercenaryState)
        {
            case MercenaryState.Alert:
            case MercenaryState.Move:
            case MercenaryState.Breakaway:
            case MercenaryState.Attack:
                StartCoroutine(orderStr);

                break;
                //StartCoroutine("FightMoveTranslate", hitInfo.point);
                break;
        }
    }
    public void ActiveStateMovement(string orderStr, Vector3 option)
    {
        switch (mercenaryState)
        {
            case MercenaryState.Alert:
            case MercenaryState.Move:
            case MercenaryState.Breakaway:
            case MercenaryState.Attack:
                StartCoroutine(orderStr, option);

                break;
                //StartCoroutine("FightMoveTranslate", hitInfo.point);
                break;
        }
    }

    public GameObject EnemyParent;
    public float radius = 10f;

    public GameObject[] GetSightEnemys()
    {
        List<GameObject> enemyList = new List<GameObject>();

        for (int i = 0; i < EnemyParent.transform.childCount; ++i)
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

    IEnumerator AlertMovement()
    {
        mercenaryState = MercenaryState.Alert;
        //Vector3 dir = default;
        int range = rnd.Next() / 180;
        Vector3 Rot = transform.rotation.eulerAngles;
        Rot.y += range;
        //
        //dir = transform.position;
        //dir.z = transform.position.z - range;
        //insightEnemys = GetSightEnemys();

        while (mercenaryState == MercenaryState.Alert)
        {
            Vector3.Lerp(transform.rotation.eulerAngles, Rot, Time.deltaTime * moveSpeed);

            insightEnemys = GetSightEnemys();
            if (insightEnemys != null)
            {
                StartCoroutine("FightEnemys", insightEnemys);
                yield break;
            }
            yield return null;
        }

        insightEnemys = GetSightEnemys();
    }

    IEnumerator MoveTranslate(Vector3 wayPoint)
    {
        mercenaryState = MercenaryState.Move;
        Vector3 dir = default;

        agent.destination = wayPoint;


        float dist = Vector3.Distance(transform.position, wayPoint);

        anim.SetFloat("MoveBlend", agent.destination.magnitude);

        while (dist >= (moveSpeed * moveSpeed * Time.deltaTime) * 2)
        {
            insightEnemys = GetSightEnemys();
            dist = Vector3.Distance(transform.position, wayPoint);
            yield return null;

            //적발견 예외처리
            if (insightEnemys != null && dist < fightRange &&
                operationGuide == OperationGuide.Combat)
            {
                anim.SetFloat("MoveBlend", 0f);
                StartCoroutine("FightEnemys", insightEnemys);
                yield break;
            }
        }
        mercenaryState = MercenaryState.Alert;
        StartCoroutine("AlertMovement");
        anim.SetFloat("MoveBlend", 0f);
    }

    IEnumerator FightMoveTranslate(Vector3 wayPoint)
    {
        insightEnemys = GetSightEnemys();
        if (insightEnemys == null)
        {
            StartCoroutine("MoveTranslate", wayPoint);
            yield break;
        }
        agent.destination = wayPoint;
        mercenaryState = MercenaryState.Attack;

        Vector3 dir = default;
        dir.x = wayPoint.x - transform.position.x;
        dir.z = wayPoint.z - transform.position.z;

        Vector3 enemyDir = default;
        int targetStep = 0;
        EnemyFSM enemyDatas = insightEnemys[targetStep].GetComponent<EnemyFSM>();
        enemyDir.x = insightEnemys[targetStep].transform.position.x - transform.position.x;
        enemyDir.z = insightEnemys[targetStep].transform.position.z - transform.position.z;


        float dist = Vector3.Distance(transform.position, wayPoint);

        anim.SetFloat("MoveBlend", dir.magnitude);

        while (dist >= moveSpeed * moveSpeed * Time.deltaTime)
        {
            Quaternion lookRotation = Quaternion.LookRotation(enemyDir);
            Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * moveSpeed).eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

            insightEnemys = GetSightEnemys();
            dist = Vector3.Distance(transform.position, wayPoint);

            if (enemyDatas != null)
            {
                StartCoroutine("MercenaryHitEffect");
                enemyDatas.HitEnemy(3);
                if (enemyDatas.hp <= 0)
                {
                    mercenaryState = MercenaryState.Move;
                    StartCoroutine("MoveTranslate", wayPoint);
                    anim.SetFloat("MoveBlend", 0f);
                    yield break;
                }
            }
            yield return null;

        }
        mercenaryState = MercenaryState.Alert;
        StartCoroutine("AlertMovement");
        anim.SetFloat("MoveBlend", 0f);
    }

    IEnumerator FightEnemys(GameObject[] insightEnemysArr)
    {
        mercenaryState = MercenaryState.Attack;
        int targetStep = 0;
        EnemyFSM enemyDatas = insightEnemysArr[targetStep].GetComponent<EnemyFSM>();

        //생존시간 처리용
        {
            int shortDistArr = -1;
            float dist1 = 0f;
            for (int i = 0; i < CoverList.transform.childCount; ++i)
            {
                if (CoverList.transform.GetChild(i).transform.GetComponent<CoverData>().useLock)
                    continue;

                dist1 = Vector3.Distance(transform.position, CoverList.transform.GetChild(i).transform.position);
                float dist2 = Vector3.Distance(transform.position,
                    CoverList.transform.GetChild(shortDistArr).transform.position);

                if (dist2 < 10 && dist1 < dist2)
                {
                    shortDistArr = i;
                }
            }

            if (shortDistArr != -1)
            {
                agent.destination = CoverList.transform.GetChild(shortDistArr).transform.position;
                float dist = Vector3.Distance(transform.position, agent.destination);
                anim.SetFloat("MoveBlend", agent.destination.magnitude);
                while (dist >= (moveSpeed * moveSpeed * Time.deltaTime) * 2f)
                {
                    dist = Vector3.Distance(transform.position, agent.destination);
                    yield return null;
                }
                anim.SetFloat("MoveBlend", 0f);
            }
        }

        Vector3 dir = default;
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo = new RaycastHit();
        dir.x = insightEnemysArr[targetStep].transform.position.x - transform.position.x;
        dir.z = insightEnemysArr[targetStep].transform.position.z - transform.position.z;

        while (targetStep < insightEnemysArr.Length)
        {
            if (insightEnemysArr[targetStep] == null)
            {
                mercenaryState = MercenaryState.Alert;
                StartCoroutine("AlertMovement");
                anim.SetFloat("MoveBlend", 0f);
                yield break;
            }
            dir.x = insightEnemysArr[targetStep].transform.position.x - transform.position.x;
            dir.z = insightEnemysArr[targetStep].transform.position.z - transform.position.z;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * moveSpeed * 2).eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

            if (!BulletFirePS.isPlaying)
            {
                ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, out hitInfo))
                {
                    BulletFire.transform.position = hitInfo.point;
                    BulletFire.transform.forward = hitInfo.normal;

                    StartCoroutine("MercenaryHitEffect");
                    if (enemyDatas != null)
                    {
                        enemyDatas.HitEnemy(3);
                        if (enemyDatas.hp <= 0)
                        {
                            mercenaryState = MercenaryState.Alert;
                            StartCoroutine("AlertMovement");
                            anim.SetFloat("MoveBlend", 0f);
                            yield break;
                        }
                    }
                }
            }

            if (operationGuide == OperationGuide.Snick)
            {
                yield break;
            }

            yield return null;
        }

    }

    public void GetDamage(int strengh)
    {
        HP -= strengh;
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
