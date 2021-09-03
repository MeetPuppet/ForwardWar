using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    // ȸ�� �ӵ� ����
    public float rotSpeed = 200f;

    // ȸ�� �� ����
    float mx = 0;

    void Start()
    {

    }

    void Update()
    {
        // ������� ���콺 �Է��� �޾Ƽ� �÷��̾ ȸ����Ű�� �ʹ�.       
        // 1. ���콺 �¿� �Է��� �޴´�.
        float mouse_X = Input.GetAxis("Mouse X");

        // 1-1. ȸ�� �� ������ ���콺 �Է� ����ŭ �̸� ������Ų��.
        mx += mouse_X * rotSpeed * Time.deltaTime;

        // 2. ȸ�� �������� ��ü�� ȸ����Ų��.
        transform.eulerAngles = new Vector3(0, mx, 0);
        if (Input.GetKey(KeyCode.M))
        {
            Destroy(this);
        }
    }

}
