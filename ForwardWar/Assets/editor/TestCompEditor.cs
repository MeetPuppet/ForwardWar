using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestComp))]
public class TestCompEditor : Editor
{
    TestComp module = null;
    void OnEnable()
    {
        module = target as TestComp;
    }

    //public override void OnInspectorGUI()
    //{
    //    if (GUILayout.Button("TestComp"))
    //    {
    //        module.Activate();
    //
    //        EditorUtility.SetDirty(module.FAA);
    //        AssetDatabase.Refresh();
    //    }
    //
    //}
}
