using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumChecker : MonoBehaviour
{
    public Transform EdgeObj;
    public Transform Player;
    AudioSource AS;

    // Start is called before the first frame update
    void Start()
    {
        AS = GetComponent<AudioSource>();
        if (EdgeObj == null)
            EdgeObj = transform;
        if (Player == null)
            Player = transform;
    }

    static float PlayerDistance;
    static float EdgeDistance;
    // Update is called once per frame
    void Update()
    {
        PlayerDistance = Vector3.Distance(transform.position, Player.position);
        EdgeDistance = Vector3.Distance(transform.position, EdgeObj.position);

        AS.volume = PlayerDistance / EdgeDistance;
    }
}
