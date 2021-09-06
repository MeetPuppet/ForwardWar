using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MeshCutter
{
    public class MeshBuilder
    {
        private static Plane _blade;
        private static Mesh _victim_mesh;

        // Caching
        private static MeshInfo _leftSide = new MeshInfo();
        private static MeshInfo _rightSide = new MeshInfo();
        private static MeshInfo.Triangle _triangleCache = new MeshInfo.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        private static List<Vector3> _newVerticesCache = new List<Vector3>();
        private static bool[] _isLeftSideCache = new bool[3];
        private static int _capMatSub = 1;

        public static GameObject[] nothing(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial)
        {

            // set the blade relative to victim
            _blade = new Plane(victim.transform.InverseTransformDirection(-normalDirection),
                victim.transform.InverseTransformPoint(anchorPoint));

            // get the victims mesh
            Mesh bake = new Mesh();
            victim.GetComponent<SkinnedMeshRenderer>().BakeMesh(bake);
            _victim_mesh = bake;

            // two new meshes
            _leftSide.Clear();
            _rightSide.Clear();
            _newVerticesCache.Clear();


            int index_1, index_2, index_3;

            var mesh_vertices = _victim_mesh.vertices;
            var mesh_normals = _victim_mesh.normals;
            var mesh_uvs = _victim_mesh.uv;
            var mesh_tangents = _victim_mesh.tangents;
            if (mesh_tangents != null && mesh_tangents.Length == 0)
                mesh_tangents = null;

            // go through the submeshes
            for (int submeshIterator = 0; submeshIterator < _victim_mesh.subMeshCount; submeshIterator++)
            {

                // Triangles
                var indices = _victim_mesh.GetTriangles(submeshIterator);

                for (int i = 0; i < indices.Length; i += 3)
                {

                    index_1 = indices[i];
                    index_2 = indices[i + 1];
                    index_3 = indices[i + 2];

                    // verts
                    _triangleCache.vertices[0] = mesh_vertices[index_1];
                    _triangleCache.vertices[1] = mesh_vertices[index_2];
                    _triangleCache.vertices[2] = mesh_vertices[index_3];

                    // normals
                    _triangleCache.normals[0] = mesh_normals[index_1];
                    _triangleCache.normals[1] = mesh_normals[index_2];
                    _triangleCache.normals[2] = mesh_normals[index_3];

                    // uvs
                    _triangleCache.uvs[0] = mesh_uvs[index_1];
                    _triangleCache.uvs[1] = mesh_uvs[index_2];
                    _triangleCache.uvs[2] = mesh_uvs[index_3];

                    // tangents
                    if (mesh_tangents != null)
                    {
                        _triangleCache.tangents[0] = mesh_tangents[index_1];
                        _triangleCache.tangents[1] = mesh_tangents[index_2];
                        _triangleCache.tangents[2] = mesh_tangents[index_3];
                    }
                    else
                    {
                        _triangleCache.tangents[0] = Vector4.zero;
                        _triangleCache.tangents[1] = Vector4.zero;
                        _triangleCache.tangents[2] = Vector4.zero;
                    }

                    _leftSide.AddTriangle(_triangleCache, submeshIterator);
                }
            }


            // Left Mesh
            Mesh left_HalfMesh = _leftSide.GetMesh();
            left_HalfMesh.name = "Split Mesh Left";
            // assign the game objects

            victim.name = "origin";
            victim.GetComponent<SkinnedMeshRenderer>().sharedMesh = left_HalfMesh;
            victim.GetComponent<SkinnedMeshRenderer>().rootBone = null;


            GameObject obj = new GameObject();
            obj.transform.parent = victim.transform.parent;

            victim.transform.localScale = obj.transform.localScale;

            GameObject leftSideObj = victim;

            //GameObject.Destroy(obj);
            return new GameObject[] { leftSideObj };

        }
        public static GameObject[] Cut(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial)
        {
            _blade = new Plane(victim.transform.InverseTransformDirection(-normalDirection),
                victim.transform.InverseTransformPoint(anchorPoint));

            _victim_mesh = victim.GetComponent<MeshFilter>().mesh;

            _leftSide.Clear();
            _rightSide.Clear();
            _newVerticesCache.Clear();


            int index_1, index_2, index_3;

            var mesh_vertices = _victim_mesh.vertices;
            var mesh_normals = _victim_mesh.normals;
            var mesh_uvs = _victim_mesh.uv;
            var mesh_tangents = _victim_mesh.tangents;
            if (mesh_tangents != null && mesh_tangents.Length == 0)
                mesh_tangents = null;

            for (int submeshIterator = 0; submeshIterator < _victim_mesh.subMeshCount; submeshIterator++)
            {

                var indices = _victim_mesh.GetTriangles(submeshIterator);

                for (int i = 0; i < indices.Length; i += 3)
                {

                    index_1 = indices[i];
                    index_2 = indices[i + 1];
                    index_3 = indices[i + 2];

                    _triangleCache.vertices[0] = mesh_vertices[index_1];
                    _triangleCache.vertices[1] = mesh_vertices[index_2];
                    _triangleCache.vertices[2] = mesh_vertices[index_3];

                    _triangleCache.normals[0] = mesh_normals[index_1];
                    _triangleCache.normals[1] = mesh_normals[index_2];
                    _triangleCache.normals[2] = mesh_normals[index_3];

                    _triangleCache.uvs[0] = mesh_uvs[index_1];
                    _triangleCache.uvs[1] = mesh_uvs[index_2];
                    _triangleCache.uvs[2] = mesh_uvs[index_3];

                    if (mesh_tangents != null)
                    {
                        _triangleCache.tangents[0] = mesh_tangents[index_1];
                        _triangleCache.tangents[1] = mesh_tangents[index_2];
                        _triangleCache.tangents[2] = mesh_tangents[index_3];
                    }
                    else
                    {
                        _triangleCache.tangents[0] = Vector4.zero;
                        _triangleCache.tangents[1] = Vector4.zero;
                        _triangleCache.tangents[2] = Vector4.zero;
                    }

                    _isLeftSideCache[0] = _blade.GetSide(mesh_vertices[index_1]);
                    _isLeftSideCache[1] = _blade.GetSide(mesh_vertices[index_2]);
                    _isLeftSideCache[2] = _blade.GetSide(mesh_vertices[index_3]);


                    if (_isLeftSideCache[0] == _isLeftSideCache[1] && _isLeftSideCache[0] == _isLeftSideCache[2])
                    {

                        if (_isLeftSideCache[0])
                            _leftSide.AddTriangle(_triangleCache, submeshIterator);
                        else
                            _rightSide.AddTriangle(_triangleCache, submeshIterator);

                    }
                    else
                    {

                        Cut_this_Face(ref _triangleCache, submeshIterator);
                    }
                }
            }

            Material[] mats = victim.GetComponent<MeshRenderer>().sharedMaterials;
            if (mats[mats.Length - 1].name != capMaterial.name)
            {
                Material[] newMats = new Material[mats.Length + 1];
                mats.CopyTo(newMats, 0);
                newMats[mats.Length] = capMaterial;
                mats = newMats;
            }
            _capMatSub = mats.Length - 1;

            Cap_the_Cut();


            Mesh left_HalfMesh = _leftSide.GetMesh();
            left_HalfMesh.name = "Split Mesh Left";

            Mesh right_HalfMesh = _rightSide.GetMesh();
            right_HalfMesh.name = "Split Mesh Right";

            victim.name = "left side";
            victim.GetComponent<MeshFilter>().mesh = left_HalfMesh;
            victim.GetComponent<MeshCollider>().sharedMesh = left_HalfMesh;

            GameObject leftSideObj = victim;

            GameObject rightSideObj = new GameObject("right side", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            rightSideObj.transform.position = victim.transform.position;
            rightSideObj.transform.rotation = victim.transform.rotation;
            rightSideObj.GetComponent<MeshFilter>().mesh = right_HalfMesh;
            rightSideObj.GetComponent<MeshCollider>().sharedMesh = right_HalfMesh;
            rightSideObj.GetComponent<MeshCollider>().convex = true;

            if (victim.transform.parent != null)
            {
                rightSideObj.transform.parent = victim.transform.parent;
            }

            rightSideObj.transform.localScale = victim.transform.localScale;

            leftSideObj.GetComponent<MeshRenderer>().materials = mats;
            rightSideObj.GetComponent<MeshRenderer>().materials = mats;

            return new GameObject[] { leftSideObj, rightSideObj };
        }
        public static GameObject[] MeshSlice(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial)
        {

            // set the blade relative to victim
            _blade = new Plane(victim.transform.InverseTransformDirection(-normalDirection),
                victim.transform.InverseTransformPoint(anchorPoint));

            // get the victims mesh
            Mesh bake = new Mesh();
            victim.GetComponent<SkinnedMeshRenderer>().BakeMesh(bake);
            _victim_mesh = bake;

            // two new meshes
            _leftSide.Clear();
            _rightSide.Clear();
            _newVerticesCache.Clear();


            int index_1, index_2, index_3;

            var mesh_vertices = _victim_mesh.vertices;
            var mesh_normals = _victim_mesh.normals;
            var mesh_uvs = _victim_mesh.uv;
            var mesh_tangents = _victim_mesh.tangents;
            if (mesh_tangents != null && mesh_tangents.Length == 0)
                mesh_tangents = null;

            // go through the submeshes
            for (int submeshIterator = 0; submeshIterator < _victim_mesh.subMeshCount; submeshIterator++)
            {

                // Triangles
                var indices = _victim_mesh.GetTriangles(submeshIterator);

                for (int i = 0; i < indices.Length; i += 3)
                {

                    index_1 = indices[i];
                    index_2 = indices[i + 1];
                    index_3 = indices[i + 2];

                    // verts
                    _triangleCache.vertices[0] = mesh_vertices[index_1];
                    _triangleCache.vertices[1] = mesh_vertices[index_2];
                    _triangleCache.vertices[2] = mesh_vertices[index_3];

                    // normals
                    _triangleCache.normals[0] = mesh_normals[index_1];
                    _triangleCache.normals[1] = mesh_normals[index_2];
                    _triangleCache.normals[2] = mesh_normals[index_3];

                    // uvs
                    _triangleCache.uvs[0] = mesh_uvs[index_1];
                    _triangleCache.uvs[1] = mesh_uvs[index_2];
                    _triangleCache.uvs[2] = mesh_uvs[index_3];

                    // tangents
                    if (mesh_tangents != null)
                    {
                        _triangleCache.tangents[0] = mesh_tangents[index_1];
                        _triangleCache.tangents[1] = mesh_tangents[index_2];
                        _triangleCache.tangents[2] = mesh_tangents[index_3];
                    }
                    else
                    {
                        _triangleCache.tangents[0] = Vector4.zero;
                        _triangleCache.tangents[1] = Vector4.zero;
                        _triangleCache.tangents[2] = Vector4.zero;
                    }

                    // which side are the vertices on
                    _isLeftSideCache[0] = _blade.GetSide(mesh_vertices[index_1]);
                    _isLeftSideCache[1] = _blade.GetSide(mesh_vertices[index_2]);
                    _isLeftSideCache[2] = _blade.GetSide(mesh_vertices[index_3]);


                    // whole triangle
                    if (_isLeftSideCache[0] == _isLeftSideCache[1] && _isLeftSideCache[0] == _isLeftSideCache[2])
                    {

                        if (_isLeftSideCache[0]) // left side
                            _leftSide.AddTriangle(_triangleCache, submeshIterator);
                        else // right side
                            _rightSide.AddTriangle(_triangleCache, submeshIterator);

                    }
                    else
                    { // cut the triangle

                        Cut_this_Face(ref _triangleCache, submeshIterator);
                    }
                }
            }

            // The capping Material will be at the end
            SkinnedMeshRenderer smr = victim.GetComponent<SkinnedMeshRenderer>();
            Material[] mats = smr.sharedMaterials;
            if (mats[mats.Length - 1].name != capMaterial.name)
            {
                Material[] newMats = new Material[mats.Length + 1];
                mats.CopyTo(newMats, 0);
                newMats[mats.Length] = capMaterial;
                mats = newMats;
            }
            _capMatSub = mats.Length - 1; // for later use

            // cap the opennings
            Cap_the_Cut();


            // Left Mesh
            Mesh left_HalfMesh = _leftSide.GetMesh();
            left_HalfMesh.name = "Split Mesh Left";

            // Right Mesh
            Mesh right_HalfMesh = _rightSide.GetMesh();
            right_HalfMesh.name = "Split Mesh Right";


            // assign the game objects

            victim.name = "origin";
            victim.GetComponent<SkinnedMeshRenderer>().sharedMesh = left_HalfMesh;
            victim.GetComponent<SkinnedMeshRenderer>().rootBone = null;

            MeshCollider meshCollider = victim.GetComponent<MeshCollider>();
            if (victim.GetComponent<MeshCollider>() == null)
            {
                meshCollider = victim.AddComponent<MeshCollider>();
            }
            meshCollider.sharedMesh = left_HalfMesh;
            meshCollider.convex = true;


            GameObject rightSideObj = new GameObject("apart", typeof(SkinnedMeshRenderer), typeof(MeshCollider));
            rightSideObj.transform.position = victim.transform.position;
            rightSideObj.transform.rotation = victim.transform.rotation;
            rightSideObj.GetComponent<SkinnedMeshRenderer>().sharedMesh = right_HalfMesh;
            rightSideObj.GetComponent<MeshCollider>().sharedMesh = right_HalfMesh;
            rightSideObj.GetComponent<MeshCollider>().convex = true;

            if (victim.transform.parent != null)
            {
                rightSideObj.transform.parent = victim.transform.parent;
            }

            victim.transform.localScale = rightSideObj.transform.localScale;

            GameObject leftSideObj = victim;

            // assign mats
            leftSideObj.GetComponent<SkinnedMeshRenderer>().materials = mats;
            rightSideObj.GetComponent<SkinnedMeshRenderer>().materials = mats;

            if (_rightSide.VertCount <= 0)
            {
                return new GameObject[] { leftSideObj };
            }
            return new GameObject[] { leftSideObj, rightSideObj };

        }

        private static List<int> _capUsedIndicesCache = new List<int>();
        private static List<int> _capPolygonIndicesCache = new List<int>();
        // Functions
        private static void Cap_the_Cut()
        {

            _capUsedIndicesCache.Clear();
            _capPolygonIndicesCache.Clear();

            // find the needed polygons
            // the cut faces added new vertices by 2 each time to make an edge
            // if two edges contain the same Vector3 point, they are connected
            for (int i = 0; i < _newVerticesCache.Count; i += 2)
            {
                // check the edge
                if (!_capUsedIndicesCache.Contains(i)) // if it has one, it has this edge
                {
                    //new polygon started with this edge
                    _capPolygonIndicesCache.Clear();
                    _capPolygonIndicesCache.Add(i);
                    _capPolygonIndicesCache.Add(i + 1);

                    _capUsedIndicesCache.Add(i);
                    _capUsedIndicesCache.Add(i + 1);

                    Vector3 connectionPointLeft = _newVerticesCache[i];
                    Vector3 connectionPointRight = _newVerticesCache[i + 1];
                    bool isDone = false;

                    // look for more edges
                    while (!isDone)
                    {
                        isDone = true;

                        // loop through edges
                        for (int index = 0; index < _newVerticesCache.Count; index += 2)
                        {   // if it has one, it has this edge
                            if (!_capUsedIndicesCache.Contains(index))
                            {
                                Vector3 nextPoint1 = _newVerticesCache[index];
                                Vector3 nextPoint2 = _newVerticesCache[index + 1];

                                // check for next point in the chain
                                if (connectionPointLeft == nextPoint1 ||
                                    connectionPointLeft == nextPoint2 ||
                                    connectionPointRight == nextPoint1 ||
                                    connectionPointRight == nextPoint2)
                                {
                                    _capUsedIndicesCache.Add(index);
                                    _capUsedIndicesCache.Add(index + 1);

                                    // add the other
                                    if (connectionPointLeft == nextPoint1)
                                    {
                                        _capPolygonIndicesCache.Insert(0, index + 1);
                                        connectionPointLeft = _newVerticesCache[index + 1];
                                    }
                                    else if (connectionPointLeft == nextPoint2)
                                    {
                                        _capPolygonIndicesCache.Insert(0, index);
                                        connectionPointLeft = _newVerticesCache[index];
                                    }
                                    else if (connectionPointRight == nextPoint1)
                                    {
                                        _capPolygonIndicesCache.Add(index + 1);
                                        connectionPointRight = _newVerticesCache[index + 1];
                                    }
                                    else if (connectionPointRight == nextPoint2)
                                    {
                                        _capPolygonIndicesCache.Add(index);
                                        connectionPointRight = _newVerticesCache[index];
                                    }

                                    isDone = false;
                                }
                            }
                        }
                    }// while isDone = False

                    // check if the link is closed
                    // first == last
                    if (_newVerticesCache[_capPolygonIndicesCache[0]] == _newVerticesCache[_capPolygonIndicesCache[_capPolygonIndicesCache.Count - 1]])
                        _capPolygonIndicesCache[_capPolygonIndicesCache.Count - 1] = _capPolygonIndicesCache[0];
                    else
                        _capPolygonIndicesCache.Add(_capPolygonIndicesCache[0]);

                    // cap
                    FillCap_Method1(_capPolygonIndicesCache);
                }
            }
        }
        private static void FillCap_Method1(List<int> indices)
        {

            // center of the cap
            Vector3 center = Vector3.zero;
            foreach (var index in indices)
                center += _newVerticesCache[index];

            center = center / indices.Count;

            // you need an axis based on the cap
            Vector3 upward = Vector3.zero;
            // 90 degree turn
            upward.x = _blade.normal.y;
            upward.y = -_blade.normal.x;
            upward.z = _blade.normal.z;
            Vector3 left = Vector3.Cross(_blade.normal, upward);

            Vector3 displacement = Vector3.zero;
            Vector2 newUV1 = Vector2.zero;
            Vector2 newUV2 = Vector2.zero;
            Vector2 newUV3 = Vector2.zero;

            // indices should be in order like a closed chain

            // go through edges and eliminate by creating triangles with connected edges
            // each new triangle removes 2 edges but creates 1 new edge
            // keep the chain in order
            int iterator = 0;
            while (indices.Count > 2)
            {

                Vector3 link1 = _newVerticesCache[indices[iterator]];
                Vector3 link2 = _newVerticesCache[indices[(iterator + 1) % indices.Count]];
                Vector3 link3 = _newVerticesCache[indices[(iterator + 2) % indices.Count]];

                displacement = link1 - center;
                newUV1 = Vector3.zero;
                newUV1.x = 0.5f + Vector3.Dot(displacement, left);
                newUV1.y = 0.5f + Vector3.Dot(displacement, upward);
                //newUV1.z = 0.5f + Vector3.Dot(displacement, _blade.normal);

                displacement = link2 - center;
                newUV2 = Vector3.zero;
                newUV2.x = 0.5f + Vector3.Dot(displacement, left);
                newUV2.y = 0.5f + Vector3.Dot(displacement, upward);
                //newUV2.z = 0.5f + Vector3.Dot(displacement, _blade.normal);

                displacement = link3 - center;
                newUV3 = Vector3.zero;
                newUV3.x = 0.5f + Vector3.Dot(displacement, left);
                newUV3.y = 0.5f + Vector3.Dot(displacement, upward);
                //newUV2.z = 0.5f + Vector3.Dot(displacement, _blade.normal);


                // add triangle
                _newTriangleCache.vertices[0] = link1;
                _newTriangleCache.uvs[0] = newUV1;
                _newTriangleCache.normals[0] = -_blade.normal;
                _newTriangleCache.tangents[0] = Vector4.zero;

                _newTriangleCache.vertices[1] = link2;
                _newTriangleCache.uvs[1] = newUV2;
                _newTriangleCache.normals[1] = -_blade.normal;
                _newTriangleCache.tangents[1] = Vector4.zero;

                _newTriangleCache.vertices[2] = link3;
                _newTriangleCache.uvs[2] = newUV3;
                _newTriangleCache.normals[2] = -_blade.normal;
                _newTriangleCache.tangents[2] = Vector4.zero;

                // add to left side
                NormalCheck(ref _newTriangleCache);

                _leftSide.AddTriangle(_newTriangleCache, _capMatSub);

                // add to right side
                _newTriangleCache.normals[0] = _blade.normal;
                _newTriangleCache.normals[1] = _blade.normal;
                _newTriangleCache.normals[2] = _blade.normal;

                NormalCheck(ref _newTriangleCache);

                _rightSide.AddTriangle(_newTriangleCache, _capMatSub);


                // adjust indices by removing the middle link
                indices.RemoveAt((iterator + 1) % indices.Count);

                // move on
                iterator = (iterator + 1) % indices.Count;
            }

        }
        private static MeshInfo.Triangle _leftTriangleCache = new MeshInfo.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        private static MeshInfo.Triangle _rightTriangleCache = new MeshInfo.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        private static MeshInfo.Triangle _newTriangleCache = new MeshInfo.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        // Functions
        private static void Cut_this_Face(ref MeshInfo.Triangle triangle, int submesh)
        {

            _isLeftSideCache[0] = _blade.GetSide(triangle.vertices[0]); // true = left
            _isLeftSideCache[1] = _blade.GetSide(triangle.vertices[1]);
            _isLeftSideCache[2] = _blade.GetSide(triangle.vertices[2]);


            int leftCount = 0;
            int rightCount = 0;

            for (int i = 0; i < 3; i++)
            {
                if (_isLeftSideCache[i])
                { // left

                    _leftTriangleCache.vertices[leftCount] = triangle.vertices[i];
                    _leftTriangleCache.uvs[leftCount] = triangle.uvs[i];
                    _leftTriangleCache.normals[leftCount] = triangle.normals[i];
                    _leftTriangleCache.tangents[leftCount] = triangle.tangents[i];

                    leftCount++;
                }
                else
                { // right

                    _rightTriangleCache.vertices[rightCount] = triangle.vertices[i];
                    _rightTriangleCache.uvs[rightCount] = triangle.uvs[i];
                    _rightTriangleCache.normals[rightCount] = triangle.normals[i];
                    _rightTriangleCache.tangents[rightCount] = triangle.tangents[i];

                    rightCount++;
                }
            }

            // find the new triangles X 3
            // first the new vertices

            // this will give me a triangle with the solo point as first
            if (leftCount == 1)
            {
                _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
                _triangleCache.normals[0] = _leftTriangleCache.normals[0];
                _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _rightTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _rightTriangleCache.uvs[0];
                _triangleCache.normals[1] = _rightTriangleCache.normals[0];
                _triangleCache.tangents[1] = _rightTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _rightTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _rightTriangleCache.uvs[1];
                _triangleCache.normals[2] = _rightTriangleCache.normals[1];
                _triangleCache.tangents[2] = _rightTriangleCache.tangents[1];
            }
            else // rightCount == 1
            {
                _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
                _triangleCache.normals[0] = _rightTriangleCache.normals[0];
                _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _leftTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _leftTriangleCache.uvs[0];
                _triangleCache.normals[1] = _leftTriangleCache.normals[0];
                _triangleCache.tangents[1] = _leftTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _leftTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _leftTriangleCache.uvs[1];
                _triangleCache.normals[2] = _leftTriangleCache.normals[1];
                _triangleCache.tangents[2] = _leftTriangleCache.tangents[1];
            }

            // now to find the intersection points between the solo point and the others
            float distance = 0;
            float normalizedDistance = 0.0f;
            Vector3 edgeVector = Vector3.zero; // contains edge length and direction

            edgeVector = _triangleCache.vertices[1] - _triangleCache.vertices[0];
            _blade.Raycast(new Ray(_triangleCache.vertices[0], edgeVector.normalized), out distance);

            normalizedDistance = distance / edgeVector.magnitude;
            _newTriangleCache.vertices[0] = Vector3.Lerp(_triangleCache.vertices[0], _triangleCache.vertices[1], normalizedDistance);
            _newTriangleCache.uvs[0] = Vector2.Lerp(_triangleCache.uvs[0], _triangleCache.uvs[1], normalizedDistance);
            _newTriangleCache.normals[0] = Vector3.Lerp(_triangleCache.normals[0], _triangleCache.normals[1], normalizedDistance);
            _newTriangleCache.tangents[0] = Vector4.Lerp(_triangleCache.tangents[0], _triangleCache.tangents[1], normalizedDistance);

            edgeVector = _triangleCache.vertices[2] - _triangleCache.vertices[0];
            _blade.Raycast(new Ray(_triangleCache.vertices[0], edgeVector.normalized), out distance);

            normalizedDistance = distance / edgeVector.magnitude;
            _newTriangleCache.vertices[1] = Vector3.Lerp(_triangleCache.vertices[0], _triangleCache.vertices[2], normalizedDistance);
            _newTriangleCache.uvs[1] = Vector2.Lerp(_triangleCache.uvs[0], _triangleCache.uvs[2], normalizedDistance);
            _newTriangleCache.normals[1] = Vector3.Lerp(_triangleCache.normals[0], _triangleCache.normals[2], normalizedDistance);
            _newTriangleCache.tangents[1] = Vector4.Lerp(_triangleCache.tangents[0], _triangleCache.tangents[2], normalizedDistance);

            if (_newTriangleCache.vertices[0] != _newTriangleCache.vertices[1])
            {
                //tracking newly created points
                _newVerticesCache.Add(_newTriangleCache.vertices[0]);
                _newVerticesCache.Add(_newTriangleCache.vertices[1]);
            }
            // make the new triangles
            // one side will get 1 the other will get 2

            if (leftCount == 1)
            {
                // first one on the left
                _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
                _triangleCache.normals[0] = _leftTriangleCache.normals[0];
                _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
                _triangleCache.normals[1] = _newTriangleCache.normals[0];
                _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // check if it is facing the right way
                NormalCheck(ref _triangleCache);

                // add it
                _leftSide.AddTriangle(_triangleCache, submesh);


                // other two on the right
                _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
                _triangleCache.normals[0] = _rightTriangleCache.normals[0];
                _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
                _triangleCache.normals[1] = _newTriangleCache.normals[0];
                _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // check if it is facing the right way
                NormalCheck(ref _triangleCache);

                // add it
                _rightSide.AddTriangle(_triangleCache, submesh);

                // third
                _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
                _triangleCache.normals[0] = _rightTriangleCache.normals[0];
                _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _rightTriangleCache.vertices[1];
                _triangleCache.uvs[1] = _rightTriangleCache.uvs[1];
                _triangleCache.normals[1] = _rightTriangleCache.normals[1];
                _triangleCache.tangents[1] = _rightTriangleCache.tangents[1];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // check if it is facing the right way
                NormalCheck(ref _triangleCache);

                // add it
                _rightSide.AddTriangle(_triangleCache, submesh);
            }
            else
            {
                // first one on the right
                _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
                _triangleCache.normals[0] = _rightTriangleCache.normals[0];
                _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
                _triangleCache.normals[1] = _newTriangleCache.normals[0];
                _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // check if it is facing the right way
                NormalCheck(ref _triangleCache);

                // add it
                _rightSide.AddTriangle(_triangleCache, submesh);


                // other two on the left
                _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
                _triangleCache.normals[0] = _leftTriangleCache.normals[0];
                _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
                _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
                _triangleCache.normals[1] = _newTriangleCache.normals[0];
                _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // check if it is facing the right way
                NormalCheck(ref _triangleCache);

                // add it
                _leftSide.AddTriangle(_triangleCache, submesh);

                // third
                _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
                _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
                _triangleCache.normals[0] = _leftTriangleCache.normals[0];
                _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

                _triangleCache.vertices[1] = _leftTriangleCache.vertices[1];
                _triangleCache.uvs[1] = _leftTriangleCache.uvs[1];
                _triangleCache.normals[1] = _leftTriangleCache.normals[1];
                _triangleCache.tangents[1] = _leftTriangleCache.tangents[1];

                _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
                _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
                _triangleCache.normals[2] = _newTriangleCache.normals[1];
                _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

                // check if it is facing the right way
                NormalCheck(ref _triangleCache);

                // add it
                _leftSide.AddTriangle(_triangleCache, submesh);
            }

        }
        private static void NormalCheck(ref MeshInfo.Triangle triangle)
        {
            Vector3 crossProduct = Vector3.Cross(triangle.vertices[1] - triangle.vertices[0], triangle.vertices[2] - triangle.vertices[0]);
            Vector3 averageNormal = (triangle.normals[0] + triangle.normals[1] + triangle.normals[2]) / 3.0f;
            float dotProduct = Vector3.Dot(averageNormal, crossProduct);
            if (dotProduct < 0)
            {
                Vector3 temp = triangle.vertices[2];
                triangle.vertices[2] = triangle.vertices[0];
                triangle.vertices[0] = temp;

                temp = triangle.normals[2];
                triangle.normals[2] = triangle.normals[0];
                triangle.normals[0] = temp;

                Vector2 temp2 = triangle.uvs[2];
                triangle.uvs[2] = triangle.uvs[0];
                triangle.uvs[0] = temp2;

                Vector4 temp3 = triangle.tangents[2];
                triangle.tangents[2] = triangle.tangents[0];
                triangle.tangents[0] = temp3;
            }

        }
    }
}