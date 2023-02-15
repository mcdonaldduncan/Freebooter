using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
#if UNITY_EDITOR
[CustomEditor(typeof(MovingPlatform))]
public class PlatformInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MovingPlatform platform = (MovingPlatform)target;

        if (GUILayout.Button("Add Node"))
        {
            GameObject temp = platform.AddNode();
            Selection.activeGameObject = temp;
        }

        if (GUILayout.Button("Remove Node"))
        {
            Transform temp = platform.m_Nodes[platform.m_Nodes.Count - 1];
            platform.m_Nodes.RemoveAt(platform.m_Nodes.Count - 1);
            DestroyImmediate(temp.gameObject);
        }
    }
}
#endif