using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
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
    public int hp = 20;

    // �ִ� ü�� ����
    int maxHp = 20;

    public Slider hpSlider;

    public GameObject hitEffect;

    //��Ʈ���� �÷��� ������Ʈ
    public GameObject FlexComp;
    private FlexSourceActor flexSource;

    //�� plane���� ������ object
    public GameObject waterSector;

    //�ִϸ����� ����
    Animator anim;
    void Start()
    {
        flexSource = FlexComp.GetComponent<FlexSourceActor>();

        // ĳ���� ��Ʈ�ѷ� ������Ʈ �޾ƿ���
        cc = GetComponent<CharacterController>();

        //
        anim = GetComponentInChildren<Animator>();
        Debug.Log("start");
    }

    void Update()
    {
        // Ű���� <W>, <A>, <S>, <D> ��ư�� �Է��ϸ� ĳ���͸� �� �������� �̵���Ű�� �ʹ�.
        // Ű���� <Space> ��ư�� �Է��ϸ� ĳ���͸� �������� ������Ű�� �ʹ�.

        // 1. ������� �Է��� �޴´�.
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 2. �̵� ������ �����Ѵ�.
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
        yield return new WaitForSeconds(0.3f);
        //�ǰ� UI ����
        hitEffect.SetActive(false);

    }



    private void OnTriggerEnter(Collider other)
    {
        if (flexSource == null || flexSource.isActive == true)
        {
            return;
        }

        //waterSector�� �����ϴ� �ݶ��̴��� ���˽� FlexComp�� �۵���Ŵ
        for (int i = 0; i < waterSector.transform.childCount; ++i)
        {
            if (waterSector.transform.GetChild(i).Equals(other.transform))
                flexSource.isActive = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (flexSource == null || flexSource.isActive == true)
        {
            flexSource.isActive = false;
        }
    }



}
