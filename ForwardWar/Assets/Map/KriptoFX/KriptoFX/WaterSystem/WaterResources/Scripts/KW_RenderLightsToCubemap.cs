using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KW_RenderLightsToCubemap : MonoBehaviour
{
   // private ReflectionProbe reflectionProbe;
    private Camera cam;
    private GameObject camObj;
    private RenderTexture lightCubemap;
    //private GameObject reflectionProbeGO;
    Dictionary<Light, GameObject> lightsGO = new Dictionary<Light, GameObject>();

    private const float HeightOffset = 23000f;

    const string cubemapLight_ShaderName = "Hidden/KriptoFX/Water/CubemapLight";
    private Material cubeLightMaterial;
    MaterialPropertyBlock propertyBlock;

    // Start is called before the first frame update
    void Initialize()
    {
        propertyBlock = new MaterialPropertyBlock();
        if (cam == null)
        {
            camObj = new GameObject("CubemapLightsCamera");
            camObj.transform.parent = transform;
            cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;

            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.enabled = false;
        }

        if (!lightCubemap)
        {
            lightCubemap = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGBHalf);
            lightCubemap.dimension = TextureDimension.Cube;
            lightCubemap.useMipMap = true;
        }
        //if (reflectionProbeGO == null)
        //{
        //    reflectionProbeGO = new GameObject("LightReflectionProbe");
        //    reflectionProbeGO.transform.parent = transform;
        //    reflectionProbe = reflectionProbeGO.AddComponent<ReflectionProbe>();
        //    reflectionProbe.boxProjection = true;
        //    reflectionProbe.clearFlags = ReflectionProbeClearFlags.SolidColor;
        //    reflectionProbe.backgroundColor = Color.black;
        //    reflectionProbe.resolution = 512;
        //    reflectionProbe.hdr = false;

        //    reflectionProbe.mode = ReflectionProbeMode.Realtime;
        //    reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        //    reflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
        //}
        //if (lightCubemap == null)
        //{
        //    lightCubemap = new RenderTexture(reflectionProbe.resolution, reflectionProbe.resolution, 0, RenderTextureFormat.ARGB32);
        //    lightCubemap.dimension = TextureDimension.Cube;
        //    lightCubemap.useMipMap = true;
        //}


    }

    void OnDisable()
    {
        KW_Extensions.SafeDestroy(camObj);
        if (lightCubemap != null) lightCubemap.Release();
        foreach (var lightGO in lightsGO)
        {
            KW_Extensions.SafeDestroy(lightGO.Value);
        }
        lightsGO.Clear();
        KW_Extensions.SafeDestroy(cubeLightMaterial);
        print("RenderLightsToCubemap.Disabled");
    }

    public void Release()
    {
        OnDisable();
    }

    public void AddMaterialsToWaterRendering(List<Material> waterShaderMaterials)
    {
        if (cubeLightMaterial == null) cubeLightMaterial = KW_Extensions.CreateMaterial(cubemapLight_ShaderName);
        if (!waterShaderMaterials.Contains(cubeLightMaterial)) waterShaderMaterials.Add(cubeLightMaterial);
    }

    [System.Flags]
    public enum CubemapSides
    {
        Left = (1 << 0),
        Right = (1 << 1),
        Up = (1 << 2),
        Down = (1 << 3),
        Back = (1 << 4),
        Front = (1 << 5)
    }

    public void RenderCubemap(Vector3 cubeLightPos, Camera currentCamera, List<Material> waterSharedMaterials)
    {
        if (cam == null) Initialize();
        UpdateLightsObjects(currentCamera);
        if (cubeLightMaterial == null)
        {
            Debug.LogWarning("You must add materials to water rendering");
            return;
        }

        camObj.transform.position = cubeLightPos + Vector3.up * HeightOffset;

        camObj.transform.rotation = currentCamera.transform.rotation;
        cam.RenderToCubemap(lightCubemap, (int)(CubemapSides.Right | CubemapSides.Left | CubemapSides.Up | CubemapSides.Front | CubemapSides.Back));

        foreach (var mat in waterSharedMaterials)
        {
            if (mat == null) continue;

            mat.SetTexture("KW_LightsCube", lightCubemap);
            Shader.SetGlobalTexture("KW_LightsCube", lightCubemap);
        }
    }

    void UpdateLightsObjects(Camera currentCamera)
    {
        var pointLights = KW_WaterLights.GetPointLights();
        foreach (var pointLight in pointLights)
        {
            UpdateLight(currentCamera, pointLight);
        }

        var spotLights = KW_WaterLights.GetSpotLights();
        foreach (var spotLight in spotLights)
        {
            UpdateLight(currentCamera, spotLight);
        }

        var tempDictionary = new Dictionary<Light, GameObject>();
        foreach (var lightObj in lightsGO)
        {
            if (lightObj.Key == null)
            {
                KW_Extensions.SafeDestroy(lightObj.Value);
            }
            else
            {
                tempDictionary.Add(lightObj.Key, lightObj.Value);
            }
        }
        lightsGO.Clear();
        lightsGO = tempDictionary;
    }

    private void UpdateLight(Camera currentCamera, Light pointLight)
    {
        if (!lightsGO.ContainsKey(pointLight))
        {
            var temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
            temp.name = "CubemapLight";
            KW_Extensions.SafeDestroy(temp.GetComponent<MeshCollider>());
            temp.transform.parent = transform;
            temp.GetComponent<MeshRenderer>().sharedMaterial = cubeLightMaterial;
            lightsGO.Add(pointLight, temp);
        }

        var lightObj = lightsGO[pointLight];
        var lightObjTranform = lightObj.transform;
        lightObjTranform.position = pointLight.transform.position + Vector3.up * HeightOffset;
        lightObjTranform.LookAt(camObj.transform.position);
        UpdateLightShaderParams(pointLight, lightObj, currentCamera);
    }

    void UpdateLightShaderParams(Light currentLight, GameObject lightObj, Camera currentCamera)
    {
        var distToCam = Mathf.Clamp01((currentCamera.transform.position - currentLight.transform.position).magnitude / 1000f);

        lightObj.transform.localScale = currentLight.transform.localScale * Mathf.Lerp(currentLight.range / 10f, currentLight.range / 10f, distToCam);
        var rend = lightObj.GetComponent<Renderer>();

        rend.GetPropertyBlock(propertyBlock);

        Color lightCol;
        if (currentLight.isActiveAndEnabled) lightCol = currentLight.color;
        else lightCol = Color.black;

        var lightIntensity = (1 - Mathf.Pow(distToCam, 0.25f)) * currentLight.intensity / 16.0f;
        propertyBlock.SetVector("_CubemapLightColor", new Vector4(lightCol.r, lightCol.g, lightCol.b, currentLight.intensity / 16.0f));
        propertyBlock.SetFloat("_HeightOffset", HeightOffset);
        rend.SetPropertyBlock(propertyBlock);
    }

}
