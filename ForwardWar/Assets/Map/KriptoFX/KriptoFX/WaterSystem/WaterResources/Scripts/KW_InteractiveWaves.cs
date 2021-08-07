using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class KW_InteractiveWaves
{
    private const int defaultMaxArrayCapacity = 20;

    public RenderTexture bufferCurrent;
    public RenderTexture bufferPrev;
    private RenderTexture normalCurrent;
    private RenderTexture normalPrev;

    private int bufferCount = 0;
    //List<DrawPointInfo> drawPoints = new List<DrawPointInfo>(100);
    private int currentDrawIdx;
    private float[] drawPointsX = new float[defaultMaxArrayCapacity];
    private float[] drawPointsY = new float[defaultMaxArrayCapacity];
    private float[] drawPointsSize = new float[defaultMaxArrayCapacity];
    private float[] drawPointsForce = new float[defaultMaxArrayCapacity];

    private Mesh GridVBO;
    private Vector3[]     vertices;
    private Color[]       colors;
    private List<Vector3> uv;
    private int[]         triangles;

    void IncreaseDrawArray()
    {
        var newSize = (int) (drawPointsX.Length * 1.2f); //increase capacity on 20%
        Array.Resize(ref drawPointsX, newSize);
        Array.Resize(ref drawPointsY, newSize);
        Array.Resize(ref drawPointsSize, newSize);
        Array.Resize(ref drawPointsForce, newSize);
        
        KW_Extensions.SafeDestroy(GridVBO);
        CreateGridVBO(newSize);
    }

    public void AddPositionToDrawArray(Vector3 areaPos, Vector3 position, float size, float force, float areaSize)
    {
        if (currentDrawIdx >= drawPointsX.Length) IncreaseDrawArray();
      
        areaSize *= 0.5f;
        position -= areaPos;
        drawPointsX[currentDrawIdx] = (position.x / areaSize) * 0.5f + 0.5f;
        drawPointsY[currentDrawIdx] = (position.z / areaSize) * 0.5f + 0.5f;
        drawPointsSize[currentDrawIdx] = size;
        drawPointsForce[currentDrawIdx] = force;
        ++currentDrawIdx;
    }
   
    public void RenderWaves(int FPS, float waveSpeed, float quality, int areaSize, Material interactiveWavesMaterial, params Material[] materials)
    {
        float pixelsPerMeter = (FPS / waveSpeed) * quality;
        var bufferSize = (int)(areaSize * pixelsPerMeter);
        var pixelSpeed = quality * quality; //we change texture size relative to quality, so "0.5" it's not a "x2 speed slow". We resize texture x2 times and should slow time x4 

        InitializeBuffers(bufferSize);
        if (GridVBO == null) CreateGridVBO(defaultMaxArrayCapacity);
        UpdateVBO(areaSize);

        UpdateBuffers(interactiveWavesMaterial, areaSize, pixelSpeed, materials);
    }

    public int GetTextureResolution()
    {
        if (bufferCurrent != null) return bufferCurrent.width;
        else return 0;
    }

    public void Release()
    {
        if (bufferCurrent != null) bufferCurrent.Release();
        if (bufferPrev != null) bufferPrev.Release();
        if (normalCurrent != null) normalCurrent.Release();
        if (normalPrev != null) normalPrev.Release();
        KW_Extensions.SafeDestroy(GridVBO);
    }

    void InitializeBuffers(int bufferSize)
    {
        if (bufferCurrent != null && bufferCurrent.width != bufferSize)
        {
            RenderTexture.ReleaseTemporary(bufferCurrent);
            RenderTexture.ReleaseTemporary(bufferPrev);
            RenderTexture.ReleaseTemporary(normalCurrent);
            RenderTexture.ReleaseTemporary(normalPrev);
        }
        if (bufferCurrent == null || bufferCurrent.width != bufferSize)
        {
            bufferCurrent = RenderTexture.GetTemporary(bufferSize, bufferSize, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            RenderTexture.active = bufferCurrent;
            GL.Clear(false, true, Color.gray);
            RenderTexture.active = null;

            bufferPrev           = RenderTexture.GetTemporary(bufferSize, bufferSize, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            RenderTexture.active = bufferPrev;
            GL.Clear(false, true, Color.gray);
            RenderTexture.active = null;

            normalCurrent = RenderTexture.GetTemporary(bufferSize, bufferSize, 0, RenderTextureFormat.RG16, RenderTextureReadWrite.Linear);
            normalPrev = RenderTexture.GetTemporary(bufferSize, bufferSize, 0, RenderTextureFormat.RG16, RenderTextureReadWrite.Linear);
        }
        
    }

    void UpdateBuffers(Material mat, int areaSize, float pixelSpeed, params Material[] materials)
    {
        var bufferOne = (bufferCount % 2 == 0) ? bufferCurrent : bufferPrev;
        var bufferTwo = (bufferCount % 2 == 0) ? bufferPrev : bufferCurrent;
        var bufferNormal = (bufferCount % 2 == 0) ? normalCurrent : normalPrev;
        

        var drawedPoints = RenderTexture.GetTemporary(bufferOne.width, bufferOne.height, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        var temp = RenderTexture.GetTemporary(bufferOne.width, bufferOne.height, 0, bufferOne.format, RenderTextureReadWrite.Linear);

       // DrawArrayToTexture(drawedPoints, mat, areaSize);
        DrawInstancedArrayToTexture(drawedPoints, mat, areaSize);

        mat.SetTexture("KW_DrawedPointTex", drawedPoints);

        Graphics.Blit(bufferOne, temp, mat, 1);
        Graphics.Blit(temp, bufferOne);

        mat.SetFloat("KW_InteractiveWavesPixelSpeed", pixelSpeed);
        mat.SetTexture("_PrevTex", bufferTwo);
        Graphics.Blit(bufferOne, temp, mat, 2);
        Graphics.Blit(temp, bufferTwo);

        mat.SetFloat("KW_InteractiveWavesAreaSize", areaSize);
        Graphics.Blit(bufferTwo, bufferNormal, mat, 3);

        RenderTexture.ReleaseTemporary(drawedPoints);
        RenderTexture.ReleaseTemporary(temp);

        UpdateShaderParams(bufferTwo, bufferOne, bufferNormal, normalPrev, areaSize, materials);

        bufferCount++;

    }

    void UpdateShaderParams(RenderTexture current, RenderTexture last, RenderTexture normal, RenderTexture normalLast, int areaSize, params Material[] materials)
    {
        foreach (var mat in materials)
        {
            mat.SetTexture("KW_InteractiveWavesTex", current);
            mat.SetTexture("KW_InteractiveWavesTexPrev", current);
            mat.SetTexture("KW_InteractiveWavesNormalTex", normal);
            mat.SetTexture("KW_InteractiveWavesNormalTexPrev", normalLast);
            mat.SetFloat("KW_InteractiveWavesAreaSize", areaSize);
        }
    }

    void DrawInstancedArrayToTexture(RenderTexture rt, Material mat, int areaSize)
    {
        if (mat == null || rt == null) return;
        Graphics.SetRenderTarget(rt);
        RenderTexture.active = rt;
        mat.SetPass(0);
        GL.PushMatrix();
        GL.GetGPUProjectionMatrix(Matrix4x4.identity, true);

        GL.Clear(false, true, Color.black);

        Graphics.DrawMeshNow(GridVBO, Matrix4x4.identity);

        Graphics.SetRenderTarget(null);
        
        GL.PopMatrix();
    }

    void UpdateVBO(int areaSize)
    {
        int currentVertex = 0;
        for (int i = 0; i < currentDrawIdx; i++)
        {
            var size = (2 * drawPointsSize[i]) / areaSize;
            var halfSize = size / 2;

            vertices[currentVertex].x = drawPointsX[i] - halfSize;
            vertices[currentVertex].y = drawPointsY[i] - size * 0.2886751345948129f;

            vertices[currentVertex + 1].x = drawPointsX[i];
            vertices[currentVertex + 1].y = drawPointsY[i] + size * 0.5773502691896258f;

            vertices[currentVertex + 2].x = drawPointsX[i] + halfSize;
            vertices[currentVertex + 2].y = drawPointsY[i] - size * 0.2886751345948129f;

            colors[currentVertex].r = colors[currentVertex + 1].r = colors[currentVertex + 2].r = drawPointsForce[i];

            currentVertex += 3;
        }

        var count = vertices.Length;
        var zero = Vector2.zero;
        for (int i = currentVertex; i < count; i++)
        {
            vertices[i] = zero;
        }

        GridVBO.vertices = vertices;
        GridVBO.colors = colors;
        currentDrawIdx = 0;
    }


    void CreateGridVBO(int trisCount)
    {
        GridVBO = new Mesh();
        vertices = new Vector3[trisCount * 3];
        colors = new Color[vertices.Length];
        uv = new List<Vector3>();
        triangles = new int[vertices.Length];

        for (int i = 0; i < vertices.Length; i += 3)
        {
            var offset = (float)i / vertices.Length;

            vertices[i] = new Vector3(offset, offset);
            vertices[i + 1] = new Vector3(1.0f / trisCount + offset, offset);
            vertices[i + 2] = new Vector3(offset, 1.0f / trisCount + offset);

            uv.Add(new Vector3(1, 0, 0));
            uv.Add(new Vector3(0, 1, 0));
            uv.Add(new Vector3(0, 0, 1));

            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;
        }

        GridVBO.vertices = vertices;
        GridVBO.colors = colors;
        GridVBO.triangles = triangles;
        GridVBO.SetUVs(0, uv);

        GridVBO.MarkDynamic();
    }

    //void DrawArrayToTexture(RenderTexture rt, Material mat, int areaSize)
    //{
    //    Graphics.SetRenderTarget(rt);
    //    GL.PushMatrix();
    //    mat.SetPass(0);

    //    GL.LoadOrtho();
    //    GL.Clear(false, true, Color.black);

    //    GL.Begin(GL.TRIANGLES);

    //    for(int i = 0; i < currentDrawIdx; i++)
    //    {
    //        GL.Color(Color.white * drawPointsForce[i]);
    //        DrawQuad(drawPointsX[i], drawPointsY[i], drawPointsSize[i], areaSize);
    //    }

    //    GL.End();

    //    GL.PopMatrix();
    //    Graphics.SetRenderTarget(null);
    //    currentDrawIdx = 0;
    //}

    //private static void DrawQuad(float x, float y, float size, int areaSize)
    //{
    //    size = (2 * size) / areaSize;

    //    var halfSize = size / 2;
    //    GL.TexCoord3(1, 0, 0);
    //    GL.Vertex3(x - halfSize, y - size * 0.2886751345948129f, 0);

    //    GL.TexCoord3(0, 1, 0);
    //    GL.Vertex3( x, y + size * 0.5773502691896258f, 0);

    //    GL.TexCoord3(0, 0, 1);
    //    GL.Vertex3(x + halfSize, y - size * 0.2886751345948129f, 0);
    //}
}
