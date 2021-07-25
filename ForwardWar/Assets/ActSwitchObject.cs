using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActSwitchObject : MonoBehaviour
{
    public GameObject ActivateButton;

    // Start is called before the first frame update
    void Start()
    {
        initialize();
    }
    protected virtual void initialize()
    {
        ActivateButton = GameObject.Find("InteractiveButton");
    }

    public virtual void OnActivateButton()
    {
        ActivateButton.SetActive(true);
    }
    public virtual void OffActivateButton()
    {
        ActivateButton.SetActive(false);
    }

    //상호작용 작동 시
    public virtual void ActivateObject()
    {
        Debug.Log("Activate");
    }
}
