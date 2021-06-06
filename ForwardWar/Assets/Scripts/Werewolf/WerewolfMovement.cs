using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Werewolf;

public class WerewolfMovement : MonoBehaviour
{
    enum State
    {
        //SearchLine
        Idle = 0,
        Wander,
        Search,

        //BattleLine
        Run,
        Attack,
        Rush,
        Dodge,
        Dead
    }
    //public GameObject Chaser;

    public Animator anim;
    public Rigidbody rigid;
    public NavMeshAgent agent;
    public BlurSetter Blurs;

    public int MaxHP = 10;
    public int HP = 10;
    public float Speed = 10f;

    private GameObject target;
    public int WanderDistance = 5;
    public float ThinkTime = 0.5f;
    private WaitForSeconds waitTime;

    public float RushDistance = 10f;
    public float SearchRange = 30f;

    State state = State.Idle;

    void Start()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
        if (rigid == null)
            rigid = GetComponent<Rigidbody>();
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (Blurs == null)
            Blurs = GetComponent<BlurSetter>();

        agent.speed = Speed;
        state = State.Idle;
        waitTime = new WaitForSeconds(ThinkTime);
        MachineStateAcitvate();
    }

    void Update()
    {
        if(state == State.Dead)
        {
            StopAllCoroutines();
        }

        switch (state)
        {
            case State.Idle:
                break;
            case State.Wander:
                break;
            case State.Search:
                break;
            case State.Run:
                break;
            case State.Attack:
                break;
            case State.Rush:
                break;
            case State.Dodge:
                break;
            case State.Dead:
                break;
        }
    }

    void MachineStateAcitvate()
    {
        StartCoroutine("Wander");
    }

    IEnumerator Wander()
    {
        agent.speed = Speed;
        Debug.Log("Wander");
        anim.SetFloat("Blend", 1f);
        agent.destination = transform.position + (transform.forward * WanderDistance);

        while (transform.position != agent.destination)
        {
            //Chaser.transform.position = agent.destination;
            yield return null;
        }

        yield return waitTime;
        yield return StartCoroutine("SearchTarget");
        yield break;
    }

    IEnumerator SearchTarget()
    {
        agent.speed = Speed;
        state = State.Search;
        Debug.Log(state);
        Debug.Log("SearchTarget");
        anim.SetFloat("Blend", 0f);

        Vector3 SearchDir = transform.forward * -1;

        float time = 0f;
        while (SearchDir != transform.forward)
        {
            Quaternion lookRotation = Quaternion.LookRotation(SearchDir);

            time += Time.deltaTime * 0.5f;
            if (time > 1f)
                time = 1f;

            Vector3 rotation = Quaternion.LerpUnclamped(transform.rotation, lookRotation, time).eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.transform.gameObject.layer == 7
                    && Vector3.Distance(hitInfo.transform.position, transform.position) < SearchRange)
                {
                    Debug.Log(Vector3.Distance(hitInfo.transform.position + Vector3.up, transform.position));
                    //Debug.Log($"Found Target: {hitInfo.transform.gameObject.name}");
                    target = hitInfo.transform.gameObject;
                    anim.SetBool("isFight", true);
                    anim.SetFloat("TargetDistance", Vector3.Distance(hitInfo.transform.position, transform.position));
                    yield return waitTime;
                    yield return StartCoroutine("CombatReady");
                    yield break;
                }
            }

            yield return null;
        }

        yield return waitTime;
        yield return StartCoroutine("Wander");
        yield break;
    }

    IEnumerator CombatReady()
    {
        if(target == null)
        {
            yield return waitTime;
            yield return StartCoroutine("SearchTarget");
            yield break;
        }

        Debug.Log("CombatReady");
        agent.speed = Speed * 4;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        anim.SetFloat("TargetDistance", distance);
        Debug.Log(distance);
        if (distance < RushDistance)
        {
            yield return waitTime;
            yield return StartCoroutine("CombatStart");
            yield break;
        }

        Vector3 dir = default;
        dir.x = target.transform.position.x - transform.position.x;
        dir.z = target.transform.position.z - transform.position.z;
        dir = dir.normalized;

        agent.destination = target.transform.position - (dir * RushDistance);
        //Chaser.transform.position = agent.destination;
        anim.SetFloat("TargetDistance", distance);

        distance = Vector3.Distance(transform.position, agent.destination);
        while (distance > 0.1f)
        {
            dir.x = target.transform.position.x - transform.position.x;
            dir.z = target.transform.position.z - transform.position.z;
            dir = dir.normalized;

            agent.destination = target.transform.position - (dir * RushDistance);
            //Chaser.transform.position = agent.destination;
            distance = Vector3.Distance(transform.position, agent.destination);

            yield return null;
        }

        distance = Vector3.Distance(transform.position, target.transform.position);
        anim.SetFloat("TargetDistance", distance);
        yield return waitTime;
        Blurs.SwitchBlur(true);
        yield return StartCoroutine("CombatStart");
        yield break;
    }

    IEnumerator CombatStart()
    {
        if (target == null)
        {
            yield return waitTime;
            yield return StartCoroutine("SearchTarget");
            yield break;
        }
        Debug.Log("CombatStart");

        Vector3 dir = default;
        dir.x = target.transform.position.x - transform.position.x;
        dir.z = target.transform.position.z - transform.position.z;
        dir = dir.normalized;

        float time = 0f;
        while (dir != transform.forward)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir);

            time += Time.deltaTime * agent.speed;
            if (time > 1f)
                time = 1f;

            Vector3 rotation = Quaternion.LerpUnclamped(transform.rotation, lookRotation, time).eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
            yield return null;
        }
        agent.updateRotation = true;

        float distance = Vector3.Distance(transform.position,
            target.transform.position);
        anim.SetFloat("TargetDistance", distance);

        Debug.Log(distance);
        if (distance < 2)
        {
            yield return waitTime;
            yield return StartCoroutine("AttackTarget");
            yield break;
        }
        else if(distance < RushDistance + 1)
        {
            agent.speed = Speed * 10;
            distance = Vector3.Distance(transform.position, target.transform.position);
            anim.SetFloat("TargetDistance", distance);
            yield return waitTime;
            yield return StartCoroutine("RushTarget");
            yield break;
        }

        yield return waitTime;
        yield return StartCoroutine("CombatReady");
        yield break;
    }

    IEnumerator AttackTarget()
    {
        if (target == null)
        {
            yield return waitTime;
            yield return StartCoroutine("SearchTarget");
            yield break;
        }

        Debug.Log("AttackTarget");


        for (int i = 0; i < 4; ++i)
            yield return waitTime;
        yield return StartCoroutine("CombatStart");
        yield break;
    }

    IEnumerator RushTarget()
    {
        if (target == null)
        {
            yield return StartCoroutine("SearchTarget");
            yield break;
        }
        agent.updateRotation = false;
        Debug.Log("RushTarget");

        float distance = Vector3.Distance(transform.position, target.transform.position) - 2;
        Vector3 dir = default;
        dir.x = target.transform.position.x - transform.position.x;
        dir.z = target.transform.position.z - transform.position.z;
        anim.SetFloat("TargetDistance", distance);

        float time = 0f;
        agent.destination = transform.position + (transform.forward * distance);
        //Chaser.transform.position = agent.destination;
        while (transform.position != agent.destination)
        {
            distance = Vector3.Distance(transform.position, target.transform.position);

            Quaternion lookRotation = Quaternion.LookRotation(dir);

            time += Time.deltaTime * agent.speed;
            if (time > 1f)
                time = 1f;

            Vector3 rotation = Quaternion.LerpUnclamped(transform.rotation, lookRotation, time).eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
            yield return null;
        }

        anim.SetFloat("TargetDistance", distance);
        agent.speed = Speed * 4;
        agent.updateRotation = true;

        for (int i = 0; i < 4; ++i)
            yield return waitTime;
        yield return StartCoroutine("CombatStart");
        yield break;
    }
    IEnumerator Dodge()
    {
        Debug.Log("Dodge");

        agent.destination = transform.position + (transform.right * RushDistance);
        Vector3 dir = default;
        dir.x = target.transform.position.x - transform.position.x;
        dir.z = target.transform.position.z - transform.position.z;
        dir = dir.normalized;
        //Chaser.transform.position = agent.destination;
        while (transform.position != agent.destination)
        {
            yield return null;
        }

        agent.updateRotation = false;




        yield return waitTime;
        agent.updateRotation = true;
        yield return StartCoroutine("CombatStart");
        yield break;
    }
}
