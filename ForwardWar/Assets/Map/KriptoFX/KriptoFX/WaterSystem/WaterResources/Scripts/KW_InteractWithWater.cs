using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class KW_InteractWithWater : MonoBehaviour
{
    public float DrawSize = 0.25f;
    public Vector3 Offset = Vector3.zero;

    [HideInInspector]
    public Transform CachedTransform;

    Vector3 lastPos;

    void Awake()
    {
        CachedTransform = transform;
        lastPos = CachedTransform.position;
    }

    public float GetForce()
    {
        var force = (Vector3.Distance(CachedTransform.position, lastPos));
       
        force = Mathf.Clamp((Mathf.Exp(force) - 1) * 10, 0, Mathf.Lerp(0.5f, 0.125f, Mathf.Clamp01(DrawSize * 4)));
        lastPos = CachedTransform.position;
     //   force /= Mathf.Max(DrawSize, 1.0f);
        return force;
    }

    void OnEnable()
    {
        KW_InteractiveWavesVariables.AddInteractScript(this);
    }

    void OnDisable()
    {
        KW_InteractiveWavesVariables.RemoveInteractScript(this);
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Offset, DrawSize * 0.5f);
    }
}
