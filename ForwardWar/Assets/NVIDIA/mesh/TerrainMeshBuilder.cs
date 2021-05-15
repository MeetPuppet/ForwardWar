using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainMeshBuilder : MonoBehaviour
{
    public int Width = 1000;
    public int Height = 1000;
    public int Mag = 3;
    public int Scale = 1;

    public int startX = 0;
    public int endX = 1000;
    public int startY = 0;
    public int endY = 1000;

    public bool WidthBlock = false;
    public bool HeightBlock = false;
    Mesh testMesh = null;

    // Start is called before the first frame update
    void Start()
    {
    }


    TerrainData data = null;
    public void generateMesh()
    {
        TerrainCollider terrainCollider = transform.GetComponent<TerrainCollider>();
        if(data == null)
            data = terrainCollider.terrainData;

        if(transform.childCount != 0)
        {
            Transform[] childList = GetComponentsInChildren<Transform>();
            if (childList != null)
            {
                for (int i = 1; i < childList.Length; i++)
                {
                    if (childList[i] == null)
                        continue;

                    GameObject go = childList[i].gameObject;
                    DestroyImmediate(childList[i].gameObject);
                }
            }
        }

        GameObject terrainMesh = new GameObject("terrainMesh");
        terrainMesh.transform.parent = transform;

        float strangeScale = 1.17f;
        terrainMesh.transform.localScale = new Vector3(strangeScale * Scale, 1, strangeScale * Scale);

        MeshCollider meshCollider = terrainMesh.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = terrainMesh.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = new Mesh();
        meshCollider.sharedMesh.name = "Test Mesh";
        testMesh = meshCollider.sharedMesh;

        testMesh.bounds = data.bounds;
        //terrainCollider.terrainData.bounds

        generateVertices();

        generateTriangles();

        meshCollider.enabled = false;
        meshCollider.enabled = true;
    }

    public void generateTriangles()
    {
        List<int> li = new List<int>();
        for (int j = startY; j < endY; ++j)
        {
            for (int i = startX; i < endX; ++i)
            {
                li.Add(i + (j * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)));
                li.Add(i + (j * (Height + 1)));

                li.Add(i + (j * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)));
                if (WidthBlock)
                    break;
            }
            if (HeightBlock)
                break;
        }

        //for (int i = 0; i < li.Count; ++i)
        //{
        //    Debug.Log($"{li[i]} - {li.}")
        //}

        //65535를 넘기면 처음지점으로 일부가 이동됨 - 수치는 정상
        testMesh.triangles = li.ToArray();
    }

    public void generateVertices()
    {
        List<Vector3> lv = new List<Vector3>();
        for (int j = Height; j >= 0; --j)
        {
            for (int i = 0; i <= Width; ++i)
            {
                Vector3 vec = new Vector3(i, data.GetHeight(i * Mag, j * Mag), j);
                lv.Add(vec);
            }
        }
        testMesh.vertices = lv.ToArray();
    }

    //public void MeshRefresh()
    //{
    //    MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
    //    if (meshCollider == null)
    //    {
    //        Debug.LogError("Need Generate All");
    //        return;
    //    }
    //
    //    meshCollider.enabled = false;
    //    meshCollider.enabled = true;
    //}
}
/*분할 메쉬 생성기
 
    public void FakeOneDivideMesh(int cutline, int iter)
    {
        TerrainCollider terrainCollider = transform.GetComponent<TerrainCollider>();
        if (data == null)
            data = terrainCollider.terrainData;

        GameObject obj = new GameObject($"littleMesh_{iter}");
        obj.transform.parent = transform;
        obj.transform.localPosition -= new Vector3 (0, 0,(iter * cutline));

        MeshCollider lMeshCollider = obj.AddComponent<MeshCollider>();
        lMeshCollider.sharedMesh = new Mesh();
        Mesh littleMesh = lMeshCollider.sharedMesh;
        littleMesh.name = $"littleMesh_{iter}";
        littleMesh.bounds = data.bounds;


        List<Vector3> lListVec3 = new List<Vector3>();
        for (int j = Height; j >= Height - cutline ; --j)
        {
            for (int i = 0; i <= Width; ++i)
            {
                Vector3 vec = new Vector3(i, data.GetHeight(i, j), j);
                lListVec3.Add(vec);
            }
        }

        littleMesh.vertices = lListVec3.ToArray();

        List<int> li = new List<int>();
        for (int j = 0; j < cutline ; ++j)
        {
            for (int i = 0; i < 1000; ++i)
            {
                li.Add(i + (j * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)));
                li.Add(i + (j * (Height + 1)));

                li.Add(i + (j * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)));
            }
        }

        littleMesh.triangles = li.ToArray();

        lMeshCollider.enabled = false;
        lMeshCollider.enabled = true;
    }

    public void FakeDivideMesh()
    {
        Transform[] childList = GetComponentsInChildren<Transform>();
        if (childList != null)
        {
            for (int i = 1; i < childList.Length; i++)
            {
                if (childList[i] == null)
                    continue;

                GameObject go = childList[i].gameObject;
                DestroyImmediate(childList[i].gameObject);
            }
        }

        int cutline = (65535 - Width) / Width;
        int calcCount = cutline * Width;

        int leftVertex = ((Height+1)*(Width+1)) / calcCount;

        for (int i = 0; i < leftVertex; ++i)
        {
            FakeOneDivideMesh(cutline, i);
        }
        int leftline = ((65535 - Width) % Width) / Width;
        FakeOneDivideMesh(leftline, leftVertex);
    }

    public void OneDivideMesh(int cutline, int iter)
    {
        TerrainCollider terrainCollider = transform.GetComponent<TerrainCollider>();
        if (data == null)
            data = terrainCollider.terrainData;

        GameObject obj = new GameObject($"littleMesh_{iter}");
        obj.transform.parent = transform;

        MeshCollider lMeshCollider = obj.AddComponent<MeshCollider>();
        lMeshCollider.sharedMesh = new Mesh();
        Mesh littleMesh = lMeshCollider.sharedMesh;
        littleMesh.name = $"littleMesh_{iter}";
        littleMesh.bounds = data.bounds;


        List<Vector3> lListVec3 = new List<Vector3>();
        for (int j = Height; j >= Height - cutline; --j)
        {
            for (int i = 0; i <= Width; ++i)
            {
                Vector3 vec = new Vector3(i, data.GetHeight(i, j), j);
                lListVec3.Add(vec);
            }
        }

        littleMesh.vertices = lListVec3.ToArray();

        List<int> li = new List<int>();
        for (int j = 0; j < cutline; ++j)
        {
            for (int i = 0; i < 1000; ++i)
            {
                li.Add(i + (j * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)));
                li.Add(i + (j * (Height + 1)));

                li.Add(i + (j * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)));
            }
        }

        littleMesh.triangles = li.ToArray();

        lMeshCollider.enabled = false;
        lMeshCollider.enabled = true;
    }
    public void DivideMesh()
    {
        Transform[] childList = GetComponentsInChildren<Transform>();
        if (childList != null)
        {
            for (int i = 1; i < childList.Length; i++)
            {
                if (childList[i] == null)
                    continue;

                GameObject go = childList[i].gameObject;
                DestroyImmediate(childList[i].gameObject);
            }
        }

        int cutline = (65535 - Width) / Width;
        int calcCount = cutline * Width;

        int leftVertex = ((Height + 1) * (Width + 1)) / calcCount;

        for (int i = 0; i < leftVertex; ++i)
        {
            OneDivideMesh(cutline, i);
        }
        int leftline = ((65535 - Width) % Width) / Width;
        OneDivideMesh(leftline, leftVertex);
    }

    Vector3[] DivideVerties(Mesh dMesh,int min, int max)
    {
        List<Vector3> lListVec3 = new List<Vector3>();
        for (int i = min; i <= max; ++i)
        {
            for (int j = 0; j <= Width; ++j)
            {
                lListVec3.Add(testMesh.vertices[j+(i* Width)]);

            }
        }
       
        return lListVec3.ToArray();
    }

    int[] DivideTriangles(Mesh dMesh, int maxX, int maxY)
    {
        Mesh littleMesh = dMesh;

        List<int> li = new List<int>();
        for (int j = 0; j < maxY; ++j)
        {
            for (int i = 0; i < maxX; ++i)
            {
                li.Add(i + (j * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)));
                li.Add(i + (j * (Height + 1)));

                li.Add(i + (j * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)) + 1);
                li.Add(i + ((j + 1) * (Height + 1)));
            }
        }

        return li.ToArray();
    }

     */
