using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KW_CameraColorTexture : MonoBehaviour
{
    public RenderTexture CameraColorTexture;
    public RenderTexture CameraColorTexture_Blured;
    public RenderTexture CameraColorTextureFinal;

    private string cb_Name = "CopyColorTexture";
    private string cbFinal_Name = "CopyColorTextureFinal";
    private CommandBuffer cb;
    private CommandBuffer cbFinal;
    void OnDisable()
    {
        if (CameraColorTexture != null) CameraColorTexture.Release();
        if (CameraColorTexture_Blured != null) CameraColorTexture_Blured.Release();
        if (CameraColorTextureFinal != null) CameraColorTextureFinal.Release();

    }

    public void Release()
    {
        OnDisable();
    }

    void InitializeTextures(int width, int height, bool useHDR)
    {
        if (CameraColorTexture != null) CameraColorTexture.Release();
        CameraColorTexture = new RenderTexture(width, height, 0, useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);

        if (CameraColorTexture_Blured != null) CameraColorTexture_Blured.Release();
        CameraColorTexture_Blured = new RenderTexture(width, height, 0, useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);


    }

    void InitializeFinalTextures(int width, int height, bool useHDR)
    {
        if (CameraColorTextureFinal != null) CameraColorTextureFinal.Release();
        CameraColorTextureFinal = new RenderTexture(width, height, 0, useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
        Debug.Log(CameraColorTextureFinal.format);
    }

    public void RenderColorTexture(Camera currentCamera, bool useHDR, bool isScreenSpaceWaterRendering, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers)
    {
        if (cb != null && waterSharedBuffers.ContainsKey(cb)) return;
        //print("KW_CameraColorTexture.CreatedCommand");

        if (cb == null) cb = new CommandBuffer() { name = cb_Name };
        else cb.Clear();

        var actualScreenWidth = (int)(currentCamera.pixelWidth);
        var actualScreenHeight = (int)(currentCamera.pixelHeight);

        InitializeTextures(actualScreenWidth, actualScreenHeight, useHDR);

        cb.Blit(BuiltinRenderTextureType.CurrentActive, CameraColorTexture);
        cb.SetGlobalTexture("_CameraColorTexture", CameraColorTexture);


        waterSharedBuffers.Add(cb, isScreenSpaceWaterRendering ? CameraEvent.AfterForwardAlpha : CameraEvent.BeforeForwardAlpha);
    }


    public void RenderColorTextureFinal(Camera currentCamera, bool useHDR, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers)
    {
        if (cbFinal != null && waterSharedBuffers.ContainsKey(cbFinal)) return;
        //print("KW_CameraColorTexture.CreatedCommand");

        if (cbFinal == null) cbFinal = new CommandBuffer() { name = cbFinal_Name };
        else cbFinal.Clear();

        var actualScreenWidth = (int)(currentCamera.pixelWidth);
        var actualScreenHeight = (int)(currentCamera.pixelHeight);

        InitializeFinalTextures(actualScreenWidth, actualScreenHeight, useHDR);

        cbFinal.Blit(BuiltinRenderTextureType.CurrentActive, CameraColorTextureFinal);
        cbFinal.SetGlobalTexture("_CameraColorTextureFinal", CameraColorTextureFinal);

        waterSharedBuffers.Add(cbFinal, CameraEvent.AfterImageEffects);
    }
}
