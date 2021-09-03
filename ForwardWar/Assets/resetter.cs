using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resetter : MonoBehaviour
{
    public GameObject prefab;
    
    // Start is called before the first frame update
    void Start()
    {
        prefab.transform.position = transform.position;
        GameManager.Updater.Add(summon());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator summon()
    {
        while(true)
        {
            Instantiate<GameObject>(prefab);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
