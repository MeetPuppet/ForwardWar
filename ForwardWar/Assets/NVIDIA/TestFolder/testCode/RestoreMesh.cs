using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreMesh : MonoBehaviour
{
    Mesh mesh = null;
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }
}
