using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommunicateBox : MonoBehaviour
{
    public GameObject EndPop;
    public GameObject text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            GameManager.GameOver();
            EndPop.SetActive(true);
            text.transform.GetComponent<Text>().text = GameManager.playTime.ToString();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            GameManager.GameOver();
            GameObject EndPop = GameObject.Find("End");
            EndPop.SetActive(true);
            GameObject text = GameObject.Find("Text");
            text.transform.GetComponent<Text>().text = GameManager.playTime.ToString();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 7)
        {
            GameManager.GameOver();
            GameObject EndPop = GameObject.Find("End");
            EndPop.SetActive(true);
            GameObject text = GameObject.Find("Text");
            text.transform.GetComponent<Text>().text = GameManager.playTime.ToString();
        }
        
    }
}
