using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(BarrelGroupBehavior))]
public class BarrelGroupInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BarrelGroupBehavior barrelGroup = (BarrelGroupBehavior)target;

        if (GUILayout.Button("Add Barrel"))
        {
            GameObject tempBarrel = barrelGroup.AddBarrel();
            Selection.activeGameObject = tempBarrel;
        }
    }
}
#endif