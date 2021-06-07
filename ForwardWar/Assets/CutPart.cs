using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutPart : MonoBehaviour
{
    public GameObject[] go;
    public Rigidbody[] rda;
    public MeshCollider[] mca;

    // Start is called before the first frame update
    void Start()
    {
        List<Rigidbody> lr = new List<Rigidbody>();
        List<MeshCollider> lm = new List<MeshCollider>();

        for (int i = 0; i < go.Length; ++i)
        {
            Rigidbody rd = go[i].AddComponent<Rigidbody>();
            rd.constraints = RigidbodyConstraints.FreezeAll;
            lr.Add(rd);

            MeshCollider mc = go[i].AddComponent<MeshCollider>();
            mc.convex = true;
            lm.Add(mc);
        }

        rda = lr.ToArray();
        mca = lm.ToArray();
    }


    public void Activate()
    {
        // ��� �̰͸����� �����̴°�
        for (int i = 0; i < go.Length; ++i)
        {
            rda[i].constraints = RigidbodyConstraints.None;
            mca[i].enabled = true;
        }
    }

    /*
    private void PushObeject(Vector3 pos)
    {
        for (int i = 0; i < go.Length; ++i)
        {
            //��� ���� Ȯ��
            Vector3 dir = default;
            dir.x = rda[i].transform.position.x - pos.x;
            dir.z = rda[i].transform.position.z - pos.z;
            dir = dir.normalized;

            rda[i].AddForce(dir*5);

        }
    }
    */
}
