using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutPart : MonoBehaviour
{
    public Transform[] go;
    public Rigidbody[] rda;
    public MeshCollider[] mca;

    // Start is called before the first frame update
    void Start()
    {
        go = GetComponentsInChildren<Transform>();

        List<Rigidbody> lr = new List<Rigidbody>();
        List<MeshCollider> lm = new List<MeshCollider>();
        int i = 0;
        if(go.Length != 1)
        {
            i = 1;
        }
        for (; i < go.Length; ++i)
        {
            Rigidbody rd = go[i].gameObject.AddComponent<Rigidbody>();
            rd.constraints = RigidbodyConstraints.FreezeAll;
            lr.Add(rd);

            MeshCollider mc = go[i].gameObject.AddComponent<MeshCollider>();
            mc.convex = true;
            lm.Add(mc);
        }

        rda = lr.ToArray();
        mca = lm.ToArray();
    }


    public void Activate()
    {
        // ��� �̰͸����� �����̴°�
        for (int i = 0; i < rda.Length; ++i)
        {
            rda[i].constraints = RigidbodyConstraints.None;
            mca[i].enabled = true;
        }
    }

    public void PushObeject(Vector3 pos)
    {
        for (int i = 0; i < rda.Length; ++i)
        {


        }
    }
}
