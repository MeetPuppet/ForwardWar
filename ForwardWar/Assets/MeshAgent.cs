using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeshAgent : MonoBehaviour
{
    public Transform goal;
    public Terrain terrain;
    private NavMeshAgent agent = null;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MovePosition(Transform MoveDest)
    {
        goal = MoveDest;
        agent.SetDestination(goal.position);
    }
}
