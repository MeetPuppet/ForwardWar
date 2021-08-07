using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KW_DepthRendering : MonoBehaviour
{
    KW_PyramidBlur pyramidBlurMask;
    KW_PyramidBlur pyramidBlurDepth;
    private Material maskDepthNormalMaterial;
    private Material blitToDepthMaterial;

    private const string maskDepthNormal_ShaderName = "Hidden/KriptoFX/Water/KW_MaskDepthNormalPass";
    private const string BlitToDepthShaderName = "Hidden/KriptoFX/Water/BlitToDepthTexture";

    private const string cb_WaterHalfLine_Name = "KW_RenderHalfWaterMask";
    private const string cb_CopyDepth_Name = "KW_CopyDepthBeforeWaterRendering";
    private const string cb_WaterDepth_Name = "KW_RenderWaterToDepthTexture";

    private CommandBuffer cb_CopyDepth;
    private CommandBuffer cb_DrawWaterDepth;
    private CommandBuffer cb_WaterHalfLine;

    private RenderTexture waterDepthRT;
    private RenderTexture waterMaskRT;
    private RenderTexture waterMaskRT_Blured;

    private RenderTexture sceneDepthWithoutWaterRT;
    private RenderTexture sceneDepthWithoutWaterRT_Blured;

    private Mesh lastMesh;

    //private int KW_WaterMask_RT = Shader.PropertyToID("KW_WaterMaskRT");
    //private int KW_WaterDepth_RT = Shader.PropertyToID("KW_WaterDepthRT");

    private int KW_WaterMaskScatterNormals_ID = Shader.PropertyToID("KW_WaterMaskScatterNormals");
    private int KW_WaterDepth_ID = Shader.PropertyToID("KW_WaterDepth");
    private int KW_WaterMaskScatterNormals_Blured_ID = Shader.PropertyToID("KW_WaterMaskScatterNormals_Blured");
    private int CopyDepthBeforeWaterRenderingTexture_ID = Shader.PropertyToID("CopyDepthBeforeWaterRenderingTexture");

    public void Release()
    {
        if (pyramidBlurMask != null) pyramidBlurMask.Release();
        if (pyramidBlurDepth != null) pyramidBlurDepth.Release();
        if (maskDepthNormalMaterial != null) KW_Extensions.SafeDestroy(maskDepthNormalMaterial);
        if (blitToDepthMaterial != null) KW_Extensions.SafeDestroy(blitToDepthMaterial);

        if (waterDepthRT != null) waterDepthRT.Release();
        if (waterMaskRT != null) waterMaskRT.Release();
        if (waterMaskRT_Blured != null) waterMaskRT_Blured.Release();

        if (sceneDepthWithoutWaterRT != null) sceneDepthWithoutWaterRT.Release();
        if (sceneDepthWithoutWaterRT_Blured != null) sceneDepthWithoutWaterRT_Blured.Release();

    }

    void OnDisable()
    {
        Release();
    }

    public void AddMaterialsToWaterRendering(List<Material> waterShaderMaterials)
    {
        if (maskDepthNormalMaterial == null) maskDepthNormalMaterial = KW_Extensions.CreateMaterial(maskDepthNormal_ShaderName);
        if (!waterShaderMaterials.Contains(maskDepthNormalMaterial)) waterShaderMaterials.Add(maskDepthNormalMaterial);
    }

    void InitializeMaskTextures(int sizeX, int sizeY)
    {
        if (waterDepthRT != null) waterDepthRT.Release();
        waterDepthRT = new RenderTexture(sizeX, sizeY, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
      //  waterDepthRT.name = "waterDepthRT";

        if (waterMaskRT != null) waterMaskRT.Release();
        waterMaskRT = new RenderTexture(sizeX, sizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
       // waterMaskRT.name = "waterMaskRT";

        if (waterMaskRT_Blured != null) waterMaskRT_Blured.Release();
        waterMaskRT_Blured = new RenderTexture((int)(sizeX), (int)(sizeY), 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
       // waterMaskRT_Blured.name = "waterMaskRT_Blured";
    }

    void InitializeSceneDepthTextures(int sizeX, int sizeY)
    {
        if (sceneDepthWithoutWaterRT != null) sceneDepthWithoutWaterRT.Release();
        sceneDepthWithoutWaterRT = new RenderTexture(sizeX, sizeY, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        //sceneDepthWithoutWaterRT.name = "sceneDepthWithoutWaterRT";

        if (sceneDepthWithoutWaterRT_Blured != null) sceneDepthWithoutWaterRT_Blured.Release();
        sceneDepthWithoutWaterRT_Blured = new RenderTexture((int)(sizeX * 0.75), (int)(sizeY * 0.75), 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        //sceneDepthWithoutWaterRT_Blured.name = "sceneDepthWithoutWaterRT_Blured";
    }

    public void RenderWaterMaskDepthNormal(Camera currentCamera, float resolutionScale,  Mesh mesh, Matrix4x4 worldMatrix, bool useTesselation, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers, bool forceUpdate)
    {
        if (forceUpdate && cb_WaterHalfLine != null && waterSharedBuffers.ContainsKey(cb_WaterHalfLine)) waterSharedBuffers.Remove(cb_WaterHalfLine);

        if (cb_WaterHalfLine != null && waterSharedBuffers.ContainsKey(cb_WaterHalfLine)) return;

        //print("KW_DepthRendering.CreatedCommand_Mask");
        var camEvent = currentCamera.actualRenderingPath == RenderingPath.Forward ? CameraEvent.BeforeDepthTexture : CameraEvent.BeforeGBuffer;
        if (cb_WaterHalfLine == null) cb_WaterHalfLine = new CommandBuffer() {name = cb_WaterHalfLine_Name};
        else cb_WaterHalfLine.Clear();


        var sizeX = (int) (currentCamera.pixelWidth * resolutionScale);
        var sizeY = (int) (currentCamera.pixelHeight * resolutionScale);
        InitializeMaskTextures(sizeX, sizeY);

        //cb_WaterHalfLine.GetTemporaryRT(maskRT, sizeX, sizeY, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);//bug, unity can't set the tempRT properly as target. In some time it's null =/

        cb_WaterHalfLine.SetGlobalTexture(KW_WaterDepth_ID, waterDepthRT);
        cb_WaterHalfLine.SetGlobalTexture(KW_WaterMaskScatterNormals_ID, waterMaskRT);

        cb_WaterHalfLine.SetRenderTarget(waterMaskRT, waterDepthRT);
        cb_WaterHalfLine.ClearRenderTarget(true, true, Color.black);

        var shaderPass = useTesselation && SystemInfo.graphicsShaderLevel >= 46 ? 0 : 1;
        cb_WaterHalfLine.DrawMesh(mesh, worldMatrix, maskDepthNormalMaterial, 0, shaderPass);


        if (pyramidBlurMask == null) pyramidBlurMask = new KW_PyramidBlur();
        pyramidBlurMask.ComputeBlurPyramid(3.0f, waterMaskRT, waterMaskRT_Blured, cb_WaterHalfLine);
        cb_WaterHalfLine.SetGlobalTexture(KW_WaterMaskScatterNormals_Blured_ID, waterMaskRT_Blured);

        waterSharedBuffers.Add(cb_WaterHalfLine, camEvent);
    }



    public void RenderDepthCopy(Camera currentCamera, float resolutionScale, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers)
    {
        if (cb_CopyDepth != null && waterSharedBuffers.ContainsKey(cb_CopyDepth)) return;

        //print("KW_DepthRendering.CreatedCommand_DepthCopy");
        if (currentCamera.renderingPath == RenderingPath.Forward && currentCamera.depthTextureMode != DepthTextureMode.Depth) currentCamera.depthTextureMode = DepthTextureMode.Depth;
        var camEvent = currentCamera.actualRenderingPath == RenderingPath.Forward ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting;
        RenderTargetIdentifier depthID = (currentCamera.actualRenderingPath == RenderingPath.Forward)
            ? new RenderTargetIdentifier(BuiltinRenderTextureType.Depth)
            : new RenderTargetIdentifier(BuiltinRenderTextureType.ResolvedDepth);

        if (cb_CopyDepth == null) cb_CopyDepth = new CommandBuffer() {name = cb_CopyDepth_Name};
        else cb_CopyDepth.Clear();

        var sizeX = (int) (currentCamera.pixelWidth * resolutionScale);
        var sizeY = (int) (currentCamera.pixelHeight * resolutionScale);
        InitializeSceneDepthTextures(sizeX, sizeY);

        cb_CopyDepth.Blit(depthID, sceneDepthWithoutWaterRT);

        if(pyramidBlurDepth == null) pyramidBlurDepth = new KW_PyramidBlur();
        pyramidBlurDepth.ComputeBlurPyramid(2, sceneDepthWithoutWaterRT, sceneDepthWithoutWaterRT_Blured, cb_CopyDepth);
        cb_CopyDepth.SetGlobalTexture(Shader.PropertyToID("_CameraDepthTextureBeforeWaterZWrite_Blured"), sceneDepthWithoutWaterRT_Blured);
        waterSharedBuffers.Add(cb_CopyDepth, camEvent);
    }

    public void RenderWaterToPosteffectDepth(Camera currentCamera, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers)
    {
        if (cb_DrawWaterDepth != null && waterSharedBuffers.ContainsKey(cb_DrawWaterDepth)) return;

        //print("KW_DepthRendering.CreatedCommand_cb_DrawWaterDepth");
        //if (currentCamera.renderingPath == RenderingPath.Forward && currentCamera.depthTextureMode != DepthTextureMode.Depth) currentCamera.depthTextureMode = DepthTextureMode.Depth;
        //var camEvent = currentCamera.actualRenderingPath == RenderingPath.Forward ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting;
        RenderTargetIdentifier depthID = (currentCamera.actualRenderingPath == RenderingPath.Forward)
            ? new RenderTargetIdentifier(BuiltinRenderTextureType.Depth)
            : new RenderTargetIdentifier(BuiltinRenderTextureType.ResolvedDepth);

        if (cb_DrawWaterDepth == null) cb_DrawWaterDepth = new CommandBuffer() { name = cb_WaterDepth_Name };
        else cb_DrawWaterDepth.Clear();

        if (blitToDepthMaterial == null) blitToDepthMaterial = KW_Extensions.CreateMaterial(BlitToDepthShaderName);

        var tempRT = Shader.PropertyToID("TempDepthCopy");
        cb_DrawWaterDepth.GetTemporaryRT(tempRT, -1, -1, 32);
        cb_DrawWaterDepth.Blit(depthID, tempRT);
        //cb_DrawWaterDepth.SetGlobalTexture("SceneDepth");
        cb_DrawWaterDepth.Blit(tempRT, depthID, blitToDepthMaterial);

        cb_DrawWaterDepth.ReleaseTemporaryRT(tempRT);
        waterSharedBuffers.Add(cb_DrawWaterDepth, CameraEvent.AfterForwardAlpha);
        //renderdCameras.Add(currentCamera);
    }



}
