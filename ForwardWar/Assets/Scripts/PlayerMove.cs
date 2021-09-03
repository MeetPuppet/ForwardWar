using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class PlayerMove : MonoBehaviour
{
    //컷씬 제어 변수
    public PlayableDirector playableDirector_start;
    public PlayableDirector playableDirector;
    public PlayableDirector playableDirector_end;
    // 이동 속도 변수
    public float moveSpeed = 7f;

    // 캐릭터 콘트롤러 변수
    CharacterController cc;

    // 중력 변수
    float gravity = -20f;

    // 수직 속력 변수
    public float yVelocity = 0;

    // 점프력 변수
    public float jumpPower = 10f;

    // 점프 상태 변수
    public bool isJumping = false;

    // 플레이어 체력 변수
    public float hp = 20;
    public Text Thp;

    // 최대 체력 변수
    float maxHp = 20;

    public Slider hpSlider;

    public GameObject hitEffect;

    public GameObject Cam_pos;
    //컨트롤할 플렉스 오브젝트
    public GameObject FlexComp;

    public Transform[] Weapons;

    private FlexSourceActor flexSource;

    ////물 plane들을 포함한 object
    //public GameObject waterSector;

    //애니메이터 변수
    public Animator anim;

    public GameObject ActivateButton;
    void Start()
    {
        RefreshItem(1);
        Cursor.visible = false;
        if (ActivateButton == null)
            ActivateButton = GameObject.Find("InteractiveButton");
        flexSource = FlexComp.GetComponent<FlexSourceActor>();

        maxHp = hp;

        // 캐릭터 콘트롤러 컴포넌트 받아오기
        cc = GetComponent<CharacterController>();

        //
        //anim = GetComponentInChildren<Animator>();

        //AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        //int i = 0;
        //foreach (AnimationClip ac in clips)
        //{
        //    clips[i++] = ac;
        //}


        Debug.Log("start");
    }

    void Update()
    {
        // 사용자의 입력을 받는다.
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 이동 방향을 설정
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

        //변경지점
        if (hp <= 0f)
        {
            anim.SetTrigger("Dead");
        }
        anim.SetFloat("Blend", dir.magnitude);
        Debug.Log(dir.magnitude);
        //변경지점

        // 2-1. 메인 카메라를 기준으로 방향을 변환한다.
        dir = Camera.main.transform.TransformDirection(dir);

        // 2-2. 만일, 점프 중이었고, 다시 바닥에 착지했다면
        if (isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            // 점프 전 상태로 초기화한다.
            isJumping = false;
            // 캐릭터 수직 속도를 0으로 만든다.
            yVelocity = 0;
        }

        // 2-3. 만일, 키보드 <Space> 버튼을 입력했고, 점프를 안 한 상태라면
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            // 캐릭터 수직 속도에 점프력을 적용하고 점프 상태로 변경한다.
            yVelocity = jumpPower;
            isJumping = true;
        }

        // 2-4. 캐릭터 수직 속도에 중력 값을 적용한다.
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        // 3. 이동 속도에 맞춰 이동한다.
        cc.Move(dir * moveSpeed * Time.deltaTime);

        // 4. 현재 플레이어 hp를 hp 슬라이더에 반영
        hpSlider.value = (float)hp / (float)maxHp;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 5))
        {
            ActSwitchObject ASO = hit.transform.GetComponent<ActSwitchObject>();
            if (ASO)
            {
                ASO.OnActivateButton();
                if (Input.GetKeyDown(KeyCode.F))
                    hit.transform.GetComponent<ActSwitchObject>()?.ActivateObject(this);
            }
            else
            {
                ActivateButton.SetActive(false);
            }
        }

        if(Thp)
        {
            Thp.text = (hp / maxHp * 100).ToString() + "%";
        }

        if (Input.GetKey(KeyCode.G))
        {
            playableDirector.gameObject.SetActive(true);
            playableDirector.Play();
        }
        if (Input.GetKey(KeyCode.K))
        {
            playableDirector_start.gameObject.SetActive(true);
            playableDirector_start.Play();
        }
        if (Input.GetKey(KeyCode.M))
        {
            playableDirector_end.gameObject.SetActive(true);
            playableDirector_end.Play();
            Destroy(this);
        }

    }

    void updateOld()
    {
        // 사용자의 입력을 받는다.
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 이동 방향을 설정
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

        //이덩 블랜딩 트리를 호출하고 벡터의 크기 값 넘겨주기
        anim.SetFloat("Blend", dir.magnitude);

        // 2-1. 메인 카메라를 기준으로 방향을 변환한다.
        dir = Camera.main.transform.TransformDirection(dir);

        // 2-2. 만일, 점프 중이었고, 다시 바닥에 착지했다면
        if (isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            // 점프 전 상태로 초기화한다.
            isJumping = false;
            // 캐릭터 수직 속도를 0으로 만든다.
            yVelocity = 0;
        }

        // 2-3. 만일, 키보드 <Space> 버튼을 입력했고, 점프를 안 한 상태라면
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            // 캐릭터 수직 속도에 점프력을 적용하고 점프 상태로 변경한다.
            yVelocity = jumpPower;
            isJumping = true;
        }

        // 2-4. 캐릭터 수직 속도에 중력 값을 적용한다.
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        // 3. 이동 속도에 맞춰 이동한다.
        cc.Move(dir * moveSpeed * Time.deltaTime);

        // 4. 현재 플레이어 hp를 hp 슬라이더에 반영
        hpSlider.value = (float)hp / (float)maxHp;
    }

    // 플레이어의 피격 함수
    public void DamageAction(int damage)
    {
        // 에너미의 공격력만큼 플레이어의 체력을 깎는다.
        hp -= damage;

        if (hp > 0)
        {
            StartCoroutine(PlayHitEffect());
        }
    }
    IEnumerator PlayHitEffect()
    {
        //피격 UI 실행
        hitEffect.SetActive(true);
        //대기
        Cam_pos.transform.Translate(0.15f, 0.15f, 0);
        yield return new WaitForSeconds(0.05f);
        Cam_pos.transform.Translate(0, -0.3f, 0);
        yield return new WaitForSeconds(0.05f);
        Cam_pos.transform.Translate(-0.3f, 0.15f, 0);
        yield return new WaitForSeconds(0.05f);
        Cam_pos.transform.Translate(0.15f, -0.15f, 0);
        yield return new WaitForSeconds(0.05f);
        Cam_pos.transform.Translate(-0.15f, 0.3f, 0);
        yield return new WaitForSeconds(0.05f);
        Cam_pos.transform.Translate(0.15f, 0, 0);
        yield return new WaitForSeconds(0.05f);
        Cam_pos.transform.Translate(0, -0.15f, 0);
        //피격 UI 종료
        hitEffect.SetActive(false);

    }

    public void EatAction()
    {
        if (hp > 0)
        {
            StartCoroutine(PlayEatEffect());
        }
    }
    IEnumerator PlayEatEffect()
    {
        //먹는 애니메이션 실행

        yield return new WaitForSeconds(0.3f);
    }



    private void OnTriggerEnter(Collider other)
    {
        if (flexSource == null || flexSource.isActive == true)
        {
            return;
        }

        //waterSector에 존재하는 콜라이더와 접촉시 FlexComp를 작동시킴
        //for (int i = 0; i < waterSector.transform.childCount; ++i)
        //{
        //    if (waterSector.transform.GetChild(i).Equals(other.transform))
        //        flexSource.isActive = true;
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        if (flexSource == null || flexSource.isActive == true)
        {
            flexSource.isActive = false;
        }
    }

    public int WeapoenNum;
    public void RefreshItem(int num)
    {
        if (Weapons.Length <= num)
            return;

        for(int i = 0; i < Weapons.Length; ++i)
        {
            Weapons[i].gameObject.SetActive(false);
        }
        Weapons[num].gameObject.SetActive(true);

        WeapoenNum = num;
        anim.SetInteger("AnimInit", num);
        anim.Play($"Idle {num}");
    }

    public GameObject Navi;
    public void FindWay(Vector3 dest)
    {
        GameObject go = Instantiate(Navi, transform.position, transform.rotation);
        go.GetComponent<NaviComp>().Find(dest);
    }
    //IEnumerator TriggerControl()
    //{
    //}

}
