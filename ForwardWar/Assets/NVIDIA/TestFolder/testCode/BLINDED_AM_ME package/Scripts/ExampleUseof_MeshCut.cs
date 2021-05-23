using UnityEngine;
using System.Collections;
using NVIDIA.Flex;

public class ExampleUseof_MeshCut : MonoBehaviour {

    public GameObject ParticleFlex;
	public Material capMaterial;
    public float SearchSector = 0.5f;
    public float Accuracy = 0.1f;

    // Use this for initialization
    void Start()
    {
        //RaycastHit hit;
        //
        //if (Physics.Raycast(transform.position, transform.forward, out hit))
        //{
        //    GameObject victim = hit.collider.gameObject;
        //    Mesh _victim_mesh = victim.GetComponent<MeshFilter>().mesh;
        //
        //    //Debug.Log($"vertexConut: {_victim_mesh.vertexCount} - vertex: {_victim_mesh.vertices.Length}");
        //    //Debug.Log($"triangles: {_victim_mesh.triangles.Length}");
        //
        //    GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(victim, ParticleFlex, transform.position, transform.right, capMaterial);
        //
        //
        //    Instantiate(pieces[1]);
        //
        //    //if (!pieces[1].GetComponent<Rigidbody>())
        //    //    pieces[1].AddComponent<Rigidbody>();
        //
        //}
    }

    void Update(){

        if (Input.GetMouseButtonDown(0))
        {
            
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit))
            {
                GameObject victim = hit.collider.gameObject;
                Mesh _victim_mesh = victim.GetComponent<MeshFilter>().mesh;

                //Debug.Log($"vertexConut: {_victim_mesh.vertexCount} - vertex: {_victim_mesh.vertices.Length}");
                //Debug.Log($"triangles: {_victim_mesh.triangles.Length}");

                GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(victim, ParticleFlex, transform.position, transform.right, capMaterial);


                Instantiate(pieces[1]);

                //if (!pieces[1].GetComponent<Rigidbody>())
                //    pieces[1].AddComponent<Rigidbody>();

            }
            
            /*
            Vector3 startPos = transform.position;
            startPos.x -= SearchSector;
            for (int j=0;j <= SearchSector / Accuracy * 2; ++j, startPos.x += Accuracy)
            {
                RaycastHit[] hit = Physics.RaycastAll(startPos, transform.forward, Mathf.Infinity);

                for (int i = 0; i < hit.Length; ++i)
                {
                    GameObject victim = hit[i].collider.gameObject;
                    Mesh _victim_mesh = victim.GetComponent<MeshFilter>().mesh;

                    //Debug.Log($"vertexConut: {_victim_mesh.vertexCount} - vertex: {_victim_mesh.vertices.Length}");
                    //Debug.Log($"triangles: {_victim_mesh.triangles.Length}");

                    GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(victim,ParticleFlex, startPos, transform.right, capMaterial);


                    Instantiate(pieces[1]);

                    //Destroy(pieces[1],5);
                }
            }
            */

        }
	}


    void OnDrawGizmosSelected() {

		Gizmos.color = Color.green;

		Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5.0f);
		Gizmos.DrawLine(transform.position + transform.up * SearchSector, transform.position + transform.up * SearchSector + transform.forward * 5.0f);
		Gizmos.DrawLine(transform.position + -transform.up * SearchSector, transform.position + -transform.up * SearchSector + transform.forward * 5.0f);

		Gizmos.DrawLine(transform.position, transform.position + transform.up * SearchSector);
		Gizmos.DrawLine(transform.position,  transform.position + -transform.up * SearchSector);

	}

}
