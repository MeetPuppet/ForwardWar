using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFire : MonoBehaviour
{

    // 피격 이펙트 오브젝트
    public GameObject bulletEffect;

    // 피격 이펙트 파티클 시스템
    ParticleSystem ps;

    // 발사 위치
    public GameObject firePosition;

    // 투척 무기 오브젝트
    public GameObject bombFactory;

    // 투척 무기 오브젝트
    public GameObject SmokeFactory;

    // 투척 파워
    public float throwPower = 15f;

    // 발사 무기 공격력
    public int weaponPower = 5;

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

    //먹기 관리
    Transform player;
    public KeyCode eat;
    public Image eatimage;
    public float cooldown_eat = 5;
    bool isCooldown_eat = false;

    //애니메이터 변수
    Animator anim;
    void Start()
    {
        // 플레이어의 트랜스폼 컴포넌트 받아오기
        player = GameObject.Find("Player").transform;

        // 피격 이펙트 오브젝트에서 파티클 시스템 컴포넌트 가져오기
        ps = bulletEffect.GetComponent<ParticleSystem>();

        //애니메이터 컴포넌트 가져오기
        anim = GetComponentInChildren<Animator>();

        //스킬 쿨타임 초기값
        skill1image.fillAmount = 0f;
        skill2image.fillAmount = 0f;
        eatimage.fillAmount = 0f;
    }
 
    void Update()
    {
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

        // 마우스 왼쪽 버튼을 누르면 시선이 바라보는 방향으로 총을 발사하고 싶다.

        // 마우스 왼쪽 버튼 입력을 받는다.
        if (Input.GetMouseButtonDown(0) && anim.GetFloat("Blend") == 0)
        {
            //만일 이동블렌드 트리 파라미터의 값이 0이면 공격 실행
            if(anim.GetFloat("Blend") == 0)
            {
                anim.SetTrigger("Attack");
            }
            // 레이를 생성하고 발사될 위치와 진행 방향을 설정한다.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // 레이가 부딪힌 대상의 정보를 저장할 변수를 생성한다.
            RaycastHit hitInfo = new RaycastHit();

            // 레이를 발사하고, 만일 부딪힌 물체가 있으면...
            if (Physics.Raycast(ray, out hitInfo))
            {
                // 피격 이펙트의 위치를 레이가 부딪힌 지점으로 이동시킨다.
                bulletEffect.transform.position = hitInfo.point;

                // 피격 이펙트의 forward 방향을 레이가 부딪힌 지점의 법선 벡터와 일치시킨다.
                bulletEffect.transform.forward = hitInfo.normal;

                // 피격 이펙트를 플레이한다.
                ps.Play();

                EnemyFSM enemy = hitInfo.transform.GetComponent<EnemyFSM>();
                if (enemy != null)
                {
                    enemy.HitEnemy(3);
                    enemy.BloodActive(hitInfo);
                }
            }
        }

        
    }
}