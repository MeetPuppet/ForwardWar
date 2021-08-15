using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEffect : MonoBehaviour
{
    // ���ŵ� �ð� ����
    public float destroyTime = 1.5f;
    private ParticleSystem partS;
    public AudioSource boom;
    // ��� �ð� ������ ����
    float currentTime = 0;
    public bool is_smoke = false;

    private void Start()
    {
    }

    void Update()
    {
        if (currentTime > (destroyTime - 14) && is_smoke)
        {
            partS = GetComponent<ParticleSystem>();
            partS.Stop();
        }
        // ���� ��� �ð��� ���ŵ� �ð��� �ʰ��Ѵٸ�, �ڱ� �ڽ��� �����Ѵ�.
        if (currentTime > destroyTime)
        {
            Destroy(gameObject);
        }
        // ��� �ð��� �����Ѵ�.
        currentTime += Time.deltaTime;
    }
}
