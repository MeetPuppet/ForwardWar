using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFire : MonoBehaviour
{
    PlayerMove pm;
    // �ǰ� ����Ʈ ������Ʈ
    public GameObject bulletEffect;

    // �߻� ����Ʈ ������Ʈ
    public GameObject shootEffect;

    // �߻� ��ġ
    public GameObject firePosition;

    public GameObject shootPosition;
    ParticleSystem ps;

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

    public AudioSource gun;

    //�ִϸ����� ����
    public Animator anim;
    void Start()
    {
        // �÷��̾��� Ʈ������ ������Ʈ �޾ƿ���
        player = GameObject.Find("Player").transform;
        pm = transform.GetComponent<PlayerMove>();

        //��ų ��Ÿ�� �ʱⰪ
        skill1image.fillAmount = 0f;
        skill2image.fillAmount = 0f;
        //skill3image.fillAmount = 0f;
        eatimage.fillAmount = 0f;

        gun.mute = false;
        gun.loop = false;
        gun.playOnAwake = false;
    }
 
    void Update()
    {
        if (Input.GetKey(KeyCode.M))
        {
            Destroy(this);
        }
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

        switch(pm.WeapoenNum)
        {
            case 1:
                AssaultRifle();
                break;
        }
        
    }

    void AssaultRifle()
    {
        // ���콺 ���� ��ư�� ������ �ü��� �ٶ󺸴� �������� ���� �߻��ϰ� �ʹ�.

        if (Input.GetMouseButtonDown(0))
        {
            //GameManager.Updater.Add(Shoot());
            anim.SetBool("Shoot", true);
            // ����Ʈ �������� �����Ѵ�.
            shootPosition = Instantiate(shootEffect);
            ps = shootPosition.GetComponent<ParticleSystem>();
        }
        if (Input.GetMouseButtonUp(0))
        {
            anim.SetBool("Shoot", false);
            Destroy(shootPosition);
        }
        // ���콺 ���� ��ư �Է��� �޴´�.
        if (Input.GetMouseButton(0) && !gun.isPlaying)
        {
            anim.Play($"Shoot {pm.WeapoenNum}");
            ps.Play();
            // ���̸� �����ϰ� �߻�� ��ġ�� ���� ������ �����Ѵ�.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // ���̰� �ε��� ����� ������ ������ ������ �����Ѵ�.
            RaycastHit hitInfo = new RaycastHit();
            // ���̸� �߻��ϰ�, ���� �ε��� ��ü�� ������...
            if (Physics.Raycast(ray, out hitInfo))
            {
                // �ǰ� ����Ʈ�� ��ġ�� ���̰� �ε��� �������� �̵���Ų��.
                shootPosition.transform.position = hitInfo.point;

                // �ǰ� ����Ʈ�� forward ������ ���̰� �ε��� ������ ���� ���Ϳ� ��ġ��Ų��.
                shootPosition.transform.forward = hitInfo.normal;
                gun.Play();


                EnemyBase enemy = hitInfo.transform.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.HitEnemy(3);
                    GameManager.Score.editScore(10);
                    enemy.BloodActive(hitInfo);
                }
            }
        }
    }
}