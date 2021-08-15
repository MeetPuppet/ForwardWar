using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBall : MonoBehaviour
{
    public GameObject target;
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
        Vector3 dir = (target.transform.position - transform.position).normalized;
        transform.forward = dir;
        transform.position += dir;

        currentTime += Time.deltaTime;
        if (currentTime >= 1f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 7)
        {
            PlayerMove pm = other.GetComponent<PlayerMove>();
            VillagerComp vill = other.GetComponent<VillagerComp>();
            if (pm == null && vill == null)
            {
            }
            else if (pm != null)
            {
                pm.DamageAction(2);
            }
            else if (vill != null)
            {
                vill.HitVillager(2);
            }
            Destroy(gameObject);
        }
    }
}
