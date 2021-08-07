using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class KW_FlowMap : MonoBehaviour
{
   // public int CurrentAreaSize;

   // public Vector3  LastDrawFlowMapPosition;
   private FlowMapData currentFlowmapData;
   private FlowMapData LoadedFlowmapData;

    string FlowMapShaderName = "Hidden/KriptoFX/Water/FlowMapEdit";
    //string flowMapPath     = "/KriptoFX/WaterSystem/FlowMaps/WaterFlowMap.png";
    //string flowMapPathInfo = "/KriptoFX/WaterSystem/FlowMaps/WaterFlowMapInfo.txt";
    private const string folderName = "FlowMaps";
    private const string textureName = "FlowMaps/OrthoDepthTex";
    private const string textureDataName = "FlowMaps/WaterFlowMapData";

    RenderTexture flowmapRT;
    private Texture2D flowMapTex2D;
    Material _flowMaterial;


    [System.Serializable]
    public class FlowMapData
    {
        [SerializeField] public int AreaSize;
        [SerializeField] public int TextureSize;
    }

    Material flowMaterial
    {
        get
        {
            if(_flowMaterial == null) _flowMaterial = KW_Extensions.CreateMaterial(FlowMapShaderName);
            return _flowMaterial;
        }
    }

    public void Release()
    {
        if (flowmapRT != null) flowmapRT.Release();
        KW_Extensions.SafeDestroy(_flowMaterial);
        KW_Extensions.SafeDestroy(flowMapTex2D);
    }

    void OnDisable()
    {
        print("FlowMap.Disabled");
        Release();
    }

    public void ClearFlowMap(string GUID)
    {
        var pathToDepthDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        var pathToTexture = Path.Combine(pathToDepthDataFolder, folderName, GUID, textureName + ".gz");
        var pathToData = Path.Combine(pathToDepthDataFolder, folderName, GUID, textureDataName + ".gz");
        if (File.Exists(pathToTexture)) File.Delete(pathToTexture);
        if (File.Exists(pathToData))  File.Delete(pathToData);
        ClearRT();
        Shader.SetGlobalTexture("KW_FlowMapTex", flowmapRT);
    }

    public void InitializeFlowMapEditorResources(int size, int areaSize)
    {
        if (flowmapRT == null)
        {
            flowmapRT = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            ClearRT();
            flowmapRT.filterMode = FilterMode.Bilinear;
        }

        if (flowMapTex2D != null)
        {
            var activeRT = RenderTexture.active;
            Graphics.Blit(flowMapTex2D, flowmapRT);
            RenderTexture.active = activeRT;
            KW_Extensions.SafeDestroy(flowMapTex2D);
        }

        if (currentFlowmapData == null) currentFlowmapData = new FlowMapData();
        currentFlowmapData.AreaSize = areaSize;
        currentFlowmapData.TextureSize = size;
        Shader.SetGlobalTexture("KW_FlowMapTex", flowmapRT);
    }

    void ClearRT()
    {
        var activeRT = RenderTexture.active;
        RenderTexture.active = flowmapRT;
        GL.Clear(true, true, new Color(0.5f, 0.5f, 0, 0));
        RenderTexture.active = activeRT;
    }

    private Vector3 flowMapLastClickedPos;
    public void DrawOnFlowMap(Vector3 brushPosition, Vector3 brushMoveDirection, float circleRadius, float brushStrength, bool eraseMode = false)
    {
        float brushSize = currentFlowmapData.AreaSize / circleRadius;

        var uv = new Vector2(brushPosition.x / currentFlowmapData.AreaSize + 0.5f, brushPosition.z / currentFlowmapData.AreaSize + 0.5f);
        if (brushMoveDirection.magnitude < 0.001f) brushMoveDirection = Vector3.zero;

        var tempRT = RenderTexture.GetTemporary(flowmapRT.width, flowmapRT.height, 0, flowmapRT.format, RenderTextureReadWrite.Linear);
        tempRT.filterMode = FilterMode.Bilinear;


        flowMaterial.SetVector("_MousePos", uv);
        flowMaterial.SetVector("_Direction", new Vector2(brushMoveDirection.x, brushMoveDirection.z));
        flowMaterial.SetFloat("_Size", brushSize * 0.85f);
        flowMaterial.SetFloat("_BrushStrength", brushStrength / 10f);
        flowMaterial.SetFloat("isErase", eraseMode ? 1 : 0);

        var activeRT = RenderTexture.active;
        Graphics.Blit(flowmapRT, tempRT, flowMaterial, 0);
        Graphics.Blit(tempRT, flowmapRT);
        RenderTexture.active = activeRT;
        RenderTexture.ReleaseTemporary(tempRT);
    }

    public void SaveFlowMap(int areaSize, string GUID)
    {
        if (currentFlowmapData == null) currentFlowmapData = new FlowMapData() {AreaSize =  areaSize};

        var pathToDepthDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        flowmapRT.SaveRenderTextureToFile(Path.Combine(pathToDepthDataFolder, folderName, GUID, textureName), TextureFormat.RGBAHalf);
        KW_Extensions.SerializeToFile(Path.Combine(pathToDepthDataFolder, folderName, GUID, textureDataName), currentFlowmapData);
    }

    public void RedrawFlowMap(int resolution, int newAreaSize)
    {
        var tempRT = RenderTexture.GetTemporary(resolution, resolution, 0, flowmapRT.format, RenderTextureReadWrite.Linear);
        tempRT.filterMode = FilterMode.Bilinear;

        var uvScale = (float)newAreaSize / currentFlowmapData.AreaSize;
        currentFlowmapData.AreaSize = newAreaSize;
        flowMaterial.SetFloat("_UvScale", uvScale);
       // Debug.Log("        flow.RedrawFlowMapArea ");
        var activeRT = RenderTexture.active;
        Graphics.Blit(flowmapRT, tempRT, flowMaterial, 1);
        Graphics.Blit(tempRT, flowmapRT);

        RenderTexture.active = activeRT;
        RenderTexture.ReleaseTemporary(tempRT);
    }

    public async Task<bool> ReadFlowMap(List<Material> sharedMaterials, string GUID)
    {
       // Debug.Log("        flow.ReadFlowMap");
        var pathToDepthDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        if(flowMapTex2D != null) KW_Extensions.SafeDestroy(flowMapTex2D);
        flowMapTex2D = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToDepthDataFolder, folderName, GUID, textureName), true, FilterMode.Bilinear, TextureWrapMode.Clamp);
        if (flowMapTex2D == null)
        {
            return false;
        }

        LoadedFlowmapData = await KW_Extensions.DeserializeFromFile<FlowMapData>(Path.Combine(pathToDepthDataFolder, folderName, GUID, textureDataName));
        currentFlowmapData = LoadedFlowmapData;
        Shader.SetGlobalTexture("KW_FlowMapTex", flowMapTex2D);
        //foreach (var sharedMaterial in sharedMaterials)
        //{

        //    sharedMaterial.SetTexture("KW_FlowMapTex", flowMapTex2D);
        //}

        return true;
    }

    public FlowMapData GetFlowMapDataFromFile()
    {
        return LoadedFlowmapData;
    }
}

