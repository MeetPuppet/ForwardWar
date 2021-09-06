using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NaviComp : MonoBehaviour
{
    public NavMeshAgent agent;

    // Start is called before the first frame update
    public void Find(Vector3 dest)
    {
        agent.SetDestination(dest);
    }
    private void Update()
    {
        if(agent.destination != transform.position &&
            Vector3.Distance(agent.destination, transform.position) <= 3f)
        {
            agent.SetDestination(transform.position);
            Destroy(gameObject, 2);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
