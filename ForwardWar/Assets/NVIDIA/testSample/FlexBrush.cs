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
    public Transform FleXCam;
    public Vector2Int TerrainSize;

    public RenderTexture CopyTex;

    public RenderTexture Fog;
    public Vector2Int sizeStart;
    Vector2Int sizeEnd;
    //public Vector2Int sizeFog;
    public Vector2 scale;
    public Vector2 offset;
    int FogSclice = 0;
    int CopyTexSclice = 0;

    public RenderTexture Clean;
    public RenderTexture Result;
    public Vector2Int sizeResult;

    private void Start()
    {
        Graphics.Blit(Clean, Result);
    }

    // Update is called once per frame
    void Update()
    {
        //info.CopyTex = CopyTex;
        //info.Fog = Fog;
        //info.scale = scale;
        //info.offset = offset;
        //info.FogSclice = FogSclice;
        //info.CopyTexSclice = CopyTexSclice;
        //info.updated = true;
        Debug.Log(CopyTex.texelSize);

        Graphics.Blit(CopyTex, Fog, scale, offset, FogSclice, CopyTexSclice);

        sizeEnd.x = Fog.width / 64;
        sizeEnd.y = Fog.height / 64;

        float x = FleXCam.position.x / TerrainSize.x;
        float y = FleXCam.position.z / TerrainSize.y;

        sizeResult.x = (int)(Result.width * x - (sizeEnd.x/2));
        sizeResult.y = (int)(Result.height * y - (sizeEnd.y / 2));

        Graphics.CopyTexture(Fog, 0,0, sizeStart.x, sizeStart.y, sizeStart.x + sizeEnd.x, sizeStart.y + sizeEnd.y,
            Result, 0,0, sizeResult.x, sizeResult.y);
        //Graphics.CopyTexture(Fog, Result);
    }

    //void CopyBrushing(FlexBrushInfo info)
    //{
    //    GameManager.thread.UpdateTexColor(info);
    //}
}
