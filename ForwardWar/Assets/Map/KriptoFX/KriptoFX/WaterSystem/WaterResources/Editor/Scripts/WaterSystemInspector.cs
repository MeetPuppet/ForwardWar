
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[System.Serializable]
[CustomEditor(typeof(WaterSystem))]
public class WaterSystemInspector : Editor
{
    private Texture2D ButtonTex;
    private GUIStyle buttonStyle;
    private WaterSystem waterSystem;

    private float floatMapCircleRadiusDefault = 1f;
    private bool  leftKeyPressed;
    private bool keyPressed;
    private bool  isFlowMapChanged;
    private bool isActive;

    private static float InteractiveWaveHelperEnableTime = 300000;
    private float interactiveWaveCurrentTime;
    private const int InteractMaxResolution = 2048;

    GUIStyle helpBoxStyle;
    private VideoTooltipWindow window;
    private string pathToHelpVideos;

    void OnDestroy()
    {
        KW_Extensions.SafeDestroy(ButtonTex);
    }

    void Awake()
    {
        buttonStyle = new GUIStyle();
        buttonStyle.overflow.left = buttonStyle.overflow.right = 3;
        buttonStyle.overflow.top = 2;
        buttonStyle.overflow.bottom = 0;
        if (ButtonTex == null)
        {
            if(EditorGUIUtility.isProSkin) ButtonTex = CreateTex(32, 32, new Color(80 / 255f, 80 / 255f, 80 / 255f));
            else ButtonTex = CreateTex(32, 32, new Color(171 / 255f, 171 / 255f, 171 / 255f));
        }
        buttonStyle.normal.background = ButtonTex;

        helpBoxStyle = new GUIStyle("button");
        helpBoxStyle.alignment = TextAnchor.MiddleCenter;
        helpBoxStyle.stretchHeight = false;
        helpBoxStyle.stretchWidth = false;

    }

    public override void OnInspectorGUI()
    {
        waterSystem = (WaterSystem)target;
        if (waterSystem.enabled && waterSystem.gameObject.activeSelf && waterSystem.IsEditorAllowed())
        {
            isActive = true;
            GUI.enabled = true;
        }
        else
        {
            isActive = false;
            GUI.enabled = false;
        }
        UpdateWaterGUI();
    }

    public void OnSceneGUI()
    {

        if (!isActive) return;

        if (waterSystem.FlowMapInEditMode) DrawFlowMapEditor();
        if (waterSystem.ShorelineInEditMode) DrawShorelineEditor();
        //if (waterSystem.UseInteractiveWaves && interactiveWaveCurrentTime < InteractiveWaveHelperEnableTime)
        //{
        //    interactiveWaveCurrentTime += KW_Extensions.DeltaTime();
        //    DrawInteractiveWavesHelpers();
        //}

    }

    void UpdateWaterGUI()
    {
       // EditorGUI.BeginChangeCheck();

        //waterSystem.ShorelineMaterial = EditorGUILayout.ObjectField("ShorelineMaterial", waterSystem.ShorelineMaterial, typeof(Material), false) as Material;

        //waterSystem.FoamTex = EditorGUILayout.ObjectField("FoamTex", waterSystem.FoamTex, typeof(Texture2D), false) as Texture2D;
        //waterSystem.FoamDispTex = EditorGUILayout.ObjectField("FoamDispTex", waterSystem.FoamDispTex, typeof(Texture2D), false) as Texture2D;
        //waterSystem.FoamNormalTex = EditorGUILayout.ObjectField("FoamNormalTex", waterSystem.FoamNormalTex, typeof(Texture2D), false) as Texture2D;

        //waterSystem.NoiseTex = EditorGUILayout.ObjectField("NoiseTex", waterSystem.NoiseTex, typeof(Texture2D), false) as Texture2D;

        //waterSystem.Wave_Y = EditorGUILayout.CurveField("Wave_Y", waterSystem.Wave_Y);
        //waterSystem.Wave_negY = EditorGUILayout.CurveField("Wave_negY", waterSystem.Wave_negY);
        //waterSystem.Wave_Z = EditorGUILayout.CurveField("Wave_Z", waterSystem.Wave_Z);
        //waterSystem.Foam = EditorGUILayout.CurveField("Foam", waterSystem.Foam);

       // waterSystem.Test4 = EditorGUILayout.Vector4Field("Test4", waterSystem.Test4);
       // waterSystem.TestObj = (GameObject) EditorGUILayout.ObjectField(waterSystem.TestObj, typeof(GameObject), true);
        //waterSystem.Iterations     = EditorGUILayout.IntField("Iterations", waterSystem.Iterations);
        //waterSystem.Interpolations = EditorGUILayout.FloatField("Interpolations", waterSystem.Interpolations);
        ////waterSystem.aTexture = (Texture2D)EditorGUILayout.ObjectField("Image", waterSystem.aTexture, typeof(Texture2D), false);


        AddColorSetting();
        AddWaves();
        AddReflection();
        //AddInteractiveWaves();
        AddFlowMap();
        AddShorelineMap();
        AddVolumeScattering();
        AddCausticEffect();
        AddUnderwaterEffect();
        AddRendering();

        Undo.RecordObject(target, "Changed water parameters");
    }

    float Slider(string text, string description, float value, float leftValue, float rightValue)
    {
        EditorGUILayout.BeginHorizontal();
        var newValue = EditorGUILayout.Slider(new GUIContent(text, description), value, leftValue, rightValue);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow(text);
        EditorGUILayout.EndHorizontal();
        return newValue;
    }

    int IntSlider(string text, string description, int value, int leftValue, int rightValue)
    {
        EditorGUILayout.BeginHorizontal();
        var newValue = EditorGUILayout.IntSlider(new GUIContent(text, description), value, leftValue, rightValue);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow(text);
        EditorGUILayout.EndHorizontal();
        return newValue;
    }

    int IntField(string text, string description, int value)
    {
        EditorGUILayout.BeginHorizontal();
        var newValue = EditorGUILayout.IntField(new GUIContent(text, description), value);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow(text);
        EditorGUILayout.EndHorizontal();
        return newValue;
    }

