using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[Serializable]
public class WaterSystem : MonoBehaviour
{

    #region PublicVariables

    public Vector4 Test4 = Vector4.zero;
    public GameObject TestObj;

    //Color settings
    public bool ShowColorSettings = true;

    public float Transparent = 10;
    public Color WaterColor = new Color(175 / 255.0f, 225 / 255.0f, 240 / 255.0f);

    public Color TurbidityColor = new Color(10 / 255.0f, 110 / 255.0f, 100 / 255.0f);
    public float Turbidity = 0.08f;


    //Waves settings
    public bool ShowWaves;
    public FFT_GPU.SizeSetting FFT_SimulationSize = FFT_GPU.SizeSetting.Size_256;
    public bool UseMultipleSimulations = true;
    public float WindSpeed = 2;
    public float WindRotation = 0;
    public float WindTurbulence = 0.75f;
    public float TimeScale = 1;

    //Reflection settings
    public bool ShowReflectionSettings;
    public bool ReflectSun = true;
    public ReflectionModeEnum ReflectionMode = ReflectionModeEnum.ScreenSpaceReflection;
    public float CubemapUpdateInterval = 1;
    public int CubemapCullingMask = 0;
    public int CubemapTextureSize = 128;
    public bool FixUnderwaterSkyReflection;
    public float ReflectionTextureScale = 1f;
    public float PlanarReflectionClipPlaneOffset = 0.002f;
    public int SSR_DepthHolesFillDistance = 5;
    public float ScreenSpaceClipPlaneOffset = 0.002f;
   // public bool SSR_ReflectImageEffects;
   // public float SSR_ReflectedImageEffectsHDR_Mul;


    //Volumetric settings
    public bool UseVolumetricLight = true;
    public bool ShowVolumetricLightSettings;
    public float VolumetricLightResolutionScale = 0.5f;
    public int VolumetricLightIteration = 4;
    public float VolumetricLightDistance = 100;
    public float VolumetricLightBlurRadius = 1;
    ////InteractiveWaves settings
    //public bool UseInteractiveWaves = false;
    //public bool ShowInteractiveWaves;
    //public int InteractiveWavesAreaSize = 40;
    //public float InteractiveWavesQuality = 1;
    //public int InteractiveWavesFPS = 60;

    //FlowMap settings
    public bool UseFlowMap = false;
    public bool ShowFlowMap;
    public bool FlowMapInEditMode = false;
    public Vector2 FlowMapOffset = new Vector2(0,  0);
    public int FlowMapAreaSize = 200;
    public int FlowMapTextureResolution = 1024;
    public float FlowMapBrushStrength = 0.2f;
    public float FlowMapSpeed = 1;

    //Shoreline settings
    public bool UseShorelineRendering = false;
    public bool ShowShorelineMap;
    public QualityEnum FoamLodQuality = QualityEnum.Medium;
    public bool ShorelineInEditMode = false;
    public Vector2 ShorelineOffset = new Vector2(0, 0);
    public int ShorelineAreaSize = 512;
    public int ShorelineTextureResolution = 1024;
    public int OrthoDepthAreaSize = 512;
    public int OrthoDepthTextureResolution = 2048;


    //Caustic settings
    public bool UseCausticEffect = false;
    public bool ShowCausticEffectSettings;
    public bool UseCausticFiltering = true;
    public bool UseCausticDispersion = true;
    public int CausticTextureSize = 768;
    public int CausticMeshResolution = 320;
    public int CausticActiveLods = 4;
    public float CausticStrength = 1;
    public float CausticDepthScale = 1;

    //Underwater settings
    public bool UseUnderwaterEffect = true;
    public bool ShowUnderwaterEffectSettings;
    public bool UseHighQualityUnderwater = true;
    public bool UseUnderwaterBlur;
    public float UnderwaterResolutionScale = 1.0f;
    public float UnderwaterBlurRadius = 1.75f;


    //Rendering settings
    public bool ShowRendering;
    public WaterMeshTypeEnum WaterMeshType;
    public bool DrawScreenSpaceWaterMesh;
    public float ScreenSpaceWaterResolution = 1.0f;
    public bool DrawToPosteffectsDepth;
    public int MeshQuality = 10;
    public bool UseTesselation = true;
    public float TesselationFactor = 1.0f;
    public float TesselationMaxDistance = 1000f;

    public enum QualityEnum
    {
        High = 0,
        Medium = 1,
        Low = 2
    }

    public enum ReflectionModeEnum
    {
        CubemapReflection,
        PlanarReflection,
        ScreenSpaceReflection,
    }

    public enum WaterMeshTypeEnum
    {
        Infinite,
        //Finite,
       // CustomMesh
    }
    #endregion

    public string _waterGUID;
    private string waterGUID
    {
        get
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(_waterGUID)) _waterGUID = UnityEditor.GUID.Generate().ToString();
            return _waterGUID;
#else
            if (string.IsNullOrEmpty(_waterGUID)) Debug.LogError("Water GUID is empty ");
            Debug.Log("Water GUID is empty " + _waterGUID);
            return _waterGUID;
#endif
        }
    }

#region private variables



    public Camera currentCamera;
    private Camera sceneCamera;
    private float prevWindSpeed = -1;
    int causticTexSizeScaledByWind = -1;
    int causticTrisCountScaledByWind = -1;


    private GameObject causticDecalGO;
    private GameObject causticMeshGO;
    private GameObject causticCameraGO;



    //private FFT_GPU fftGPU;
    //private FFT_GPU fftGPU_LOD1;
    //private FFT_GPU fftGPU_LOD2;
    //private FFT_GPU fftGPU_Detailed;

    //private KW_ComputeOrthoDepth computeOrthoDepth = new KW_ComputeOrthoDepth();

   // [SerializeField]




    private const string WaterShaderName = "KriptoFX/Water/Water";
    private const string WaterTesselatedShaderName = "KriptoFX/Water/Water(Tesselated)";
    private const string PlaneDepth_ShaderName = "Hidden/KriptoFX/Water/KW_DepthOtrhoPlane";
    private const string DistanceField_ShaderName = "Hidden/KriptoFX/Water30/KW_ComputeDistanceField";
    private const string Shoreline_ShaderName = "Hidden/KriptoFX/Water/ComputeShoreline";
    private const string interactiveWavesShaderName = "Hidden/KriptoFX/Water/KW_InteractiveWaves";



    //const string CausticComputeDX11ShaderName = "Hidden/KriptoFX/Water30/ComputeCaustic_DX11";


    private Material planeDepthMaterial;
    //private Material distanceFieldMaterial;
    // private Material shorelineMaterial;
    private Material interactiveWavesMaterial;








    //private KW_OnWillRenderWithoutRenderer updateHelper;

    private bool isGameView;
    private bool prevGameView;


    private const float UpdatePositionEveryMeters = 2.5f;
    private bool IsPositionMatrixChanged;
    private const float UpdateDepthPositionEveryMeters = 200;


#endregion


    //------------------------------------------------------- new -----------------------------------------------------------------------------------------------

    private Mesh currentWaterMesh;
    private GameObject waterMeshGO;
    public GameObject tempGameObject;

    private MeshRenderer waterMeshRenderer;
    private MeshFilter waterMeshFilter;

    private List<Material> waterSharedMaterials;
    private Dictionary<CommandBuffer, CameraEvent> commandBuffers;
    private Dictionary<Camera, Dictionary<CommandBuffer, CameraEvent>> camerasWithBuffers;

    private Material waterMaterial;
   // private KW_InteractiveWaves interactiveWaves;

