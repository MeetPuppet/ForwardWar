using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class PlayerMove : MonoBehaviour
{
    //�ƾ� ���� ����
    public PlayableDirector playableDirector_start;
    public PlayableDirector playableDirector;
    public PlayableDirector playableDirector_end;
    // �̵� �ӵ� ����
    public float moveSpeed = 7f;

    // ĳ���� ��Ʈ�ѷ� ����
    CharacterController cc;

    // �߷� ����
    float gravity = -20f;

    // ���� �ӷ� ����
    public float yVelocity = 0;

    // ������ ����
    public float jumpPower = 10f;

    // ���� ���� ����
    public bool isJumping = false;

    // �÷��̾� ü�� ����
    public float hp = 20;
    public Text Thp;

    // �ִ� ü�� ����
    float maxHp = 20;

    public Slider hpSlider;

    public GameObject hitEffect;

    public GameObject Cam_pos;
    //��Ʈ���� �÷��� ������Ʈ
    public GameObject FlexComp;

    public Transform[] Weapons;

    private FlexSourceActor flexSource;

    ////�� plane���� ������ object
    //public GameObject waterSector;

    //�ִϸ����� ����
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

        // ĳ���� ��Ʈ�ѷ� ������Ʈ �޾ƿ���
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
        // ������� �Է��� �޴´�.
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // �̵� ������ ����
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

        //��������
        if (hp <= 0f)
        {
            anim.SetBool("Dead", true);
        }
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("Shoot");
        }
        anim.SetFloat("Blend", dir.magnitude);
        Debug.Log(dir.magnitude);
        //��������

        // 2-1. ���� ī�޶� �������� ������ ��ȯ�Ѵ�.
        dir = Camera.main.transform.TransformDirection(dir);

        // 2-2. ����, ���� ���̾���, �ٽ� �ٴڿ� �����ߴٸ�
        if (isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            // ���� �� ���·� �ʱ�ȭ�Ѵ�.
            isJumping = false;
            // ĳ���� ���� �ӵ��� 0���� �����.
            yVelocity = 0;
        }

        // 2-3. ����, Ű���� <Space> ��ư�� �Է��߰�, ������ �� �� ���¶��
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            // ĳ���� ���� �ӵ��� �������� �����ϰ� ���� ���·� �����Ѵ�.
            yVelocity = jumpPower;
            isJumping = true;
        }

        // 2-4. ĳ���� ���� �ӵ��� �߷� ���� �����Ѵ�.
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        // 3. �̵� �ӵ��� ���� �̵��Ѵ�.
        cc.Move(dir * moveSpeed * Time.deltaTime);

        // 4. ���� �÷��̾� hp�� hp �����̴��� �ݿ�
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

        if (Physics.Raycast(ray, out hit))
            if (Input.GetKeyDown(KeyCode.F))
            FindWay(hit.point);
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
        // ������� �Է��� �޴´�.
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // �̵� ������ ����
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

        //�̵� ���� Ʈ���� ȣ���ϰ� ������ ũ�� �� �Ѱ��ֱ�
        anim.SetFloat("Blend", dir.magnitude);

        // 2-1. ���� ī�޶� �������� ������ ��ȯ�Ѵ�.
        dir = Camera.main.transform.TransformDirection(dir);

        // 2-2. ����, ���� ���̾���, �ٽ� �ٴڿ� �����ߴٸ�
        if (isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            // ���� �� ���·� �ʱ�ȭ�Ѵ�.
            isJumping = false;
            // ĳ���� ���� �ӵ��� 0���� �����.
            yVelocity = 0;
        }

        // 2-3. ����, Ű���� <Space> ��ư�� �Է��߰�, ������ �� �� ���¶��
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            // ĳ���� ���� �ӵ��� �������� �����ϰ� ���� ���·� �����Ѵ�.
            yVelocity = jumpPower;
            isJumping = true;
        }

        // 2-4. ĳ���� ���� �ӵ��� �߷� ���� �����Ѵ�.
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        // 3. �̵� �ӵ��� ���� �̵��Ѵ�.
        cc.Move(dir * moveSpeed * Time.deltaTime);

        // 4. ���� �÷��̾� hp�� hp �����̴��� �ݿ�
        hpSlider.value = (float)hp / (float)maxHp;
    }

    // �÷��̾��� �ǰ� �Լ�
    public void DamageAction(int damage)
    {
        // ���ʹ��� ���ݷ¸�ŭ �÷��̾��� ü���� ��´�.
        hp -= damage;

        if (hp > 0)
        {
            StartCoroutine(PlayHitEffect());
        }
    }
    IEnumerator PlayHitEffect()
    {
        //�ǰ� UI ����
        hitEffect.SetActive(true);
        //���
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
        //�ǰ� UI ����
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
        //�Դ� �ִϸ��̼� ����

        yield return new WaitForSeconds(0.3f);
    }



    private void OnTriggerEnter(Collider other)
    {
        if (flexSource == null || flexSource.isActive == true)
        {
            return;
        }

        //waterSector�� �����ϴ� �ݶ��̴��� ���˽� FlexComp�� �۵���Ŵ
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

    public void RefreshItem(int num)
    {
        if (Weapons.Length <= num)
            return;

        for(int i = 0; i < Weapons.Length; ++i)
        {
            Weapons[i].gameObject.SetActive(false);
        }
        Weapons[num].gameObject.SetActive(true);

        anim.SetInteger("AnimInit", num);
        anim.Play($"Idle {num}");
    }

    public GameObject Navi;
    void FindWay(Vector3 dest)
    {
        GameObject go = Instantiate(Navi, transform.position, transform.rotation);
        go.GetComponent<NaviComp>().Find(dest);
    }
    //IEnumerator TriggerControl()
    //{
    //}

}
