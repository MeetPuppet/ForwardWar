using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KW_PlanarReflection : MonoBehaviour
{
    private GameObject reflCameraGO;
    private Camera reflectionCamera;
    private RenderTexture reflectionRT;
    //float m_ClipPlaneOffset = 0.1f;

    public void Release()
    {
        KW_Extensions.SafeDestroy(reflCameraGO);
        if(reflectionRT != null) reflectionRT.Release();
        Debug.Log("KW_PlanarReflection.Released ");
    }

    void OnDisable()
    {
        Release();
    }

    void InitializeTexture(int width, int height)
    {
        if (reflectionRT != null) reflectionRT.Release();
        reflectionRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBHalf);
        reflectionRT.useMipMap = true;
    }

    public void RenderPlanar(Camera currentCamera, Vector3 waterPosition, float resolutionScale, List<Material> waterShaderMaterials)
    {
        if (reflCameraGO == null)
        {
            reflCameraGO = new GameObject("WaterReflectionCamera");
            reflCameraGO.transform.parent = transform;
            reflectionCamera = reflCameraGO.AddComponent<Camera>();
            reflectionCamera.enabled = false;
        }

        reflectionCamera.CopyFrom(currentCamera);
        reflectionCamera.allowHDR = true; //bug, without hdr camera doesn't render to texture
        reflectionCamera.allowMSAA = false;



        var width = (int)(currentCamera.pixelWidth * resolutionScale);
        var height = (int)(currentCamera.pixelHeight * resolutionScale);
        if (reflectionRT == null || reflectionRT.width != width || reflectionRT.height != height) InitializeTexture(width, height);

        var clipPlaneOffset = 0; //another way to do clip offset
        float d = -Vector3.Dot(Vector3.up, waterPosition) - clipPlaneOffset * 0.5f;

        Vector4 reflectionPlane = new Vector4(0, 1, 0, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 oldpos = currentCamera.transform.position;
        reflectionCamera.transform.position = reflection.MultiplyPoint(oldpos);
        reflectionCamera.worldToCameraMatrix = currentCamera.worldToCameraMatrix * reflection;

        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, waterPosition, Vector3.up, 1.0f, -clipPlaneOffset * 0.9f);
        Matrix4x4 projection = currentCamera.projectionMatrix;

        //var clipMatrix = CalculateObliqueMatrix(reflectionPlane, reflectionCamera);
        //reflectionCamera.projectionMatrix = clipMatrix;
        CalculateObliqueMatrix(ref projection, clipPlane);
        reflectionCamera.projectionMatrix = projection;
       // reflectionCamera.clearFlags = CameraClearFlags.SolidColor;
       // reflectionCamera.backgroundColor = Color.black;
        reflectionCamera.cullingMask = ~(1 << 4);



        var euler = currentCamera.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);

        GL.invertCulling = true;
        reflectionCamera.targetTexture = reflectionRT;
        reflectionCamera.Render();

        GL.invertCulling = false;

        foreach (var mat in waterShaderMaterials)
        {
            if (mat == null) continue;

            mat.SetTexture("KW_PlanarReflection", reflectionRT);

        }
    }

    private static float sgn(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }


    //private Matrix4x4 CalculateObliqueMatrix(Vector4 plane, Camera cam)
    //{
    //    var viewSpacePlane = cam.worldToCameraMatrix.inverse.transpose * plane;
    //    var projectionMatrix = cam.projectionMatrix;

    //    var clipSpaceFarPanelBoundPoint = new Vector4(Mathf.Sign(viewSpacePlane.x), Mathf.Sign(viewSpacePlane.y), 1, 1);
    //    var viewSpaceFarPanelBoundPoint = cam.projectionMatrix.inverse * clipSpaceFarPanelBoundPoint;

    //    var m4 = new Vector4(projectionMatrix.m30, projectionMatrix.m31, projectionMatrix.m32, projectionMatrix.m33);
    //    var u = 2.0f / Vector4.Dot(viewSpaceFarPanelBoundPoint, viewSpacePlane);
    //    var newViewSpaceNearPlane = u * viewSpacePlane;
    //    var m3 = newViewSpaceNearPlane - m4;

    //    projectionMatrix.m20 = m3.x;
    //    projectionMatrix.m21 = m3.y;
    //    projectionMatrix.m22 = m3.z;
    //    projectionMatrix.m23 = m3.w;

    //    return projectionMatrix;
    //}

    private static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
    {
        Vector4 q = projection.inverse * new Vector4
        (sgn(clipPlane.x),
            sgn(clipPlane.y),
            1.0f,
            1.0f);

        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));

        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign, float clipPlaneOffset)
    {
        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

}
