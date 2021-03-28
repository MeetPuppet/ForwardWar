using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideUI : MonoBehaviour
{
    public GameObject player;
    MercenaryControl controler;

    public GameObject Select;

    // Start is called before the first frame update
    void Start()
    {
        controler = player.GetComponent<MercenaryControl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controler.guideLine == 0)
        {
            Select.transform.localPosition = Vector3.Lerp(Select.transform.localPosition, new Vector3(-50, 0,0), Time.deltaTime * 10);
        }
        else
        {
            Select.transform.localPosition = Vector3.Lerp(Select.transform.localPosition, new Vector3(50, 0,0), Time.deltaTime * 10);
        }
    }
}
