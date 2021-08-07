using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class KW_AddLightToWaterRendering : MonoBehaviour
{
    private Light currentLight;
    private LightType lastLightType;
    private LightShadows lastLightShadows;
    private Color lastLightColor;
    private float lastLightRange;
    private float lastLightIntencity;
    private Vector3 lastLightPos;
    private Vector3 lastLightDir;

    private static readonly int KW_DirLightForward_ID = Shader.PropertyToID("KW_DirLightForward");
    private static readonly int KW_DirLightColor_ID = Shader.PropertyToID("KW_DirLightColor");

    private KW_WaterVolumetricLighting.VolumeLight currentVolumeLight;

    private void OnEnable()
    {
        if (currentLight == null) currentLight = GetComponent<Light>();

        if (currentVolumeLight == null) currentVolumeLight = new KW_WaterVolumetricLighting.VolumeLight();
        currentVolumeLight.Light                    = currentLight;
        currentVolumeLight.RequiredUpdate = true;
        KW_WaterVolumetricLighting.ActiveLights.Add(currentVolumeLight);
    }

    private void OnDisable()
    {
        KW_WaterVolumetricLighting.ActiveLights.Remove(currentVolumeLight);

        if (currentVolumeLight.CopyShadowMapBuffer != null) currentLight.RemoveCommandBuffer(LightEvent.AfterShadowMap, currentVolumeLight.CopyShadowMapBuffer);
        if (currentVolumeLight.LightCommandBuffer != null)
        {
            currentVolumeLight.Light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, currentVolumeLight.LightCommandBuffer);
            currentVolumeLight.Light.RemoveCommandBuffer(LightEvent.AfterShadowMap, currentVolumeLight.LightCommandBuffer);
        }
    }

    bool IsRequiredLightUpdate()
    {
        bool isRequiredUpdate = false;
        var lightT = currentLight.transform;
        if (lastLightType != currentLight.type
            || lastLightShadows != currentLight.shadows
            || lastLightColor != currentLight.color
            || Math.Abs(lastLightIntencity - currentLight.intensity) > 0.001f
            || Math.Abs(lastLightRange - currentLight.range) > 0.001f
            || lastLightPos != lightT.position
            || lastLightDir != lightT.forward)
        {
            isRequiredUpdate = true;
        }

        lastLightType = currentLight.type;
        lastLightShadows = currentLight.shadows;
        lastLightColor = currentLight.color;
        lastLightIntencity = currentLight.intensity;
        lastLightRange = currentLight.range;
        lastLightPos = lightT.position;
        lastLightDir = lightT.forward;

        return isRequiredUpdate;
    }

    private void Update()
    {
        if (currentLight.type == LightType.Spot) ComputeSpotLightShadowMatrix();
        if (currentLight.type == LightType.Directional) UpdateDirLight();
        if (IsRequiredLightUpdate()) currentVolumeLight.RequiredUpdate = true;


    }

    void UpdateDirLight()
    {
        Shader.SetGlobalVector(KW_DirLightForward_ID,  -currentLight.transform.forward);
        Shader.SetGlobalVector(KW_DirLightColor_ID, currentLight.color * currentLight.intensity);
    }

    private void ComputeSpotLightShadowMatrix()
    {
        var       clip = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
        Matrix4x4 proj;
        if (SystemInfo.usesReversedZBuffer)
            proj = Matrix4x4.Perspective(currentLight.spotAngle, 1, currentLight.range, currentLight.shadowNearPlane);
        else
            proj = Matrix4x4.Perspective(currentLight.spotAngle, 1, currentLight.shadowNearPlane, currentLight.range);

        var m = clip * proj;
        m[0, 2] *= -1;
        m[1, 2] *= -1;
        m[2, 2] *= -1;
        m[3, 2] *= -1;

        var view = Matrix4x4.TRS(currentLight.transform.position, currentLight.transform.rotation, Vector3.one).inverse;

        currentVolumeLight.SpotMatrix = m * view;
    }
}
