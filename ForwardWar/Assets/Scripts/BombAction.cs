using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombAction : MonoBehaviour
{
    // ���� ����Ʈ ������ ����
    public GameObject bombEffect;

    // �浹���� ���� ó��
    private void OnCollisionEnter(Collision collision)
    {
        // ����Ʈ �������� �����Ѵ�.
        GameObject eff = Instantiate(bombEffect);

        // ����Ʈ �������� ��ġ�� ����ź ������Ʈ �ڽ��� ��ġ�� �����ϴ�.
        eff.transform.position = transform.position;

        // �ڱ� �ڽ��� �����Ѵ�.
        Destroy(gameObject);
    }
}
