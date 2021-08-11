using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Rigidbody[] rigidbodies;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < rigidbodies.Length; ++i)
        {
            rigidbodies[i].AddForce(transform.forward * 2000);
        }
    }
}
