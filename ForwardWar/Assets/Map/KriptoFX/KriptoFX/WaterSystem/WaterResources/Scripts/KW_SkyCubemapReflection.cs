using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KW_SkyCubemapReflection : MonoBehaviour
{
    private GameObject reflCameraGO;
    private Camera reflectionCamera;
    private RenderTexture reflectionRT;
   // private const float UpdateIntervalSeconds = 1;
    private float currentInterval = 100000;
    private int sideIdx = 0;

    private int[] cubeMapSides = new int[]
    {
        (1 << 0),
        (1 << 1),
        (1 << 2),
        (1 << 3),
        (1 << 4),
        (1 << 5)
    };

    public void Release()
    {
        KW_Extensions.SafeDestroy(reflCameraGO);
        if (reflectionRT != null) reflectionRT.Release();
        currentInterval = 100000;
    }

    void OnDisable()
    {
        Release();
    }

    void InitializeCubeRT(int size)
    {
        if (reflectionRT != null) reflectionRT.Release();
        reflectionRT = new RenderTexture(size, size, 16, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
        reflectionRT.useMipMap = false;
        reflectionRT.dimension = TextureDimension.Cube;
    }

    public void RenderCubemap(Camera currentCamera, float waterOffset, float interval, int cullingMask, int texSize, List<Material> waterShaderMaterials)
    {

        currentInterval += KW_Extensions.DeltaTime();
        if (currentInterval < interval / 6.0f) return; //6 faces x 6 time faster
        currentInterval = 0;

        if (reflCameraGO == null)
        {
            reflCameraGO = new GameObject("WaterReflectionCamera");
            reflCameraGO.transform.parent = transform;
            reflectionCamera = reflCameraGO.AddComponent<Camera>();
            reflectionCamera.allowHDR = true;
            reflectionCamera.allowMSAA = false;
            reflectionCamera.enabled = false;
           // reflectionCamera.di
        }

        if (reflectionRT == null || reflectionRT.width != texSize) InitializeCubeRT(texSize);

        var pos = currentCamera.transform.position;
        //  reflectionCamera.CopyFrom(currentCamera);
        reflectionCamera.renderingPath = currentCamera.renderingPath;
        reflectionCamera.backgroundColor = currentCamera.backgroundColor;
        reflectionCamera.clearFlags = currentCamera.clearFlags;

        reflectionCamera.transform.position = new Vector3(pos.x, pos.y * -1  + waterOffset * 2, pos.z);
        reflectionCamera.transform.rotation = Quaternion.Euler(Vector3.down);
        var currentSide = cubeMapSides[sideIdx];
        sideIdx = (sideIdx < 5) ? sideIdx += 1 : 0;

        reflectionCamera.cullingMask = cullingMask;

        if(interval > 0.001f) reflectionCamera.RenderToCubemap(reflectionRT, currentSide);
        else reflectionCamera.RenderToCubemap(reflectionRT);

        foreach (var mat in waterShaderMaterials)
        {
            if (mat == null) continue;
            mat.SetTexture("KW_ReflectionCube", reflectionRT);
        }
    }
}
