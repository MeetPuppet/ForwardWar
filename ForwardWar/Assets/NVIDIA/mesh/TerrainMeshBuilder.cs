using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainMeshBuilder : MonoBehaviour
{
    public int Width = 1000;
    public int Height = 1000;

    public int startX = 0;
    public int endX = 1000;
    public int startY = 0;
    public int endY = 1000;

    public bool WidthBlock = false;
    public bool HeightBlock = false;
    Mesh testMesh;

    // Start is called before the first frame update
    void Start()
    {
        generateMesh();
    }

    public void generateMesh()
    {
        TerrainCollider terrainCollider = transform.GetComponent<TerrainCollider>();
        TerrainData data = terrainCollider.terrainData;

        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = new Mesh();
        meshCollider.sharedMesh.name = "Test Mesh";
        testMesh = meshCollider.sharedMesh;

        testMesh.bounds = data.bounds;
        //terrainCollider.terrainData.bounds

        List<Vector3> lv = new List<Vector3>();
        for (int j = Height; j >= 0; --j)
        {
            for (int i = 0; i <= Width; ++i)
            {
                Vector3 vec = new Vector3(i, data.GetHeight(i, j), j);
                lv.Add(vec);
            }
        }
        testMesh.vertices = lv.ToArray();


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
        testMesh.triangles = li.ToArray();

        meshCollider.enabled = false;
        meshCollider.enabled = true;
    }
}
