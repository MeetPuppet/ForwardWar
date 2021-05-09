using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodMarkGenrator : MonoBehaviour
{
    static public IEnumerator cor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       //if(cor != null)
       //{
       //    cor.MoveNext();
       //}
    }

    static public void FleXActive(FlexContainer.ParticleData _particleData, int max)
    {
        if (cor == null)
            cor = Work(_particleData, max);

        //cor.MoveNext();
    }

    static IEnumerator Work(FlexContainer.ParticleData _particleData, int max)
    {
        int iter = 0;
        Vector4[] getPart = new Vector4[max/100];
        for (int i = 0; i < max / 10; ++i)
        {
            _particleData.GetParticles(iter, iter + max / 100, getPart);
            if (getPart[i] == null)
            {
                Debug.Log($"{i}");
                break;
            }
            else if (getPart[i] != Vector4.zero)
            {
                Debug.Log($"{i}, {getPart[i]}");
            }

            yield return null;
        }

        yield break;
    }
}
