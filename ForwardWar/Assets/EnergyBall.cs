using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBall : MonoBehaviour
{
    GameObject target;
    Transform start;
    int power;

    float currentTime;

    public void InfoSet(int Power, GameObject Target)
    {
        start = transform;
        target = Target;
        power = Power;
        currentTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        transform.position = Vector3.Lerp(start.position, target.transform.position, currentTime);

        if (currentTime >= 1f)
            Destroy(gameObject);
    }
}
