using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class FFT_GPU : MonoBehaviour
{
    public enum SizeSetting
    {
        Size_32 = 32,
        Size_64 = 64,
        Size_128 = 128,
        Size_256 = 256,
        Size_512 = 512,
    }



    public RenderTexture DisplaceTextureRaw;
    public RenderTexture DisplaceTextureMip;
    public RenderTexture NormalTexture;

    RenderTexture spectrumInitRenderTexture;
    RenderTexture spectrumHeight;
    RenderTexture spectrumDisplaceX;
    RenderTexture spectrumDisplaceZ;
    RenderTexture fftTemp1;
    RenderTexture fftTemp2;
    RenderTexture fftTemp3;

    Material normalComputeMaterial;

    ComputeShader spectrumShader;
    ComputeShader shaderFFT;

    float prevWindTurbulence;
    float prevWindSpeed;
    private float prevWindRotation;

    int kernelSpectrumInit;
    int kernelSpectrumUpdate;
    Color[] butterflyColors;
    Texture2D texButterfly;

    float[] displaceData;
    private int frameNumber;

    //float WindDirection;
    //float WindSpeed;
    //float Choppines;
    //float WaterScale = 25;
    //float TimeScale;


    public void Release()
    {
        if (spectrumShader != null) KW_Extensions.SafeDestroy(spectrumShader);
        if (shaderFFT != null) KW_Extensions.SafeDestroy(shaderFFT);
        if (texButterfly != null) KW_Extensions.SafeDestroy(texButterfly);
        if (spectrumInitRenderTexture != null) spectrumInitRenderTexture.Release();
        if (spectrumHeight != null) spectrumHeight.Release();
        if (spectrumDisplaceX != null) spectrumDisplaceX.Release();
        if (spectrumDisplaceZ != null) spectrumDisplaceZ.Release();
        if (fftTemp1 != null) fftTemp1.Release();
        if (fftTemp2 != null) fftTemp2.Release();
        if (fftTemp3 != null) fftTemp3.Release();
        //if (DisplaceTexture != null)
        //{
        //    AsyncTextureReader.ReleaseTempResources(DisplaceTexture);
        //    DisplaceTexture.Release();
        //}
        if (DisplaceTextureRaw != null) DisplaceTextureRaw.Release();
        if (DisplaceTextureMip != null) DisplaceTextureMip.Release();
        if (NormalTexture != null) NormalTexture.Release();

        if (normalComputeMaterial != null) KW_Extensions.SafeDestroy(normalComputeMaterial);

        prevWindTurbulence = -1;
        prevWindSpeed = -1;
        prevWindRotation = -1;
        isInitialized = false;

    }


    private bool isInitialized = false;
    void InitializeTextures(int size)
    {
        Release();

        //prevSize = Size;
        //prevWindDirection = WindDirection;
        //prevWindSpeed = WindSpeed;

        spectrumShader = Object.Instantiate(Resources.Load<ComputeShader>("Spectrum_GPU"));
        kernelSpectrumInit = spectrumShader.FindKernel("SpectrumInitalize");
        kernelSpectrumUpdate = spectrumShader.FindKernel("SpectrumUpdate");
        shaderFFT = Object.Instantiate(Resources.Load<ComputeShader>("ComputeFFT_GPU"));
        normalComputeMaterial = new Material(Shader.Find("KriptoFX/Water/ComputeNormal"));

        texButterfly = new Texture2D(size, Mathf.RoundToInt(Mathf.Log(size, 2)), TextureFormat.RGBAFloat, false, true);
        spectrumInitRenderTexture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        spectrumInitRenderTexture.enableRandomWrite = true;
        spectrumInitRenderTexture.Create();

        spectrumHeight = new RenderTexture(size, size, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
        spectrumHeight.enableRandomWrite = true;
        spectrumHeight.Create();

        spectrumDisplaceX = new RenderTexture(size, size, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
        spectrumDisplaceX.enableRandomWrite = true;
        spectrumDisplaceX.Create();

        spectrumDisplaceZ = new RenderTexture(size, size, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
        spectrumDisplaceZ.enableRandomWrite = true;
        spectrumDisplaceZ.Create();

        fftTemp1 = new RenderTexture(size, size, 0, RenderTextureFormat.RGFloat);
        fftTemp1.enableRandomWrite = true;
        fftTemp1.Create();

        fftTemp2 = new RenderTexture(size, size, 0, RenderTextureFormat.RGFloat);
        fftTemp2.enableRandomWrite = true;
        fftTemp2.Create();

        fftTemp3 = new RenderTexture(size, size, 0, RenderTextureFormat.RGFloat);
        fftTemp3.enableRandomWrite = true;
        fftTemp3.Create();

        DisplaceTextureRaw = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        DisplaceTextureRaw.enableRandomWrite = true;
        DisplaceTextureRaw.wrapMode = TextureWrapMode.Repeat;
        DisplaceTextureRaw.filterMode = FilterMode.Point;
        DisplaceTextureRaw.useMipMap = false;
        DisplaceTextureRaw.Create();

        DisplaceTextureMip = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        DisplaceTextureMip.wrapMode = TextureWrapMode.Repeat;
        DisplaceTextureMip.filterMode = FilterMode.Bilinear;
        DisplaceTextureMip.useMipMap = true;

        //displaceData = new float[DisplaceTexture.width * DisplaceTexture.height * 4];
        //AsyncTextureReader.RequestTextureData(DisplaceTexture);

        NormalTexture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        NormalTexture.filterMode = FilterMode.Bilinear;
        NormalTexture.wrapMode = TextureWrapMode.Repeat;
        NormalTexture.useMipMap = true;

        InitializeButterfly(size);
        isInitialized = true;
    }

    void OnDisable()
    {
        Release();
        //print("FFT.Disable");
    }


    public void ComputeFFT(LodPrefix lodPrefix, int size, bool requiredGPUReadbackForWaterPhysx,
        float domainSize, float windTurbulence, float windSpeed, float windRotation, float timeOffset, List<Material> waterSharedMaterials)
    {
        if(!isInitialized || DisplaceTextureRaw.width != size) InitializeTextures(size);

        if (Mathf.Abs(prevWindTurbulence - windTurbulence) > 0.001f || Mathf.Abs(prevWindSpeed - windSpeed) > 0.01f || Mathf.Abs(prevWindRotation - windRotation) > 0.01f)
        {
            prevWindTurbulence = windTurbulence;
            prevWindSpeed = windSpeed;
            prevWindRotation = windRotation;
            InitializeSpectrum(size, domainSize, windSpeed, windTurbulence, windRotation);
        }

        UpdateSpectrum(size, timeOffset, windRotation);
        DispatchFFT(size, domainSize, windSpeed);
        //if (requiredGPUReadbackForWaterPhysx) ReadDisplaceTextureToRawData();

        var normalLodSize = Mathf.RoundToInt(Mathf.Log(size, 2)) - 4;
        var mipCount = Mathf.RoundToInt(Mathf.Log(size, 2));

        foreach (var mat in waterSharedMaterials)
        {
            if (mat == null) continue;
            UpdateMaterialParameters(lodPrefix, normalLodSize, domainSize, mipCount, mat, windSpeed);
        }
        frameNumber++;
    }

    private int ID_DispTex = Shader.PropertyToID("KW_DispTex");
    private int ID_DispTex1 = Shader.PropertyToID("KW_DispTex_LOD1");
    private int ID_DispTex2 = Shader.PropertyToID("KW_DispTex_LOD2");
    private int ID_DispTexDetail = Shader.PropertyToID("KW_DispTex_Detail");

    private int ID_NormTex = Shader.PropertyToID("KW_NormTex");
    private int ID_NormTex1 = Shader.PropertyToID("KW_NormTex_LOD1");
    private int ID_NormTex2 = Shader.PropertyToID("KW_NormTex_LOD2");
    private int ID_NormTexDetail = Shader.PropertyToID("KW_NormTex_Detail");

    private int ID_MipCount = Shader.PropertyToID("KW_NormMipCount");
    private int ID_MipCount1 = Shader.PropertyToID("KW_NormMipCount_LOD1");
    private int ID_MipCount2 = Shader.PropertyToID("KW_NormMipCount_LOD2");
    private int ID_MipCountDetail = Shader.PropertyToID("KW_NormMipCount_Detail");

    private int ID_FFTDomainSize = Shader.PropertyToID("KW_FFTDomainSize");
    private int ID_FFTDomainSize1 = Shader.PropertyToID("KW_FFTDomainSize_LOD1");
    private int ID_FFTDomainSize2 = Shader.PropertyToID("KW_FFTDomainSize_LOD2");
    private int ID_FFTDomainSizeDetail = Shader.PropertyToID("KW_FFTDomainSize_Detail");

   // private int ID_WindSpeed = Shader.PropertyToID("KW_WindSpeed");
    private int ID_NormalLod = Shader.PropertyToID("KW_NormalLod");


    public enum LodPrefix
    {
        LOD0,
        LOD1,
        LOD2,
        Detailed
    }

    void UpdateMaterialParameters(LodPrefix lodPrefix, float normalLodSize, float domainSize, float mipCount, Material material, float windSpeed)
    {
        switch (lodPrefix)
        {
            case LodPrefix.LOD0:
                material.SetTexture(ID_DispTex, DisplaceTextureMip);
                material.SetTexture(ID_NormTex, NormalTexture);
                material.SetFloat(ID_FFTDomainSize, domainSize);
             //   material.SetFloat(ID_WindSpeed, windSpeed);
                material.SetFloat(ID_NormalLod, normalLodSize);
                material.SetFloat(ID_MipCount, mipCount);
                break;
            case LodPrefix.LOD1:
                material.SetTexture(ID_DispTex1, DisplaceTextureMip);
                material.SetTexture(ID_NormTex1, NormalTexture);
                material.SetFloat(ID_FFTDomainSize1, domainSize);
                material.SetFloat(ID_MipCount1, mipCount);
                break;
            case LodPrefix.LOD2:
                material.SetTexture(ID_DispTex2, DisplaceTextureMip);
                material.SetTexture(ID_NormTex2, NormalTexture);
                material.SetFloat(ID_FFTDomainSize2, domainSize);
                material.SetFloat(ID_MipCount2, mipCount);
                break;
            case LodPrefix.Detailed:
                material.SetTexture(ID_DispTexDetail, DisplaceTextureMip);
                material.SetTexture(ID_NormTexDetail, NormalTexture);
                material.SetFloat(ID_FFTDomainSizeDetail, domainSize);
                material.SetFloat(ID_MipCountDetail, mipCount);
                break;
        }
    }

    void InitializeButterfly(int size)
    {
        var log2Size = Mathf.RoundToInt(Mathf.Log(size, 2));
        butterflyColors = new Color[size * log2Size];

        int offset = 1, numIterations = size >> 1;
        for (int rowIndex = 0; rowIndex < log2Size; rowIndex++)
        {
            int rowOffset = rowIndex * size;
            {
                int start = 0, end = 2 * offset;
                for (int iteration = 0; iteration < numIterations; iteration++)
                {
                    var bigK = 0.0f;
                    for (int K = start; K < end; K += 2)
                    {
                        var phase = 2.0f * Mathf.PI * bigK * numIterations / size;
                        var cos = Mathf.Cos(phase);
                        var sin = Mathf.Sin(phase);
                        butterflyColors[rowOffset + K / 2] = new Color(cos, -sin, 0, 1);
                        butterflyColors[rowOffset + K / 2 + offset] = new Color(-cos, sin, 0, 1);

                        bigK += 1.0f;
                    }
                    start += 4 * offset;
                    end = start + 2 * offset;
                }
            }
            numIterations >>= 1;
            offset <<= 1;
        }

        texButterfly.SetPixels(butterflyColors);
        texButterfly.Apply();
    }

    void InitializeSpectrum(int size, float domainSize, float windSpeed, float windTurbulence, float windRotation)
    {
        spectrumShader.SetInt("size", size);

        spectrumShader.SetFloat("domainSize", domainSize);
        spectrumShader.SetFloats("turbulence", windTurbulence);
        spectrumShader.SetFloat("windSpeed", windSpeed);
        spectrumShader.SetFloat("windRotation", windRotation);
        spectrumShader.SetTexture(kernelSpectrumInit, "resultInit", spectrumInitRenderTexture);
        spectrumShader.Dispatch(kernelSpectrumInit, size / 8, size / 8, 1);
    }

    void UpdateSpectrum(int size, float timeOffset, float windRotation)
    {
        spectrumShader.SetFloat("time", timeOffset);
        spectrumShader.SetTexture(kernelSpectrumUpdate, "init0", spectrumInitRenderTexture);
        spectrumShader.SetTexture(kernelSpectrumUpdate, "resultHeight", spectrumHeight);
        spectrumShader.SetTexture(kernelSpectrumUpdate, "resultDisplaceX", spectrumDisplaceX);
        spectrumShader.SetTexture(kernelSpectrumUpdate, "resultDisplaceZ", spectrumDisplaceZ);

        spectrumShader.Dispatch(kernelSpectrumUpdate, size / 8, size / 8, 1);
    }

    void DispatchFFT(int size, float domainSize, float windSpeed)
    {
        var kernelOffset = 0;
        switch (size)
        {
            case (int)SizeSetting.Size_32:
                kernelOffset = 0;
                break;
            case (int)SizeSetting.Size_64:
                kernelOffset = 2;
                break;
            case (int)SizeSetting.Size_128:
                kernelOffset = 4;
                break;
            case (int)SizeSetting.Size_256:
                kernelOffset = 6;
                break;
            case (int)SizeSetting.Size_512:
                kernelOffset = 8;
                break;
        }

        shaderFFT.SetTexture(kernelOffset, "inputH", spectrumHeight);
        shaderFFT.SetTexture(kernelOffset, "inputX", spectrumDisplaceX);
        shaderFFT.SetTexture(kernelOffset, "inputZ", spectrumDisplaceZ);
        shaderFFT.SetTexture(kernelOffset, "inputButterfly", texButterfly);
        shaderFFT.SetTexture(kernelOffset, "output1", fftTemp1);
        shaderFFT.SetTexture(kernelOffset, "output2", fftTemp2);
        shaderFFT.SetTexture(kernelOffset, "output3", fftTemp3);
        shaderFFT.Dispatch(kernelOffset, 1, size, 1);

        shaderFFT.SetTexture(kernelOffset + 1, "inputH", fftTemp1);
        shaderFFT.SetTexture(kernelOffset + 1, "inputX", fftTemp2);
        shaderFFT.SetTexture(kernelOffset + 1, "inputZ", fftTemp3);
        shaderFFT.SetTexture(kernelOffset + 1, "inputButterfly", texButterfly);
        shaderFFT.SetTexture(kernelOffset + 1, "output", DisplaceTextureRaw);
        shaderFFT.Dispatch(kernelOffset + 1, size, 1, 1);
        Graphics.Blit(DisplaceTextureRaw, DisplaceTextureMip);

        normalComputeMaterial.SetFloat("KW_FFTDomainSize", domainSize);
        normalComputeMaterial.SetTexture("_DispTex", DisplaceTextureRaw);
        var sizeLog = Mathf.RoundToInt(Mathf.Log(size, 2)) - 4;
        normalComputeMaterial.SetFloat("_SizeLog", sizeLog);
        normalComputeMaterial.SetFloat("_WindSpeed", windSpeed);

        var temp = RenderTexture.active;
        if (!Application.isPlaying ) RenderTexture.active = null;
        Graphics.Blit(null, NormalTexture, normalComputeMaterial);
        if (!Application.isPlaying) RenderTexture.active = temp;
    }


    //public Vector3 GetWaterPos(Vector3 pos)
    //{
    //    pos.x = pos.x % WaterScale;
    //    if (pos.x < 0) pos.x = WaterScale + pos.x;
    //    pos.z = pos.z % WaterScale;
    //    if (pos.z < 0) pos.z = WaterScale + pos.z;

    //    var x = (int)((int) Size / WaterScale * pos.x);
    //    var z = (int)((int) Size / WaterScale * pos.z);

    //    var pixelIdx = x * 4 + DisplaceTexture.height * z * 4;
    //    return new Vector3(displaceData[pixelIdx]* Choppines, displaceData[pixelIdx + 1], displaceData[pixelIdx + 2]* Choppines) * WaterScale / 20 + new Vector3(0, transform.position.y, 0);
    //}

    //private void ReadDisplaceTextureToRawData()
    //{
    //    var status = AsyncTextureReader.RetrieveTextureData(DisplaceTexture, displaceData);
    //    if (status == AsyncTextureReader.Status.Succeeded)
    //    {
    //        AsyncTextureReader.RequestTextureData(DisplaceTexture);
    //    }
    //}

}


