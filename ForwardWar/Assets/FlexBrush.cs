using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlexBrush : MonoBehaviour
{
    FlexSourceActor Actor;
    FlexContainer.ParticleData particleData;
    int maxSize;

    public Vector2 scale;
    public Vector2 offset;
    public RenderTexture Fog;
    public int FogSclice;
    public RenderTexture CopyTex;
    public int CopyTexSclice;
    // Start is called before the first frame update
    void Start()
    {
        scale = new Vector2(1f, 1f);
        offset = new Vector2(0f, 1f);
        Actor = transform.GetComponent<FlexSourceActor>();
        Vector4[] vec4 = Actor.container.m_particleArray;
    }

    // Update is called once per frame
    void Update()
    {

        Graphics.Blit(CopyTex, Fog, scale, offset, FogSclice, CopyTexSclice);
    }

    public void UpdateParticle(FlexContainer.ParticleData _particleData, int _maxSize)
    {
        particleData = _particleData;
        maxSize = _maxSize;

    }
}
