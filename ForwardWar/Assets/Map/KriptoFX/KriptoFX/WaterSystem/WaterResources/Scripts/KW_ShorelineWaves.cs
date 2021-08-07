using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class KW_ShorelineWaves : MonoBehaviour
{
    private const string shorelineWaveBakinng_shaderName = "Hidden/KriptoFX/Water/KW_ShorelineWavePosition";
    private const string VAT_shaderName = "Hidden/KriptoFX/Water/KW_FoamParticles";

    private const string path_shoreLineFolder = "ShorelineMaps";
    private const string path_shoreLineWavesData = "KW_ShorelineWavesData";
    private const string path_shorelineMapUV1 = "KW_Shoreline1_UV_Angle_Alpha";
    private const string path_shorelineMapData1 = "KW_BakedWaves1_TimeOffset_Scale";
    private const string path_shorelineMapUV2 = "KW_Shoreline2_UV_Angle_Alpha";
    private const string path_shorelineMapData2 = "KW_BakedWaves2_TimeOffset_Scale";

    private const string path_ToShorelineWaveTex = "Shoreline_Pos_14x15";
    private const string path_ToShorelineWaveNormTex = "Shoreline_Norm_14x15";
    private const string path_VAT_MeshTexLod0 = "VAT_Mesh_Lod0";
    private const string path_VAT_MeshTexLod1 = "VAT_Mesh_Lod1";
    private const string path_VAT_MeshTexLod2 = "VAT_Mesh_Lod2";
    private const string path_VAT_MeshTexLod3 = "VAT_Mesh_Lod3";
    private const string path_VAT_MeshTexLod4 = "VAT_Mesh_Lod4";
    private const string path_VAT_MeshTexLod5 = "VAT_Mesh_Lod5";
    private const string path_VAT_MeshTexLod6 = "VAT_Mesh_Lod6";
    private const string path_VAT_MeshTexLod7 = "VAT_Mesh_Lod7";
    private const string path_VAT_PositionTex = "VAT_Position";
    private const string path_VAT_AlphaTex = "VAT_Alpha";
    private const string path_VAT_RangeLookupTex = "VAT_RangeLookup";
    private const string path_VAT_OffsetTex = "BeachWaveParticlesOffset";

    private int ID_KW_ShorelineWaveDisplacement = Shader.PropertyToID("KW_ShorelineWaveDisplacement");
    private int ID_KW_ShorelineWaveNormal = Shader.PropertyToID("KW_ShorelineWaveNormal");
    private int ID_KW_ShorelineVAT_Mesh = Shader.PropertyToID("KW_Vat_Mesh");
    private int ID_KW_ShorelineVAT_Position = Shader.PropertyToID("KW_VAT_Position");
    private int ID_KW_ShorelineVAT_Alpha = Shader.PropertyToID("KW_VAT_Alpha");
    private int ID_KW_ShorelineVAT_RangeLookup = Shader.PropertyToID("KW_VAT_RangeLookup");
    private int ID_KW_ShorelineVAT_Offset = Shader.PropertyToID("KW_VAT_Offset");
    private int ID_KW_ShorelineMapSize = Shader.PropertyToID("KW_WavesMapSize");

    private Texture2D shorelineWaveDisplacementTex;
    private Texture2D shorelineWaveNormalTex;
    private Mesh VAT_Mesh_Lod0;
    private Mesh VAT_Mesh_Lod1;
    private Mesh VAT_Mesh_Lod2;
    private Mesh VAT_Mesh_Lod3;
    private Mesh VAT_Mesh_Lod4;
    private Mesh VAT_Mesh_Lod5;
    private Mesh VAT_Mesh_Lod6;
    private Mesh VAT_Mesh_Lod7;
    public Texture2D VAT_Position;
    public Texture2D VAT_Alpha;
    private Texture2D VAT_RangeLookup;
    private Texture2D VAT_Offset;

    GameObject camGO;
    private Camera cam;
    Material waveMaterial;
    private Material vatMaterial;

    private GameObject quad;

    public RenderTexture rt_wavesTex1;
    public RenderTexture rt_wavesTex2;
    public RenderTexture rt_wavesDataTex1;
    public RenderTexture rt_wavesDataTex2;

    public Texture2D wavesTex1;
    public Texture2D wavesTex2;
    public Texture2D wavesDataTex1;
    public Texture2D wavesDataTex2;

    List<GameObject> wavesObjects =  new List<GameObject>();
    private Dictionary<int, CustomLod> foamGameObjects = new Dictionary<int, CustomLod>();
    List<GameObject> foamLodsForLateDeactivation = new List<GameObject>();
    List<ShorelineWaveInfo> _shorelineWavesData;

    private bool isInitializedBakingResources;
    private bool isInitializedShorelineResources;

    private const float HeightWave1 = 7000;
    private const float HeightWave2 = 7010;
    private const float GlobalTimeOffsetMultiplier = 34;
    private const float GlobalTimeSpeedMultiplier = 1.0f;

    [Serializable]
    public class ShorelineWaveInfo
    {
        [SerializeField] public int ID;
        [SerializeField] public float PositionX;
        [SerializeField] public float PositionZ;
        [SerializeField] public float EulerRotation;
        [SerializeField] public float ScaleX = 14;
        [SerializeField] public float ScaleY = 4.5f;
        [SerializeField] public float ScaleZ = 16;
        [SerializeField] public float TimeOffset = 0;
        [SerializeField] public float DefaultScaleX = 14;
        [SerializeField] public float DefaultScaleY = 4.5f;
        [SerializeField] public float DefaultScaleZ = 16;
    }

    class CustomLod
    {
        public GameObject Parent;
        public GameObject[] LodObjects;
        public float[] LodFoamSize = new float[] { 0, 0.02f, 0.045f, 0.07f, 0.11f, 0.15f, 0.22f, 0.3f };

        public float[] LodDistances_High = new float[] { 40, 43, 46, 50, 54, 60, 70, 90 };


        public float[] LodDistances_Medium = new float[] { 20, 25, 30, 35, 40, 45, 50, 70 };
        //public float[] LodFoamSize_Medium = new float[] { 0, 0.02f, 0.045f, 0.07f, 0.11f, 0.15f, 0.22f, 0.3f };

        public float[] LodDistances_Low = new float[] { 10, 13, 16, 20, 24, 30, 40, 60 };
        //public float[] LodFoamSize_Low = new float[] { 0, 0.02f, 0.045f, 0.07f, 0.11f, 0.15f, 0.22f, 0.3f };


        public Vector3 Position;
        public int ActiveLod = -1;
    }

    public void AddMaterialsToWaterRendering(List<Material> waterSharedMaterials)
    {
        if (vatMaterial == null) vatMaterial = KW_Extensions.CreateMaterial(VAT_shaderName);
        if (!waterSharedMaterials.Contains(vatMaterial)) waterSharedMaterials.Add(vatMaterial);
    }

    public async Task<List<ShorelineWaveInfo>> GetShorelineWavesData(string GUID)
    {
        if (_shorelineWavesData == null || _shorelineWavesData.Count == 0)
        {
            var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();

            _shorelineWavesData = await KW_Extensions.DeserializeFromFile<List<ShorelineWaveInfo>>(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shoreLineWavesData));
            if(_shorelineWavesData == null) _shorelineWavesData = new List<ShorelineWaveInfo>();
        }

        return _shorelineWavesData;
    }

    public async Task BakeWavesToTexture(int areaSize, Vector3 areaPosition, int texSize, string GUID)
    {
        var shorelineWavesData = await GetShorelineWavesData(GUID);
        if (shorelineWavesData == null || shorelineWavesData.Count == 0) return;

        if (!isInitializedBakingResources) InitializeBakingResources(areaSize, areaPosition);

        foreach (var wavesObject in wavesObjects) wavesObject.SetActive(false);

        for (var i = 0; i < shorelineWavesData.Count; i++)
        {
            if (i == wavesObjects.Count) wavesObjects.Add(CreateTempGO());
            wavesObjects[i].SetActive(true);

            var meterInPixels = (1f * areaSize / texSize);
            var roundedPos = new Vector2(Mathf.Round(shorelineWavesData[i].PositionX / meterInPixels) * meterInPixels, Mathf.Round(shorelineWavesData[i].PositionZ / meterInPixels) * meterInPixels);
            var roundedScale = new Vector2(Mathf.Round(shorelineWavesData[i].ScaleX / meterInPixels) * meterInPixels, Mathf.Round(shorelineWavesData[i].ScaleZ / meterInPixels) * meterInPixels);
            var height = shorelineWavesData[i].ID % 2 == 0 ? HeightWave1 : HeightWave2;

            var tempT = wavesObjects[i].transform;
            tempT.position = new Vector3(roundedPos.x, height, roundedPos.y);
            tempT.rotation = Quaternion.Euler(270, 0, shorelineWavesData[i].EulerRotation);
            tempT.localScale = roundedScale;

            var scaleTexels = new Vector2(meterInPixels * tempT.localScale.x, meterInPixels * tempT.localScale.y);
            var uvOffset = new Vector2(1 - (scaleTexels.x - meterInPixels) / scaleTexels.x, 1 - (scaleTexels.y - meterInPixels) / scaleTexels.y);

            var pixelsInQuad = new Vector2(meterInPixels * tempT.localScale.x, meterInPixels * tempT.localScale.y);
            var uvScale = new Vector2(1f - (pixelsInQuad.x - 1) / pixelsInQuad.x, 1f - (pixelsInQuad.y - 1) / pixelsInQuad.y);

            var props = new MaterialPropertyBlock();
            var scaleMultiplier = shorelineWavesData[i].ScaleY / shorelineWavesData[i].DefaultScaleY;
            if (scaleMultiplier < 1.0f) scaleMultiplier *= Mathf.Lerp(0.25f, 1f, scaleMultiplier);

            var rend = wavesObjects[i].GetComponent<MeshRenderer>();
            rend.GetPropertyBlock(props);
            props.SetVector("KW_WavesUVOffset", new Vector4(uvOffset.x, uvOffset.y, uvScale.x, uvScale.y));
            props.SetFloat("KW_WaveScale", scaleMultiplier);
            props.SetFloat("KW_WaveTimeOffset", shorelineWavesData[i].TimeOffset);
            props.SetFloat("KW_WaveAngle", ((tempT.rotation.eulerAngles.y) % 360f) / 360f);
            rend.SetPropertyBlock(props);
        }

        InitializeCameraTextures(texSize);


        areaPosition.y = HeightWave1;
        cam.transform.position = areaPosition;

        cam.SetTargetBuffers(new [] { rt_wavesTex1.colorBuffer, rt_wavesDataTex1.colorBuffer}, rt_wavesTex1.depthBuffer);
        cam.Render();

        areaPosition.y = HeightWave2;
        cam.transform.position = areaPosition;

        cam.SetTargetBuffers(new[] { rt_wavesTex2.colorBuffer, rt_wavesDataTex2.colorBuffer }, rt_wavesTex2.depthBuffer);
        cam.Render();

        UpdateShaderParameters(areaSize);
    }

    public void SaveWavesDataToFile(string GUID)
    {
        var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();

        KW_Extensions.SerializeToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shoreLineWavesData), _shorelineWavesData);
        //rt_wavesTex1.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shorelineMapUV1), TextureFormat.BC7);
        //rt_wavesTex2.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shorelineMapUV2), TextureFormat.BC7);
        //rt_wavesDataTex1.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shorelineMapData1), TextureFormat.BC6H);
        //rt_wavesDataTex2.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shorelineMapData2), TextureFormat.BC6H);
        rt_wavesTex1.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapUV1), TextureFormat.RGBAHalf);
        rt_wavesTex2.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapUV2), TextureFormat.RGBAHalf);
        rt_wavesDataTex1.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapData1), TextureFormat.RGHalf);
        rt_wavesDataTex2.SaveRenderTextureToFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapData2), TextureFormat.RGHalf);
    }

    public async Task<bool> RenderShorelineWavesWithFoam(int shorelineAreaSize, string GUID)
    {
        var shorelineWavesData = await GetShorelineWavesData(GUID);

        if (shorelineWavesData == null || shorelineWavesData.Count == 0) return false;

        if (!isInitializedShorelineResources) await InitializeShorelineResources(GUID);

        UpdateShaderParameters(shorelineAreaSize);


        RenderFoam(shorelineWavesData);
        return true;
    }

    void RenderFoam(List<ShorelineWaveInfo> shorelineWavesData)
    {
        //Debug.Log("RenderFoam");

        if (VAT_Mesh_Lod0 == null || vatMaterial == null) return;

        var lodArray = new [] {VAT_Mesh_Lod0, VAT_Mesh_Lod1, VAT_Mesh_Lod2, VAT_Mesh_Lod3, VAT_Mesh_Lod4, VAT_Mesh_Lod5, VAT_Mesh_Lod6, VAT_Mesh_Lod7};
        var props = new MaterialPropertyBlock();
        foreach (var wave in shorelineWavesData)
        {
            if (!foamGameObjects.ContainsKey(wave.ID))
            {
                var vatGO = new GameObject("FoamParticles");
                vatGO.transform.parent = transform;
                var customLod = new CustomLod();
                customLod.Parent = vatGO;
                customLod.LodObjects = new GameObject[lodArray.Length];


                for (int i = 0; i < lodArray.Length; i++)
                {
                    //var lod = new GameObject("FoamParticles_Lod" + i);
                    //lod.transform.parent = vatGO.transform;
                    ////lod.layer = waterLayer;
                    //lod.layer = 4 << 0;
                    //lod.AddComponent<MeshFilter>().sharedMesh = lodArray[i];

                    //var vatRend = lod.AddComponent<MeshRenderer>();
                    //vatRend.enabled = false;
                    //vatRend.sharedMaterial = vatMaterial;
                    //vatRend.shadowCastingMode = ShadowCastingMode.Off;
                    //vatRend.GetPropertyBlock(props);
                    //props.SetFloat("KW_WaveTimeOffset", wave.TimeOffset);
                    //props.SetFloat("KW_SizeAdditiveScale", customLod.LodFoamSize[i]);
                    //vatRend.SetPropertyBlock(props);
                    var lodGO = CreateLodWave("FoamParticles_Lod" + i, vatGO.transform, lodArray[i], props, wave.TimeOffset, customLod.LodFoamSize[i], false);

                    var shadowMesh = lodArray.Length > i + 3 ? lodArray[i + 3] : lodArray.Last();
                     CreateLodWave("FoamParticlesShadow_Lod" + i, lodGO.transform, shadowMesh, props, wave.TimeOffset, customLod.LodFoamSize[i] + 0.15f, true);


                    customLod.LodObjects[i] = lodGO;
                }

                foamGameObjects.Add(wave.ID, customLod);
            }
            var vatParticlesGO = foamGameObjects[wave.ID];
            var lessScaleFix = Mathf.Lerp(0.15f, 0, Mathf.Clamp01((wave.ScaleY / wave.DefaultScaleY)));
            vatParticlesGO.Parent.transform.position = new Vector3(wave.PositionX, transform.position.y + lessScaleFix, wave.PositionZ);
            vatParticlesGO.Parent.transform.rotation = Quaternion.Euler(0, wave.EulerRotation, 0);
            vatParticlesGO.Parent.transform.localScale = new Vector3(wave.ScaleX / wave.DefaultScaleX, wave.ScaleY / wave.DefaultScaleY, wave.ScaleZ / wave.DefaultScaleZ);
            vatParticlesGO.Position = vatParticlesGO.Parent.transform.position;
        }
    }

    GameObject CreateLodWave(string lodName, Transform parent, Mesh mesh, MaterialPropertyBlock props, float timeOffset, float lodParticleSize, bool useShadows)
    {
        var lod = new GameObject(lodName);
        lod.SetActive(useShadows);
        lod.transform.parent = parent;
        //lod.layer = waterLayer;
        lod.layer = 4 << 0;
        lod.AddComponent<MeshFilter>().sharedMesh = mesh;

        var vatRend = lod.AddComponent<MeshRenderer>();
        vatRend.sharedMaterial = vatMaterial;
        vatRend.shadowCastingMode = useShadows ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.Off;
        vatRend.GetPropertyBlock(props);
        props.SetFloat("KW_WaveTimeOffset", timeOffset);
        props.SetFloat("KW_SizeAdditiveScale", lodParticleSize);
        vatRend.SetPropertyBlock(props);
        return lod;
    }

    public void ClearFoam()
    {
        if (wavesObjects != null)
        {
            foreach (var waveObj in wavesObjects) KW_Extensions.SafeDestroy(waveObj);
            wavesObjects.Clear();
        }

        if (foamGameObjects != null)
        {
            foreach (var vatParticles in foamGameObjects)
            {
                KW_Extensions.SafeDestroy(vatParticles.Value.Parent);
            }
            foamGameObjects.Clear();
        }
        foamLodsForLateDeactivation.Clear();
    }

    public void ClearShorelineWavesWithFoam(string GUID)
    {
        ClearFoam();
        if (_shorelineWavesData != null) _shorelineWavesData.Clear();

        var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        File.Delete(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shoreLineWavesData + ".gz"));
        File.Delete(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapUV1 + ".gz"));
        File.Delete(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapUV2 + ".gz"));
        File.Delete(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapData1 + ".gz"));
        File.Delete(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapData2 + ".gz"));

        ReleaseTextures();
    }

    public void Release()
    {
        OnDisable();
    }

    private float UpdateEachSeconds = 0.5f;
    private float lastLodUpdateLeftTime = 0;
    private bool updateLodTest = false;

    public void UpdateLodLevels(Vector3 camPos, int qualityLevel)
    {
        foreach (var foamRend in foamLodsForLateDeactivation)
        {
            if (foamRend != null) foamRend.SetActive(false);
        }

        if (lastLodUpdateLeftTime < 0) lastLodUpdateLeftTime = 0;
        lastLodUpdateLeftTime += KW_Extensions.DeltaTime();
        if (lastLodUpdateLeftTime < UpdateEachSeconds) return;
        lastLodUpdateLeftTime = 0;
        foamLodsForLateDeactivation.Clear();

        foreach (var vatLodGO in foamGameObjects)
        {
            if (vatLodGO.Value != null)
            {
                var customLod = vatLodGO.Value;
                var lods = customLod.LodObjects;
                var distance = Vector3.Distance(camPos, customLod.Position);
                int n = 0;
                for (var i = 0; i < lods.Length; i++)
                {
                    float lodDist = 10f;
                    if (qualityLevel == 0) lodDist = customLod.LodDistances_High[i];
                    else if (qualityLevel == 1) lodDist = customLod.LodDistances_Medium[i];
                    else if (qualityLevel == 2) lodDist = customLod.LodDistances_Low[i];

                    if (distance > lodDist) n = i;
                }
                if (n != customLod.ActiveLod)
                {
                    if(customLod.ActiveLod != -1) foamLodsForLateDeactivation.Add(lods[customLod.ActiveLod]);
                    customLod.ActiveLod = n;
                    lods[n].SetActive(true);

                }
            }
        }

    }

    void OnDisable ()
    {
       // print("ShorelineMap.Disabled");

        KW_Extensions.SafeDestroy(camGO);
		KW_Extensions.SafeDestroy(waveMaterial);
        KW_Extensions.SafeDestroy(quad);

        ClearFoam();

        if (_shorelineWavesData != null) _shorelineWavesData.Clear();

        ReleaseTextures();

        KW_Extensions.SafeDestroy(vatMaterial);

        KW_Extensions.SafeDestroy(shorelineWaveDisplacementTex);
        KW_Extensions.SafeDestroy(shorelineWaveNormalTex);
        KW_Extensions.SafeDestroy(VAT_Mesh_Lod0);
        KW_Extensions.SafeDestroy(VAT_Mesh_Lod1);
        KW_Extensions.SafeDestroy(VAT_Mesh_Lod2);
        KW_Extensions.SafeDestroy(VAT_Mesh_Lod3);
        KW_Extensions.SafeDestroy(VAT_Mesh_Lod4);
        KW_Extensions.SafeDestroy(VAT_Mesh_Lod5);
        KW_Extensions.SafeDestroy(VAT_Mesh_Lod6);
        KW_Extensions.SafeDestroy(VAT_Mesh_Lod7);
        KW_Extensions.SafeDestroy(VAT_Position);
        KW_Extensions.SafeDestroy(VAT_Alpha);
        KW_Extensions.SafeDestroy(VAT_RangeLookup);
        KW_Extensions.SafeDestroy(VAT_Offset);

        isInitializedShorelineResources = false;
        isInitializedBakingResources = false;

        lastLodUpdateLeftTime = 0;
    }

    void ReleaseTextures()
    {
        if (rt_wavesTex1 != null) rt_wavesTex1.Release();
        if (rt_wavesTex2 != null) rt_wavesTex2.Release();
        if (rt_wavesDataTex1 != null) rt_wavesDataTex1.Release();
        if (rt_wavesDataTex2 != null) rt_wavesDataTex2.Release();

        KW_Extensions.SafeDestroy(wavesTex1);
        KW_Extensions.SafeDestroy(wavesTex2);
        KW_Extensions.SafeDestroy(wavesDataTex1);
        KW_Extensions.SafeDestroy(wavesDataTex2);
    }

    GameObject CreateTempGO()
    {
        var go = Instantiate(quad);
        go.transform.parent = transform;
        var rend = go.GetComponent<MeshRenderer>();
        rend.sharedMaterial = waveMaterial;
        go.SetActive(false);
        return go;
    }

    async Task InitializeShorelineResources(string GUID)
    {
        var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();

        if (wavesTex1 == null) wavesTex1 = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapUV1), true, FilterMode.Bilinear, TextureWrapMode.Clamp);
        if (wavesTex2 == null) wavesTex2 = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapUV2), true, FilterMode.Bilinear, TextureWrapMode.Clamp);
        if (wavesDataTex1 == null) wavesDataTex1 = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapData1), true, FilterMode.Point, TextureWrapMode.Clamp);
        if (wavesDataTex2 == null) wavesDataTex2 = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, GUID, path_shorelineMapData2), true, FilterMode.Point, TextureWrapMode.Clamp);

        if (shorelineWaveDisplacementTex == null) shorelineWaveDisplacementTex = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_ToShorelineWaveTex));
        if (shorelineWaveNormalTex == null) shorelineWaveNormalTex = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_ToShorelineWaveNormTex), true);

        if (VAT_Position == null) VAT_Position = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_PositionTex), true, FilterMode.Point, TextureWrapMode.Clamp);
        if (VAT_Alpha == null) VAT_Alpha = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_AlphaTex), true, FilterMode.Point, TextureWrapMode.Clamp);
        if (VAT_RangeLookup == null) VAT_RangeLookup = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_RangeLookupTex), true, FilterMode.Point, TextureWrapMode.Clamp);
        if (VAT_Offset == null) VAT_Offset = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_OffsetTex));
        if (VAT_Mesh_Lod0 == null)
        {
            VAT_Mesh_Lod0 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod0), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod1 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod1), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod2 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod2), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod3 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod3), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod4 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod4), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod5 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod5), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod6 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod6), new Vector3(14, 4.5f, 16) * 0.75f);
            VAT_Mesh_Lod7 = await KW_Extensions.DeserializeMeshFromFile(Path.Combine(pathToBakedDataFolder, path_shoreLineFolder, path_VAT_MeshTexLod7), new Vector3(14, 4.5f, 16) * 0.75f);
        }

        if (vatMaterial != null)
        {

            vatMaterial.SetTexture(ID_KW_ShorelineVAT_Position, VAT_Position);
            vatMaterial.SetTexture(ID_KW_ShorelineVAT_Alpha, VAT_Alpha);
            vatMaterial.SetTexture(ID_KW_ShorelineVAT_RangeLookup, VAT_RangeLookup);
            vatMaterial.SetTexture(ID_KW_ShorelineVAT_Offset, VAT_Offset);

            isInitializedShorelineResources = true;
        }

    }

    void InitializeCameraTextures(int size)
    {

        if (rt_wavesTex1 != null) rt_wavesTex1.Release();
        if (rt_wavesTex2 != null) rt_wavesTex2.Release();
        if (rt_wavesDataTex1 != null) rt_wavesDataTex1.Release();
        if (rt_wavesDataTex2 != null) rt_wavesDataTex2.Release();

        rt_wavesTex1 = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        rt_wavesTex1.wrapMode = TextureWrapMode.Clamp;
        rt_wavesTex1.filterMode = FilterMode.Bilinear;
        rt_wavesTex1.name = "wave1";

        rt_wavesTex2 = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        rt_wavesTex2.wrapMode = TextureWrapMode.Clamp;
        rt_wavesTex2.filterMode = FilterMode.Bilinear;
        rt_wavesTex2.name = "wave2";

        rt_wavesDataTex1 = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        rt_wavesDataTex1.filterMode = FilterMode.Point;
        rt_wavesDataTex1.wrapMode = TextureWrapMode.Clamp;
        rt_wavesDataTex1.name = "wave1_data";

        rt_wavesDataTex2 = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        rt_wavesDataTex2.filterMode = FilterMode.Point;
        rt_wavesDataTex2.wrapMode = TextureWrapMode.Clamp;
        rt_wavesDataTex2.name = "wave2_data";
    }

    void InitializeBakingResources(int areaSize, Vector3 position)
    {
        waveMaterial = KW_Extensions.CreateMaterial(shorelineWaveBakinng_shaderName);
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        KW_Extensions.SafeDestroy(quad.GetComponent<Collider>());

        camGO = new GameObject("ShorelineCamera");
        camGO.transform.parent = transform;
        cam = camGO.AddComponent<Camera>();


        position.y = HeightWave1;
        cam.transform.position = position;

        cam.transform.rotation = Quaternion.Euler(90, 0, 0);
        cam.orthographicSize = areaSize * 0.5f;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.clearFlags = CameraClearFlags.Color;
        cam.allowHDR = false;
        cam.allowMSAA = false;
        cam.nearClipPlane = -2;
        cam.farClipPlane = 2;
        cam.depth = -1000;
        cam.orthographic = true;
        cam.enabled = false;

        isInitializedBakingResources = true;
    }

    void UpdateShaderParameters(int shorelineAreaSize)
    {
        Shader.SetGlobalTexture("KW_BakedWaves1_UV_Angle_Alpha", rt_wavesTex1 != null ? rt_wavesTex1 as Texture: wavesTex1);
        Shader.SetGlobalTexture("KW_BakedWaves2_UV_Angle_Alpha", rt_wavesTex2 != null ? rt_wavesTex2 as Texture : wavesTex2);
        Shader.SetGlobalTexture("KW_BakedWaves1_TimeOffset_Scale", rt_wavesDataTex1 != null ? rt_wavesDataTex1 as Texture : wavesDataTex1);
        Shader.SetGlobalTexture("KW_BakedWaves2_TimeOffset_Scale", rt_wavesDataTex2 != null ? rt_wavesDataTex2 as Texture : wavesDataTex2);
        Shader.SetGlobalFloat("KW_GlobalTimeOffsetMultiplier", GlobalTimeOffsetMultiplier);
        Shader.SetGlobalFloat("KW_GlobalTimeSpeedMultiplier", GlobalTimeSpeedMultiplier);

        Shader.SetGlobalTexture(ID_KW_ShorelineWaveDisplacement, shorelineWaveDisplacementTex);
        Shader.SetGlobalTexture(ID_KW_ShorelineWaveNormal, shorelineWaveNormalTex);
        Shader.SetGlobalFloat(ID_KW_ShorelineMapSize, shorelineAreaSize);
    }

}
