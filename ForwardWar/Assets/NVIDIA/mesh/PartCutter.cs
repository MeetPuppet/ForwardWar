using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartCutter : MonoBehaviour
{
    public GameObject flex;
    public Material capMaterial;
    public bool isWorked = false;

    public void ActivateBlades()
    {
        if (isWorked)
            return;
        GameObject[] pieces = MeshCutter.MeshBuilder.MeshSlice(transform.parent.gameObject,
        transform.position,
        transform.up,
        capMaterial);


        if (!pieces[0].GetComponent<Rigidbody>())
            pieces[0].AddComponent<Rigidbody>();


        for (int i=0; i < pieces.Length; ++i)
        {
            if (!pieces[i].GetComponent<Rigidbody>() && pieces[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.vertexCount > 100)
            {
                pieces[i].AddComponent<Rigidbody>();
                Instantiate(flex, transform);
                //isWorked = true;
                //Destroy(pieces[i]);
            }
        }
    }
}