    float FloatField(string text, string description, float value)
    {
        EditorGUILayout.BeginHorizontal();
        var newValue = EditorGUILayout.FloatField(new GUIContent(text, description), value);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow(text);
        EditorGUILayout.EndHorizontal();
        return newValue;
    }

    Vector2 Vector2Field(string text, string description, Vector2 value)
    {
        EditorGUILayout.BeginHorizontal();
        var newValue = EditorGUILayout.Vector2Field(new GUIContent(text, description), value);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow(text);
        EditorGUILayout.EndHorizontal();
        return newValue;
    }

    Color ColorField(string text, string description, Color value, bool shoeEyedropper, bool showAlpha, bool hdr)
    {
        EditorGUILayout.BeginHorizontal();
        var newValue = EditorGUILayout.ColorField(new GUIContent(text, description), value, shoeEyedropper, showAlpha, hdr);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow(text);
        EditorGUILayout.EndHorizontal();
        return newValue;
    }

    Enum EnumPopup(string text, string description, Enum value)
    {
        EditorGUILayout.BeginHorizontal();
        var newValue = EditorGUILayout.EnumPopup(new GUIContent(text, description), value);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow(text);
        EditorGUILayout.EndHorizontal();
        return newValue;
    }

    int MaskField(string text, string description, int mask, string[] layers)
    {
        EditorGUILayout.BeginHorizontal();
        var newValue = EditorGUILayout.MaskField(new GUIContent(text, description), mask, layers);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow(text);
        EditorGUILayout.EndHorizontal();
        return newValue;
    }

    bool Toggle(string text, string description, bool value, params GUILayoutOption[] options)
    {
        EditorGUILayout.BeginHorizontal();
        var newValue = EditorGUILayout.Toggle(new GUIContent(text, description), value, options);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow(text);
        EditorGUILayout.EndHorizontal();
        return newValue;
    }

    bool Toggle(bool value, string toogleName, string description, params GUILayoutOption[] options)
    {

        var newValue = EditorGUILayout.Toggle(value, description, options);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow(toogleName);

        return newValue;
    }

    void OpenHelpVideoWindow(string filename)
    {
        if (window != null) window.Close();
        window = (VideoTooltipWindow)EditorWindow.GetWindow(typeof(VideoTooltipWindow));
        if(string.IsNullOrEmpty(pathToHelpVideos)) pathToHelpVideos = GetPathToHelpVideos();
        window.VideoClipFileURI = Path.Combine(pathToHelpVideos, filename + ".mp4");
        window.tempGO = waterSystem.tempGameObject;
        window.maxSize = new Vector2(854, 480);
        window.minSize = new Vector2(854, 480);
        window.Show();
    }


    public static string GetPathToHelpVideos()
    {
        var dirs = Directory.GetDirectories(Application.dataPath, "HelpVideos", SearchOption.AllDirectories);
        Debug.Log("pathToHelpVideosFolder " + dirs[0]);
        return dirs.Length != 0 ? dirs[0] : string.Empty;
    }

    void AddColorSetting()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal(buttonStyle);

        EditorGUILayout.LabelField("", GUILayout.MaxWidth(4));
        waterSystem.ShowColorSettings = EditorGUILayout.Foldout(waterSystem.ShowColorSettings, new GUIContent("Color Settings"), true);

        EditorGUILayout.EndHorizontal();
        //EditorGUILayout.LabelField(waterSystem._waterGUID);
        if (waterSystem.ShowColorSettings)
        {


            waterSystem.Transparent = Slider("Transparent", "Opacity in meters", waterSystem.Transparent, 0.1f, 50f);
            waterSystem.WaterColor = ColorField("Water Color", "This is the solution color of clean water without impurities", waterSystem.WaterColor, false, false, false);
            EditorGUILayout.Space();
            waterSystem.TurbidityColor = ColorField("Turbidity Color", "Color of suspended particles, such as algae or dirt", waterSystem.TurbidityColor, false, false, false);
            waterSystem.Turbidity = Slider("Turbidity", "Total suspended solids in water, water purity", waterSystem.Turbidity, 0.05f, 1f);
        }