#region  properties

    private FFT_GPU _fft_lod0;
    private FFT_GPU fft_lod0
    {
        get
        {
            if (_fft_lod0 == null) _fft_lod0 = tempGameObject.AddComponent<FFT_GPU>();
            else if (!_fft_lod0.enabled) _fft_lod0.enabled = true;
            return _fft_lod0;
        }
    }

    private FFT_GPU _fft_lod1;
    private FFT_GPU fft_lod1
    {
        get
        {
            if (_fft_lod1 == null) _fft_lod1 = tempGameObject.AddComponent<FFT_GPU>();
            else if (!_fft_lod1.enabled) _fft_lod1.enabled = true;
            return _fft_lod1;
        }
    }

    private FFT_GPU _fft_lod2;
    private FFT_GPU fft_lod2
    {
        get
        {
            if (_fft_lod2 == null) _fft_lod2 = tempGameObject.AddComponent<FFT_GPU>();
            else if (!_fft_lod2.enabled) _fft_lod2.enabled = true;
            return _fft_lod2;
        }
    }


    private KW_DepthRendering _depthRendering;
    private KW_DepthRendering depthRendering
    {
        get
        {
            if (_depthRendering == null) _depthRendering = tempGameObject.AddComponent<KW_DepthRendering>();
            else if (!_depthRendering.enabled) _depthRendering.enabled = true;
            return _depthRendering;
        }
    }

    private KW_CausticRendering _causticRendering;
    public KW_CausticRendering causticRendering
    {
        get
        {
            if (_causticRendering == null) _causticRendering = tempGameObject.AddComponent<KW_CausticRendering>();
            else if (!_causticRendering.enabled) _causticRendering.enabled = true;
            return _causticRendering;
        }
    }

    KW_Underwater _underwaterFX;
    public KW_Underwater underwaterFX
    {
        get
        {
            if (_underwaterFX == null) _underwaterFX = tempGameObject.AddComponent<KW_Underwater>();
            else if (!_underwaterFX.enabled) _underwaterFX.enabled = true;
            return _underwaterFX;
        }
    }

    private KW_FlowMap _flowMap;
    KW_FlowMap flowMap
    {
        get
        {
            if (_flowMap == null) _flowMap = tempGameObject.AddComponent<KW_FlowMap>();
            else if (!_flowMap.enabled) _flowMap.enabled = true;
            return _flowMap;
        }
    }
    private KW_CameraColorTexture _cameraColorTexture;

    private KW_CameraColorTexture cameraColorTexture
    {
        get
        {
            if (_cameraColorTexture == null) _cameraColorTexture = tempGameObject.AddComponent<KW_CameraColorTexture>();
            else if (!_cameraColorTexture.enabled) _cameraColorTexture.enabled = true;
            return _cameraColorTexture;
        }
    }
    //private KW_RenderLightsToCubemap renderLightsToCubemap;

    private KW_ScreenSpacePlanarReflection _screenSpaceReflection;
    private KW_ScreenSpacePlanarReflection screenSpaceReflection
    {
        get
        {
            if (_screenSpaceReflection == null) _screenSpaceReflection = tempGameObject.AddComponent<KW_ScreenSpacePlanarReflection>();
            else if (!_screenSpaceReflection.enabled) _screenSpaceReflection.enabled = true;
            return _screenSpaceReflection;
        }
    }


    private KW_PlanarReflection _planarReflection;
    private KW_PlanarReflection planarReflection
    {
        get
        {
            if (_planarReflection == null) _planarReflection = tempGameObject.AddComponent<KW_PlanarReflection>();
            else if (!_planarReflection.enabled) _planarReflection.enabled = true;
            return _planarReflection;
        }
    }

    private KW_SkyCubemapReflection _skyCubemapReflection;
    private KW_SkyCubemapReflection skyCubemapReflection
    {
        get
        {
            if (_skyCubemapReflection == null) _skyCubemapReflection = tempGameObject.AddComponent<KW_SkyCubemapReflection>();
            else if (!_skyCubemapReflection.enabled) _skyCubemapReflection.enabled = true;
            return _skyCubemapReflection;
        }
    }


    private KW_WaterVolumetricLighting _waterVolumetricLighting;
    private KW_WaterVolumetricLighting waterVolumetricLighting
    {
        get
        {
            if (_waterVolumetricLighting == null) _waterVolumetricLighting = tempGameObject.AddComponent<KW_WaterVolumetricLighting>();
            else if (!_waterVolumetricLighting.enabled) _waterVolumetricLighting.enabled = true;
            return _waterVolumetricLighting;
        }
    }


    private KW_WaterOrthoDepth _orthoDepth;
    private KW_WaterOrthoDepth orthoDepth
    {
        get
        {
            if (_orthoDepth == null) _orthoDepth = tempGameObject.AddComponent<KW_WaterOrthoDepth>();
            else if (!_orthoDepth.enabled) _orthoDepth.enabled = true;
            return _orthoDepth;
        }
    }

    private KW_ShorelineWaves _shorelineMap;
    private KW_ShorelineWaves shorelineMap
    {
        get
        {
            if (_shorelineMap == null) _shorelineMap = tempGameObject.AddComponent<KW_ShorelineWaves>();
            else if (!_shorelineMap.enabled) _shorelineMap.enabled = true;
            return _shorelineMap;
        }
    }

    private KW_ScreenSpaceWaterMeshRendering _screenSpaceWaterMeshRendering;
    private KW_ScreenSpaceWaterMeshRendering screenSpaceWaterMeshRendering
    {
        get
        {
            if (_screenSpaceWaterMeshRendering == null) _screenSpaceWaterMeshRendering = tempGameObject.AddComponent<KW_ScreenSpaceWaterMeshRendering>();
            else if (!_screenSpaceWaterMeshRendering.enabled) _screenSpaceWaterMeshRendering.enabled = true;
            return _screenSpaceWaterMeshRendering;
        }
    }

#endregion

    private int prevMeshQuality = -1;
    public enum AsyncInitializingStatusEnum
    {
        NonInitialized,
        Started,
        Initialized,
        Failed
    }

    AsyncInitializingStatusEnum shoreLineInitializingStatus;
    AsyncInitializingStatusEnum flowmapInitializingStatus;
    AsyncInitializingStatusEnum causticInitializingStatus;
    private AsyncInitializingStatusEnum orthoDepthInitializingStatus;

    private const int waterLayer = 4; //water layer
    private const int waterCullingMask = 1<<4; //water layer

    private const float DomainSize = 10f;
    private const float DomainSize_LOD1 = 40f;
    private const float DomainSize_LOD2 = 160f;

    private const float VolumeLightDistanceLimitMin = 3;
    private const float VolumeLightDistanceLimitMax = 40;
    private const float VolumeLightBlurLimitMin = 1;
    private const float VolumeLightBlurLimitMax = 4.5f;

    private const float MaxTesselationFactor = 10;

    private const int DepthMaskTextureHeightLimit = 540; //fullHD * 0.5 enough even for 4k

