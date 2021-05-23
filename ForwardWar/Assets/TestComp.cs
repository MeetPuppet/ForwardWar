using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestComp : MonoBehaviour
{
    public Material mat;
    public GameObject go;

    [SerializeField]
    public FlexArrayAsset FAA;

    // Start is called before the first frame update
    void Start()
    {
        Activate();
    }

    public void Activate()
    {


        List<MeshFilter> lMesh = new List<MeshFilter>();
        for(int i=0;i< transform.childCount; ++i)
        {
            lMesh.Add(transform.GetChild(i).GetComponent<MeshFilter>());
        }
        CombineInstance[] combine = new CombineInstance[lMesh.Count];

        go = Instantiate(go, transform);

        for (int i = 0; i < lMesh.Count; ++i)
        {
            combine[i].mesh = lMesh[i].sharedMesh;
            combine[i].transform = lMesh[i].transform.localToWorldMatrix;
            lMesh[i].gameObject.SetActive(false);
        }

        MeshFilter meshFilter = go.transform.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = go.transform.gameObject.AddComponent<MeshFilter>();

        //필터
        meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh.CombineMeshes(combine);
        meshFilter.sharedMesh.name = transform.gameObject.name;

        //렌더러
        MeshRenderer meshRenderer = go.transform.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = go.transform.gameObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = mat;

        //MeshCollider meshCollider = go.transform.GetComponent<MeshCollider>();
        //if (meshCollider == null)
        //{
        //    meshCollider = go.transform.gameObject.AddComponent<MeshCollider>();
        //}
        //meshCollider.sharedMesh = meshFilter.sharedMesh;

        //위치가 매우 기괴함
        go.transform.localPosition = new Vector3(-transform.position.x, -transform.position.y, -transform.position.z);
        //
        //
        FAA.m_boundaryMesh = meshFilter.sharedMesh;
        FAA.m_rebuildAsset = true;
        FAA.OnValidate();


        go.transform.gameObject.SetActive(false);
        go.transform.gameObject.SetActive(true);
        //mesh.CombineMeshes(combine,)
    }
}
