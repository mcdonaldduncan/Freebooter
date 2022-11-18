using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

#if UNITY_EDITOR

[CustomEditor(typeof(BarrierSegment))]
public class SegmentInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BarrierSegment segment = (BarrierSegment)target;

        if (GUILayout.Button("Add Node"))
        {
            GameObject temp = segment.AddNode();
            Selection.activeGameObject = temp;
        }

        if (GUILayout.Button("Remove Node"))
        {
            Transform temp = segment.m_Nodes[segment.m_Nodes.Count - 1];
            segment.m_Nodes.RemoveAt(segment.m_Nodes.Count - 1);
            DestroyImmediate(temp.gameObject);
        }

    }
}
#endif