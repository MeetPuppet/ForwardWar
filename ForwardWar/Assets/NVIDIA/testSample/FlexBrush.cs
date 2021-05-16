using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FlexBrushInfo
{
    public Vector2 scale;
    public Vector2 offset;
    public RenderTexture Fog;
    public int FogSclice;
    public RenderTexture CopyTex;
    public int CopyTexSclice;

    public bool updated;
}

public class FlexBrush : MonoBehaviour
{
    //[SerializeField]
    //FlexBrushInfo info;
    public Vector2Int TerrainSize;

    public Vector2Int sizeStart = new Vector2Int(0, 0);
    Vector2Int sizeEnd;
    //public Vector2Int sizeFog;
    public Vector2 scale = new Vector2(1f, 1f);
    public Vector2 offset = new Vector2(0f, 0f);
    int FogSclice = 0;
    int CopyTexSclice = 0;

    Vector2Int sizeResult;

    public RenderTexture Result;

    public RenderTexture Clean;
    public RenderTexture CopyTex;

    private void Start()
    {
        Graphics.Blit(Clean, Result);
    }

    // Update is called once per frame
    void Update()
    {
        Graphics.Blit(CopyTex, Result, scale, offset, FogSclice, CopyTexSclice);
    }

    /*1000 사이즈 테스트
    public RenderTexture Fog;
    public RenderTexture privateTex;
    void oldUpdate()
    {
        //info.CopyTex = CopyTex;
        //info.Fog = Fog;
        //info.scale = scale;
        //info.offset = offset;
        //info.FogSclice = FogSclice;
        //info.CopyTexSclice = CopyTexSclice;
        //info.updated = true;

        //Graphics.Blit(privateTex, CopyTex, scale, offset, FogSclice, CopyTexSclice);
        Graphics.Blit(CopyTex, Fog, scale, offset, FogSclice, CopyTexSclice);

        sizeEnd.x = Fog.width / (int)scale.x;
        sizeEnd.y = Fog.height / (int)scale.y;

        float x = transform.position.x / TerrainSize.x;
        float y = transform.position.z / TerrainSize.y;

        sizeResult.x = (int)(Result.width * x - (sizeEnd.x / 2));
        sizeResult.y = (int)(Result.height * y - (sizeEnd.y / 2));

        Graphics.CopyTexture(Fog, 0, 0, sizeStart.x, sizeStart.y, sizeStart.x + sizeEnd.x, sizeStart.y + sizeEnd.y,
            Result, 0, 0, sizeResult.x, sizeResult.y);
        //Graphics.Blit(Fog, Result);

        //Graphics.Blit(Result, privateTex);
        //Graphics.Blit(Clean, Result);
        //Graphics.CopyTexture(Fog, Result);
    }
    */
}
