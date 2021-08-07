using System.IO;
using System.Threading.Tasks;

using UnityEngine;


public class KW_WaterOrthoDepth : MonoBehaviour
{
    public RenderTexture depthRT;
    public Texture2D depthTex;
    Camera depthCam;
    private GameObject cameraGameObject;

    private const int nearPlane = -2;
    private const int farPlane = 100;
    private const string folderName = "OrthoDepth";
    private const string textureName = "OrthoDepthTex";
    private const string textureDataName = "OrthoDepthTexData";
    private OrthoDepthParams lastRenderedParams;

    [System.Serializable]
    class OrthoDepthParams
    {
        [SerializeField] public int CamProjectionSize;
        [SerializeField] public float PositionX;
        [SerializeField] public float PositionY;
        [SerializeField] public float PositionZ;
    }


    public void Release()
    {
        OnDisable();
    }

    void OnDisable()
    {
      //  print("OrthoDepth.Disabled");

        KW_Extensions.SafeDestroy(cameraGameObject);
        KW_Extensions.SafeDestroy(depthTex);
        if (depthRT != null) depthRT.Release();
    }

    private void InitializeDepthTexture(int size)
    {
        if (depthRT != null) depthRT.Release();
        depthRT = new RenderTexture(size, size, 32, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
        depthRT.wrapMode = TextureWrapMode.Repeat;
    }

    void InitializeCamera()
    {
        if (cameraGameObject == null)
        {
            cameraGameObject = new GameObject("WaterOrthoDepthCamera");
            cameraGameObject.transform.parent = transform;
            depthCam = cameraGameObject.AddComponent<Camera>();
            depthCam.targetTexture = depthRT;
            depthCam.renderingPath = RenderingPath.Forward;
            depthCam.orthographic = true;
            depthCam.allowMSAA = false;
            depthCam.allowHDR = false;
            depthCam.nearClipPlane = nearPlane;
            depthCam.farClipPlane = farPlane;
            depthCam.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    void UpdateShaderVariables(Texture tex)
    {
        Shader.SetGlobalTexture("KW_OrthoDepth", tex);
        Shader.SetGlobalFloat("KW_DepthOrthographicSize", lastRenderedParams.CamProjectionSize);
        Shader.SetGlobalVector("KW_DepthNearFarDistance", new Vector3(nearPlane, farPlane, farPlane - nearPlane));
        Shader.SetGlobalVector("KW_DepthPos", new Vector3(lastRenderedParams.PositionX, lastRenderedParams.PositionY, lastRenderedParams.PositionZ));
    }

    public void Render(int IgnoredLayer, Vector3 position, int depthAresSize, int depthTextureSize)
    {
        InitializeDepthTexture(depthTextureSize);
        InitializeCamera();

        depthCam.orthographicSize = depthAresSize * 0.5f;
        depthCam.transform.position = position;
        depthCam.cullingMask = ~IgnoredLayer;
        depthCam.enabled = false;
        depthCam.targetTexture = depthRT;
        depthCam.Render();

        if (lastRenderedParams == null) lastRenderedParams = new OrthoDepthParams();
        lastRenderedParams.CamProjectionSize = depthAresSize;
        lastRenderedParams.PositionX = position.x;
        lastRenderedParams.PositionY = position.y;
        lastRenderedParams.PositionZ = position.z;

        UpdateShaderVariables(depthRT);
    }

    public void SaveOrthoDepthData(string GUID)
    {
        var tempRT = new RenderTexture(depthRT.width, depthRT.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        Graphics.Blit(depthRT, tempRT);

        var pathToDepthDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        tempRT.SaveRenderTextureToFile(Path.Combine(pathToDepthDataFolder, folderName, GUID, textureName), TextureFormat.RFloat);
        KW_Extensions.SerializeToFile(Path.Combine(pathToDepthDataFolder, folderName, GUID, textureDataName), lastRenderedParams);
        tempRT.Release();
    }

    public async Task<bool> ReadOrthoDepth(string GUID)
    {
        var pathToDepthDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        if(depthTex != null) KW_Extensions.SafeDestroy(depthTex);
        depthTex = await KW_Extensions.ReadTextureFromFileAsync(Path.Combine(pathToDepthDataFolder, folderName, GUID, textureName));
        lastRenderedParams = await KW_Extensions.DeserializeFromFile<OrthoDepthParams>(Path.Combine(pathToDepthDataFolder, folderName, GUID, textureDataName));
        if (depthTex == null || lastRenderedParams.CamProjectionSize < 1) return false;
        UpdateShaderVariables(depthTex);
        return true;
    }

    public void ClearOrthoDepth(string GUID)
    {
        var pathToDepthDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
        File.Delete(Path.Combine(pathToDepthDataFolder, folderName, GUID, textureName + ".gz"));
    }
}
