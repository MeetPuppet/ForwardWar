using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotate : MonoBehaviour
{
    // ȸ�� �ӵ� ����
    public float rotSpeed = 200f;

    // ȸ�� �� ����
    float mx = 0;
    float my = 0;

    void Update()
    {
        // ������� ���콺 �Է��� �޾Ƽ� ��ü�� ȸ����Ű�� �ʹ�.       
        // 1. ���콺 �Է��� �޴´�.
        float mouse_X = Input.GetAxis("Mouse X");
        float mouse_Y = Input.GetAxis("Mouse Y");

        // 1-1. ȸ�� �� ������ ���콺 �Է� ����ŭ �̸� ������Ų��.
        mx += mouse_X * rotSpeed * Time.deltaTime;
        my += mouse_Y * rotSpeed * Time.deltaTime;

        // 1-2. ���콺 ���� �̵� ȸ�� ����(my)�� ���� -90�� ~ 90�� ���̷� �����Ѵ�.
        my = Mathf.Clamp(my, -90f, 90f);

        // 2. ���콺 �Է� ���� �̿��Ͽ� ȸ�� ������ �����Ѵ�.        
        //Vector3 dir = new Vector3(-mouse_Y, mouse_X, 0);

        // 2. ȸ�� �������� ��ü�� ȸ����Ų��.
        // r = r0 + vt
        //transform.eulerAngles += dir * rotSpeed * Time.deltaTime;
        transform.eulerAngles = new Vector3(-my, mx, 0);

        // 4. x�� ȸ��(���� ȸ��) ���� -90�� ~ 90�� ���̷� �����Ѵ�.
        //Vector3 rot = transform.eulerAngles;
        //rot.x = Mathf.Clamp(rot.x, -90f, 90f);
        //transform.eulerAngles = rot;
    }
}
