using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
#if UNITY_EDITOR
[CustomEditor(typeof(BarrierNode))]
public class BNodeInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BarrierNode node = (BarrierNode)target;

        if (node == null) return;

        BarrierSegment segment = node.transform.parent.GetComponentInChildren<BarrierSegment>();

        if (segment == null) return;

        if (GUILayout.Button("Remove Node"))
        {
            if (segment.m_Nodes.Contains(node.transform))
            {
                segment.m_Nodes.Remove(node.transform);
                DestroyImmediate(node.gameObject);

                foreach (var item in segment.m_Nodes)
                {
                    item.gameObject.name = $"Node_{segment.m_Nodes.IndexOf(item) + 1}";
                }
            }
        }
    }
}
#endif