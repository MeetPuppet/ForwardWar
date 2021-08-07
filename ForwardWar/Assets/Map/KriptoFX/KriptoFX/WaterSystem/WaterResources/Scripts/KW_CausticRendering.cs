using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

//[ExecuteInEditMode]
public class KW_CausticRendering : MonoBehaviour
{
    public RenderTexture causticLod0;
    public RenderTexture causticLod1;
    public RenderTexture causticLod2;
    public RenderTexture causticLod3;

    private GameObject cameraGameObject;
    private GameObject causticBakeGameObject;
    private GameObject causticDecalGameObject;
    private Material causticBakeMaterial;
    private Material causticDecalMaterial;
    private Mesh causticMesh;
    private Mesh decalMesh;

    Camera causticCam;
    private const string CausticBakeShaderName = "Hidden/KriptoFX/Water/ComputeCaustic";
    private const string CausticDecalShaderName = "Hidden/KriptoFX/Water/CausticDecal";
    private const int CameraHeight = 7855;

    private int currentMeshResolution;
    Vector4 lodSettings = new Vector4(10, 20, 40, 80);

    public void Release()
    {
        OnDisable();
    }

    void OnDisable()
    {
      //  print("KW_CausticRendering.Disabled");

        KW_Extensions.SafeDestroy(cameraGameObject);
        KW_Extensions.SafeDestroy(causticBakeGameObject);
        KW_Extensions.SafeDestroy(causticDecalGameObject);
        KW_Extensions.SafeDestroy(causticBakeMaterial);
        KW_Extensions.SafeDestroy(causticDecalMaterial);
        KW_Extensions.SafeDestroy(causticMesh);
        KW_Extensions.SafeDestroy(decalMesh);
        if (causticLod0 != null) causticLod0.Release();
        if (causticLod1 != null) causticLod1.Release();
        if (causticLod2 != null) causticLod2.Release();
        if (causticLod3 != null) causticLod3.Release();

         currentMeshResolution = 0;
    }

