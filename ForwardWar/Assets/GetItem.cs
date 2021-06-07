using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItem : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 7)
        {
            collision.gameObject.GetComponent<PlayerMove>().hp += 2;
            Destroy(gameObject);
        }
    }
}
