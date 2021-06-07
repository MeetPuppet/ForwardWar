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


        //�Ÿ� 10���� ��� �ݶ��̴� Ȯ��
        Collider[] collider = Physics.OverlapSphere(transform.position, 10);
        for (int i = 0; i < collider.Length; ++i)
        {
            Transform parent = collider[i].transform.parent;
            //�θ� �ִ� ���
            if (parent)
            {
                //CutPart�� �ҷ��� �Լ� �۵�
                CutPart cp = parent.GetComponent<CutPart>();
                if (cp)
                    cp.Activate();
            }
        }

        // �ڱ� �ڽ��� �����Ѵ�.
        Destroy(gameObject);
    }
}
