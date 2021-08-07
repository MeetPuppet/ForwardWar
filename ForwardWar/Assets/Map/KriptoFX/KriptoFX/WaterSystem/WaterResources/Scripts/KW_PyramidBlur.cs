using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KW_PyramidBlur
{
    private string BlurShaderName = "Hidden/KriptoFX/Water/GaussianBlur";
    const int kMaxIterations = 8;
    public Material blurMaterial;
    public RenderTexture[] blurBuffer1 = new RenderTexture[kMaxIterations];
    public RenderTexture[] blurBuffer2 = new RenderTexture[kMaxIterations];
    public BufferSize[] blurBufferSizes = new BufferSize[kMaxIterations];

    public class BufferSize
    {
        public int Width;
        public int Height;
    }

    public void Release()
    {

        for (int i = 0; i < kMaxIterations; i++)
        {
            if (blurBuffer1[i] != null) blurBuffer1[i].Release();
            if (blurBuffer2[i] != null) blurBuffer2[i].Release();
        }
        KW_Extensions.SafeDestroy(blurMaterial);
    }

    public void ComputeBlurPyramid(float blurRadius, RenderTexture sourceID, RenderTexture targetID, CommandBuffer cb)
    {
        if (blurMaterial == null) blurMaterial = KW_Extensions.CreateMaterial(BlurShaderName);

        RenderTexture last = null;

        var logh = Mathf.Log(Mathf.Max(targetID.width, targetID.height), 2) + blurRadius - 8;
        var logh_i = (int)logh;
        var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);

        blurMaterial.SetFloat("_SampleScale", 0.5f + logh - logh_i);
        cb.SetGlobalFloat("_SampleScale", 0.5f + logh - logh_i);
        var lastWidth = targetID.width;
        var lastHeight = targetID.height;

        for (var level = 0; level < iterations; level++)
        {
            if(blurBuffer1[level] != null) blurBuffer1[level].Release();
            blurBuffer1[level] = new RenderTexture(lastWidth, lastHeight, 0, sourceID.format);
            blurBuffer1[level].name = cb.name + "_blurPassDown" + level;

            blurBufferSizes[level] = new BufferSize { Width = lastWidth, Height = lastHeight };
            cb.SetRenderTarget(blurBuffer1[level]);
            var target = iterations == 1 ? targetID : blurBuffer1[level];
            cb.Blit(level == 0 ? sourceID : last, target, blurMaterial, 3);
            lastWidth = lastWidth / 2;
            lastHeight = lastHeight / 2;
            last = blurBuffer1[level];
        }

        for (var level = iterations - 2; level >= 0; level--)
        {
            var baseSize = blurBufferSizes[level];
            if(blurBuffer2[level] != null) blurBuffer2[level].Release();
            blurBuffer2[level] = new RenderTexture(baseSize.Width, baseSize.Height, 0, sourceID.format);
            blurBuffer2[level].name = cb.name + "_blurPassUp" + level;
            cb.Blit(last, level == 0 ? targetID : blurBuffer2[level], blurMaterial, 4);
            last = blurBuffer2[level];
        }

    }

    public void ComputeBlurPyramid(float blurRadius, RenderTexture sourceID, RenderTexture targetID)
    {
        if (blurMaterial == null) blurMaterial = KW_Extensions.CreateMaterial(BlurShaderName);

        RenderTexture last = null;

        var logh = Mathf.Log(Mathf.Max(targetID.width, targetID.height), 2) + blurRadius - 8;
        var logh_i = (int)logh;
        var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);

        blurMaterial.SetFloat("_SampleScale", 0.5f + logh - logh_i);
        blurMaterial.SetFloat("_SampleScale", 0.5f + logh - logh_i);
        var lastWidth = targetID.width;
        var lastHeight = targetID.height;

        for (var level = 0; level < iterations; level++)
        {
            blurBuffer1[level] = KW_Extensions.ReinitializeRenderTexture(blurBuffer1[level], lastWidth, lastHeight, 0, sourceID.format);
            blurBufferSizes[level] = new BufferSize { Width = lastWidth, Height = lastHeight };
            var target = iterations == 1 ? targetID : blurBuffer1[level];
            Graphics.Blit(level == 0 ? sourceID : last, target, blurMaterial, 3);
            lastWidth = lastWidth / 2;
            lastHeight = lastHeight / 2;
            last = blurBuffer1[level];
        }

        for (var level = iterations - 2; level >= 0; level--)
        {
            var baseSize = blurBufferSizes[level];
            blurBuffer2[level] = KW_Extensions.ReinitializeRenderTexture(blurBuffer2[level], baseSize.Width, baseSize.Height, 0, sourceID.format);
            Graphics.Blit(last, level == 0 ? targetID : blurBuffer2[level], blurMaterial, 4);
            last = blurBuffer2[level];
        }

    }
}
