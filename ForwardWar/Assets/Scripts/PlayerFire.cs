using UnityEngine;
using UnityEngine.UI;

public class PlayerFire : MonoBehaviour
{
    // 피격 이펙트 오브젝트
    public GameObject bulletEffect;
    System.Random rand = new System.Random(0);

    // 발사 이펙트 오브젝트
    public GameObject shootEffect;

    // 발사 위치
    public GameObject firePosition;

    public GameObject shootPosition;
    ParticleSystem ps;

    // 투척 무기 오브젝트
    public GameObject bombFactory;

    public GameObject SmokeFactory;

    public GameObject MissileFactory;

    // 투척 파워
    public float throwPower = 15f;

    // 발사 무기 공격력
    public int weaponPower = 1;
    int WeaponNum = -1;
    int Ammor = -1;

    //수류탄의 쿨타임 관리
    //public KeyCode skill1;
    public Image skill1image;
    public float cooldown_skill1 = 7;
    bool isCooldown_skill1 = false;


    //연막탄의 쿨타임 관리
    public KeyCode skill2;
    public Image skill2image;
    public float cooldown_skill2 = 11;
    bool isCooldown_skill2 = false;


    //미사일의 쿨타임 관리
    public KeyCode skill3;
    public Image skill3image;
    public float cooldown_skill3 = 11;
    bool isCooldown_skill3 = false;


    //먹기 관리
    Transform player;
    public KeyCode eat;
    public Image eatimage;
    public float cooldown_eat = 5;
    bool isCooldown_eat = false;

    public AudioSource Reload;
    public AudioSource gun;

