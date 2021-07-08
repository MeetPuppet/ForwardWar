using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFire : MonoBehaviour
{

    // �ǰ� ����Ʈ ������Ʈ
    public GameObject bulletEffect;

    // �ǰ� ����Ʈ ��ƼŬ �ý���
    ParticleSystem ps;

    // �߻� ����Ʈ ������Ʈ
    public GameObject shootEffect;

    // �߻� ��ġ
    public GameObject firePosition;

    public GameObject shootPosition;

    // ��ô ���� ������Ʈ
    public GameObject bombFactory;

    public GameObject SmokeFactory;

    public GameObject MissileFactory;

    // ��ô �Ŀ�
    public float throwPower = 15f;

    // �߻� ���� ���ݷ�
    public int weaponPower = 5;

    //����ź�� ��Ÿ�� ����
    //public KeyCode skill1;
    public Image skill1image;
    public float cooldown_skill1 = 7;
    bool isCooldown_skill1 = false;


    //����ź�� ��Ÿ�� ����
    public KeyCode skill2;
    public Image skill2image;
    public float cooldown_skill2 = 11;
    bool isCooldown_skill2 = false;


    //�̻����� ��Ÿ�� ����
    public KeyCode skill3;
    public Image skill3image;
    public float cooldown_skill3 = 11;
    bool isCooldown_skill3 = false;


    //�Ա� ����
    Transform player;
    public KeyCode eat;
    public Image eatimage;
    public float cooldown_eat = 5;
    bool isCooldown_eat = false;
    
    //�ִϸ����� ����
    Animator anim;
    void Start()
    {
        // �÷��̾��� Ʈ������ ������Ʈ �޾ƿ���
        player = GameObject.Find("Player").transform;

        // �ǰ� ����Ʈ ������Ʈ���� ��ƼŬ �ý��� ������Ʈ ��������
        ps = bulletEffect.GetComponent<ParticleSystem>();

        //�ִϸ����� ������Ʈ ��������
        anim = GetComponentInChildren<Animator>();

        //��ų ��Ÿ�� �ʱⰪ
        skill1image.fillAmount = 0f;
        skill2image.fillAmount = 0f;
        //skill3image.fillAmount = 0f;
        eatimage.fillAmount = 0f;
    }
 
    void Update()
    {
        // ���콺 ������ ��ư �Է��� �޴´�.  // ���� ����ź
        if (Input.GetMouseButtonDown(1) && isCooldown_skill1 == false)
        {
            isCooldown_skill1 = true;
            skill1image.fillAmount = 1f;

            // ����ź ������Ʈ�� �����ϰ�, ����ź�� ���� ��ġ�� �߻� ��ġ�� �Ѵ�.
            GameObject bomb = Instantiate(bombFactory);
            bomb.transform.position = firePosition.transform.position;

            // ����ź ������Ʈ�� Rigidbody ������Ʈ�� �����´�.
            Rigidbody rb = bomb.GetComponent<Rigidbody>();

            // ī�޶��� ���� �������� ����ź�� �������� ���� ���Ѵ�.
            rb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);
        }
        if (isCooldown_skill1)
        {
            skill1image.fillAmount -= 1f / cooldown_skill1 * Time.deltaTime;
            if(skill1image.fillAmount <= 0f)
            {
                skill1image.fillAmount = 0f;
                isCooldown_skill1 = false;
            }
        }

        if (Input.GetKey(skill2) && isCooldown_skill2 == false)
        {
            isCooldown_skill2 = true;
            skill2image.fillAmount = 1f;

            // ����ź ������Ʈ�� �����ϰ�, ����ź�� ���� ��ġ�� �߻� ��ġ�� �Ѵ�.
            GameObject smoke_shell = Instantiate(SmokeFactory);
            smoke_shell.transform.position = firePosition.transform.position;

            // ����ź ������Ʈ�� Rigidbody ������Ʈ�� �����´�.
            Rigidbody arb = smoke_shell.GetComponent<Rigidbody>();

            // ī�޶��� ���� �������� ����ź�� �������� ���� ���Ѵ�.
            arb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);
        }
        if (isCooldown_skill2)
        {
            skill2image.fillAmount -= 1f / cooldown_skill2 * Time.deltaTime;
            if (skill2image.fillAmount <= 0f)
            {
                skill2image.fillAmount = 0f;
                isCooldown_skill2 = false;
            }
        }

        if (Input.GetKey(skill3) && isCooldown_skill3 == false)
        {
            isCooldown_skill3 = true;
            skill3image.fillAmount = 1f;

            // ����ź ������Ʈ�� �����ϰ�, ����ź�� ���� ��ġ�� �߻� ��ġ�� �Ѵ�.
            GameObject Missile_pos = Instantiate(MissileFactory);
            Missile_pos.transform.position = firePosition.transform.position;

            // ����ź ������Ʈ�� Rigidbody ������Ʈ�� �����´�.
            Rigidbody arb = Missile_pos.GetComponent<Rigidbody>();

            // ī�޶��� ���� �������� ����ź�� �������� ���� ���Ѵ�.
            arb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);
        }
        if (isCooldown_skill3)
        {
            skill3image.fillAmount -= 1f / cooldown_skill3 * Time.deltaTime;
            if (skill3image.fillAmount <= 0f)
            {
                skill3image.fillAmount = 0f;
                isCooldown_skill3 = false;
            }
        }

        if (Input.GetKey(eat) && isCooldown_eat == false)
        {
            eatimage.fillAmount = 1f;
            isCooldown_eat = true;
            player.GetComponent<PlayerMove>().EatAction();
        }

        if (isCooldown_eat)
        {
            eatimage.fillAmount -= 1f / cooldown_eat * Time.deltaTime;
            player.GetComponent<PlayerMove>().hp += 5f / cooldown_eat * Time.deltaTime;
            if (eatimage.fillAmount <= 0f)
            {
                eatimage.fillAmount = 0f;
                isCooldown_eat = false;
            }
        }

        // ���콺 ���� ��ư�� ������ �ü��� �ٶ󺸴� �������� ���� �߻��ϰ� �ʹ�.

        // ���콺 ���� ��ư �Է��� �޴´�.
        if (Input.GetMouseButtonDown(0) && anim.GetFloat("Blend") == 0)
        {
            // ���̸� �����ϰ� �߻�� ��ġ�� ���� ������ �����Ѵ�.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // ����Ʈ �������� �����Ѵ�.
            GameObject eff = Instantiate(shootEffect);
            // ����Ʈ �������� ��ġ�� player ��ġ
            eff.transform.position = shootPosition.transform.position;
        


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


                EnemyBase enemy = hitInfo.transform.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.HitEnemy(3);
                    //enemy.BloodActive(hitInfo);
                }
            }

            //Destroy(eff);
        }

        
    }

}