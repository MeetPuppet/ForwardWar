using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGunGenerator : MonoBehaviour
{
    public GameObject BulletObject = null;
    public int BulletPower = 1;

    int objIndex;

    // Start is called before the first frame update
    void Start()
    {
        objIndex = transform.childCount;

        for(int i = 0; i < objIndex; ++i)
        {
            GameObject obj = Instantiate<GameObject>(BulletObject, transform.GetChild(i));
            obj.GetComponent<Rigidbody>().AddForce(Vector3.forward * BulletPower);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