#region  ShaderID

    private int ID_transparent = Shader.PropertyToID("KW_Transparent");
    private int ID_turbidity = Shader.PropertyToID("KW_Turbidity");
    private int ID_turbidityColor = Shader.PropertyToID("KW_TurbidityColor");
    private int ID_waterColor = Shader.PropertyToID("KW_WaterColor");

    private int ID_TesselationMaxDisplace = Shader.PropertyToID("_TesselationMaxDisplace");
    private int ID_tesselationFactor = Shader.PropertyToID("_TesselationFactor");
    private int ID_tesselationMaxDistance = Shader.PropertyToID("_TesselationMaxDistance");

    private int ID_KW_ShadowDistance = Shader.PropertyToID("KW_ShadowDistance");
    private int ID_KW_NormalScattering_Lod = Shader.PropertyToID("KW_NormalScattering_Lod");
    private int ID_KW_WaterPosition = Shader.PropertyToID("KW_WaterPosition");
    private int ID_KW_ViewToWorld = Shader.PropertyToID("KW_ViewToWorld");
    private int ID_KW_ProjToView = Shader.PropertyToID("KW_ProjToView");
    private int ID_KW_CameraMatrix = Shader.PropertyToID("KW_CameraMatrix");

    private int ID_KW_WaterFarDistance = Shader.PropertyToID("KW_WaterFarDistance");
    private int ID_FFT_Size_Normalized = Shader.PropertyToID("KW_FFT_Size_Normalized");
    private int ID_KW_FlowMapSize = Shader.PropertyToID("KW_FlowMapSize");
    private int ID_KW_FlowMapOffset = Shader.PropertyToID("KW_FlowMapOffset");
    private int ID_KW_FlowMapSpeed = Shader.PropertyToID("KW_FlowMapSpeed");
    private int ID_WindSpeed = Shader.PropertyToID("KW_WindSpeed");
    private int ID_PlanarReflectionClipPlaneOffset = Shader.PropertyToID("KW_PlanarReflectionClipPlaneOffset");
    private int ID_KW_SSR_ClipOffset = Shader.PropertyToID("KW_SSR_ClipOffset");
#endregion

#region ShaderKeywords

    private string Keyword_MultipleSimulation = "USE_MULTIPLE_SIMULATIONS";
    private string Keyword_FlowMapEditMode = "KW_FLOW_MAP_EDIT_MODE";
    private string Keyword_FlowMap = "KW_FLOW_MAP";
    private string Keyword_InteractiveWaves = "KW_INTERACTIVE_WAVES";
    private string Keyword_Caustic = "USE_CAUSTIC";
    private string Keyword_ReflectSun = "REFLECT_SUN";
    private string Keyword_UseVolumetricLight = "USE_VOLUMETRIC_LIGHT";
    private string Keyword_FixUnderwaterSkyReflection = "FIX_UNDERWATER_SKY_REFLECTION";

