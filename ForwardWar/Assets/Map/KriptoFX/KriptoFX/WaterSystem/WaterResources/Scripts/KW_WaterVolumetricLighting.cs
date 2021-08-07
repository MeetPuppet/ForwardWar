using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class KW_WaterVolumetricLighting : MonoBehaviour
{
    public static List<VolumeLight> ActiveLights = new List<VolumeLight>();


    private const string cb_ClearPass_Name          = "WaterVolumetricLight_ClearPass";
    private const string cb_BlurPass_Name           = "WaterVolumetricLight_FinalBlur";
    private const string cb_LightPass_Name = "WaterVolumetricLightPass";
    private const string cb_CopyDirShadowMap_Name = "WaterVolumetricLight_CopyShadowmap";

    private const string VolumeShaderName = "Hidden/KriptoFX/Water/VolumetricLighting";

    private const string Keyword_SHADOWS_DEPTH = "SHADOWS_DEPTH";
    private const string Keyword_DIRECTIONAL   = "DIRECTIONAL";
    private const string Keyword_POINT         = "POINT";
    private const string Keyword_SPOT          = "SPOT";

    private const string cb_CopyDirShadowMap_TextureName = "_DirShadowMapTexture";
    private const string cb_CopyPointShadowMap_TextureName = "_PointShadowMapTexture";
    private const string cb_CopySpotShadowMap_TextureName = "_SpotShadowMapTexture";

    private KW_PyramidBlur pyramidBlur = new KW_PyramidBlur();
    private Material volumeLightMat;

    private CommandBuffer cb_BlurPass;
    private CommandBuffer cb_ClearPass;

    private  Vector4[] frustum  = new Vector4[4];
    private  Vector4[] uv_World = new Vector4[4];

    public Texture2D     spotLightTexture;
    public RenderTexture volumeLightLastRT;
    public RenderTexture volumeLightTextureFinal;
    public RenderTexture VolumeLightTextureRaw;
    public Texture2D     ditheringTexture;
    private bool          isInitialized;


    private void Initialize()
    {
        GenerateDitherTexture();
        GenerateSpotLightTexture();
        isInitialized = true;
    }

    public void Release()
    {
        if (VolumeLightTextureRaw != null) VolumeLightTextureRaw.Release();
        if (volumeLightTextureFinal != null) volumeLightTextureFinal.Release();
        if (volumeLightLastRT != null) volumeLightLastRT.Release();

        if (pyramidBlur != null) pyramidBlur.Release();
        KW_Extensions.SafeDestroy(volumeLightMat);

        KW_Extensions.SafeDestroy(ditheringTexture);
        KW_Extensions.SafeDestroy(spotLightTexture);

        isInitialized = false;

        foreach (var activeLight in ActiveLights)
        {
            RemoveCommnadBuffersFromLight(activeLight);
        }
    }

    void OnDisable()
    {
        //print("VolumetricLight.Disabled");
        Release();
    }

    public void AddMaterialsToWaterRendering(List<Material> waterShaderMaterials)
    {
        if (volumeLightMat == null) volumeLightMat = KW_Extensions.CreateMaterial(VolumeShaderName);
        if (!waterShaderMaterials.Contains(volumeLightMat)) waterShaderMaterials.Add(volumeLightMat);
    }

    private void InitializeVolumetricTexture(int width, int height)
    {
        if (VolumeLightTextureRaw != null) VolumeLightTextureRaw.Release();
         VolumeLightTextureRaw = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
       // VolumeLightTextureRaw.name = "VolumeLightTextureRaw";
        VolumeLightTextureRaw.filterMode = FilterMode.Bilinear;

        if (volumeLightTextureFinal != null) volumeLightTextureFinal.Release();
         volumeLightTextureFinal = new RenderTexture(VolumeLightTextureRaw.width, VolumeLightTextureRaw.height, 0, VolumeLightTextureRaw.format, RenderTextureReadWrite.Linear);
        //volumeLightTextureFinal.name = "VolumeLightTexture";
        volumeLightTextureFinal.filterMode = FilterMode.Bilinear;

        if (volumeLightLastRT != null) volumeLightLastRT.Release();
        volumeLightLastRT = new RenderTexture(VolumeLightTextureRaw.width, VolumeLightTextureRaw.height, 0, VolumeLightTextureRaw.format, RenderTextureReadWrite.Linear);
       // volumeLightLastRT.name = "volumeLightLastRT";
        volumeLightLastRT.filterMode = FilterMode.Bilinear;

    }

    public void RenderVolumeLights(Camera currentCamera,  float resolutionScale, float maxDistance, int steps, float blurSize,
        Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers, float test)
    {

        if (!isInitialized) Initialize();
        //if (currentCamera.renderingPath == RenderingPath.Forward) currentCamera.depthTextureMode = DepthTextureMode.Depth;

        var targetWidth = (int)(currentCamera.pixelWidth * resolutionScale);
        var targetHeight = (int)(currentCamera.pixelHeight * resolutionScale);

        if (VolumeLightTextureRaw == null || volumeLightTextureFinal == null || VolumeLightTextureRaw.width != targetWidth || VolumeLightTextureRaw.height != targetHeight)
        {
            InitializeVolumetricTexture(targetWidth, targetHeight);
        }

        SetMaterialParams(currentCamera, maxDistance, steps, blurSize);

        if (cb_BlurPass == null || !waterSharedBuffers.ContainsKey(cb_BlurPass))
        {
            //print("VolumetricLighting.CreatedCommand_FinalBlur");
            if (cb_BlurPass == null) cb_BlurPass = new CommandBuffer() { name = cb_BlurPass_Name };
            else cb_BlurPass.Clear();
            foreach (var activeLight in ActiveLights) activeLight.RequiredUpdate = true;

            pyramidBlur.ComputeBlurPyramid(blurSize, VolumeLightTextureRaw, volumeLightTextureFinal, cb_BlurPass);
            //Shader.SetGlobalTexture("KW_VolumetricLight", volumeLightTextureFinal);
            cb_BlurPass.SetGlobalTexture("KW_VolumetricLight", volumeLightTextureFinal);

            if (cb_ClearPass == null) cb_ClearPass = new CommandBuffer() { name = cb_ClearPass_Name };
            else cb_ClearPass.Clear();

            var activeRT = RenderTexture.active;
            cb_ClearPass.SetRenderTarget(volumeLightTextureFinal);
            cb_ClearPass.ClearRenderTarget(false, true, new Color(0, 0, 0, 0));

            cb_ClearPass.SetRenderTarget(VolumeLightTextureRaw);
            cb_ClearPass.ClearRenderTarget(false, true, new Color(0, 0, 0, 0));

            cb_ClearPass.SetRenderTarget(volumeLightLastRT);
            cb_ClearPass.ClearRenderTarget(false, true, new Color(0, 0, 0, 0));

            cb_ClearPass.SetRenderTarget(activeRT);

            waterSharedBuffers.Add(cb_BlurPass, CameraEvent.BeforeForwardAlpha);
            waterSharedBuffers.Add(cb_ClearPass, currentCamera.renderingPath == RenderingPath.Forward ? CameraEvent.BeforeDepthTexture : CameraEvent.BeforeGBuffer);
        }

        foreach (var activeLight in ActiveLights)
        {
            UpdateLightBuffer(currentCamera, activeLight, waterSharedBuffers);
        }
    }


    private void UpdateLightBuffer(Camera currentCamera, VolumeLight volumeLight, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers)
    {

        if (!volumeLight.RequiredUpdate) return;

        var currentLight = volumeLight.Light;

        var cb_light = volumeLight.LightCommandBuffer;
        volumeLight.LightCommandBuffer.Clear();

        var anisoMie = Vector4.zero;
        if (currentLight.type == LightType.Directional)
        {
            cb_light.SetGlobalVector("KW_LightDir", currentLight.transform.forward);
            cb_light.SetGlobalVector("KW_LightDirColor", currentLight.color * currentLight.intensity);

            var ambientColor = SampleLightProbesUp(transform.position + Vector3.up * 10, 0.85f);
            Shader.SetGlobalColor("KW_AmbientColor", ambientColor);

            volumeLight.CopyShadowMapBuffer.Clear();
            volumeLight.CopyShadowMapBuffer.SetGlobalTexture(cb_CopyDirShadowMap_TextureName, new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive));
            anisoMie = ComputeMieVector(0.25f);
        }
        if (volumeLight.Light.type == LightType.Point)
        {
            anisoMie = ComputeMieVector(-0.75f);
            if (volumeLight.Light.shadows != LightShadows.None)
            {
                cb_light.SetGlobalTexture(cb_CopyPointShadowMap_TextureName, new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive));
            }
        }
        if (currentLight.type == LightType.Spot)
        {
            anisoMie = ComputeMieVector(-0.5f);
            cb_light.SetGlobalMatrix("KW_SpotWorldToShadow", volumeLight.SpotMatrix);
            cb_light.SetGlobalTexture(cb_CopySpotShadowMap_TextureName, new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive));
        }

        var pos = currentLight.transform.position;
        cb_light.SetGlobalVector("KW_LightPos", new Vector4(pos.x, pos.y, pos.z, currentLight.range));
        cb_light.SetGlobalVector("KW_LightColor", currentLight.color * currentLight.intensity);
        cb_light.SetGlobalVector("KW_LightAnisotropy", anisoMie);

        UpdateKeywords(volumeLight);

        cb_light.Blit(VolumeLightTextureRaw, volumeLightLastRT);
        cb_light.Blit(volumeLightLastRT, VolumeLightTextureRaw, volumeLightMat, 0);

        RemoveCommnadBuffersFromLight(volumeLight);

        // if (currentLight.lightmapBakeType != LightmapBakeType.Baked && currentLight.shadows != LightShadows.None) //todo unity bug with LightmapBakeType in build

        if (currentLight.shadows != LightShadows.None)
        {
            switch (currentLight.type)
            {
                case LightType.Directional:
                {
                    currentLight.AddCommandBuffer(LightEvent.AfterShadowMap, volumeLight.CopyShadowMapBuffer);
                    currentLight.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, cb_light);
                    break;
                }
                case LightType.Point:
                    currentLight.AddCommandBuffer(LightEvent.AfterShadowMap, cb_light);
                    break;
                case LightType.Spot:
                    currentLight.AddCommandBuffer(LightEvent.AfterShadowMap, cb_light);
                    break;
            }
            if (waterSharedBuffers.ContainsKey(cb_light)) waterSharedBuffers.Remove(cb_light);
        }
        else
        {
            if (!waterSharedBuffers.ContainsKey(cb_light)) waterSharedBuffers.Add(cb_light, currentCamera.renderingPath == RenderingPath.Forward ? CameraEvent.AfterForwardOpaque : CameraEvent.AfterGBuffer);
            //print("VolumetricLighting.CreatedCommand_VolumeLight");
        }

        volumeLight.RequiredUpdate = false;
    }

    void RemoveCommnadBuffersFromLight(VolumeLight volumeLight)
    {
        if (volumeLight.CopyShadowMapBuffer != null) volumeLight.Light.RemoveCommandBuffer(LightEvent.AfterShadowMap, volumeLight.CopyShadowMapBuffer);
        if (volumeLight.LightCommandBuffer != null)
        {
            volumeLight.Light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, volumeLight.LightCommandBuffer);
            volumeLight.Light.RemoveCommandBuffer(LightEvent.AfterShadowMap, volumeLight.LightCommandBuffer);
        }
    }

    private void UpdateKeywords(VolumeLight volumeLight)
    {
        if (volumeLight.Light.shadows != LightShadows.None) volumeLight.LightCommandBuffer.EnableShaderKeyword(Keyword_SHADOWS_DEPTH);
        else volumeLight.LightCommandBuffer.DisableShaderKeyword(Keyword_SHADOWS_DEPTH);

        if (volumeLight.Light.type == LightType.Directional) volumeLight.LightCommandBuffer.EnableShaderKeyword(Keyword_DIRECTIONAL);
        else volumeLight.LightCommandBuffer.DisableShaderKeyword(Keyword_DIRECTIONAL);

        if (volumeLight.Light.type == LightType.Point) volumeLight.LightCommandBuffer.EnableShaderKeyword(Keyword_POINT);
        else volumeLight.LightCommandBuffer.DisableShaderKeyword(Keyword_POINT);

        if (volumeLight.Light.type == LightType.Spot) volumeLight.LightCommandBuffer.EnableShaderKeyword(Keyword_SPOT);
        else volumeLight.LightCommandBuffer.DisableShaderKeyword(Keyword_SPOT);
    }



    private void SetMaterialParams(Camera cam, float maxDistance, int steps, float blurRadius)
    {
        //Shader.SetGlobalTexture("KW_VolumetricLight", volumeLightTextureFinal);
        Shader.SetGlobalTexture("KW_DitherTexture",   ditheringTexture);
        Shader.SetGlobalTexture("KW_SpotLightTex",    spotLightTexture);

        volumeLightMat.SetFloat("KW_VolumeLightMaxDistance", maxDistance);
        volumeLightMat.SetFloat("KW_RayMarchSteps", steps);
        volumeLightMat.SetFloat("KW_VolumeLightBlurRadius", blurRadius);
        volumeLightMat.SetVector("KW_DitherSceenScale", new Vector2(VolumeLightTextureRaw.width, VolumeLightTextureRaw.height));
        //volumeLightMat.SetFloat("KW_WaterYPos", waterLevel);

        frustum[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.farClipPlane));
        frustum[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.farClipPlane));
        frustum[2] = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.farClipPlane));
        frustum[3] = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.farClipPlane));
        volumeLightMat.SetVectorArray("KW_Frustum", frustum);

        uv_World[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        uv_World[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane));
        uv_World[2] = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));
        uv_World[3] = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        volumeLightMat.SetVectorArray("KW_UV_World", uv_World);


        //var projToView = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true).inverse;
        //projToView[1, 1] *= -1;

        //volumeLightMat.SetMatrix("KW_ViewToWorld", cam.cameraToWorldMatrix);
        //volumeLightMat.SetMatrix("KW_ProjToView",  projToView);
    }

    Vector4 ComputeMieVector(float MieG)
    {
       return new Vector4(1 - (MieG * MieG), 1 + (MieG * MieG), 2 * MieG, 1.0f / (4.0f * Mathf.PI));
    }

    private void GenerateDitherTexture()
    {
        if (ditheringTexture != null) return;

        var size = 8;

        ditheringTexture            = new Texture2D(size, size, TextureFormat.Alpha8, false, true);
        ditheringTexture.filterMode = FilterMode.Point;
        var c = new Color32[size * size];

        byte b;

        var i = 0;
        b      = (byte) (1.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (49.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (13.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (61.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (4.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (52.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (16.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (64.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);

        b      = (byte) (33.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (17.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (45.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (29.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (36.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (20.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (48.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (32.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);

        b      = (byte) (9.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (57.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (5.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (53.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (12.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (60.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (8.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (56.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);

        b      = (byte) (41.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (25.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (37.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (21.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (44.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (28.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (40.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (24.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);

        b      = (byte) (3.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (51.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (15.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (63.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (2.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (50.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (14.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (62.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);

        b      = (byte) (35.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (19.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (47.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (31.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (34.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (18.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (46.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (30.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);

        b      = (byte) (11.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (59.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (7.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (55.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (10.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (58.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (6.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (54.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);

        b      = (byte) (43.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (27.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (39.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (23.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (42.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (26.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (38.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);
        b      = (byte) (22.0f / 65.0f * 255);
        c[i++] = new Color32(b, b, b, b);

        ditheringTexture.SetPixels32(c);
        ditheringTexture.Apply();
    }

    private void GenerateSpotLightTexture()
    {
        if (spotLightTexture != null) return;

        var texSize = 128;
        spotLightTexture          = new Texture2D(texSize, texSize, TextureFormat.ARGB32, true);
        spotLightTexture.wrapMode = TextureWrapMode.Clamp;

        var colors = new Color[texSize * texSize];

        for (var i = 0; i < texSize; i++)
        for (var j = 0; j < texSize; j++)
        {
            var dist = Vector2.Distance(new Vector2(i, j), new Vector2(texSize / 2f, texSize / 2f)) / texSize * 2f;
            colors[i * spotLightTexture.height + j] = new Color(1 - dist, 0, 0, 0);
        }

        spotLightTexture.SetPixels(colors);

        spotLightTexture.Apply();
    }

    public class VolumeLight
    {
        public CommandBuffer LightCommandBuffer = new CommandBuffer() { name = cb_LightPass_Name };
        public CommandBuffer CopyShadowMapBuffer = new CommandBuffer() { name = cb_CopyDirShadowMap_Name };
        public Light         Light;
        public Matrix4x4     SpotMatrix;
        public bool RequiredUpdate;
    }

    public Color SampleLightProbesUp(Vector3 pos, float grayScaleFactor)
    {
        SphericalHarmonicsL2 sh;
        LightProbes.GetInterpolatedProbe(pos, null, out sh);

        var unity_SHAr = new Vector4(sh[0, 3], sh[0, 1], sh[0, 2], sh[0, 0] - sh[0, 6]);
        var unity_SHAg = new Vector4(sh[1, 3], sh[1, 1], sh[1, 2], sh[1, 0] - sh[1, 6]);
        var unity_SHAb = new Vector4(sh[2, 3], sh[2, 1], sh[2, 2], sh[2, 0] - sh[2, 6]);

        var unity_SHBr = new Vector4(sh[0, 4], sh[0, 6], sh[0, 5] * 3, sh[0, 7]);
        var unity_SHBg = new Vector4(sh[1, 4], sh[1, 6], sh[1, 5] * 3, sh[1, 7]);
        var unity_SHBb = new Vector4(sh[2, 4], sh[2, 6], sh[2, 5] * 3, sh[2, 7]);

        var unity_SHC = new Vector3(sh[0, 8], sh[2, 8], sh[1, 8]);

        var norm = new Vector4(0, 1, 0, 1);

        Color colorLinear = Color.black;
        colorLinear.r = Vector4.Dot(unity_SHAr, norm);
        colorLinear.g = Vector4.Dot(unity_SHAg, norm);
        colorLinear.b = Vector4.Dot(unity_SHAb, norm);

        // half4 vB = normal.xyzz * normal.yzzx;
        var normB = new Vector4(norm.x * norm.y, norm.y * norm.z, norm.z * norm.z, norm.z * norm.x);
        Color colorQuad = Color.black;
        colorQuad.r = Vector4.Dot(unity_SHBr, normB);
        colorQuad.g = Vector4.Dot(unity_SHBg, normB);
        colorQuad.b = Vector4.Dot(unity_SHBb, normB);

        float vC = norm.x * norm.x - norm.y * norm.y;
        var finalQuad = unity_SHC * vC;
        Color colorFinalQuad = new Color(finalQuad.x, finalQuad.y, finalQuad.z);
        Color finalColor = colorLinear + colorQuad + colorFinalQuad;
        var grayColor = finalColor.r * 0.33f + finalColor.g * 0.33f + finalColor.b * 0.33f;
        finalColor = Color.Lerp(finalColor, Color.white * grayColor, grayScaleFactor);

        if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
            return (colorLinear + colorQuad + colorFinalQuad).gamma;
        else return finalColor;
    }

}
