using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItem : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 7)
        {
            PlayerMove pm = collision.gameObject.GetComponent<PlayerMove>();
            pm.hp += 2;
            if (pm.hp > 20)
                pm.hp = 20f;
            pm.pf.Ammor += 5;
            pm.pf.AmmorUI.text = pm.pf.Ammor.ToString();
            Destroy(gameObject);
        }
    }
}