#endregion

    void OnAwake()
    {

    }

    async void OnEnable()
    {

        Camera.onPreRender += MyPreRender;

        //Debug.Log("Water.OnEnable");
        currentCamera = Camera.main;

        tempGameObject = new GameObject("TempWaterResources") { hideFlags = HideFlags.DontSave };
        tempGameObject.transform.parent = transform;
        tempGameObject.transform.localPosition = Vector3.zero;

        waterSharedMaterials = new List<Material>();
        commandBuffers = new Dictionary<CommandBuffer, CameraEvent>();
        camerasWithBuffers = new Dictionary<Camera, Dictionary<CommandBuffer, CameraEvent>>();

        InitializeWaterMeshGO();
        InitializeInfiniteMesh(MeshQuality);
        InitializeWaterMaterial(UseTesselation);
    }

    void OnDisable()
    {
        Camera.onPreRender -= MyPreRender;

       // Debug.Log("Water.OnDisable");
        currentCamera = Camera.main;

        if (fft_lod0 != null) fft_lod0.Release();
        if (fft_lod1 != null) fft_lod1.Release();
        if (fft_lod2 != null) fft_lod2.Release();
        if (_orthoDepth != null) _orthoDepth.Release();
        if (_depthRendering != null) _depthRendering.Release();
        if (_waterVolumetricLighting != null) _waterVolumetricLighting.Release();
        //if (renderLightsToCubemap != null) renderLightsToCubemap.Release();
        if (_screenSpaceReflection != null) _screenSpaceReflection.Release();
        if (_planarReflection != null) _planarReflection.Release();
        if (_skyCubemapReflection != null) _skyCubemapReflection.Release();
        if (_cameraColorTexture != null) _cameraColorTexture.Release();
        if (_shorelineMap != null) _shorelineMap.Release();
        if (_flowMap != null) _flowMap.Release();
        if (_causticRendering != null) _causticRendering.Release();
        if (_underwaterFX != null) _underwaterFX.Release();
        if (_screenSpaceWaterMeshRendering != null) _screenSpaceWaterMeshRendering.Release();

        KW_Extensions.SafeDestroy(waterMaterial);
        KW_Extensions.SafeDestroy(currentWaterMesh);
        KW_Extensions.SafeDestroy(waterMeshGO);
        KW_Extensions.SafeDestroy(tempGameObject);


        RemoveCommandBuffersFromCameras();

        camerasWithBuffers.Clear();
        commandBuffers.Clear();
        waterSharedMaterials.Clear();

        prevMeshQuality = -1;
        shoreLineInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        flowmapInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        causticInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        orthoDepthInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;

        Resources.UnloadUnusedAssets();
    }

    public void MyPreRender(Camera cam)
    {
        Profiler.BeginSample("WaterRendering");
#if UNITY_EDITOR
        if (cam == Camera.main || (SceneView.currentDrawingSceneView != null && cam == SceneView.currentDrawingSceneView.camera))
        {
            UpdateManual();
        }
#else
        if (cam == Camera.main)
        {
            UpdateManual();
        }
#endif
        Profiler.EndSample();
    }

    async void UpdateManual()
    {

        KW_Extensions.UpdateDeltaTime();
        GetCameraOfFocusedWindow();
        //Debug.Log(currentCamera.name);

        if (currentCamera.renderingPath == RenderingPath.Forward) currentCamera.depthTextureMode = DepthTextureMode.Depth;

        UpdateSceneLights();
        UpdateWaterPos();

        RemoveCommandBuffersFromCameras();
        if (IsRequiredUpdateCommandBuffers()) commandBuffers.Clear();

        RenderFFT(UseMultipleSimulations);

        RenderWaterDepth();

        if (UseVolumetricLight) RenderVolumetricLight();
        if (UseCausticEffect) RenderCausticEffect();
        if (UseShorelineRendering)
        {
            if (shoreLineInitializingStatus == AsyncInitializingStatusEnum.NonInitialized)
            {
                shoreLineInitializingStatus = AsyncInitializingStatusEnum.Started;

                RenderShorelineWavesWithFoam();
            }


            shorelineMap.UpdateLodLevels(currentCamera.transform.position, (int)FoamLodQuality);
        }

        if (orthoDepthInitializingStatus == AsyncInitializingStatusEnum.NonInitialized && (UseCausticEffect || UseShorelineRendering))
        {
            orthoDepthInitializingStatus = AsyncInitializingStatusEnum.Started;
            await ReadOrthoDepth();
        }
        if (UseFlowMap)
        {
            if (flowmapInitializingStatus == AsyncInitializingStatusEnum.NonInitialized)
            {
                flowmapInitializingStatus = AsyncInitializingStatusEnum.Started;
                await ReadFlowMap();
            }
        }

        RenderCameraColorTexture();

        if (ReflectionMode == ReflectionModeEnum.ScreenSpaceReflection) RenderSSPR();
        if (ReflectionMode == ReflectionModeEnum.PlanarReflection) RenderPlanarReflection();
        if (ReflectionMode == ReflectionModeEnum.CubemapReflection || ReflectionMode == ReflectionModeEnum.ScreenSpaceReflection) RenderSkyReflection();

        if (DrawScreenSpaceWaterMesh)
        {
            waterMeshRenderer.enabled = false;
            RenderSreenSpaceWaterMesh();
        }
        else waterMeshRenderer.enabled = true;

        if (UseUnderwaterEffect) RenderUnderwaterEffect();
        if (DrawToPosteffectsDepth) DrawWaterToPosteffectDepth();

        //RenderLightsToCubemap();

        AddCommandBuffersToCurrentCamera(commandBuffers);
        ReleaseInactiveResources();
        // if (UseInteractiveWaves) UpdateInteractiveWaves();


        UpdateShaderParameters();

    }

    void InitializeWaterMeshGO()
    {
        waterMeshGO = new GameObject("WaterMesh");
        waterMeshGO.hideFlags = HideFlags.DontSave;
        waterMeshGO.layer = waterLayer;
        waterMeshGO.transform.parent = tempGameObject.transform;
        waterMeshGO.transform.localPosition = Vector3.zero;
        waterMeshRenderer = waterMeshGO.AddComponent<MeshRenderer>();

        waterMeshFilter = waterMeshGO.AddComponent<MeshFilter>();
       // print("Initialize.WaterMeshGO");
    }

    public void InitializeInfiniteMesh(int newMeshQuality)
    {
        if (prevMeshQuality == newMeshQuality) return;
        prevMeshQuality = newMeshQuality;

        //float startSizeMeters = FFT_SimulationSize;
        int quadsPerStartSize = (newMeshQuality + 1) * 4;
        if(currentWaterMesh != null) KW_Extensions.SafeDestroy(currentWaterMesh);
        currentWaterMesh = KW_MeshGenerator.GeneratePlane(DomainSize * 2, quadsPerStartSize, Camera.main.farClipPlane);
        KW_Extensions.SafeDestroy(waterMeshFilter.sharedMesh);
        waterMeshFilter.sharedMesh = currentWaterMesh;
        waterMeshRenderer.sharedMaterial = waterMaterial;
       // print("Initialize.InfiniteMesh");
    }

    public void InitializeWaterMaterial(bool tryUseTesselation)
    {
        KW_Extensions.SafeDestroy(waterMaterial);
        var currentWaterShaderName = (tryUseTesselation && SystemInfo.graphicsShaderLevel >= 46) ? WaterTesselatedShaderName : WaterShaderName;
        waterMaterial = KW_Extensions.CreateMaterial(currentWaterShaderName);
        if (!waterSharedMaterials.Contains(waterMaterial)) waterSharedMaterials.Add(waterMaterial);
        if (waterMeshRenderer != null) waterMeshRenderer.sharedMaterial = waterMaterial;
    }

    private void GetCameraOfFocusedWindow()
    {
#if UNITY_EDITOR
        var focusedWindow = EditorWindow.focusedWindow;
        var currentDrawView = SceneView.currentDrawingSceneView;

        if (focusedWindow == null) isGameView = prevGameView;
        else
        {
            isGameView = EditorWindow.focusedWindow.ToString().Contains("GameView");
            var isSceneView = EditorWindow.focusedWindow.ToString().Contains("SceneView");

            if (!isGameView && !isSceneView)
            {
                isGameView = prevGameView;
            }

            prevGameView = isGameView;
        }

        if (isGameView) currentCamera = Camera.main;
        else if (currentDrawView != null) currentCamera = SceneView.currentDrawingSceneView.camera;
#else
        currentCamera = Camera.main;
#endif

    }

    void RenderFFT(bool useMultipleSimulations)
    {
        var time = KW_Extensions.Time();
        time *= TimeScale;
        //time = 0.001f;
        var windDir = Mathf.Lerp(0.05f, 0.5f, WindTurbulence);
        int fftSize = (int) FFT_SimulationSize;
        var timeScaleRelativeToFFTSize = (Mathf.RoundToInt(Mathf.Log(fftSize, 2)) - 5)/4.0f;


        float lod0_Time = Mathf.Lerp(time, time, WindTurbulence);
        lod0_Time = Mathf.Lerp(lod0_Time, lod0_Time * 0.65f, timeScaleRelativeToFFTSize);


        fft_lod0.ComputeFFT(FFT_GPU.LodPrefix.LOD0, fftSize, false, DomainSize, windDir, Mathf.Clamp(WindSpeed, 0, 2), WindRotation * Mathf.Deg2Rad, lod0_Time, waterSharedMaterials);

        if (useMultipleSimulations)
        {
            var fftSizeLod = (FFT_SimulationSize == FFT_GPU.SizeSetting.Size_512) ? 128 : 64;

            fft_lod1.ComputeFFT(FFT_GPU.LodPrefix.LOD1, fftSizeLod, false, DomainSize_LOD1, windDir, Mathf.Clamp(WindSpeed, 0, 6), WindRotation * Mathf.Deg2Rad, time * 0.9f, waterSharedMaterials);
            fft_lod2.ComputeFFT(FFT_GPU.LodPrefix.LOD2, fftSizeLod, false, DomainSize_LOD2, windDir, Mathf.Clamp(WindSpeed, 0, 40), WindRotation * Mathf.Deg2Rad, time * 0.4f, waterSharedMaterials);
        }
    }



    void RenderWaterDepth()
    {
        depthRendering.AddMaterialsToWaterRendering(waterSharedMaterials);
        var matrix = waterMeshGO.transform.localToWorldMatrix;


        var currentHeight = UseHighQualityUnderwater ? currentCamera.scaledPixelHeight * 0.5f : currentCamera.scaledPixelHeight * 0.25f;
        var targetResolutionScale = UseHighQualityUnderwater ? 0.5f : 0.25f;
        if (currentHeight > DepthMaskTextureHeightLimit)
        {
            var newRelativeScale = DepthMaskTextureHeightLimit / currentHeight;
            targetResolutionScale *= newRelativeScale;
        }

        depthRendering.RenderWaterMaskDepthNormal(currentCamera, targetResolutionScale, waterMeshFilter.sharedMesh, matrix, UseTesselation, commandBuffers, IsPositionMatrixChanged);
        IsPositionMatrixChanged = false;
        depthRendering.RenderDepthCopy(currentCamera, 0.5f, commandBuffers);


    }

    void DrawWaterToPosteffectDepth()
    {
        depthRendering.RenderWaterToPosteffectDepth(currentCamera, commandBuffers);
    }

    void RenderVolumetricLight()
    {
        waterVolumetricLighting.AddMaterialsToWaterRendering(waterSharedMaterials);
        //float distanceLimit_Res = Mathf.Lerp(-5, VolumeLightDistanceLimitMax, VolumetricLightResolutionScale);
        //float distanceLimit_Iter = Mathf.Lerp(-5, VolumeLightDistanceLimitMax, VolumetricLightIteration / 8f);
        //float distanceScale = Mathf.Clamp(distanceLimit_Res * 0.5f + distanceLimit_Iter * 0.5f, 2f, Transparent * 1.5f);

        ////float fftSizeNormalized = (Mathf.RoundToInt(Mathf.Log((int)FFT_SimulationSize, 2)) - 5) / 4.0f;
        float blurLimit_Res = Mathf.Lerp(VolumeLightBlurLimitMax, VolumeLightBlurLimitMin, VolumetricLightResolutionScale);
        float blurLimit_Iter = Mathf.Lerp(VolumeLightBlurLimitMax, VolumeLightBlurLimitMin, VolumetricLightIteration / 8f);
        float blurSize = blurLimit_Res * 0.5f + blurLimit_Iter * 0.5f;
        var dist = Mathf.Max(0.3f, Transparent * 2);

        waterVolumetricLighting.RenderVolumeLights(currentCamera, VolumetricLightResolutionScale, dist, VolumetricLightIteration, VolumetricLightBlurRadius, commandBuffers, Test4.x);

    }

    void RenderSreenSpaceWaterMesh()
    {
        var matrix = waterMeshGO.transform.localToWorldMatrix;
        screenSpaceWaterMeshRendering.RenderWaterMeshToTexture(currentCamera, ScreenSpaceWaterResolution, currentWaterMesh, matrix, waterMaterial, commandBuffers);
        screenSpaceWaterMeshRendering.BlitToTargetColor(commandBuffers);
    }

    void RenderUnderwaterEffect()
    {
        underwaterFX.AddMaterialsToWaterRendering(waterSharedMaterials);
        if(UseUnderwaterBlur) underwaterFX.RenderUnderwaterBlured(currentCamera, UnderwaterResolutionScale, UnderwaterBlurRadius, commandBuffers);
        else underwaterFX.RenderUnderwater(commandBuffers);
    }

    void RenderPlanarReflection()
    {
        //var windStr = Mathf.Clamp01((WindSpeed - 0.5f) / 5f);
       // var clipPlaneOffsetRelativeToResolution = Mathf.Lerp(10f, 1f, ReflectionTextureScale);
       // clipPlaneOffsetRelativeToResolution = Mathf.Min(clipPlaneOffsetRelativeToResolution, PlanarReflectionClipPlaneOffset);
      //  var clipPlaneOffsetRelativeToWind = Mathf.Lerp(clipPlaneOffsetRelativeToResolution, PlanarReflectionClipPlaneOffset, windStr);

        planarReflection.RenderPlanar(currentCamera, transform.position, ReflectionTextureScale*0.75f, waterSharedMaterials);
    }

    void RenderSkyReflection()
    {
        skyCubemapReflection.RenderCubemap(currentCamera, transform.position.y, CubemapUpdateInterval, CubemapCullingMask, CubemapTextureSize, waterSharedMaterials);
    }

    void RenderSSPR()
    {
        screenSpaceReflection.AddMaterialsToWaterRendering(waterSharedMaterials);
        screenSpaceReflection.RenderReflection(currentCamera, ReflectionTextureScale, SSR_DepthHolesFillDistance, commandBuffers);
    }

    void RenderCausticEffect()
    {
        causticRendering.AddMaterialsToWaterRendering(waterSharedMaterials);
        var dispersionStrength = 1 - (Mathf.RoundToInt(Mathf.Log((int)FFT_SimulationSize, 2)) - 5) / 4.0f; // 0 - 4 => 1-0
        causticRendering.Render(currentCamera, CausticStrength, CausticDepthScale, CausticTextureSize, CausticActiveLods, CausticMeshResolution, UseCausticFiltering, UseCausticDispersion, dispersionStrength, waterSharedMaterials, commandBuffers);

        causticInitializingStatus = AsyncInitializingStatusEnum.Initialized;
    }

    void RenderCameraColorTexture()
    {
        cameraColorTexture.RenderColorTexture(currentCamera, currentCamera.allowHDR, DrawScreenSpaceWaterMesh, commandBuffers);
       // if(SSR_ReflectImageEffects) cameraColorTexture.RenderColorTextureFinal(currentCamera, currentCamera.allowHDR, commandBuffers);
    }


