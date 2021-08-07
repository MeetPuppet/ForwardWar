using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KW_Underwater : MonoBehaviour
{
    private readonly string cb_Underwater_Name = "UnderwaterPosteffect";
    private readonly string cb_UnderwaterBlur_Name = "UnderwaterBlurPosteffect";
    const string UnderwaterShaderName = "KriptoFX/Water30/Underwater";
    private CommandBuffer cb_Underwater;
    private CommandBuffer cb_UnderwaterBlur;

    private Material underwaterMaterial;
    KW_PyramidBlur pyramidBlur;
    private RenderTexture underwaterRT;
    private RenderTexture underwaterRT_Blured;

    public void AddMaterialsToWaterRendering(List<Material> waterShaderMaterials)
    {
        if (underwaterMaterial == null) underwaterMaterial = KW_Extensions.CreateMaterial(UnderwaterShaderName);
        if (!waterShaderMaterials.Contains(underwaterMaterial)) waterShaderMaterials.Add(underwaterMaterial);
    }

    void InitializeTextures(int sizeX, int sizeY)
    {
        if(underwaterRT != null) underwaterRT.Release();
        underwaterRT = new RenderTexture(sizeX, sizeY, 0, RenderTextureFormat.ARGBHalf);

        if (underwaterRT_Blured != null) underwaterRT_Blured.Release();
        underwaterRT_Blured = new RenderTexture(sizeX, sizeY, 0, RenderTextureFormat.ARGBHalf);

    }

    public void RenderUnderwaterBlured(Camera currentCamera, float resolutionScale, float blurSize, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers)
    {
        if (cb_UnderwaterBlur != null && waterSharedBuffers.ContainsKey(cb_UnderwaterBlur)) return;
        if (cb_UnderwaterBlur == null) cb_UnderwaterBlur = new CommandBuffer() {name = cb_UnderwaterBlur_Name };
        else cb_UnderwaterBlur.Clear();

        if (cb_Underwater != null && waterSharedBuffers.ContainsKey(cb_Underwater)) return;
        if (cb_Underwater == null) cb_Underwater = new CommandBuffer() { name = cb_Underwater_Name };
        else cb_Underwater.Clear();

        var targetWidth = (int) (currentCamera.pixelWidth * resolutionScale);
        var targetHeight = (int) (currentCamera.pixelHeight * resolutionScale);


        if (underwaterRT == null || underwaterRT.width != targetWidth || underwaterRT.height != targetHeight) InitializeTextures(targetWidth, targetHeight);

        cb_UnderwaterBlur.SetRenderTarget(underwaterRT);
        cb_UnderwaterBlur.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
        cb_UnderwaterBlur.Blit(null, underwaterRT, underwaterMaterial, 0);

        if (pyramidBlur == null) pyramidBlur = new KW_PyramidBlur();

        pyramidBlur.ComputeBlurPyramid(blurSize, underwaterRT, underwaterRT_Blured, cb_UnderwaterBlur);

        Shader.SetGlobalTexture("KW_UnderwaterPostFX_Blured", underwaterRT_Blured); //bug, blit(rt, active) doesn't work after unity ctrl + s
        cb_Underwater.Blit(null, new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive), underwaterMaterial, 1);

        waterSharedBuffers.Add(cb_UnderwaterBlur, CameraEvent.AfterForwardAlpha);
        waterSharedBuffers.Add(cb_Underwater, CameraEvent.AfterForwardAlpha);
    }

    public void RenderUnderwater(Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers)
    {
        if (cb_Underwater != null && waterSharedBuffers.ContainsKey(cb_Underwater)) return;
        if (cb_Underwater == null) cb_Underwater = new CommandBuffer() { name = cb_Underwater_Name };
        else cb_Underwater.Clear();

        cb_Underwater.Blit(null, new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive), underwaterMaterial, 0);
        waterSharedBuffers.Add(cb_Underwater, CameraEvent.AfterForwardAlpha);
    }


    void OnDisable()
    {
        Release();
    }

    public void Release()
    {
        KW_Extensions.SafeDestroy(underwaterMaterial);
        if (pyramidBlur != null) pyramidBlur.Release();
        if (underwaterRT != null) underwaterRT_Blured.Release();
        if (underwaterRT_Blured != null) underwaterRT_Blured.Release();

    }
}
