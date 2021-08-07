using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class KW_MeshGenerator : MonoBehaviour
{
    static List<int> triangles = new List<int>();
    static List<Vector3> vertices = new List<Vector3>();
    private static bool IsOutFarDistance;
    private static bool IsOurBoxFarDistance;
    private static float FarDistance;

    public static Mesh GeneratePlane(float startSizeMeters, int quadsPerStartSize, float maxSizeMeters)
    {
        IsOutFarDistance = false;
        IsOurBoxFarDistance = false;
        FarDistance = maxSizeMeters;

        vertices.Clear();
        triangles.Clear();

        var offset = CreateStartChunk(startSizeMeters, quadsPerStartSize);

        var newSize = quadsPerStartSize / 2 + 4;
        var count = (int)((quadsPerStartSize / 4f));
        var lastCount = newSize - 2;
        do
        {
            var currentScale = count;
            offset += CreateChunk(lastCount + 2, (startSizeMeters * 0.5f + offset), currentScale, out lastCount);

        } while (offset * 0.5f < maxSizeMeters);

        var mesh = new Mesh();
        //mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        return mesh;
    }

    private static float CreateStartChunk(float startSizeMeters, int quadsPerStartSize)
    {
        var halfSize = quadsPerStartSize / 2;
        float quadLength = startSizeMeters / quadsPerStartSize;
        for (int i = 0; i < halfSize; i++)
        {
            AddRing(quadsPerStartSize - i * 2, (startSizeMeters * 0.5f - quadLength * i),  false);
        }
        var offset = quadLength * 2;
        AddRing(halfSize + 2, (startSizeMeters * 0.5f + offset), true);
        return offset;
    }

    private static float CreateChunk(int size, float startScale, int count, out int lastCount)
    {
        float scaleOffset = 0;
        for (int i = 0; i < count; i++)
        {
            if (i < count - 1)
            {
                var newSize = size + 2*i;
                scaleOffset += 1f/(size - 2)*startScale*2;
                AddRing(newSize, startScale + scaleOffset);
            }
            else
            {
                int newSize = (size + 2 * i) / 2 + 1;
                scaleOffset += 1f / (size - 2) * startScale * 4;
                AddRing(newSize, (startScale + scaleOffset),  true);
            }
        }
        lastCount = (size + 2 * (count - 1)) / 2 + 1;
        return scaleOffset;
    }



    private static void AddRing(int size, float scale,  bool isTripple = false)
    {
        if(IsOutFarDistance) return;

        if (IsOutFarDistance == false && scale * 0.5f > FarDistance)
        {
            IsOutFarDistance = true;
        }

        if (IsOurBoxFarDistance == false && scale * 0.5f > FarDistance * 0.5f)
        {
            IsOurBoxFarDistance = true;
            AddBoxUnderwater(size, scale);
        }


        int x, y = 0;
        for (x = 0; x < size; x++)
            CreateQuad( size, scale, x, y, Side.Down, isTripple);

        x = size - 1;
        for (y = 1; y < size; y++)
            CreateQuad( size, scale, x, y, Side.Right, isTripple);

        y = size - 1;
        for (x = size - 2; x >= 0; x--)
            CreateQuad( size, scale, x, y, Side.Up, isTripple);

        x = 0;
        for (y = size - 2; y > 0; y--)
            CreateQuad(size, scale, x, y, Side.Left, isTripple);
    }

    static void AddBoxUnderwater(int size, float scale)
    {
        int x, y = 0;
        for (x = 0; x < size; x++)
            CreateQuadVertical(size, scale, x, y, Side.Down);

        x = size - 1;
        for (y = 0; y < size; y++)
            CreateQuadVertical(size, scale, x, y, Side.Right);

        y = size - 1;
        for (x = size - 1; x >= 0; x--)
            CreateQuadVertical(size, scale, x, y, Side.Up);

        x = 0;
        for (y = size - 1; y >= 0; y--)
            CreateQuadVertical(size, scale, x, y, Side.Left);

        x = 0;
        y = 0;
        for (x = 0; x < size; x++)
            CreateQuadBootom(size, scale, x, y, Side.Down);

        x = size - 1;
        for (y = 0; y < size; y++)
            CreateQuadBootom(size, scale, x, y, Side.Right);

        y = size - 1;
        for (x = size - 1; x >= 0; x--)
            CreateQuadBootom(size, scale, x, y, Side.Up);

        x = 0;
        for (y = size - 1; y >= 0; y--)
            CreateQuadBootom(size, scale, x, y, Side.Left);
    }

    static void CreateQuad(int size, float scale, int x, int y, Side side, bool isTripple)
    {
        float offset = (1f / size) * scale;
        var position = new Vector3(x / (float)size - 0.5f, 0, y / (float)size - 0.5f) * scale;

        var leftBottomIndex = GetVertexPositionIndex(vertices, position);
        var rightBottomIndex = GetVertexPositionIndex(vertices, position + new Vector3(offset, 0, 0));
        var rightUpIndex = GetVertexPositionIndex(vertices, position + new Vector3(0, 0, offset));
        var leftUpIndex = GetVertexPositionIndex(vertices, position + new Vector3(offset, 0, offset));
        if (isTripple)
        {
            if (Mathf.Abs(x - y) == size - 1 || Mathf.Abs(x - y) == 0) side = Side.Fringe;
            AddTripplePoint(side, leftBottomIndex, rightBottomIndex, rightUpIndex,
                leftUpIndex, position, offset);
        }
        else
        {
            AddQuadIndexes(leftBottomIndex, rightBottomIndex, rightUpIndex, leftUpIndex);
        }
    }

    static void CreateQuadVertical(int size, float scale, int x, int y, Side side)
    {
        float offset   = (1f / size)                                                    * scale;
        var   position = new Vector3(x / (float)size - 0.5f, 0, y / (float)size - 0.5f) * scale;

        var leftBottom_Height = Vector3.zero;
        var rightBottom_Height = new Vector3(offset, 0, 0);
        var rightUp_Height = new Vector3(0, 0, offset);
        var leftUp_Height = new Vector3(offset, 0, offset);
        switch (side)
        {
            case Side.Down:
                rightUp_Height = new Vector3(0,      -FarDistance, 0);
                leftUp_Height  = new Vector3(offset, -FarDistance, 0);
                break;
            case Side.Right:
                leftBottom_Height = new Vector3(offset, -FarDistance, 0);
                rightUp_Height = new Vector3(offset, -FarDistance, offset);
                break;
            case Side.Up:
                leftBottom_Height  = new Vector3(0,      -FarDistance, offset);
                rightBottom_Height = new Vector3(offset, -FarDistance, offset);
                break;
            case Side.Left:
                rightBottom_Height = new Vector3(0, -FarDistance, 0);
                leftUp_Height      = new Vector3(0, -FarDistance, offset);
                break;
        }

        var leftBottomIndex  = GetVertexPositionIndex(vertices, position + leftBottom_Height);
        var rightBottomIndex = GetVertexPositionIndex(vertices, position + rightBottom_Height);
        var rightUpIndex     = GetVertexPositionIndex(vertices, position + rightUp_Height);
        var leftUpIndex      = GetVertexPositionIndex(vertices, position + leftUp_Height);

        AddQuadIndexes(rightBottomIndex, leftBottomIndex, leftUpIndex, rightUpIndex);
    }

    static void CreateQuadBootom(int size, float scale, int x, int y, Side side)
    {
        float offset   = (1f / size)                                                    * scale;
        var   position = new Vector3(x / (float)size - 0.5f, 0, y / (float)size - 0.5f) * scale;

        var leftBottom_Height  = position + new Vector3(0, -FarDistance * 0.5f, 0);
        var rightBottom_Height = position + new Vector3(offset, -FarDistance * 0.5f, 0);
        var rightUp_Height     = position + new Vector3(0, -FarDistance * 0.5f, offset);
        var leftUp_Height      = position + new Vector3(offset, -FarDistance * 0.5f, offset);
        if (side == Side.Down)
        {
            rightUp_Height = new Vector3(position.x,          -FarDistance * 0.5f, -position.z);
            leftUp_Height  = new Vector3(position.x + offset, -FarDistance * 0.5f, -position.z);
        }

        var leftBottomIndex  = GetVertexPositionIndex(vertices, leftBottom_Height);
        var rightBottomIndex = GetVertexPositionIndex(vertices, rightBottom_Height);
        var rightUpIndex     = GetVertexPositionIndex(vertices, rightUp_Height);
        var leftUpIndex      = GetVertexPositionIndex(vertices, leftUp_Height);

        AddQuadIndexes(rightBottomIndex, leftBottomIndex, leftUpIndex, rightUpIndex);
    }

    static void AddTripplePoint(Side side, int leftBottomIndex, int rightBottomIndex, int rightUpIndex, int leftUpIndex, Vector3 position, float offset)
    {
        int middleIndex;
        if (side == Side.Fringe)
        {
            AddQuadIndexes(leftBottomIndex, rightBottomIndex, rightUpIndex, leftUpIndex);
            return;
        }

        if (side == Side.Down)
        {
            middleIndex = GetVertexPositionIndex(vertices, position + new Vector3(offset/2f, 0, offset));
            AddTripleIndexesDown(leftBottomIndex, rightBottomIndex, rightUpIndex, leftUpIndex, middleIndex);
        }
        if (side == Side.Right)
        {
            middleIndex = GetVertexPositionIndex(vertices, position + new Vector3(0, 0, offset/2));
            AddTripleIndexesRight(leftBottomIndex, rightBottomIndex, rightUpIndex, leftUpIndex, middleIndex);
        }
        if (side == Side.Up)
        {
            middleIndex = GetVertexPositionIndex(vertices, position + new Vector3(offset/2, 0, 0));
            AddTripleIndexesUp(leftBottomIndex, rightBottomIndex, rightUpIndex, leftUpIndex, middleIndex);
        }
        if (side == Side.Left)
        {
            middleIndex = GetVertexPositionIndex(vertices, position + new Vector3(offset, 0, offset/2));
            AddTripleIndexesLeft(leftBottomIndex, rightBottomIndex, rightUpIndex, leftUpIndex, middleIndex);
        }

    }


    static void AddQuadIndexes(int index1, int index2, int index3, int index4)
    {
        triangles.Add(index1); triangles.Add(index3); triangles.Add(index2);
        triangles.Add(index2); triangles.Add(index3); triangles.Add(index4);
    }



#region TripleIndexes

static void AddTripleIndexesDown(int index1, int index2, int index3, int index4, int index5)
    {
        triangles.Add(index3); triangles.Add(index5); triangles.Add(index1);
        triangles.Add(index1); triangles.Add(index5); triangles.Add(index2);
        triangles.Add(index5); triangles.Add(index4); triangles.Add(index2);
    }

static void AddTripleIndexesRight(int index1, int index2, int index3, int index4, int index5)
    {
        triangles.Add(index3); triangles.Add(index4); triangles.Add(index5);
        triangles.Add(index1); triangles.Add(index5); triangles.Add(index2);
        triangles.Add(index5); triangles.Add(index4); triangles.Add(index2);
    }

static void AddTripleIndexesUp(int index1, int index2, int index3, int index4, int index5)
    {
        triangles.Add(index3); triangles.Add(index4); triangles.Add(index5);
        triangles.Add(index3); triangles.Add(index5); triangles.Add(index1);
        triangles.Add(index5); triangles.Add(index4); triangles.Add(index2);
    }

    static void AddTripleIndexesLeft(int index1, int index2, int index3, int index4, int index5)
    {
        triangles.Add(index3); triangles.Add(index4); triangles.Add(index5);
        triangles.Add(index1); triangles.Add(index5); triangles.Add(index2);
        triangles.Add(index3); triangles.Add(index5); triangles.Add(index1);
    }
#endregion

    enum Side
    {
        Down,
        Right,
        Up,
        Left,
        Fringe
    }


    static int GetVertexPositionIndex(List<Vector3> vertices,  Vector3 position)
    {
        vertices.Add(position);
        return vertices.Count - 1;
    }

}