#region  Shoreline Methods

    public bool IsEditorAllowed()
    {
        if (tempGameObject == null) return false;
        else return true;
    }

    async Task ReadOrthoDepth()
    {
        //Debug.Log("ReadOrthoDepth");
        var isInitialized = await orthoDepth.ReadOrthoDepth(waterGUID);
        orthoDepthInitializingStatus = isInitialized ? AsyncInitializingStatusEnum.Initialized : AsyncInitializingStatusEnum.Failed;
    }

    public async void RenderShorelineWavesWithFoam()
    {
        shorelineMap.AddMaterialsToWaterRendering(waterSharedMaterials);
        var isInitialized = await shorelineMap.RenderShorelineWavesWithFoam(ShorelineAreaSize, waterGUID);
        shoreLineInitializingStatus = isInitialized ? AsyncInitializingStatusEnum.Initialized : AsyncInitializingStatusEnum.Failed;
    }


    public void RenderOrthoDepth()
    {
        orthoDepth.Render(waterCullingMask, transform.position, 40, OrthoDepthTextureResolution);
    }

    public void SaveOrthoDepth()
    {
        orthoDepth.SaveOrthoDepthData(waterGUID);
    }

    public void ClearOrthoDepth()
    {
        orthoDepth.ClearOrthoDepth(waterGUID);
    }

    public async Task BakeWavesToTexture()
    {
        await shorelineMap.BakeWavesToTexture(ShorelineAreaSize, transform.position, ShorelineTextureResolution, waterGUID);
    }


    public void SaveWavesDataToFile()
    {
        shorelineMap.SaveWavesDataToFile(waterGUID);
    }

    public void ClearShorelineFoam()
    {
        shorelineMap.ClearFoam();
    }
    public void ClearShorelineWavesWithFoam()
    {
        shorelineMap.ClearShorelineWavesWithFoam(waterGUID);
    }

    public async Task<List<KW_ShorelineWaves.ShorelineWaveInfo>> GetShorelineWavesData()
    {
        return await shorelineMap.GetShorelineWavesData(waterGUID);
    }

