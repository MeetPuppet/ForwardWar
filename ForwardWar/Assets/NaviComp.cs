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
        agent.destination = dest;
    }
    private void Update()
    {
        if(Vector3.Distance(agent.destination, transform.position) <= 0.1f)
        {
            Destroy(gameObject, 1);
        }
    }
}