        EditorGUILayout.EndVertical();
    }

    void AddWaves()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal(buttonStyle);

        EditorGUILayout.LabelField("", GUILayout.MaxWidth(4));
        waterSystem.ShowWaves = EditorGUILayout.Foldout(waterSystem.ShowWaves, new GUIContent("Waves"), true);

        EditorGUILayout.EndHorizontal();

        if (waterSystem.ShowWaves)
        {
            waterSystem.FFT_SimulationSize = (FFT_GPU.SizeSetting)EnumPopup("FFT Simulation Size", "Quality of wave simulation", waterSystem.FFT_SimulationSize);
            EditorGUI.BeginChangeCheck();
            waterSystem.UseMultipleSimulations = Toggle("Use Multiple Simulations", "Use this option to avoid tiling, also allows you to use waves with strong wind more than 2 meters per second",  waterSystem.UseMultipleSimulations);
            waterSystem.WindSpeed = Slider("Wind Speed", "Wind speed in meters", waterSystem.WindSpeed, 0.1f, 15.0f);
            waterSystem.WindRotation = Slider("Wind Rotation", "Wind rotation in degrees", waterSystem.WindRotation, 0.0f, 360.0f);
            waterSystem.WindTurbulence = Slider("Wind Turbulence", "The power of the wind turbidity. Smaller turbulence = calmer water", waterSystem.WindTurbulence, 0.0f, 1.0f);
            waterSystem.TimeScale = Slider("Time Scale", "Time speed of wave simulation", waterSystem.TimeScale, 0.25f, 2.0f);
            // if (EditorGUI.EndChangeCheck()) waterSystem.UpdateMultipleSimulations();

        }

        EditorGUILayout.EndVertical();
    }

    void AddReflection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal(buttonStyle);

        EditorGUILayout.LabelField("", GUILayout.MaxWidth(4));
        waterSystem.ShowReflectionSettings = EditorGUILayout.Foldout(waterSystem.ShowReflectionSettings, new GUIContent("Reflection"), true);

        EditorGUILayout.EndHorizontal();

        if (waterSystem.ShowReflectionSettings)
        {
            waterSystem.ReflectSun = Toggle("Reflect Sunlight", "Rendering of the solar path on the water", waterSystem.ReflectSun);
            waterSystem.ReflectionMode = (WaterSystem.ReflectionModeEnum)EnumPopup("Reflection Mode",
                "Cubemap(reflection probe) : most expensive but can reflect the surface behind the camera. Often used only for sky rendering" +
                " \r\n \r\n" + "Screen space planar reflection: can reflect only what you see above the water, for example, " +
                "can not reflect the bottom of a car, but this method almost free, use it if possible. " +
                "This method compute reflection using one iteration unlike standard \"screen space reflection\" with multiple iterations" +
                " \r\n \r\n" + "Planar reflection: expensive, but can reflect more accurate reflections", waterSystem.ReflectionMode);

            if (waterSystem.ReflectionMode == WaterSystem.ReflectionModeEnum.PlanarReflection )
            {
                waterSystem.ReflectionTextureScale = Slider("Texture Scale", "Reflection texture resolution relative to screen size, less resolution = faster rendering", waterSystem.ReflectionTextureScale, 0.25f, 1);
                waterSystem.PlanarReflectionClipPlaneOffset = Slider("Clip Plane Offset", "Use it for avoid reflection artefacts near to the water edge", waterSystem.PlanarReflectionClipPlaneOffset, 0, 0.07f);

            }

            if (waterSystem.ReflectionMode == WaterSystem.ReflectionModeEnum.ScreenSpaceReflection)
            {
                waterSystem.ReflectionTextureScale = Slider("Texture Scale", "Reflection texture resolution relative to screen size, less resolution = faster rendering", waterSystem.ReflectionTextureScale, 0.25f, 1);
                waterSystem.ScreenSpaceClipPlaneOffset = Slider("Clip Plane Offset", "Use it for avoid reflection artefacts near to the water edge", waterSystem.ScreenSpaceClipPlaneOffset, 0, 0.07f);
                waterSystem.SSR_DepthHolesFillDistance = IntSlider("Depth Holes Fill Distance", "Filling pixels with lost reflection information", waterSystem.SSR_DepthHolesFillDistance, 0, 25);

            }

            if (waterSystem.ReflectionMode == WaterSystem.ReflectionModeEnum.CubemapReflection || waterSystem.ReflectionMode == WaterSystem.ReflectionModeEnum.ScreenSpaceReflection)
            {
                var layerNames = new List<string>();
                for (int i = 0; i <= 31; i++)
                {
                   layerNames.Add(LayerMask.LayerToName(i));
                }
                EditorGUILayout.Space();
                var mask = MaskField("Cubemap Culling Mask", "Culling mask for reflection camera rendering", waterSystem.CubemapCullingMask, layerNames.ToArray());
                waterSystem.CubemapCullingMask = mask & ~(1 << 4);
                waterSystem.CubemapUpdateInterval = Slider("Cubemap Update Interval", "Realtime cubemap rendering very expensive, so you can render cubemap once per few frames, not each frame" +
                                                                                      "For example you can render only sky layer once in 60 seconds. In this case it's performance free", waterSystem.CubemapUpdateInterval, 0, 60);
                var texSize = IntSlider("Cubemap Size", "Cubemap texture resolution per side", waterSystem.CubemapTextureSize, 16, 512);
                texSize = Mathf.RoundToInt(Mathf.Log(texSize, 2));
                waterSystem.CubemapTextureSize = (int)Mathf.Pow(2, texSize);


            }

            waterSystem.FixUnderwaterSkyReflection = Toggle("Fix Underwater Sky Reflection", "Water reflection can't reflect other water waves and sometimes reflects the sky below the horizon." +
                                                                                             "This parameter is trying to hide this artifact using the water bottom color", waterSystem.FixUnderwaterSkyReflection);
        }

        EditorGUILayout.EndVertical();
    }

    async void AddFlowMap()
    {

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal(buttonStyle);

        EditorGUI.BeginChangeCheck();
        waterSystem.UseFlowMap = EditorGUILayout.Toggle(waterSystem.UseFlowMap, GUILayout.MaxWidth(14));
        GUILayout.Space(14);
        waterSystem.ShowFlowMap = EditorGUILayout.Foldout(waterSystem.ShowFlowMap, new GUIContent("FlowMap", "Used to imitate water flow"), true);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow("FlowMap");
        EditorGUILayout.EndHorizontal();

        if (waterSystem.ShowFlowMap)
        {

            GUI.enabled = isActive;

            if (GUILayout.Toggle(waterSystem.FlowMapInEditMode, "Edit Mode", "Button"))
            {
                waterSystem.FlowMapInEditMode = true;
                GUI.enabled = isActive;
            }
            else
            {
                waterSystem.FlowMapInEditMode = false;
                GUI.enabled = false;
            }

            if (waterSystem.FlowMapInEditMode)
            {
                EditorGUILayout.HelpBox("\"Left Mouse Click\" for painting " + " \r\n" +
                    "Hold \"Ctrl Button\" for erase mode" + " \r\n" +
                    "Use \"Mouse Wheel\" for brush size", MessageType.Info);

            }

            EditorGUI.BeginChangeCheck();
            waterSystem.FlowMapOffset = Vector2Field("World Offset", "Position offset of flow map area relative to the water pivot point", waterSystem.FlowMapOffset);
            var newAreaSize = IntSlider("Area Size Meters", "Area size in meters for drawing on the flowmap texture, this parameter relative to the texture resolution", waterSystem.FlowMapAreaSize, 10, 2000);
            var newTexRes = EditorGUILayout.IntField(new GUIContent("Texture Resolution", "Final quality of flowmap used formula = texture resolution / area size. " + " \r\n" +
                                                                           "For example 1024 pixels / 100 meters area = ~10 pixels per meter." + " \r\n" +
            "More pixels->better quality->more memory using"), waterSystem.FlowMapTextureResolution);
            waterSystem.FlowMapSpeed = Slider("Flow Speed", "Velocity of flow", waterSystem.FlowMapSpeed, 0.1f, 5f);
            if (EditorGUI.EndChangeCheck())
            {
                isFlowMapChanged = true;
                waterSystem.RedrawFlowMap(newTexRes, newAreaSize);
                waterSystem.FlowMapAreaSize = newAreaSize;
                waterSystem.FlowMapTextureResolution = newTexRes;
            }

            EditorGUILayout.Space();
            waterSystem.FlowMapBrushStrength = FloatField("Brush Strength", "Higher parameter = faster flow velocity", waterSystem.FlowMapBrushStrength);

            // if (waterSystem.FlowMapInEditMode)
            {
                if (GUILayout.Button("Load Latest Changes"))
                {
                    if (EditorUtility.DisplayDialog("Load Latest Changes?", "Are you sure you want to LOAD latest changes?", "Yes", "Cancel"))
                    {
                        waterSystem.ReadFlowMap();

                        Debug.Log("FlowMap Load");
                        isFlowMapChanged = false;
                    }
                }

                if (GUILayout.Button("Clear FlowMap"))
                {
                    if (EditorUtility.DisplayDialog("Clear FlowMap?", "Are you sure you want to CLEAR flowmap?", "Yes", "Cancel"))
                    {
                        waterSystem.ClearFlowMap();

                        Debug.Log("FlowMap Saved");
                        isFlowMapChanged = false;
                    }
                }

                GUI.enabled = isFlowMapChanged;

                if (GUILayout.Button("Save Changes"))
                {
                    waterSystem.SaveFlowMap();
                    waterSystem.ReadFlowMap();
                    Debug.Log("FlowMap Saved");
                    isFlowMapChanged = false;
                }

            }
            GUI.enabled = isActive;

        }

        if (!waterSystem.UseFlowMap) waterSystem.FlowMapInEditMode = false;

        EditorGUILayout.EndVertical();
    }

    private List<KW_ShorelineWaves.ShorelineWaveInfo> wavesData;

    async void AddShorelineMap()
    {

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal(buttonStyle);

        waterSystem.UseShorelineRendering = EditorGUILayout.Toggle(waterSystem.UseShorelineRendering, GUILayout.MaxWidth(14));
        GUILayout.Space(14);
        waterSystem.ShowShorelineMap = EditorGUILayout.Foldout(waterSystem.ShowShorelineMap, new GUIContent("Shoreline", "Used for rendering of coastal waves with foam"), true);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow("Shoreline");
        EditorGUILayout.EndHorizontal();

        if (waterSystem.ShowShorelineMap)
        {
            if(isActive) GUI.enabled = waterSystem.UseShorelineRendering;
            EditorGUILayout.HelpBox("Foam rendering in the editor will be very slow if enabled Gizmo Selection Outline and Selection Wire!", MessageType.Warning);
            waterSystem.FoamLodQuality = (WaterSystem.QualityEnum)EditorGUILayout.EnumPopup(new GUIContent("Foam Lod Quality", "Foam particles count"), waterSystem.FoamLodQuality);

            EditorGUI.BeginChangeCheck();
            waterSystem.ShorelineInEditMode = GUILayout.Toggle(waterSystem.ShorelineInEditMode, "Edit Mode", "Button");
            if (EditorGUI.EndChangeCheck())
            {
                wavesData = await waterSystem.GetShorelineWavesData();
                isRequiredUpdateShorelineParams = true;
            }


            if (isActive)
            {
                EditorGUI.BeginChangeCheck();

                GUI.enabled = isActive && waterSystem.ShorelineInEditMode;

                if (waterSystem.ShorelineInEditMode)
                {
                    EditorGUILayout.HelpBox("You can use Insert/Delete buttons for Add/Delete waves", MessageType.Info);
                    EditorGUILayout.HelpBox("Avoid crossing boxes of the same color!", MessageType.Info);
                }
                //waterSystem.ShorelineOffset = EditorGUILayout.Vector2Field("World Offset", waterSystem.ShorelineOffset);
                waterSystem.ShorelineAreaSize = EditorGUILayout.IntField(new GUIContent("Area Size Meters", "Area size in meters for baked waves, this parameter relative to the texture resolution"), waterSystem.ShorelineAreaSize);
                waterSystem.ShorelineTextureResolution = EditorGUILayout.IntField(new GUIContent("Texture Resolution", "Increase texture resolution if scaled waves have artifacts"), waterSystem.ShorelineTextureResolution);

                if (GUILayout.Button(new GUIContent("Add Wave")))
                {
                    wavesData = await waterSystem.GetShorelineWavesData();
                    AddWave(wavesData, false);
                    waterSystem.SaveWavesDataToFile();
                }

                GUILayout.Space(10);
                if (GUILayout.Button("Delete All Waves"))
                {
                    if (EditorUtility.DisplayDialog("Delete Shoreline Waves?", "Are you sure you want to DELETE shoreline waves?", "Yes", "Cancel"))
                    {
                        Debug.Log("Shoreline deleted");
                        waterSystem.ClearOrthoDepth();
                        waterSystem.ClearShorelineWavesWithFoam();

                        isRequiredUpdateShorelineParams = true;
                    }
                }

                if (GUILayout.Button("Save Changes"))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    Debug.Log("Shoreline Saved");
                    waterSystem.RenderOrthoDepth();
                    waterSystem.SaveOrthoDepth();
                    await waterSystem.BakeWavesToTexture();
                    waterSystem.SaveWavesDataToFile();
                    sw.Stop();
                    Debug.Log("total save time: " + sw.ElapsedMilliseconds);
                }
                GUI.enabled = !waterSystem.ShorelineInEditMode;
            }
            if (isActive) GUI.enabled = true;
        }

        if (!waterSystem.UseShorelineRendering || !isActive) waterSystem.ShorelineInEditMode = false;

        EditorGUILayout.EndVertical();
    }

    private void AddWave(List<KW_ShorelineWaves.ShorelineWaveInfo> wavesData, bool useMousePositionAsStartPoint)
    {
        var newWave = new KW_ShorelineWaves.ShorelineWaveInfo();
        ComputeShorelineNextTransform(newWave, wavesData, useMousePositionAsStartPoint);

        newWave.ID = (wavesData.Count == 0) ? 0 : wavesData.Last().ID + 1;
        newWave.TimeOffset = GetShorelineTimeOffset(wavesData);
        Debug.Log("ID " + newWave.ID + "   time offset " + newWave.TimeOffset);

        wavesData.Add(newWave);
        waterSystem.ClearShorelineFoam();
        isRequiredUpdateShorelineParams = true;
    }

    void ComputeShorelineNextTransform(KW_ShorelineWaves.ShorelineWaveInfo newWave, List<KW_ShorelineWaves.ShorelineWaveInfo> wavesData, bool useMousePositionAsStartPoint)
    {
        var plane = new Plane(Vector3.down, waterSystem.transform.position.y);

        var ray = useMousePositionAsStartPoint ? HandleUtility.GUIPointToWorldRay(Event.current.mousePosition) : new Ray(waterSystem.currentCamera.transform.position, waterSystem.currentCamera.transform.forward * 1000);

        if (plane.Raycast(ray, out var distanceToPlane))
        {
            var intersectionPos = ray.GetPoint(distanceToPlane);

            newWave.PositionX = intersectionPos.x;
            newWave.PositionZ = intersectionPos.z;
            if (wavesData.Count > 0)
            {
                var lastIdx = wavesData.Count - 1;
                newWave.EulerRotation = wavesData[lastIdx].EulerRotation;
                newWave.ScaleX = wavesData[lastIdx].ScaleX;
                newWave.ScaleY = wavesData[lastIdx].ScaleY;
                newWave.ScaleZ = wavesData[lastIdx].ScaleZ;
            }

            if (!useMousePositionAsStartPoint && wavesData.Count > 0)
            {
                if (wavesData.Count < 2) newWave.PositionZ += 10;
                else
                {
                    var currentIdx = wavesData.Count - 1;
                    var lastPos = new Vector2(wavesData[currentIdx].PositionX, wavesData[currentIdx].PositionZ);
                    var lastLastPos = new Vector2(wavesData[currentIdx - 1].PositionX, wavesData[currentIdx - 1].PositionZ);
                    var direction = (lastPos - lastLastPos).normalized;
                    var radius = new Vector2(wavesData[currentIdx].ScaleX, wavesData[currentIdx].ScaleZ).magnitude * 0.4f;
                    newWave.PositionX = lastPos.x + radius * direction.x;
                    newWave.PositionZ = lastPos.y + radius * direction.y;
                }
            }
        }
    }


    bool IsShorelineIntersectOther(int currentShorelineIdx, List<KW_ShorelineWaves.ShorelineWaveInfo> wavesData)
    {
        if (currentShorelineIdx < 0 || currentShorelineIdx >= wavesData.Count) return false;
        var currentWave = wavesData[currentShorelineIdx];
        var currentWavePos = new Vector3(currentWave.PositionX, 0, currentWave.PositionZ);
        var currentWaveMaxScale = Mathf.Max(currentWave.ScaleX, currentWave.ScaleZ);
        int startIdx = currentShorelineIdx % 2 == 0? 0 : 1;
        for (var i = startIdx; i < wavesData.Count; i+=2)
        {
            if(currentShorelineIdx == i) continue;

            var distance = (currentWavePos - new Vector3(wavesData[i].PositionX, 0, wavesData[i].PositionZ)).magnitude;
            var maxScale = Mathf.Max(wavesData[i].ScaleX, wavesData[i].ScaleZ);
            if (distance < (currentWaveMaxScale + maxScale) * 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    float GetShorelineTimeOffset(List<KW_ShorelineWaves.ShorelineWaveInfo>  wavesData)
    {
         if (wavesData.Count == 0) return 0;
        // if (shorelineWaves.Count == 1) return Random.Range(0.25f, 0.35f);

        // return shorelineWaves[shorelineWaves.Count - 2].TimeOffset + Random.Range(0.25f, 0.35f);
        var timeOffset = wavesData[wavesData.Count - 1].TimeOffset + Random.Range(0.15f, 0.2f);
        return timeOffset % 1;
    }

    void AddVolumeScattering()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal(buttonStyle);

        waterSystem.UseVolumetricLight = EditorGUILayout.Toggle(waterSystem.UseVolumetricLight, GUILayout.MaxWidth(14));
        GUILayout.Space(14);
        waterSystem.ShowVolumetricLightSettings = EditorGUILayout.Foldout(waterSystem.ShowVolumetricLightSettings, new GUIContent("Volumetric Scattering", "Used to simulate underwater effects (volume shadows/light/caustic) "), true);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow("Volumetric Scattering");
        EditorGUILayout.EndHorizontal();

        if (waterSystem.ShowVolumetricLightSettings)
        {
            waterSystem.VolumetricLightResolutionScale = EditorGUILayout.Slider(new GUIContent("Resolution Scale", "Texture resolution scaled relative to screen size"), waterSystem.VolumetricLightResolutionScale, 0.15f, 0.75f);
            waterSystem.VolumetricLightIteration = EditorGUILayout.IntSlider(new GUIContent("Iteration Count", "More iteration = better volume light/shadow quality"), waterSystem.VolumetricLightIteration, 2, 8);
            waterSystem.VolumetricLightDistance = EditorGUILayout.Slider(new GUIContent("Distance", "Max distance for volume rendering"), waterSystem.VolumetricLightDistance, 20, 100);
            waterSystem.VolumetricLightBlurRadius = EditorGUILayout.Slider(new GUIContent("Blur Radius", "Blur artefacts of volume light"), waterSystem.VolumetricLightBlurRadius, 0, 4);
        }

        EditorGUILayout.EndVertical();
    }

    void AddCausticEffect()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal(buttonStyle);

        waterSystem.UseCausticEffect          = EditorGUILayout.Toggle(waterSystem.UseCausticEffect, GUILayout.MaxWidth(14));
        GUILayout.Space(14);
        waterSystem.ShowCausticEffectSettings = EditorGUILayout.Foldout(waterSystem.ShowCausticEffectSettings, new GUIContent("Caustic Effect", "Used to simulate light rays on surfaces"), true);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow("Caustic");
        EditorGUILayout.EndHorizontal();

        if (waterSystem.ShowCausticEffectSettings)
        {
            waterSystem.UseCausticFiltering = Toggle("Use Filtering", "Used to smooth the caustic texture", waterSystem.UseCausticFiltering);
            waterSystem.UseCausticDispersion = Toggle("Use Dispersion", "Used to break up light into its constituent spectral colors", waterSystem.UseCausticDispersion);

            var texSize = IntSlider("Texture Size", "Caustic texture resolution", waterSystem.CausticTextureSize, 256, 1024);
            texSize = Mathf.RoundToInt(texSize / 64f);
            waterSystem.CausticTextureSize = (int) texSize * 64;
            waterSystem.CausticMeshResolution = IntSlider("Mesh Resolution", "Caustic simulation mesh size. Less size = faster", waterSystem.CausticMeshResolution, 128, 384);
            waterSystem.CausticActiveLods = IntSlider("Active Lods", "Caustic cascades, works like the standard shadow cascades", waterSystem.CausticActiveLods, 1, 4);
            waterSystem.CausticStrength = Slider("Caustic Strength", "Caustic light intensity", waterSystem.CausticStrength, 0, 2);
            waterSystem.CausticDepthScale = EditorGUILayout.Slider(new GUIContent("Caustic Depth Scale", "Strength of caustic distortion relative to the water depth"), waterSystem.CausticDepthScale, 0.1f, 3);
            if (GUILayout.Button("Bake Caustic Depth"))
            {
                waterSystem.RenderOrthoDepth();
                waterSystem.SaveOrthoDepth();
            }

        }

        EditorGUILayout.EndVertical();
    }

    void AddUnderwaterEffect()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal(buttonStyle);

        waterSystem.UseUnderwaterEffect          = EditorGUILayout.Toggle(waterSystem.UseUnderwaterEffect, GUILayout.MaxWidth(14));
        GUILayout.Space(14);
        waterSystem.ShowUnderwaterEffectSettings = EditorGUILayout.Foldout(waterSystem.ShowUnderwaterEffectSettings, new GUIContent("Underwater Effect"), true);
        if (GUILayout.Button("?", helpBoxStyle, GUILayout.Width(16), GUILayout.Height(14))) OpenHelpVideoWindow("Underwater Effect");
        EditorGUILayout.EndHorizontal();

        if (waterSystem.ShowUnderwaterEffectSettings)
        {
            waterSystem.UseHighQualityUnderwater = EditorGUILayout.Toggle( new GUIContent("High Quality", "Quality of underwater mask"), waterSystem.UseHighQualityUnderwater);
            waterSystem.UseUnderwaterBlur = Toggle("Use Blur Effect", "Blur underwater image", waterSystem.UseUnderwaterBlur);
            if (waterSystem.UseUnderwaterBlur)
            {
                waterSystem.UnderwaterResolutionScale = EditorGUILayout.Slider(new GUIContent("Resolution Scale", "Texture resolution relative to screen size"), waterSystem.UnderwaterResolutionScale, 0.25f, 1);
                waterSystem.UnderwaterBlurRadius = EditorGUILayout.Slider(new GUIContent("Blur Radius", "Blur strength"), waterSystem.UnderwaterBlurRadius, 0.25f, 3);

            }

        }

        EditorGUILayout.EndVertical();
    }

    void AddRendering()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal(buttonStyle);

        EditorGUILayout.LabelField("", GUILayout.MaxWidth(4));
        waterSystem.ShowRendering = EditorGUILayout.Foldout(waterSystem.ShowRendering, new GUIContent("Rendering"), true);

        EditorGUILayout.EndHorizontal();

        if (waterSystem.ShowRendering)
        {
            waterSystem.DrawScreenSpaceWaterMesh = Toggle("Screen Space Mesh", "Water rendering in screen space allows you to set the resolution of the rendering for water separately from the game resolution", waterSystem.DrawScreenSpaceWaterMesh);
            if(waterSystem.DrawScreenSpaceWaterMesh) waterSystem.ScreenSpaceWaterResolution = EditorGUILayout.Slider(new GUIContent("Screen Space Water Resolution", "Water rendering texture resolution scaled relative to screen size"), waterSystem.ScreenSpaceWaterResolution, 0.25f, 1f);
            waterSystem.DrawToPosteffectsDepth = Toggle("Draw To Depth", "Write water depth to the scene depth buffer after transparent geometry." + " \r\n" +
                                                                         "Required for correct rendering of \"Depth Of Field\" post effect", waterSystem.DrawToPosteffectsDepth);
            waterSystem.WaterMeshType = (WaterSystem.WaterMeshTypeEnum)EditorGUILayout.EnumPopup( new GUIContent("Render Mode", "In the current version available only infinite ocean mesh. " + " \r\n" +
                                                                                                                "In the future will be added sized meshes and custom meshes support"), waterSystem.WaterMeshType);
            if (waterSystem.WaterMeshType == WaterSystem.WaterMeshTypeEnum.Infinite)
            {
                EditorGUI.BeginChangeCheck();
                waterSystem.MeshQuality = IntSlider("Mesh Quality", "Water mesh detailing", waterSystem.MeshQuality, 1, 10);
                if (EditorGUI.EndChangeCheck()) waterSystem.InitializeInfiniteMesh(waterSystem.MeshQuality);

            }
            EditorGUI.BeginChangeCheck();
            waterSystem.UseTesselation = Toggle("Use Tesselation", "Tessellation dynamically increases the detail of the water mesh", waterSystem.UseTesselation);
            if (EditorGUI.EndChangeCheck()) waterSystem.InitializeWaterMaterial(waterSystem.UseTesselation);
            if (waterSystem.UseTesselation)
            {
                waterSystem.TesselationFactor = Slider("Tesselation Factor", "Detail factor", waterSystem.TesselationFactor, 0.1f, 1);
                waterSystem.TesselationMaxDistance = Slider("Tesselation Max Distance", "Detail distance", waterSystem.TesselationMaxDistance, 5, 2000);
            }

        }

        EditorGUILayout.EndVertical();
    }

    private Vector3 flowMapLastPos = Vector3.positiveInfinity;
    void DrawFlowMapEditor()
    {
        if (Application.isPlaying) return;

        var e = Event.current;
        if (e.type == EventType.ScrollWheel)
        {
            floatMapCircleRadiusDefault -= (e.delta.y * floatMapCircleRadiusDefault) / 40f;
            floatMapCircleRadiusDefault = Mathf.Clamp(floatMapCircleRadiusDefault, 0.1f, waterSystem.FlowMapAreaSize);
        }

        var controlId = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlId);
        if (e.type == EventType.ScrollWheel) e.Use();

        var waterPos = waterSystem.transform.position;
        var waterHeight = waterSystem.transform.position.y;
        var flowmapWorldPos = GetMouseWorldPosProjectedToWater(waterHeight, e);
        if (float.IsInfinity(flowmapWorldPos.x)) return;
        var flowPosWithOffset = new Vector3(-waterPos.x - waterSystem.FlowMapOffset.x, 0, -waterPos.z - waterSystem.FlowMapOffset.y) + (Vector3)flowmapWorldPos;

        Handles.color = e.control ? new Color(1, 0, 0) : new Color(0, 0.8f, 1);
        Handles.CircleHandleCap(controlId, (Vector3) flowmapWorldPos, Quaternion.LookRotation(Vector3.up), floatMapCircleRadiusDefault, EventType.Repaint);

        Handles.color = e.control ? new Color(1, 0, 0, 0.2f) : new Color(0, 0.8f, 1, 0.25f);
        Handles.DrawSolidDisc((Vector3) flowmapWorldPos, Vector3.up, floatMapCircleRadiusDefault);



        var flowMapAreaPos = new Vector3(waterPos.x + waterSystem.FlowMapOffset.x, waterPos.y, waterPos.z + waterSystem.FlowMapOffset.y);
        var flowMapAreaScale = new Vector3(waterSystem.FlowMapAreaSize, 0.5f, waterSystem.FlowMapAreaSize);
        Handles.matrix = Matrix4x4.TRS(flowMapAreaPos, Quaternion.identity, flowMapAreaScale);


        Handles.color = new Color(0, 0.75f, 1, 0.2f);
        Handles.CubeHandleCap(0, Vector3.zero, Quaternion.identity, 1, EventType.Repaint);
        Handles.color = new Color(0, 0.75f, 1, 0.9f);
        Handles.DrawWireCube(Vector3.zero, Vector3.one);

        if (Event.current.button == 0)
        {
            if (e.type == EventType.MouseDown)
            {
                leftKeyPressed = true;
                //waterSystem.flowMap.LastDrawFlowMapPosition = flowPosWithOffset;
            }
            if (e.type == EventType.MouseUp)
            {
                leftKeyPressed = false;
                isFlowMapChanged = true;
                flowMapLastPos = Vector3.positiveInfinity;

                Repaint();
            }
        }

        if (leftKeyPressed)
        {
            if (float.IsPositiveInfinity(flowMapLastPos.x))
            {
                flowMapLastPos = flowPosWithOffset;
            }
            else
            {
                var brushDir = (flowPosWithOffset - flowMapLastPos);
                flowMapLastPos = flowPosWithOffset;
                waterSystem.DrawOnFlowMap(flowPosWithOffset, brushDir, floatMapCircleRadiusDefault, waterSystem.FlowMapBrushStrength, e.control);
            }
        }

    }

    private bool isRequiredUpdateShorelineParams;
    private const float updateShorelineParamsEverySeconds = 0.1f;
    private float currentTimeBeforeShorelineUpdate;
    int nearMouseSelectionWaveIDX = -1;
    private bool isMousePressed = false;


    async Task DrawShorelineEditor()
    {
        if (Application.isPlaying) return;
        Profiler.BeginSample("DrawShorelineEditor");

        Handles.lighting = false;
        float waterYPos = waterSystem.transform.position.y;
        var defaultMatrix = Handles.matrix;

        var e = Event.current;

        if (e.type == EventType.MouseDown) isMousePressed = true;
        else if (e.type == EventType.MouseUp) isMousePressed = false;

        if (!isMousePressed) nearMouseSelectionWaveIDX = GetNearestWave(wavesData);


        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Insert)
            {
                wavesData = await waterSystem.GetShorelineWavesData();
                AddWave(wavesData, false);
                waterSystem.SaveWavesDataToFile();
            }

            if (Event.current.keyCode == KeyCode.Delete)
            {
                wavesData.RemoveAt(nearMouseSelectionWaveIDX);
                waterSystem.SaveWavesDataToFile();
                Event.current.Use();
            }
        }


        for (var i = 0; i < wavesData.Count; i++)
        {
            var wave = wavesData[i];
            var wavePos = new Vector3(wave.PositionX, waterYPos, wave.PositionZ);

            Handles.matrix = defaultMatrix;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            if (nearMouseSelectionWaveIDX == i)
            {
                switch (Tools.current)
                {
                    case Tool.Move:
                        var newWavePos = Handles.DoPositionHandle(wavePos, Quaternion.identity);
                        if (wavePos != newWavePos) isRequiredUpdateShorelineParams = true;

                        wave.PositionX = newWavePos.x;
                        wave.PositionZ = newWavePos.z;

                        break;
                    case Tool.Rotate:
                    {
                        var currentRotation = Quaternion.Euler(0, wave.EulerRotation, 0);
                        var newRotation = Handles.DoRotationHandle(currentRotation, wavePos);
                        if (currentRotation != newRotation) isRequiredUpdateShorelineParams = true;
                        wave.EulerRotation = newRotation.eulerAngles.y;
                        break;
                    }
                    case Tool.Scale:
                    {
                        var distToCamera = Vector3.Distance(waterSystem.currentCamera.transform.position, wavePos);
                        var handleScaleToCamera = Mathf.Lerp(1, 50, Mathf.Clamp01(distToCamera / 500));

                        var currentScale = new Vector3(wave.ScaleX, wave.ScaleY, wave.ScaleZ);
                        var newScale = Handles.DoScaleHandle(new Vector3(wave.ScaleX, wave.ScaleY, wave.ScaleZ), wavePos, Quaternion.Euler(0, wave.EulerRotation, 0), handleScaleToCamera);
                        if (currentScale != newScale)
                        {
                            isRequiredUpdateShorelineParams = true;
                            var maxNewScale = Mathf.Min(wave.ScaleX, wave.ScaleZ);
                            var maxDefaultScale = Mathf.Min(wave.DefaultScaleX, wave.DefaultScaleZ);

                            wave.ScaleX = newScale.x;
                            wave.ScaleZ = newScale.z;
                            wave.ScaleY = wave.DefaultScaleY * (maxNewScale / maxDefaultScale);

                        }



                        break;
                    }
                }
            }

            var waveColor = i % 2 == 0 ? new Color(0, 0.75f, 1, 0.95f) : new Color(0.75f, 1, 0, 0.95f);
            var selectionColor = new Color(Mathf.Clamp01(waveColor.r * 1.5f), Mathf.Clamp01(waveColor.g * 1.5f), Mathf.Clamp01(waveColor.b * 1.5f), 0.95f);
            if(IsShorelineIntersectOther(nearMouseSelectionWaveIDX, wavesData)) selectionColor = new Color(1, 0, 0, 0.9f);

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.color = nearMouseSelectionWaveIDX == i ? selectionColor : waveColor;
            Handles.matrix = Matrix4x4.TRS(wavePos, Quaternion.Euler(0, wave.EulerRotation, 0), new Vector3(wave.ScaleX, wave.ScaleY, wave.ScaleZ));
            Handles.DrawWireCube(Vector3.zero, Vector3.one);

            Handles.color = nearMouseSelectionWaveIDX == i ? selectionColor * 0.3f : waveColor * 0.2f;
            Handles.CubeHandleCap(0, Vector3.zero, Quaternion.identity, 1, EventType.Repaint);


            //Handles.matrix = defaultMatrix;
            //if (i % 2 == 1 && i >= 3)
            //{
            //    Handles.color = new Color(1f, 1f, 0f, 0.99f);
            //    Handles.DrawLine(new Vector3(wavesData[i - 2].PositionX, waterYPos, wavesData[i - 2].PositionZ), wavePos);
            //}
            //if (i % 2 == 0 && i >= 2)
            //{
            //    Handles.color = new Color(0f, 1f, 1f, 0.99f);
            //    Handles.DrawLine(new Vector3(wavesData[i - 2].PositionX, waterYPos, wavesData[i - 2].PositionZ), wavePos);
            //}

            //Handles.matrix = Matrix4x4.TRS(wavePos, Quaternion.Euler(0, wave.EulerRotation, 0), new Vector3(0.5f, 0.5f, wave.ScaleZ));
            //Handles.CubeHandleCap(0, Vector3.zero, Quaternion.identity, 1, EventType.Repaint);


        }

        if (IsCanUpdateShoreline())
        {
            waterSystem.RenderOrthoDepth();
            await waterSystem.BakeWavesToTexture();

            waterSystem.RenderShorelineWavesWithFoam();
        }

        Handles.matrix = Matrix4x4.TRS(waterSystem.transform.position, Quaternion.identity, new Vector3(waterSystem.ShorelineAreaSize, 0.1f, waterSystem.ShorelineAreaSize));


        //Handles.color = new Color(0, 0.75f, 1, 0.05f);
        //Handles.CubeHandleCap(0, Vector3.zero, Quaternion.identity, 1, EventType.Repaint);
        Handles.color = new Color(0, 0.75f, 1, 0.9f);
        Handles.DrawWireCube(Vector3.zero, Vector3.one);

        Handles.matrix = defaultMatrix;
        Profiler.EndSample();
    }

    private int GetNearestWave(List<KW_ShorelineWaves.ShorelineWaveInfo> wavesData)
    {
        var mouseWorldPos = GetMouseWorldPosProjectedToWater(waterSystem.transform.position.y, Event.current);
        float minDistance = float.PositiveInfinity;
        int minIdx = 0;
        if (!float.IsInfinity(mouseWorldPos.x))
        {
            for (var i = 0; i < wavesData.Count; i++)
            {
                var wave = wavesData[i];
                var distToMouse = new Vector2(wave.PositionX - mouseWorldPos.x, wave.PositionZ - mouseWorldPos.z).magnitude;
                var waveRadius = new Vector2(wave.ScaleX, wave.ScaleZ).magnitude * 2.0f;

                if (distToMouse < waveRadius && distToMouse < minDistance)
                {
                    minDistance = distToMouse;
                    minIdx = i;
                }
            }
        }

        return minIdx;
    }

    bool IsCanUpdateShoreline()
    {
        if (isRequiredUpdateShorelineParams)
        {
            currentTimeBeforeShorelineUpdate += KW_Extensions.DeltaTime();

            if (currentTimeBeforeShorelineUpdate > updateShorelineParamsEverySeconds)
            {
                isRequiredUpdateShorelineParams = false;
                currentTimeBeforeShorelineUpdate = 0;
                return true;
            }
        }
        return false;
    }

    private float currentTimeBeforeFlowMapUpdate;
    private const float updateFlowMapParamsEverySeconds = 0.5f;


    //void DrawInteractiveWavesHelpers()
    //{
    //    if (Application.isPlaying) return;

    //    var size = waterSystem.InteractiveWavesAreaSize;
    //    Handles.DrawWireCube(waterSystem.InteractPos, new Vector3(size, 0.01f, size));

    //}


    Vector3 GetMouseWorldPosProjectedToWater(float height, Event e)
    {
        var mousePos = e.mousePosition;
        mousePos.y = Camera.current.pixelHeight - mousePos.y;
        var plane = new Plane(Vector3.down, height);
        var   ray   = Camera.current.ScreenPointToRay(mousePos);
        float distanceToPlane;
        if (plane.Raycast(ray, out distanceToPlane))
        {
            return ray.GetPoint(distanceToPlane);
        }

        return Vector3.positiveInfinity;
    }

    Vector3 FindNearestRay(float raysCount, Vector3 startPos)
    {
        var nearestVector = Vector3.positiveInfinity;

        for (int i = 0; i < raysCount; i++)
        {
            var offset = i * (6.2831f / raysCount);
            var dir = new Vector3(Mathf.Sin(offset), 0, Mathf.Cos(offset));
            RaycastHit ray;
            if (Physics.Raycast(startPos, dir, out ray))
            {
                if (ray.distance < nearestVector.magnitude) nearestVector = ray.point - startPos;
            }
        }

        return nearestVector;
    }


    enum WaterMeshType
    {
        Infinite,
        Finite,
        CustomMesh
    }

    private Texture2D CreateTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }
}