#endregion

    void InitializeFlowMapEditorResources()
    {
        flowMap.InitializeFlowMapEditorResources(FlowMapTextureResolution, FlowMapAreaSize);
        shoreLineInitializingStatus = AsyncInitializingStatusEnum.Initialized;
    }

    public void DrawOnFlowMap(Vector3 brushPosition, Vector3 brushMoveDirection, float circleRadius, float brushStrength, bool eraseMode = false)
    {
        InitializeFlowMapEditorResources();
        flowMap.DrawOnFlowMap(brushPosition, brushMoveDirection, circleRadius, brushStrength, eraseMode);
    }

    public void RedrawFlowMap(int newTexRes, int newAreaSize)
    {
        InitializeFlowMapEditorResources();
        flowMap.RedrawFlowMap(newTexRes, newAreaSize);
    }

    public void SaveFlowMap()
    {
        InitializeFlowMapEditorResources();
        flowMap.SaveFlowMap(FlowMapAreaSize, waterGUID);
    }

    public async Task ReadFlowMap()
    {
        var isInitialized = await flowMap.ReadFlowMap(waterSharedMaterials, waterGUID);
        var flowData = flowMap.GetFlowMapDataFromFile();
        flowmapInitializingStatus = (isInitialized && flowData != null)? AsyncInitializingStatusEnum.Initialized : AsyncInitializingStatusEnum.Failed;
        if (flowData == null) return;
        FlowMapTextureResolution = flowData.TextureSize;
        FlowMapAreaSize = flowData.AreaSize;
    }

    public void ClearFlowMap()
    {
        flowMap.ClearFlowMap(waterGUID);
    }

    //void RenderLightsToCubemap()
    //{
    //    if (renderLightsToCubemap == null) renderLightsToCubemap = tempGameObject.AddComponent<KW_RenderLightsToCubemap>();
    //    renderLightsToCubemap.AddMaterialsToWaterRendering(waterSharedMaterials);

    //    var pos = currentCamera.transform.position;
    //    var cubeLightPos = new Vector3(pos.x, Mathf.Clamp(pos.y * -1 + transform.position.y * 2, -1000, 1000), pos.z);
    //    renderLightsToCubemap.RenderCubemap(cubeLightPos, currentCamera, waterSharedMaterials);
    //}

    void AddCommandBuffersToCurrentCamera(Dictionary<CommandBuffer, CameraEvent> buffers)
    {
        if(!camerasWithBuffers.ContainsKey(currentCamera)) camerasWithBuffers.Add(currentCamera, new Dictionary<CommandBuffer, CameraEvent>());
        var activeBuffers = camerasWithBuffers[currentCamera];
        foreach (var newBuffer in buffers)
        {
            if (!activeBuffers.ContainsKey(newBuffer.Key))
            {
                activeBuffers.Add(newBuffer.Key, newBuffer.Value);
                currentCamera.AddCommandBuffer(newBuffer.Value, newBuffer.Key);
                //print(currentCamera.name + "        buffer:  " + newBuffer.Key.name);
            }
        }
    }

    void RemoveCommandBuffersFromCameras()
    {
        foreach (var camWithBuffers in camerasWithBuffers)
        {
            if (camWithBuffers.Key != null)
            {
                var activeBuffers = camWithBuffers.Value;
                foreach (var cb in activeBuffers)
                {
                    camWithBuffers.Key.RemoveCommandBuffer(cb.Value, cb.Key);
                }
                activeBuffers.Clear();
            }
        }
    }

    private RenderingPath lastRenderingPath;
    private int lastScreenWidth;
    private int lastScreenHeight;
    private bool lastVolumeLightUsing;
    private float lastVolumeLightResolutuion;
    private int lastVolumeLightIterations;
    private float lastVolumeLightDistance;
    private float lastVolumeLightBlurRadius;
    private int lastLightsCount;
    private Camera lastSelectedCamera;
    private int lastMeshQuality;
    private float lastYPos;
    private bool lastTesselationUsing;
    private float lastTesselationFactor;
    private float lastTesselationMaxDistance;
    private ReflectionModeEnum lastReflectionMethod;
    private float lastReflectionTexResolution;

    private int last_SSR_DepthHolesFillDistance;
    private int lastCubemapTextureSize;
    private bool lastFixUnderwaterSkyReflection;

    private bool lastUnderwaterEffectUsing;
    private bool lastUseHighQualityUnderwater;
    private bool lastUnderwaterEffectBlurUsing;
    private float lastUnderwaterResolutionScale;
    private float lastUnderwaterEffectBlurRadius;

    private bool lastDrawScreenSpaceWaterMesh;
    float lastScreenSpaceWaterResolution;
    private bool lastDrawToPosteffectsDepth;

    bool IsRequiredUpdateCommandBuffers()
    {

        if (currentCamera == null) return false;

        bool isRequiredUpdate = false;


        if (lastSelectedCamera != currentCamera)
        {
            lastSelectedCamera = currentCamera;
            isRequiredUpdate = true;
        }
        if (lastRenderingPath != currentCamera.actualRenderingPath)
        {
            lastRenderingPath = currentCamera.actualRenderingPath;
            isRequiredUpdate =  true;
        }
        if (lastScreenWidth != currentCamera.pixelWidth || lastScreenHeight != currentCamera.pixelHeight)
        {
            lastScreenWidth = currentCamera.pixelWidth;
            lastScreenHeight = currentCamera.pixelHeight;
            isRequiredUpdate = true;
        }
        if (lastVolumeLightUsing != UseVolumetricLight)
        {
            lastVolumeLightUsing = UseVolumetricLight;
            isRequiredUpdate = true;
        }
        if (Math.Abs(lastVolumeLightResolutuion - VolumetricLightResolutionScale) > 0.0001f) //todo check bug with 0.05
        {
            lastVolumeLightResolutuion = VolumetricLightResolutionScale;
            isRequiredUpdate = true;
        }
        if (lastVolumeLightIterations != VolumetricLightIteration)
        {
            lastVolumeLightIterations = VolumetricLightIteration;
            isRequiredUpdate = true;
        }
        if (Math.Abs(lastVolumeLightDistance - VolumetricLightDistance) > 0.1f)
        {
            lastVolumeLightDistance = VolumetricLightDistance;
            isRequiredUpdate = true;
        }
        if (Math.Abs(lastVolumeLightBlurRadius - VolumetricLightBlurRadius) > 0.1f)
        {
            lastVolumeLightBlurRadius = VolumetricLightBlurRadius;
            isRequiredUpdate = true;
        }

        if (lastUnderwaterEffectUsing != UseUnderwaterEffect)
        {
            lastUnderwaterEffectUsing = UseUnderwaterEffect;
            isRequiredUpdate = true;
        }

        if (lastUseHighQualityUnderwater != UseHighQualityUnderwater)
        {
            lastUseHighQualityUnderwater = UseHighQualityUnderwater;
            isRequiredUpdate = true;
        }

        if (lastUnderwaterEffectBlurUsing != UseUnderwaterBlur)
        {
            lastUnderwaterEffectBlurUsing = UseUnderwaterBlur;
            isRequiredUpdate = true;
        }

        if (Math.Abs(lastUnderwaterEffectBlurRadius - UnderwaterBlurRadius) > 0.1f)
        {
            lastUnderwaterEffectBlurRadius = UnderwaterBlurRadius;
            isRequiredUpdate = true;
        }


        if (Math.Abs(lastUnderwaterResolutionScale - UnderwaterResolutionScale) > 0.05f)
        {
            lastUnderwaterResolutionScale = UnderwaterResolutionScale;
            isRequiredUpdate = true;
        }

        if (lastTesselationUsing != UseTesselation)
        {
            lastTesselationUsing = UseTesselation;
            isRequiredUpdate = true;
        }
        if (lastLightsCount != KW_WaterVolumetricLighting.ActiveLights.Count)
        {
            lastLightsCount = KW_WaterVolumetricLighting.ActiveLights.Count;
            isRequiredUpdate = true;
        }

        if (lastMeshQuality != MeshQuality)
        {
            lastMeshQuality = MeshQuality;
            isRequiredUpdate = true;
        }

        if (Math.Abs(lastYPos - transform.position.y) > 0.001f)
        {
            lastYPos = transform.position.y;
            isRequiredUpdate = true;
        }

        if (Math.Abs(lastTesselationFactor - TesselationFactor) > 0.01f)
        {
            lastTesselationFactor = TesselationFactor;
            isRequiredUpdate = true;
        }


        if (Math.Abs(lastTesselationMaxDistance - TesselationMaxDistance) > 0.1f)
        {
            lastTesselationMaxDistance = TesselationMaxDistance;
            isRequiredUpdate = true;
        }

        if (lastReflectionMethod != ReflectionMode)
        {
            lastReflectionMethod = ReflectionMode;
            isRequiredUpdate = true;
        }

        if (last_SSR_DepthHolesFillDistance != SSR_DepthHolesFillDistance)
        {
            last_SSR_DepthHolesFillDistance = SSR_DepthHolesFillDistance;
            isRequiredUpdate = true;
        }
        if (lastFixUnderwaterSkyReflection != FixUnderwaterSkyReflection)
        {
            lastFixUnderwaterSkyReflection = FixUnderwaterSkyReflection;
            isRequiredUpdate = true;
        }

        if (Math.Abs(lastReflectionTexResolution - ReflectionTextureScale) > 0.05f)
        {
            lastReflectionTexResolution = ReflectionTextureScale;
            isRequiredUpdate = true;
        }

        if (lastCubemapTextureSize != CubemapTextureSize)
        {
            lastCubemapTextureSize = CubemapTextureSize;
            isRequiredUpdate = true;
        }

        if (Math.Abs(lastScreenSpaceWaterResolution - ScreenSpaceWaterResolution) > 0.01)
        {
            lastScreenSpaceWaterResolution = ScreenSpaceWaterResolution;
            isRequiredUpdate = true;
        }

        if (lastDrawScreenSpaceWaterMesh != DrawScreenSpaceWaterMesh)
        {
            lastDrawScreenSpaceWaterMesh = DrawScreenSpaceWaterMesh;
            isRequiredUpdate = true;
        }

        if (lastDrawToPosteffectsDepth != DrawToPosteffectsDepth)
        {
            lastDrawToPosteffectsDepth = DrawToPosteffectsDepth;
            isRequiredUpdate = true;
        }

        return isRequiredUpdate;
    }

    private void ReleaseInactiveResources()
    {
        if (_fft_lod1 != null && _fft_lod1.enabled && !UseMultipleSimulations)
        {
            _fft_lod1.Release();
            _fft_lod1.enabled = false;
        }

        if (_fft_lod2 != null && _fft_lod2.enabled && !UseMultipleSimulations)
        {
            _fft_lod2.Release();
            _fft_lod2.enabled = false;
        }

        if (_waterVolumetricLighting != null && _waterVolumetricLighting.enabled && !UseVolumetricLight)
        {
            _waterVolumetricLighting.Release();
            _waterVolumetricLighting.enabled = false;
        }

        if (_orthoDepth != null && _orthoDepth.enabled && !UseShorelineRendering && !UseCausticEffect)
        {
            _orthoDepth.Release();
            _orthoDepth.enabled = false;
        }

        if (_shorelineMap != null && _shorelineMap.enabled && !UseShorelineRendering)
        {
            _shorelineMap.Release();
            _shorelineMap.enabled = false;
            shoreLineInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        }

        if (_causticRendering != null && _causticRendering.enabled && !UseCausticEffect)
        {
            _causticRendering.Release();
            _causticRendering.enabled = false;
            causticInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        }

        if (_flowMap != null && _flowMap.enabled && !UseFlowMap)
        {
            _flowMap.Release();
            _flowMap.enabled = false;
            flowmapInitializingStatus = AsyncInitializingStatusEnum.NonInitialized;
        }

        if (_underwaterFX != null && _underwaterFX.enabled && !UseUnderwaterEffect)
        {
            _underwaterFX.Release();
            _underwaterFX.enabled = false;
        }

        if (_screenSpaceReflection != null && _screenSpaceReflection.enabled && ReflectionMode != ReflectionModeEnum.ScreenSpaceReflection)
        {
            _screenSpaceReflection.Release();
            _screenSpaceReflection.enabled = false;
        }

        if (_planarReflection != null && _planarReflection.enabled && ReflectionMode != ReflectionModeEnum.PlanarReflection)
        {
            _planarReflection.Release();
            _planarReflection.enabled = false;
        }

        if (_skyCubemapReflection != null && _skyCubemapReflection.enabled && (ReflectionMode != ReflectionModeEnum.CubemapReflection && ReflectionMode != ReflectionModeEnum.ScreenSpaceReflection))
        {
            _skyCubemapReflection.Release();
            _skyCubemapReflection.enabled = false;
        }

        if (_screenSpaceWaterMeshRendering != null && _screenSpaceWaterMeshRendering.enabled && !DrawScreenSpaceWaterMesh)
        {
            _screenSpaceWaterMeshRendering.Release();
            _screenSpaceWaterMeshRendering.enabled = false;
        }
    }


    //------------------------------------------------------- new -----------------------------------------------------------------------------------------------


    public Vector3 waterWorldPos { get; private set; }


    private Dictionary<int, Vector2> DetailRelativeSize = new Dictionary<int, Vector2>()
    {
        { 512, new Vector2(256, 2) },
        { 256,  new Vector2(128, 3)},
        { 128,  new Vector2(64, 4)},
        { 64,  new Vector2(64, 4)},
        { 32,  new Vector2(64, 5)},
    };

    private const float TrisCountLodStep = 25;
    private const float TrisTexSizeLodStep = 16;
    private Vector4[] CausticTexTrisScale =
    {
        new Vector4(400, 1024, 140, 250),
        new Vector4(350, 768, 120, 200),
        new Vector4(300, 512, 100, 160),
        new Vector4(200, 300, 80, 130),
        new Vector4(100, 200, 60, 110),
    }; //512 - 32



    private float interactiveWaterCurrentTime;
    private Vector3 lastInteractPos;
    private Vector3 lastInteractOffset;
    public Vector3 InteractPos; //relative to camera frustrum area position center
    private Vector2 shorelineWavesLastDepthPos = new Vector3(float.MaxValue, 0);
    private float lastWaterLevel = float.MaxValue;



    public async void UpdateShaderParameters()
    {
        Shader.SetGlobalVector("Test4", Test4);

        Shader.SetGlobalFloat("KW_CameraRotation", currentCamera.transform.eulerAngles.y / 360f);
       // print(currentCamera.transform.eulerAngles.y / 360f);
       Shader.SetGlobalVector("KW_CameraForwardDir", currentCamera.transform.forward);

        Shader.SetGlobalFloat(ID_KW_ShadowDistance, QualitySettings.shadowDistance);

        var currentTessFactor = UseTesselation ? Mathf.Lerp(2, MaxTesselationFactor, TesselationFactor) : 0;


        var maxTessCullDisplace = Mathf.Max(WindSpeed, 2);

        var fftSize       = (int)FFT_SimulationSize;
        var normalLodScale = Mathf.RoundToInt(Mathf.Log(fftSize, 2)) - 4;
        var scatterLod = Mathf.Lerp(normalLodScale / 2.0f + 0.5f, normalLodScale / 2.0f + 1.5f, Mathf.Clamp01(WindSpeed / 3f));

        var projToView = GL.GetGPUProjectionMatrix(currentCamera.projectionMatrix, true).inverse;
        projToView[1, 1] *= -1;

        var viewProjection = currentCamera.nonJitteredProjectionMatrix * currentCamera.transform.worldToLocalMatrix;
        var viewToWorld = currentCamera.cameraToWorldMatrix;

        float farDist = 500;
        var mainCam = Camera.main;
        if(mainCam != null) farDist = mainCam.farClipPlane * 0.5f;

        var waterPos = transform.position;

        float fftSizeNormalized = (Mathf.RoundToInt(Mathf.Log((int)FFT_SimulationSize, 2)) - 5) / 4.0f;

        Shader.SetGlobalMatrix(ID_KW_ViewToWorld, viewToWorld);
        Shader.SetGlobalMatrix(ID_KW_ProjToView, projToView);
        Shader.SetGlobalMatrix(ID_KW_CameraMatrix, viewProjection);

        foreach (var mat in waterSharedMaterials)
        {
            if(mat == null) continue;

            mat.SetMatrix(ID_KW_ViewToWorld, viewToWorld);
            mat.SetMatrix(ID_KW_ProjToView, projToView);
            mat.SetMatrix(ID_KW_CameraMatrix, viewProjection);

            mat.SetVector(ID_KW_WaterPosition, waterPos);

            mat.SetColor(ID_turbidityColor, TurbidityColor);
            mat.SetColor(ID_waterColor, WaterColor);

            mat.SetFloat(ID_FFT_Size_Normalized, fftSizeNormalized);
            mat.SetFloat(ID_WindSpeed, WindSpeed);
            mat.SetFloat(ID_KW_NormalScattering_Lod, scatterLod);
            mat.SetFloat(ID_KW_WaterFarDistance, farDist);
            mat.SetFloat(ID_transparent, Transparent);
            mat.SetFloat(ID_turbidity, Turbidity);
            mat.SetFloat(ID_tesselationFactor, currentTessFactor);
            mat.SetFloat(ID_tesselationMaxDistance, TesselationMaxDistance);
            mat.SetFloat(ID_TesselationMaxDisplace, maxTessCullDisplace);

            if (ReflectSun) mat.EnableKeyword(Keyword_ReflectSun);
            else mat.DisableKeyword(Keyword_ReflectSun);

            if(UseVolumetricLight) mat.EnableKeyword(Keyword_UseVolumetricLight);
            else mat.DisableKeyword(Keyword_UseVolumetricLight);

            if(FixUnderwaterSkyReflection) mat.EnableKeyword(Keyword_FixUnderwaterSkyReflection);
            else mat.DisableKeyword(Keyword_FixUnderwaterSkyReflection);

            if (UseFlowMap)
            {
                mat.SetFloat(ID_KW_FlowMapSize, FlowMapAreaSize);
                mat.SetVector(ID_KW_FlowMapOffset, FlowMapOffset);
                mat.SetFloat(ID_KW_FlowMapSpeed, FlowMapSpeed);
            }

            if (UseCausticEffect && causticInitializingStatus == AsyncInitializingStatusEnum.Initialized) mat.EnableKeyword(Keyword_Caustic);
            else mat.DisableKeyword(Keyword_Caustic);

            if (UseMultipleSimulations) mat.EnableKeyword(Keyword_MultipleSimulation);
            else mat.DisableKeyword(Keyword_MultipleSimulation);

            if (FlowMapInEditMode) mat.EnableKeyword(Keyword_FlowMapEditMode);
            else mat.DisableKeyword(Keyword_FlowMapEditMode);

            if (UseFlowMap && flowmapInitializingStatus == AsyncInitializingStatusEnum.Initialized) mat.EnableKeyword(Keyword_FlowMap);
            else mat.DisableKeyword(Keyword_FlowMap);

            if (UseShorelineRendering && shoreLineInitializingStatus == AsyncInitializingStatusEnum.Initialized)
            {
                mat.EnableKeyword("USE_SHORELINE");
            }
            else mat.DisableKeyword("USE_SHORELINE");

            if (ReflectionMode == ReflectionModeEnum.PlanarReflection)
            {
                mat.SetFloat(ID_PlanarReflectionClipPlaneOffset, PlanarReflectionClipPlaneOffset);
                mat.EnableKeyword("PLANAR_REFLECTION");
                mat.DisableKeyword("SSPR_REFLECTION");
            }
            else if (ReflectionMode == ReflectionModeEnum.ScreenSpaceReflection)
            {
                mat.DisableKeyword("PLANAR_REFLECTION");
                mat.EnableKeyword("SSPR_REFLECTION");
                mat.SetFloat(ID_KW_SSR_ClipOffset, ScreenSpaceClipPlaneOffset);
            }
            else
            {
                mat.DisableKeyword("PLANAR_REFLECTION");
                mat.DisableKeyword("SSPR_REFLECTION");
            }
        }

    }

    void UpdateSceneLights()
    {
        KW_WaterLights.UpdateLightParams();
    }

    void UpdateWaterPos()
    {
        Shader.SetGlobalFloat("KW_Time", KW_Extensions.Time());
        if (WaterMeshType == WaterMeshTypeEnum.Infinite)
        {
            if (currentCamera != null)
            {
                var pos = waterMeshGO.transform.position;
                var camPos = currentCamera.transform.position;

                //transform.position = new Vector3(camPos.x, pos.y, camPos.z);
                var relativeToCamPos = new Vector3(camPos.x, pos.y, camPos.z);
                waterWorldPos = relativeToCamPos;
                if (Vector3.Distance(pos, relativeToCamPos) >= UpdatePositionEveryMeters)
                {
                    IsPositionMatrixChanged = true;
                    waterMeshGO.transform.position = relativeToCamPos;
                }

            }
        }
    }

    //void UpdateInteractiveWaves()
    //{
    //    InteractPos = KW_Extensions.GetRelativeToCameraAreaPos(currentCamera, InteractiveWavesAreaSize, transform.position.y);
    //    var deltaFPS = 1.0f / InteractiveWavesFPS;
    //    if (interactiveWaterCurrentTime >= deltaFPS)
    //    {
    //        ConstantUpdateInteractiveWaves();
    //        interactiveWaterCurrentTime = 0;
    //    }
    //    //else
    //    //{
    //    //    float interactLerp = interactiveWaterCurrentTime / deltaFPS;
    //    //    waterMaterial.SetFloat("KW_InteractLerp", interactLerp);
    //    //    depthMaterial.SetFloat("KW_InteractLerp", interactLerp);
    //    //    causticDecalMaterial.SetFloat("KW_InteractLerp", interactLerp);
    //    //}

    //    interactiveWaterCurrentTime += KW_Extensions.DeltaTime();
    //}

    //private void ConstantUpdateInteractiveWaves()
    //{
    //    InteractPos += ComputeInteractiveWavesJitter(); //we compute only 4 points around the pixel center and we should use some jitter for avoid any artifacts

    //    var offset = (InteractPos - lastInteractPos) / (InteractiveWavesAreaSize);

    //    waterMaterial.SetVector("KW_InteractPos", InteractPos);
    //    //depthMaterial.SetVector("KW_InteractPos", InteractPos);
    //    causticDecalMaterial.SetVector("KW_InteractPos", InteractPos);
    //    interactiveWavesMaterial.SetVector("KW_LastAreaOffset", lastInteractOffset + offset);
    //    interactiveWavesMaterial.SetVector("KW_AreaOffset", offset);

    //    int endIndex;
    //    var interactScripts = KW_InteractiveWavesVariables.GetInteractScriptsInArea(InteractPos, InteractiveWavesAreaSize, out endIndex);

    //    for (var i = 0; i < endIndex; i++)
    //    {
    //        var script = interactScripts[i];

    //        var forceInDepth = script.CachedTransform.position.y + script.Offset.y - InteractPos.y;
    //        var isUnderwater = forceInDepth < 0;
    //        if (isUnderwater) forceInDepth *= 0.5f; //in underwater the force works without surface intersection. Like depth intersection.
    //        forceInDepth = 1 - Mathf.Clamp01(Mathf.Abs(forceInDepth / script.DrawSize * 0.5f)); //normalize distance to 0-1, where 1 is center.
    //        forceInDepth *= forceInDepth * script.GetForce();

    //        interactiveWaves.AddPositionToDrawArray(InteractPos, script.CachedTransform.position + script.Offset + new Vector3(InteractPos.x - lastInteractPos.x, 0, InteractPos.z - lastInteractPos.z),
    //            script.DrawSize, forceInDepth, InteractiveWavesAreaSize);
    //    }

    //    for (int i = 0; i < 200; i++)
    //    {
    //        interactiveWaves.AddPositionToDrawArray(InteractPos, Random.insideUnitSphere * InteractiveWavesAreaSize, 0.025f, 0.5f, InteractiveWavesAreaSize);
    //    }

    //    interactiveWaves.RenderWaves(InteractiveWavesFPS, WaveSpeed, InteractiveWavesQuality, InteractiveWavesAreaSize,
    //        interactiveWavesMaterial, waterMaterial, causticDecalMaterial);

    //    lastInteractPos = InteractPos;
    //    lastInteractOffset = offset;
    //}

    //Vector3 ComputeInteractiveWavesJitter()
    //{
    //    var jitterSin = Mathf.Sin(Time.time);
    //    var jitterCos = Mathf.Cos(Time.time);
    //    var jitter    = (InteractiveWavesAreaSize * 0.01f) * new Vector3(jitterSin, 0, jitterCos);
    //    return jitter;
    //}

    //void UpdateCameraColor()
    //{
    //    cameraColorCopy.RenderCopyColor(currentCamera, ColorTextureScale);
    //}

    //void DrawWater()
    //{
    //    var matrix = transform.localToWorldMatrix;
    //    drawWaterMesh.RenderWater(currentCamera, waterMeshFilter.sharedMesh, matrix, waterMeshRenderer.sharedMaterial);
    //}


    void Update()
    {
        if (!Application.isPlaying) UpdateWaterPos();
    }

}
