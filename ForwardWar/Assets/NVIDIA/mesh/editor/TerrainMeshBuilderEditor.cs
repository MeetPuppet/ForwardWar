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

        EditorGUILayout.LabelField("Terrain Size");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Width X Height");
        module.Width = EditorGUILayout.IntField(module.Width);
        module.Height = EditorGUILayout.IntField(module.Height);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Iterator Control");
        GUILayout.BeginVertical("Box");
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Iter X Begin/End - default: 0/n");
            module.startX = EditorGUILayout.IntField(module.startX);
            module.endX = EditorGUILayout.IntField(module.endX);
            if (module.startX >= module.endX)
                module.endX = module.startX + 1;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Iter Y Begin/End - default: 0/n");
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

        if (GUILayout.Button("generate"))
            module.generateMesh();
    }
}