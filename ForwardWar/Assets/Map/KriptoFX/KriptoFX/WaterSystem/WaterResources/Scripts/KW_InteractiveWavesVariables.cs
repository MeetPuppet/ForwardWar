using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KW_InteractiveWavesVariables
{
    static List<KW_InteractWithWater> interactScripts = new List<KW_InteractWithWater>(50);
    static KW_InteractWithWater[] nearScripts = new KW_InteractWithWater[50];

    public static void AddInteractScript(KW_InteractWithWater script)
    {
        if(!interactScripts.Contains(script)) interactScripts.Add(script);
    }

    public static void RemoveInteractScript(KW_InteractWithWater script)
    {
        if (interactScripts.Contains(script)) interactScripts.Remove(script);
    }

    public static KW_InteractWithWater[] GetInteractScriptsInArea(Vector3 areaPos, int areaSize, out int endIdx)
    {
        if (interactScripts.Count > nearScripts.Length) Array.Resize(ref nearScripts, (int)(interactScripts.Count * 1.5f));

        var lastIdx = 0;
        foreach (var script in interactScripts)
        {
            var dist = Vector3.Distance(script.CachedTransform.position, areaPos);
            if (dist < areaSize)
            {
                nearScripts[lastIdx] = script;
                lastIdx++;
            }
        }

        endIdx = lastIdx;
        return nearScripts;
    }
}
