using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KW_ScreenSpacePlanarReflection : MonoBehaviour
{
    private CommandBuffer cb;
    private string cb_Name = "ScreenSpacePlanarReflection";
    private const string hashProjectionShaderName = "Hidden/KriptoFX/Water/SSPR_Projection";
    private const string hashReflectionShaderName = "Hidden/KriptoFX/Water/SSPR_Reflection";
    private const string keyword_KW_USE_COLOR_AFTER_EVERYTHING = "KW_USE_COLOR_AFTER_EVERYTHING";
    private const string keyworld_KW_USE_HDR_COLOR = "KW_USE_HDR_COLOR";

    private Material hashProjectionMat;
    private Material hashReflectionMat;
    private RenderTexture reflectionRT;
    private RenderTexture reflectionHash;
    private RenderTexture reflectionProjection;

    void OnDisable()
    {
        KW_Extensions.SafeDestroy(hashProjectionMat);
        KW_Extensions.SafeDestroy(hashReflectionMat);
        if (reflectionRT != null) reflectionRT.Release();
        if (reflectionHash != null) reflectionHash.Release();
        if (reflectionProjection != null) reflectionProjection.Release();
     //   print("ScreenSpacePlanarReflection.Released");
    }

    public void Release()
    {
        OnDisable();
    }

    public void AddMaterialsToWaterRendering(List<Material> waterShaderMaterials)
    {
        if (hashProjectionMat == null) hashProjectionMat = KW_Extensions.CreateMaterial(hashProjectionShaderName);
        if (!waterShaderMaterials.Contains(hashProjectionMat)) waterShaderMaterials.Add(hashProjectionMat);

        if (hashReflectionMat == null) hashReflectionMat = KW_Extensions.CreateMaterial(hashReflectionShaderName);
        if (!waterShaderMaterials.Contains(hashReflectionMat)) waterShaderMaterials.Add(hashReflectionMat);
    }

    void InitializeTextures(int width, int height)
    {
        if (reflectionRT != null) reflectionRT.Release();
        reflectionRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
        //reflectionRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBHalf);
        //if (!reflectionRT.useMipMap && reflectionRT.IsCreated()) //bug, new rendetTexture instead of GetTemporary doesn't clear properly
        //{
        //    reflectionRT.Release();
        //    reflectionRT.useMipMap = true;
        //    reflectionRT.Create();
        //}
        reflectionRT.useMipMap = true;
        reflectionRT.name = "KW_ReflectionRT";
        reflectionRT.wrapMode = TextureWrapMode.Clamp;

        if (reflectionHash != null) reflectionHash.Release();
        reflectionHash = new RenderTexture(width, height, 0, RenderTextureFormat.RInt, RenderTextureReadWrite.Linear);
        reflectionHash.enableRandomWrite = true;

        reflectionHash.Create();

        if (reflectionProjection != null) reflectionProjection.Release();
        reflectionProjection = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);

    }

    public void RenderReflection(Camera currentCamera, float resolution, float depthHolesFillDistance, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers)
    {
        if (cb != null && waterSharedBuffers.ContainsKey(cb)) return;
        //print("ScreenSpacePlanarReflection.CreatedCommand");

        if (cb == null) cb = new CommandBuffer() { name = cb_Name };
        else cb.Clear();

        var actualScreenWidth = (int)(currentCamera.pixelWidth * resolution);
        var actualScreenHeight = (int)(currentCamera.pixelHeight * resolution);
        if(reflectionRT == null || actualScreenWidth != reflectionRT.width || actualScreenHeight != reflectionRT.height) InitializeTextures(actualScreenWidth, actualScreenHeight);

        cb.SetGlobalVector("KW_SSPR_ScreenSize", new Vector2(actualScreenWidth, actualScreenHeight));

        cb.SetRenderTarget(reflectionHash);
        cb.ClearRenderTarget(false, true, new Color(-100000, 0, 0));
        cb.SetRandomWriteTarget(1, reflectionHash);


        cb.SetRenderTarget(reflectionProjection);
        cb.Blit(null, reflectionProjection, hashProjectionMat);
        cb.SetGlobalTexture("HashData", reflectionHash);
        cb.SetGlobalTexture("KW_ScreenSpaceReflectionDist", reflectionProjection);
        cb.ClearRandomWriteTargets();

        int upsideDownFix = 1;
        if (currentCamera.allowMSAA && QualitySettings.antiAliasing != 0 || currentCamera.allowHDR) upsideDownFix = -1;

        int upsideDownImageEffectsFix = -1;
        //if (reflectImageEffects && isGameView) upsideDownImageEffectsFix = 1;

        cb.SetRenderTarget(reflectionRT);
        cb.SetGlobalFloat("_UpsideDownFix", upsideDownFix);
        cb.SetGlobalFloat("_UpsideDownImageEffectsFix", upsideDownImageEffectsFix);
        cb.SetGlobalFloat("KW_DepthHolesFillDistance", depthHolesFillDistance);

        cb.Blit(null, reflectionRT, hashReflectionMat);
        cb.SetGlobalTexture("KW_ScreenSpaceReflectionTex", reflectionRT);

        waterSharedBuffers.Add(cb, CameraEvent.BeforeForwardAlpha);
    }

}
