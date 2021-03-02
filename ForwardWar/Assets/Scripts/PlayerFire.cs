using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFire : MonoBehaviour
{

    // �ǰ� ����Ʈ ������Ʈ
    public GameObject bulletEffect;

    // �ǰ� ����Ʈ ��ƼŬ �ý���
    ParticleSystem ps;

    // �߻� ��ġ
    public GameObject firePosition;

    // ��ô ���� ������Ʈ
    public GameObject bombFactory;

    // ��ô �Ŀ�
    public float throwPower = 15f;

    // �߻� ���� ���ݷ�
    public int weaponPower = 5;

    //�ִϸ����� ����
    Animator anim;
    void Start()
    {
        // �ǰ� ����Ʈ ������Ʈ���� ��ƼŬ �ý��� ������Ʈ ��������
        ps = bulletEffect.GetComponent<ParticleSystem>();

        //�ִϸ����� ������Ʈ ��������
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // ���콺 ������ ��ư �Է��� �޴´�.
        if (Input.GetMouseButtonDown(1))
        {
            // ����ź ������Ʈ�� �����ϰ�, ����ź�� ���� ��ġ�� �߻� ��ġ�� �Ѵ�.
            GameObject bomb = Instantiate(bombFactory);
            bomb.transform.position = firePosition.transform.position;

            // ����ź ������Ʈ�� Rigidbody ������Ʈ�� �����´�.
            Rigidbody rb = bomb.GetComponent<Rigidbody>();

            // ī�޶��� ���� �������� ����ź�� �������� ���� ���Ѵ�.
            rb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);
        }
        // ���콺 ���� ��ư�� ������ �ü��� �ٶ󺸴� �������� ���� �߻��ϰ� �ʹ�.

        // ���콺 ���� ��ư �Է��� �޴´�.
        if (Input.GetMouseButtonDown(0) && anim.GetFloat("MoveBlend") == 0)
        {
            //���� �̵����� Ʈ�� �Ķ������ ���� 0�̸� ���� ����
            if(anim.GetFloat("MoveBlend") == 0)
            {
                anim.SetTrigger("Attack");
            }
            // ���̸� �����ϰ� �߻�� ��ġ�� ���� ������ �����Ѵ�.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // ���̰� �ε��� ����� ������ ������ ������ �����Ѵ�.
            RaycastHit hitInfo = new RaycastHit();

            // ���̸� �߻��ϰ�, ���� �ε��� ��ü�� ������...
            if (Physics.Raycast(ray, out hitInfo))
            {
                // �ǰ� ����Ʈ�� ��ġ�� ���̰� �ε��� �������� �̵���Ų��.
                bulletEffect.transform.position = hitInfo.point;

                // �ǰ� ����Ʈ�� forward ������ ���̰� �ε��� ������ ���� ���Ϳ� ��ġ��Ų��.
                bulletEffect.transform.forward = hitInfo.normal;

                // �ǰ� ����Ʈ�� �÷����Ѵ�.
                ps.Play();

                hitInfo.transform.GetComponent<EnemyFSM>()?.HitEnemy(weaponPower);
            }
        }

        
    }
}