    private void InitializeCausticTexture(int size)
    {
        //if (screenSpaceCaustic != null) RenderTexture.ReleaseTemporary(screenSpaceCaustic);
        //screenSpaceCaustic = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);


        if (causticLod0 != null) causticLod0.Release();
        causticLod0 = new RenderTexture(size, size, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        causticLod0.wrapMode = TextureWrapMode.Clamp;
        causticLod0.useMipMap = true;

        //causticLod0.antiAliasing = 4;

        if (causticLod1 != null) causticLod1.Release();
        causticLod1 = new RenderTexture(size, size, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        causticLod1.wrapMode = TextureWrapMode.Clamp;
        causticLod1.useMipMap = true;

        if (causticLod2 != null) causticLod2.Release();
        causticLod2 = new RenderTexture(size, size, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        causticLod2.useMipMap = true;
        causticLod2.wrapMode = TextureWrapMode.Clamp;

        if (causticLod3 != null) causticLod3.Release();
        causticLod3 = new RenderTexture(size, size, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        causticLod3.useMipMap = true;
        causticLod3.wrapMode = TextureWrapMode.Clamp;
    }

    void InitializeCamera()
    {
        cameraGameObject = new GameObject("WaterCausticCamera");
        cameraGameObject.transform.parent = transform;
        cameraGameObject.transform.localPosition += Vector3.up * CameraHeight;
        cameraGameObject.transform.rotation = Quaternion.Euler(90, 0, 0);

        causticCam = cameraGameObject.AddComponent<Camera>();
       // causticCam.targetTexture = causticLod0;
        causticCam.renderingPath = RenderingPath.Forward;
        causticCam.depthTextureMode = DepthTextureMode.None;

        causticCam.orthographic = true;
        causticCam.allowMSAA = false;
        causticCam.allowHDR = false;
        causticCam.clearFlags = CameraClearFlags.Color;
        causticCam.backgroundColor = Color.black;
        causticCam.nearClipPlane = -1;
        causticCam.farClipPlane = 1;
        causticCam.enabled = false;
    }

    void InitializeCausticBakeGO()
    {
        causticBakeGameObject = new GameObject("BakeCausticMesh");
        causticBakeGameObject.transform.parent = transform;
        causticBakeGameObject.transform.localPosition += Vector3.up * CameraHeight;


        causticBakeGameObject.AddComponent<MeshFilter>().sharedMesh = causticMesh;
        causticBakeGameObject.AddComponent<MeshRenderer>().sharedMaterial = causticBakeMaterial;

    }

    void InitializeCausticDecalGO()
    {
        causticDecalGameObject = new GameObject("DecalCausticMesh");
        causticDecalGameObject.transform.parent = transform;
        causticDecalGameObject.transform.localPosition += Vector3.up * CameraHeight;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        causticDecalGameObject.AddComponent<MeshFilter>().sharedMesh = cube.GetComponent<MeshFilter>().mesh;
        causticDecalGameObject.AddComponent<MeshRenderer>().sharedMaterial = causticDecalMaterial;
        KW_Extensions.SafeDestroy(cube);
    }

    public void AddMaterialsToWaterRendering(List<Material> waterShaderMaterials)
    {
        if (causticBakeMaterial == null) causticBakeMaterial = KW_Extensions.CreateMaterial(CausticBakeShaderName);
        if (!waterShaderMaterials.Contains(causticBakeMaterial)) waterShaderMaterials.Add(causticBakeMaterial);

        if (causticDecalMaterial == null) causticDecalMaterial = KW_Extensions.CreateMaterial(CausticDecalShaderName);
        if (!waterShaderMaterials.Contains(causticDecalMaterial)) waterShaderMaterials.Add(causticDecalMaterial);
    }

    private CommandBuffer cb;
    public void RenderScreenSpaceCaustic(Camera currentCamera, Transform anchor, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers, int causticTextureSize, int meshResolution, bool useFiltering, bool useDisperstion, float dispersionStrength)
    {
        if (cb != null && waterSharedBuffers.ContainsKey(cb)) return;
        print("ScreenSpacePlanarReflection.CreatedCommand");

        if (cb == null) cb = new CommandBuffer() { name = "------ScreenSpaceCaustic" };
        else cb.Clear();

        InitializeCausticTexture(causticTextureSize);
        GeneratePlane(meshResolution, 1.0f, false);

        if (useFiltering) causticBakeMaterial.EnableKeyword("USE_FILTERING");
        else causticBakeMaterial.DisableKeyword("USE_FILTERING");

        if (useDisperstion && dispersionStrength > 0.5f)
        {
            causticDecalMaterial.EnableKeyword("USE_DISPERSION");
            dispersionStrength = Mathf.Lerp(dispersionStrength * 0.25f, dispersionStrength, causticTextureSize / 1024f);
            causticDecalMaterial.SetFloat("KW_CausticDispersionStrength", dispersionStrength);
        }
        else
        {
            causticDecalMaterial.DisableKeyword("USE_DISPERSION");
        }


        float h = Mathf.Tan(currentCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * 1 * 2f;
        var fullScreenSizeMatrix = Matrix4x4.TRS(currentCamera.transform.position + currentCamera.transform.forward, currentCamera.transform.rotation, new Vector3(h * currentCamera.aspect, h, 1));

        //cb.SetRenderTarget(screenSpaceCaustic);
        //cb.ClearRenderTarget(false, true, Color.black);
        cb.DrawMesh(causticMesh, fullScreenSizeMatrix, causticBakeMaterial);
        //causticDecalMaterial.SetTexture("KW_ScreenSpaceCaustic", screenSpaceCaustic);

        waterSharedBuffers.Add(cb, CameraEvent.BeforeForwardAlpha);

        //if (causticDecalGameObject == null) InitializeCausticDecalGO();
        //var decalPos = currentCamera.transform.position;
        //decalPos.y = transform.position.y - 15;
        //causticDecalGameObject.transform.position = decalPos;
        //causticDecalGameObject.transform.localScale = new Vector3(100, 40, 100);
    }

    void RenderLod(Vector3 camPos, Vector3 camDir, float lodDistance, RenderTexture target, float causticStr, float causticDepthScale, bool useFiltering = false)
    {
        var bakeCamPos = camPos + camDir * lodDistance * 0.5f;
        bakeCamPos.y = CameraHeight;
        causticCam.transform.position = bakeCamPos;
        causticCam.orthographicSize = lodDistance * 0.5f;
        causticBakeGameObject.transform.position = bakeCamPos;
        causticBakeGameObject.transform.localScale = Vector3.one * lodDistance;

        if (useFiltering) causticBakeMaterial.EnableKeyword("USE_FILTERING");
        causticBakeMaterial.SetFloat("KW_MeshScale", lodDistance);
        causticBakeMaterial.SetFloat("KW_CaustisStrength", causticStr);
        causticBakeMaterial.SetFloat("KW_CausticDepthScale", causticDepthScale);

        causticCam.targetTexture = target;
        causticCam.Render();

        if (useFiltering) causticBakeMaterial.DisableKeyword("USE_FILTERING");
    }

    void GenerateDecalMesh()
    {
        Vector3[] vertices = {
            new Vector3 (-0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, 0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, 0.5f),
            new Vector3 (0.5f, 0.5f, 0.5f),
            new Vector3 (0.5f, -0.5f, 0.5f),
            new Vector3 (-0.5f, -0.5f, 0.5f),
        };

        int[] triangles = {
            0, 2, 1, //face front
            0, 3, 2,
            2, 3, 4, //face top
            2, 4, 5,
            1, 2, 5, //face right
            1, 5, 6,
            0, 7, 4, //face left
            0, 4, 3,
            5, 4, 7, //face back
            5, 7, 6,
            0, 6, 7, //face bottom
            0, 1, 6
        };

        if (decalMesh == null)
        {
            decalMesh = new Mesh();
        }
        decalMesh.Clear();
        decalMesh.vertices = vertices;
        decalMesh.triangles = triangles;
        decalMesh.RecalculateNormals();
    }

    public void Render(Camera currentCamera, float causticStr, float causticDepthScale, int causticTextureSize, int activeLodCounts, int meshResolution, bool useFiltering,
        bool useDisperstion, float dispersionStrength, List<Material> waterSharedMaterials, Dictionary<CommandBuffer, CameraEvent> waterSharedBuffers)
    {


        if (causticLod0 == null || causticTextureSize != causticLod0.width) InitializeCausticTexture(causticTextureSize);
        if (cameraGameObject == null) InitializeCamera();
        if (currentMeshResolution != meshResolution) GeneratePlane(meshResolution, 1.1f);
        if (causticBakeGameObject == null) InitializeCausticBakeGO();
        if (decalMesh == null) GenerateDecalMesh();

        var camPos = currentCamera.transform.position;
        var camDir = currentCamera.transform.forward;
        var decalScale = lodSettings[activeLodCounts - 1] * 2;

        RenderLod(camPos, camDir, lodSettings.x, causticLod0, causticStr, causticDepthScale, useFiltering);
        if (activeLodCounts > 1) RenderLod(camPos, camDir, lodSettings.y, causticLod1, causticStr, causticDepthScale, useFiltering);
        if (activeLodCounts > 2) RenderLod(camPos, camDir, lodSettings.z, causticLod2, causticStr, causticDepthScale);
        if (activeLodCounts > 3) RenderLod(camPos, camDir, lodSettings.w, causticLod3, causticStr, causticDepthScale);

        //if (causticDecalGameObject == null) InitializeCausticDecalGO();
        var decalPos = currentCamera.transform.position;
        decalPos.y = transform.position.y - 15;
        //causticDecalGameObject.transform.position = decalPos;
       // causticDecalGameObject.transform.localScale = new Vector3(decalScale, 40, decalScale);

      //  var lodParams = new Vector4(decalScale / lodSettings.x, decalScale / lodSettings.y, decalScale / lodSettings.z, decalScale / lodSettings.w);
        var lodDir = camDir * 0.5f;
        UpdateMaterialParams(causticDecalMaterial, lodDir, camPos, decalScale);

        foreach (var waterSharedMaterial in waterSharedMaterials)
        {
            UpdateMaterialParams(waterSharedMaterial, lodDir, camPos, decalScale);
        }

        causticDecalMaterial.SetFloat("KW_CaustisStrength", causticStr);
        if (useDisperstion && dispersionStrength > 0.1f)
        {
            causticDecalMaterial.EnableKeyword("USE_DISPERSION");
            dispersionStrength = Mathf.Lerp(dispersionStrength * 0.25f, dispersionStrength, causticTextureSize / 1024f);
            causticDecalMaterial.SetFloat("KW_CausticDispersionStrength", dispersionStrength);
        }
        else causticDecalMaterial.DisableKeyword("USE_DISPERSION");

        Shader.DisableKeyword("USE_LOD1");
        Shader.DisableKeyword("USE_LOD2");
        Shader.DisableKeyword("USE_LOD3");
        switch (activeLodCounts)
        {
            case 2:
                Shader.EnableKeyword("USE_LOD1");
                break;
            case 3:
                Shader.EnableKeyword("USE_LOD2");
                break;
            case 4:
                Shader.EnableKeyword("USE_LOD3");
                break;
        }

        if (cb == null)
        {
            cb = new CommandBuffer() { name = "-----CausticDecal" };

        }
        else cb.Clear();

        //var decalT = causticDecalGameObject.transform;
        var decalTRS = Matrix4x4.TRS(decalPos, Quaternion.identity, new Vector3(decalScale, 40, decalScale));
        cb.DrawMesh(decalMesh, decalTRS, causticDecalMaterial);
        //causticDecalGameObject.gameObject.SetActive(false);

        if (!waterSharedBuffers.ContainsKey(cb)) waterSharedBuffers.Add(cb, CameraEvent.BeforeForwardAlpha);
    }

    void UpdateMaterialParams(Material mat, Vector3 lodDir, Vector3 lodPos, float decalScale)
    {
        if (mat == null) return;
        mat.SetTexture("KW_CausticLod0", causticLod0);
        mat.SetTexture("KW_CausticLod1", causticLod1);
        mat.SetTexture("KW_CausticLod2", causticLod2);
        mat.SetTexture("KW_CausticLod3", causticLod3);
        mat.SetVector("KW_CausticLodSettings", lodSettings);
        mat.SetVector("KW_CausticLodOffset", lodDir);
        mat.SetVector("KW_CausticLodPosition", lodPos);
        mat.SetFloat("KW_DecalScale", decalScale);
    }

    private void GeneratePlane(int meshResolution, float scale, bool useXZplane = true)
    {
        currentMeshResolution = meshResolution;
        if (causticMesh == null)
        {
            causticMesh = new Mesh();
            causticMesh.indexFormat = IndexFormat.UInt32;
        }

        var vertices = new Vector3[(meshResolution + 1) * (meshResolution + 1)];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[meshResolution * meshResolution * 6];

        for (int i = 0, y = 0; y <= meshResolution; y++)
        for (var x = 0; x <= meshResolution; x++, i++)
        {
            if (useXZplane) vertices[i] = new Vector3(x * scale / meshResolution - 0.5f * scale, 0, y * scale / meshResolution - 0.5f * scale);
            else vertices[i] = new Vector3(x * scale / meshResolution - 0.5f * scale, y * scale / meshResolution - 0.5f * scale, 0);
                uv[i] = new Vector2(x * scale / meshResolution, y * scale / meshResolution);
        }

        for (int ti = 0, vi = 0, y = 0; y < meshResolution; y++, vi++)
        for (var x = 0; x < meshResolution; x++, ti += 6, vi++)
        {
            triangles[ti] = vi;
            triangles[ti + 3] = triangles[ti + 2] = vi + 1;
            triangles[ti + 4] = triangles[ti + 1] = vi + meshResolution + 1;
            triangles[ti + 5] = vi + meshResolution + 2;
        }

        causticMesh.Clear();
        causticMesh.vertices = vertices;
        causticMesh.uv = uv;
        causticMesh.triangles = triangles;
    }

}
