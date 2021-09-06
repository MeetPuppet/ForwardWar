using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFire : MonoBehaviour
{
    // �ǰ� ����Ʈ ������Ʈ
    public GameObject bulletEffect;
    System.Random rand = new System.Random(0);
    public PlayerMove pm;

    // �߻� ����Ʈ ������Ʈ
    public GameObject shootEffect;

    // �߻� ��ġ
    public GameObject firePosition;

    public GameObject shootPosition;
    ParticleSystem ps;

    ParticleSystem bloodPs;
    public GameObject bloodEffect;
    public GameObject bloodPosition;

    // ��ô ���� ������Ʈ
    public GameObject bombFactory;

    public GameObject SmokeFactory;

    public GameObject MissileFactory;

    // ��ô �Ŀ�
    public float throwPower = 15f;

    // �߻� ���� ���ݷ�
    public int weaponPower = 1;
    public int WeaponNum = -1;
    public int Ammor = -1;
    public Text AmmorUI;

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

    public AudioSource Reload;
    public AudioSource gun;

    //�ִϸ����� ����
    public Animator anim;
    void Start()
    {
        // �÷��̾��� Ʈ������ ������Ʈ �޾ƿ���
        player = GameObject.Find("Player").transform;

        //��ų ��Ÿ�� �ʱⰪ
        skill1image.fillAmount = 0f;
        skill2image.fillAmount = 0f;
        //skill3image.fillAmount = 0f;
        //eatimage.fillAmount = 0f;

        gun.mute = false;
        gun.loop = false;
        gun.playOnAwake = false;
        AmmorUI.text = Ammor.ToString();
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

        if (block)
            return;

        if (WeaponNum > 0 && Ammor <= 0)
        {
            WeaponNum = 0;
            pm.RefreshItem(0);
            gun = pm.Weapons[0].GetComponent<AudioSource>();
            GunSetting(0, 2, Ammor);
        }

        switch (WeaponNum)
        {
            case 1:
                AssaultRifle();
                break;
            case 2:
                ShotGun();
                break;
            case 3:
                Rifle();
                break;
            default:
                HandGun();
                break;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            pm.RefreshItem(0);
            gun = pm.Weapons[0].GetComponent<AudioSource>();
            GunSetting(0, 2, Ammor);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (pm.WeaponEnables[1] == false)
            {
                pm.RefreshItem(0);
                gun = pm.Weapons[0].GetComponent<AudioSource>();
                GunSetting(0, 2, Ammor);
            }
            else
            {
                pm.RefreshItem(1);
                gun = pm.Weapons[1].GetComponent<AudioSource>();
                GunSetting(1, 2, Ammor);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (pm.WeaponEnables[2] == false)
            {
                pm.RefreshItem(0);
                gun = pm.Weapons[0].GetComponent<AudioSource>();
                GunSetting(0, 2, Ammor);
            }
            else
            {
                pm.RefreshItem(2);
                gun = pm.Weapons[2].GetComponent<AudioSource>();
                GunSetting(2, 2, Ammor);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (pm.WeaponEnables[3] == false)
            {
                pm.RefreshItem(0);
                gun = pm.Weapons[0].GetComponent<AudioSource>();
                GunSetting(0, 2, Ammor);
            }
            else
            {
                pm.RefreshItem(3);
                gun = pm.Weapons[3].GetComponent<AudioSource>();
                GunSetting(3, 20, Ammor);
            }
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ++WeaponNum;
            if (WeaponNum >= 4)
            {
                WeaponNum = 0;
                pm.RefreshItem(0);
                gun = pm.Weapons[0].GetComponent<AudioSource>();
                GunSetting(0, 2, Ammor);
            }
            else
            {
                if(Ammor <= 0 || pm.WeaponEnables[WeaponNum] == false)
                {
                    WeaponNum = 0;
                }
                pm.RefreshItem(WeaponNum);
                gun = pm.Weapons[WeaponNum].GetComponent<AudioSource>();
                switch (WeaponNum)
                {
                    case 1:
                        GunSetting(1, 1, Ammor);
                        break;
                    case 2:
                        GunSetting(2, 2, Ammor);
                        break;
                    case 3:
                        GunSetting(3, 20, Ammor);
                        break;
                    default:
                        pm.RefreshItem(0);
                        gun = pm.Weapons[0].GetComponent<AudioSource>();
                        GunSetting(0, 2, Ammor);
                        break;
                }
            }


        }
    }

    //���� ��ȣ, ����, ź���
    public void GunSetting(int WNum, int power, int ammor)
    {
        WeaponNum = WNum;
        weaponPower = power;
        Ammor = ammor;
        anim.SetInteger("AnimInit", WNum);
        while (anim.GetInteger("AnimInit") != WNum) ;
        anim.Play("AnimInit");
        Reload.Play();
    }
    void HandGun()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gun.Play();
            anim.SetBool("Shoot", true);
            shootPosition = Instantiate(shootEffect);
            ps = shootPosition.GetComponent<ParticleSystem>();
            anim.Play($"Shoot {0}");
            ps.Play();
            // ���̸� �����ϰ� �߻�� ��ġ�� ���� ������ �����Ѵ�.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // ���̰� �ε��� ����� ������ ������ ������ �����Ѵ�.
            RaycastHit hitInfo = new RaycastHit();
            // ���̸� �߻��ϰ�, ���� �ε��� ��ü�� ������...
            if (Physics.Raycast(ray, out hitInfo,float.MaxValue, -1, QueryTriggerInteraction.Ignore))
            {
                if (shootPosition)
                {
                    // �ǰ� ����Ʈ�� ��ġ�� ���̰� �ε��� �������� �̵���Ų��.
                    shootPosition.transform.position = hitInfo.point;

                    // �ǰ� ����Ʈ�� forward ������ ���̰� �ε��� ������ ���� ���Ϳ� ��ġ��Ų��.
                    shootPosition.transform.forward = hitInfo.normal;
                }


                EnemyBase enemy = hitInfo.transform.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    bloodPosition = Instantiate(bloodEffect);
                    bloodPs = bloodPosition.GetComponent<ParticleSystem>();
                    bloodPs.Play();
                    bloodPosition.transform.forward = hitInfo.normal;
                    bloodPosition.transform.position = hitInfo.point;

                    enemy.HitEnemy(weaponPower);
                    GameManager.Score.editScore(10);
                    enemy.BloodActive(hitInfo);
                }
                PartCutter pc = hitInfo.transform.GetComponent<PartCutter>();
                if(pc != null)
                {
                    pc.ActivateBlades(hitInfo);
                }
            }
            anim.SetBool("Shoot", false);
            Destroy(shootPosition, 0.1f);
            Destroy(bloodPosition, 0.1f);
            pm.AddVertical(0.1f);
            if (WeaponNum != -1 && Ammor <= 0)
            {
                GunSoundChange(pm.Weapons[0].GetComponent<AudioSource>());
            }
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
            bloodPosition = Instantiate(bloodEffect);
            bloodPs = bloodPosition.GetComponent<ParticleSystem>();
        }
        if (Input.GetMouseButtonUp(0))
        {
            Destroy(bloodPosition);
            Destroy(shootPosition);
            shootPosition = null;
            anim.SetBool("Shoot", false);
        }
        // ���콺 ���� ��ư �Է��� �޴´�.
        if (Input.GetMouseButton(0) && !gun.isPlaying)
        {
            gun.Play();
            anim.Play($"Shoot {WeaponNum}");
            ps.Play();
            // ���̸� �����ϰ� �߻�� ��ġ�� ���� ������ �����Ѵ�.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // ���̰� �ε��� ����� ������ ������ ������ �����Ѵ�.
            RaycastHit hitInfo = new RaycastHit();
            // ���̸� �߻��ϰ�, ���� �ε��� ��ü�� ������...
            if (Physics.Raycast(ray, out hitInfo, float.MaxValue, -1, QueryTriggerInteraction.Ignore))
            {
                if(shootPosition)
                {
                    // �ǰ� ����Ʈ�� ��ġ�� ���̰� �ε��� �������� �̵���Ų��.
                    shootPosition.transform.position = hitInfo.point;

                    // �ǰ� ����Ʈ�� forward ������ ���̰� �ε��� ������ ���� ���Ϳ� ��ġ��Ų��.
                    shootPosition.transform.forward = hitInfo.normal;
                }


                EnemyBase enemy = hitInfo.transform.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    bloodPs.Play();
                    bloodPosition.transform.forward = hitInfo.normal;
                    bloodPosition.transform.position = hitInfo.point;

                    enemy.HitEnemy(weaponPower);
                    GameManager.Score.editScore(10);
                    enemy.BloodActive(hitInfo);
                }
                PartCutter pc = hitInfo.transform.GetComponent<PartCutter>();
                if (pc != null)
                {
                    pc.ActivateBlades(hitInfo);
                }
            }
            --Ammor;
            AmmorUI.text = Ammor.ToString();
            if (Ammor <= 0)
            {
                GunSoundChange(pm.Weapons[0].GetComponent<AudioSource>());
                if (shootPosition != null)
                {
                    Destroy(shootPosition);
                    Destroy(bloodPosition);
                }
            }
        }
    }

    void ShotGun()
    {
        if (Input.GetMouseButtonDown(0) 
            && !anim.GetCurrentAnimatorStateInfo(0).IsName($"Shoot {WeaponNum}")
            && !anim.GetCurrentAnimatorStateInfo(0).IsName($"Reload {WeaponNum}"))
        {
            gun.Play();
            anim.SetBool("Shoot", true);
            anim.Play($"Shoot {WeaponNum}");

            // ���̸� �����ϰ� �߻�� ��ġ�� ���� ������ �����Ѵ�.
            Ray[] ray = new Ray[5];
            for (int i = 0; i < ray.Length; ++i)
            {
                float[] f = new float[3];
                for(int j = 0; j < f.Length; ++j)
                {
                    f[j] = (float)rand.NextDouble() % 0.1f;
                }
                Vector3 vec3 = new Vector3(f[0], f[1], f[2]);

                ray[i] = new Ray(Camera.main.transform.position,
                    Camera.main.transform.forward + vec3);
            }

            // ���̰� �ε��� ����� ������ ������ ������ �����Ѵ�.
            RaycastHit hitInfo = new RaycastHit();
            for(int i = 0; i < ray.Length; ++i)
            {
                shootPosition = Instantiate(shootEffect);
                Destroy(shootPosition, 0.5f);

                // ���̸� �߻��ϰ�, ���� �ε��� ��ü�� ������...
                if (Physics.Raycast(ray[i], out hitInfo, float.MaxValue, -1, QueryTriggerInteraction.Ignore))
                {
                    if (shootPosition)
                    {
                        // �ǰ� ����Ʈ�� ��ġ�� ���̰� �ε��� �������� �̵���Ų��.
                        shootPosition.transform.position = hitInfo.point;

                        // �ǰ� ����Ʈ�� forward ������ ���̰� �ε��� ������ ���� ���Ϳ� ��ġ��Ų��.
                        shootPosition.transform.forward = hitInfo.normal;
                    }


                    EnemyBase enemy = hitInfo.transform.GetComponent<EnemyBase>();
                    if (enemy != null)
                    {
                        bloodPosition = Instantiate(bloodEffect);
                        bloodPs = bloodPosition.GetComponent<ParticleSystem>();
                        bloodPs.Play();
                        bloodPosition.transform.forward = hitInfo.normal;
                        bloodPosition.transform.position = hitInfo.point;
                        Destroy(bloodPosition, 0.5f);

                        enemy.HitEnemy(weaponPower);
                        GameManager.Score.editScore(10);
                        enemy.BloodActive(hitInfo);
                    }
                    PartCutter pc = hitInfo.transform.GetComponent<PartCutter>();
                    if (pc != null)
                    {
                        pc.ActivateBlades(hitInfo);
                    }
                }
            }
            ps = shootPosition.GetComponent<ParticleSystem>();
            ps.Play();
            anim.SetBool("Shoot", false);
            shootPosition = null;
            --Ammor;
            AmmorUI.text = Ammor.ToString();
            if (Ammor <= 0)
            {
                GunSoundChange(pm.Weapons[0].GetComponent<AudioSource>());
            }
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName($"Reload {WeaponNum}")
            && !Reload.isPlaying)
            Reload.Play();

    }
    void Rifle()
    {
        if (Input.GetMouseButtonDown(0)
            && !anim.GetCurrentAnimatorStateInfo(0).IsName($"Shoot {WeaponNum}")
            && !anim.GetCurrentAnimatorStateInfo(0).IsName($"Reload {WeaponNum}"))
        {
            gun.Play();
            anim.SetBool("Shoot", true);
            shootPosition = Instantiate(shootEffect);
            ps = shootPosition.GetComponent<ParticleSystem>();
            anim.Play($"Shoot {WeaponNum}");
            ps.Play();
            // ���̸� �����ϰ� �߻�� ��ġ�� ���� ������ �����Ѵ�.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // ���̰� �ε��� ����� ������ ������ ������ �����Ѵ�.
            RaycastHit hitInfo = new RaycastHit();
            // ���̸� �߻��ϰ�, ���� �ε��� ��ü�� ������...
            if (Physics.Raycast(ray, out hitInfo, float.MaxValue, -1, QueryTriggerInteraction.Ignore))
            {
                if (shootPosition)
                {
                    // �ǰ� ����Ʈ�� ��ġ�� ���̰� �ε��� �������� �̵���Ų��.
                    shootPosition.transform.position = hitInfo.point;

                    // �ǰ� ����Ʈ�� forward ������ ���̰� �ε��� ������ ���� ���Ϳ� ��ġ��Ų��.
                    shootPosition.transform.forward = hitInfo.normal;
                }


                EnemyBase enemy = hitInfo.transform.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    bloodPosition = Instantiate(bloodEffect);
                    bloodPs = bloodPosition.GetComponent<ParticleSystem>();
                    bloodPs.Play();
                    bloodPosition.transform.forward = hitInfo.normal;
                    bloodPosition.transform.position = hitInfo.point;
                    enemy.HitEnemy(weaponPower);
                    GameManager.Score.editScore(10);
                    enemy.BloodActive(hitInfo);
                }
                PartCutter pc = hitInfo.transform.GetComponent<PartCutter>();
                if (pc != null)
                {
                    pc.ActivateBlades(hitInfo);
                }
            }
            anim.SetBool("Shoot", false);
            Destroy(shootPosition);
            Destroy(bloodPosition);
            --Ammor;
            AmmorUI.text = Ammor.ToString();
            if (Ammor <= 0)
            {
                GunSoundChange(pm.Weapons[0].GetComponent<AudioSource>());
            }
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName($"Reload {WeaponNum}")
            && !Reload.isPlaying)
            Reload.Play();

    }

    public void GunSoundChange(AudioSource GunSound)
    {
        GameManager.Updater.Add(ResetSoundChange(GunSound));
    }

    bool block = false;
    IEnumerator ResetSoundChange(AudioSource GunSound)
    {
        block = true;
        while (gun.isPlaying)
            yield return null;

        gun = GunSound;
        pm.RefreshItem(0);
        GunSetting(0, 2, Ammor);
        block = false;
        yield break;
    }
}