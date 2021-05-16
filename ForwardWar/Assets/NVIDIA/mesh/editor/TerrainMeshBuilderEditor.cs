using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainMeshBuilder))]
public class TerrainMeshBuilderEditor : Editor
{
    TerrainMeshBuilder module = null;
    void OnEnable()
    {
        module = target as TerrainMeshBuilder;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Advance Mode"))
            module.AdvanceMode = !module.AdvanceMode;

        if (module.AdvanceMode)
        {
            EditorGUILayout.LabelField("Terrain Size");
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField("Width X Height (255, 255)");
            module.Width = EditorGUILayout.IntField(module.Width);
            module.Height = EditorGUILayout.IntField(module.Height);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Mesh Size");
            GUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Mag (3)");
                module.Mag = EditorGUILayout.IntField(module.Mag);
                EditorGUILayout.LabelField("Scale");
                module.Scale = EditorGUILayout.IntField(module.Scale);
            }
            GUILayout.EndVertical();

            EditorGUILayout.LabelField("Iterator Control");
            GUILayout.BeginVertical("Box");
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Iter X Begin/End - (0, 171)");
                module.startX = EditorGUILayout.IntField(module.startX);
                module.endX = EditorGUILayout.IntField(module.endX);
                if (module.startX >= module.endX)
                    module.endX = module.startX + 1;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Iter Y Begin/End - (84, 255)");
                module.startY = EditorGUILayout.IntField(module.startY);
                module.endY = EditorGUILayout.IntField(module.endY);
                if (module.startY >= module.endY)
                    module.endY = module.startY + 1;
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Generate Block(x/y)");
            module.WidthBlock = EditorGUILayout.Toggle(module.WidthBlock);
            module.HeightBlock = EditorGUILayout.Toggle(module.HeightBlock);
            EditorGUILayout.EndHorizontal();

        }
        else
        {
            EditorGUILayout.LabelField("Mesh Size");
            GUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Scale");
                module.Scale = EditorGUILayout.IntField(module.Scale);
            }
            GUILayout.EndVertical();
        }

        EditorGUILayout.LabelField("Generator");
        EditorGUILayout.BeginVertical("Box");
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Vertices"))
                    module.generateVertices();
                if (GUILayout.Button("Triangles"))
                    module.generateTriangles();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("GenerateAll"))
                module.generateMesh();

            //if (GUILayout.Button("DivideGenerate"))
            //    module.DivideMesh();

        }
        EditorGUILayout.EndVertical();

    }
}