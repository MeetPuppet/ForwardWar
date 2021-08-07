using System.Collections.Generic;
using UnityEngine;

public static class KW_WaterLights
{
    public static Plane[] CameraPlanes;

    private static readonly int MaxDirLightsCount   = 3;
    private static readonly int MaxPointLightsCount = 100;
    private static readonly int MaxSpotLightsCount  = 100;

    private static readonly int KW_DirLightCount_ID     = Shader.PropertyToID("KW_DirLightCount");
    private static readonly int KW_DirLightPositions_ID = Shader.PropertyToID("KW_DirLightPositions");
    private static readonly int KW_DirLightColors_ID    = Shader.PropertyToID("KW_DirLightColors");

    private static readonly int KW_PointLightCount_ID     = Shader.PropertyToID("KW_PointLightCount");
    private static readonly int KW_PointLightPositions_ID = Shader.PropertyToID("KW_PointLightPositions");
    private static readonly int KW_PointLightColors_ID    = Shader.PropertyToID("KW_PointLightColors");
    private static readonly int KW_PointLightAttenuation_ID = Shader.PropertyToID("KW_PointLightAttenuation");

    private static readonly int KW_SpotLightCount_ID     = Shader.PropertyToID("KW_SpotLightCount");
    private static readonly int KW_SpotLightPositions_ID = Shader.PropertyToID("KW_SpotLightPositions");
    private static readonly int KW_SpotLightColors_ID    = Shader.PropertyToID("KW_SpotLightColors");

    private static List<Light> DirLights   = new List<Light>();
    private static List<Light> PointLights = new List<Light>();
    private static List<Light> SpotLights  = new List<Light>();

    private static readonly Vector4[] dirLightPositions   = new Vector4[MaxDirLightsCount];
    private static readonly Vector4[] dirLightColors      = new Vector4[MaxDirLightsCount];
    private static          float[]   dirLightShadowAtten = new float[MaxDirLightsCount];

    private static Vector4[] pointLightPositions   = new Vector4[MaxPointLightsCount];
    private static Vector4[] pointLightColors      = new Vector4[MaxPointLightsCount];
    private static float[]   pointLightShadowAtten = new float[MaxPointLightsCount];

    private static Vector4[] spotLightPositions   = new Vector4[MaxSpotLightsCount];
    private static Vector4[] spotLightColors      = new Vector4[MaxSpotLightsCount];
    private static float[]   spotLightShadowAtten = new float[MaxSpotLightsCount];

    private static Texture2D pointLightTexture;

    public static void UpdateLightParams()
    {
        if (pointLightTexture == null) GeneratePointAttenuationTexture();
        Shader.SetGlobalTexture(KW_PointLightAttenuation_ID, pointLightTexture);

        UpdateDirLightsPosColor();
        UpdatePointLightsPosColor();
        UpdateSpotLightsPosColor();

    }

    public static void Release()
    {
        if(pointLightTexture != null) KW_Extensions.SafeDestroy(pointLightTexture);
    }

    public static void AddLight(Light currentLight, LightType lightType)
    {
        switch (lightType)
        {
            case LightType.Directional:
            {
                if (!DirLights.Contains(currentLight)) DirLights.Add(currentLight);
                break;
            }
            case LightType.Point:
                PointLights.Add(currentLight);
                break;
            case LightType.Spot:
                SpotLights.Add(currentLight);
                break;
        }
    }

    public static void RemoveLight(Light currentLight, LightType lightType)
    {
        switch (lightType)
        {
            case LightType.Directional:
                if (DirLights.Contains(currentLight)) DirLights.Remove(currentLight);
                break;
            case LightType.Point:
                if (PointLights.Contains(currentLight)) PointLights.Remove(currentLight);
                break;
            case LightType.Spot:
                if (SpotLights.Contains(currentLight)) SpotLights.Remove(currentLight);
                break;
        }
    }

    public static List<Light> GetPointLights()
    {
        return PointLights;
    }

    public static List<Light> GetSpotLights()
    {
        return SpotLights;
    }

    private static void UpdateDirLightsPosColor()
    {
        for (var i = 0; i < DirLights.Count; i++)
        {
            if (i >= MaxDirLightsCount) return;

            dirLightPositions[i] = - DirLights[i].transform.forward;
            dirLightColors[i]    = DirLights[i].color * DirLights[i].intensity;
        }

        Shader.SetGlobalInt(KW_DirLightCount_ID, Mathf.Clamp(DirLights.Count, 0, MaxDirLightsCount));
        Shader.SetGlobalVectorArray(KW_DirLightPositions_ID, dirLightPositions);
        Shader.SetGlobalVectorArray(KW_DirLightColors_ID,    dirLightColors);
    }

    private static void UpdatePointLightsPosColor()
    {
        for (var i = 0; i < PointLights.Count; i++)
        {
            if (i >= MaxPointLightsCount) return;

            var pos = PointLights[i].transform.position;
            pointLightPositions[i] = new Vector4(pos.x, pos.y, pos.z, PointLights[i].range);
            pointLightColors[i]    = PointLights[i].color * PointLights[i].intensity;
        }

        Shader.SetGlobalInt(KW_PointLightCount_ID, Mathf.Clamp(PointLights.Count, 0, MaxPointLightsCount));
        Shader.SetGlobalVectorArray(KW_PointLightPositions_ID, pointLightPositions);
        Shader.SetGlobalVectorArray(KW_PointLightColors_ID,    pointLightColors);
    }

    private static void UpdateSpotLightsPosColor()
    {
        for (var i = 0; i < SpotLights.Count; i++)
        {
            if (i >= MaxSpotLightsCount) return;

            var pos = SpotLights[i].transform.position;
            spotLightPositions[i] = new Vector4(pos.x, pos.y, pos.z, SpotLights[i].range);
            spotLightColors[i]    = SpotLights[i].color * SpotLights[i].intensity;
        }

        Shader.SetGlobalInt(KW_SpotLightCount_ID, Mathf.Clamp(SpotLights.Count, 0, MaxSpotLightsCount));
        Shader.SetGlobalVectorArray(KW_SpotLightPositions_ID, spotLightPositions);
        Shader.SetGlobalVectorArray(KW_SpotLightColors_ID, spotLightColors);
    }

    static void GeneratePointAttenuationTexture()
    {
        pointLightTexture = new Texture2D(256, 1);
        pointLightTexture.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < 256; i++)
        {
            float distance = i / 256f;
            var   color    = Mathf.Clamp01(1.0f / (1.0f + 25.0f * distance * distance) * Mathf.Clamp01((1f - distance) * 5.0f));
            pointLightTexture.SetPixel(i, 0, Color.white * color);
        }
        pointLightTexture.Apply();

    }
}
