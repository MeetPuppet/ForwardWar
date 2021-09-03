using NVIDIA.Flex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartCutter : MonoBehaviour
{
    public Transform Center;
    public Material capMaterial;

    public void ActivateBlades(RaycastHit info)
    {
        Vector3 dir = info.point - Center.position;

        GameObject[] pieces = MeshCutter.MeshBuilder.Cut(gameObject,
        info.point,
        dir.normalized,
        capMaterial);

        if (!pieces[1].GetComponent<Rigidbody>())
            pieces[1].AddComponent<Rigidbody>();
    }
}
