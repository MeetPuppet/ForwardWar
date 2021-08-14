using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadAnim : MonoBehaviour
{
    Animator ac;

    // Start is called before the first frame update
    void Start()
    {
        ac = GetComponent<Animator>();
        ac.Play("Dead");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