    //애니메이터 변수
    public Animator anim;
    void Start()
    {
        // 플레이어의 트랜스폼 컴포넌트 받아오기
        player = GameObject.Find("Player").transform;

        //스킬 쿨타임 초기값
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
        // 마우스 오른쪽 버튼 입력을 받는다.  // 현재 수류탄
        if (Input.GetMouseButtonDown(1) && isCooldown_skill1 == false)
        {
            isCooldown_skill1 = true;
            skill1image.fillAmount = 1f;

            // 수류탄 오브젝트를 생성하고, 수류탄의 생성 위치를 발사 위치로 한다.
            GameObject bomb = Instantiate(bombFactory);
            bomb.transform.position = firePosition.transform.position;

            // 수류탄 오브젝트의 Rigidbody 컴포넌트를 가져온다.
            Rigidbody rb = bomb.GetComponent<Rigidbody>();

            // 카메라의 정면 방향으로 수류탄에 물리적인 힘을 가한다.
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

            // 수류탄 오브젝트를 생성하고, 수류탄의 생성 위치를 발사 위치로 한다.
            GameObject smoke_shell = Instantiate(SmokeFactory);
            smoke_shell.transform.position = firePosition.transform.position;

            // 수류탄 오브젝트의 Rigidbody 컴포넌트를 가져온다.
            Rigidbody arb = smoke_shell.GetComponent<Rigidbody>();

            // 카메라의 정면 방향으로 수류탄에 물리적인 힘을 가한다.
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

            // 수류탄 오브젝트를 생성하고, 수류탄의 생성 위치를 발사 위치로 한다.
            GameObject Missile_pos = Instantiate(MissileFactory);
            Missile_pos.transform.position = firePosition.transform.position;

            // 수류탄 오브젝트의 Rigidbody 컴포넌트를 가져온다.
            Rigidbody arb = Missile_pos.GetComponent<Rigidbody>();

            // 카메라의 정면 방향으로 수류탄에 물리적인 힘을 가한다.
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

        switch(WeaponNum)
        {
            case 0:
                HandGun();
                break;
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
        
    }

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
    void DefaultGun()
    {

    }
    void HandGun()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetBool("Shoot", true);
            shootPosition = Instantiate(shootEffect);
            ps = shootPosition.GetComponent<ParticleSystem>();
            anim.Play($"Shoot {0}");
            ps.Play();
            // 레이를 생성하고 발사될 위치와 진행 방향을 설정한다.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // 레이가 부딪힌 대상의 정보를 저장할 변수를 생성한다.
            RaycastHit hitInfo = new RaycastHit();
            // 레이를 발사하고, 만일 부딪힌 물체가 있으면...
            if (Physics.Raycast(ray, out hitInfo))
            {
                // 피격 이펙트의 위치를 레이가 부딪힌 지점으로 이동시킨다.
                shootPosition.transform.position = hitInfo.point;

                // 피격 이펙트의 forward 방향을 레이가 부딪힌 지점의 법선 벡터와 일치시킨다.
                shootPosition.transform.forward = hitInfo.normal;
                gun.Play();


                EnemyBase enemy = hitInfo.transform.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.HitEnemy(weaponPower);
                    GameManager.Score.editScore(10);
                    enemy.BloodActive(hitInfo);
                }
            }
            anim.SetBool("Shoot", false);
            Destroy(shootPosition);
        }
    }
    void AssaultRifle()
    {
        // 마우스 왼쪽 버튼을 누르면 시선이 바라보는 방향으로 총을 발사하고 싶다.
        if (Input.GetMouseButtonDown(0))
        {
            //GameManager.Updater.Add(Shoot());
            anim.SetBool("Shoot", true);
            // 이펙트 프리팹을 생성한다.
            shootPosition = Instantiate(shootEffect);
            ps = shootPosition.GetComponent<ParticleSystem>();
        }
        if (Input.GetMouseButtonUp(0))
        {
            anim.SetBool("Shoot", false);
            Destroy(shootPosition);
        }
        // 마우스 왼쪽 버튼 입력을 받는다.
        if (Input.GetMouseButton(0) && !gun.isPlaying)
        {
            anim.Play($"Shoot {WeaponNum}");
            ps.Play();
            // 레이를 생성하고 발사될 위치와 진행 방향을 설정한다.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // 레이가 부딪힌 대상의 정보를 저장할 변수를 생성한다.
            RaycastHit hitInfo = new RaycastHit();
            // 레이를 발사하고, 만일 부딪힌 물체가 있으면...
            if (Physics.Raycast(ray, out hitInfo))
            {
                // 피격 이펙트의 위치를 레이가 부딪힌 지점으로 이동시킨다.
                shootPosition.transform.position = hitInfo.point;

                // 피격 이펙트의 forward 방향을 레이가 부딪힌 지점의 법선 벡터와 일치시킨다.
                shootPosition.transform.forward = hitInfo.normal;
                gun.Play();


                EnemyBase enemy = hitInfo.transform.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.HitEnemy(weaponPower);
                    GameManager.Score.editScore(10);
                    enemy.BloodActive(hitInfo);
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
            anim.SetBool("Shoot", true);
            anim.Play($"Shoot {WeaponNum}");

            // 레이를 생성하고 발사될 위치와 진행 방향을 설정한다.
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

            // 레이가 부딪힌 대상의 정보를 저장할 변수를 생성한다.
            RaycastHit hitInfo = new RaycastHit();
            for(int i = 0; i < ray.Length; ++i)
            {
                shootPosition = Instantiate(shootEffect);
                Destroy(shootPosition, 0.5f);
                // 레이를 발사하고, 만일 부딪힌 물체가 있으면...
                if (Physics.Raycast(ray[i], out hitInfo))
                {
                    // 피격 이펙트의 위치를 레이가 부딪힌 지점으로 이동시킨다.
                    shootPosition.transform.position = hitInfo.point;

                    // 피격 이펙트의 forward 방향을 레이가 부딪힌 지점의 법선 벡터와 일치시킨다.
                    shootPosition.transform.forward = hitInfo.normal;
                    gun.Play();


                    EnemyBase enemy = hitInfo.transform.GetComponent<EnemyBase>();
                    if (enemy != null)
                    {
                        enemy.HitEnemy(weaponPower);
                        GameManager.Score.editScore(10);
                        enemy.BloodActive(hitInfo);
                    }
                }
            }
            ps = shootPosition.GetComponent<ParticleSystem>();
            ps.Play();
            anim.SetBool("Shoot", false);
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
            anim.SetBool("Shoot", true);
            shootPosition = Instantiate(shootEffect);
            ps = shootPosition.GetComponent<ParticleSystem>();
            anim.Play($"Shoot {WeaponNum}");
            ps.Play();
            // 레이를 생성하고 발사될 위치와 진행 방향을 설정한다.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // 레이가 부딪힌 대상의 정보를 저장할 변수를 생성한다.
            RaycastHit hitInfo = new RaycastHit();
            // 레이를 발사하고, 만일 부딪힌 물체가 있으면...
            if (Physics.Raycast(ray, out hitInfo))
            {
                // 피격 이펙트의 위치를 레이가 부딪힌 지점으로 이동시킨다.
                shootPosition.transform.position = hitInfo.point;

                // 피격 이펙트의 forward 방향을 레이가 부딪힌 지점의 법선 벡터와 일치시킨다.
                shootPosition.transform.forward = hitInfo.normal;
                gun.Play();


                EnemyBase enemy = hitInfo.transform.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.HitEnemy(weaponPower);
                    GameManager.Score.editScore(10);
                    enemy.BloodActive(hitInfo);
                }
            }
            anim.SetBool("Shoot", false);
            Destroy(shootPosition);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName($"Reload {WeaponNum}")
            && !Reload.isPlaying)
            Reload.Play();

    }
}