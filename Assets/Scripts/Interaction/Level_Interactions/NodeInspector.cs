using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlatformNode))]
public class NodeInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlatformNode node = (PlatformNode)target;
        MovingPlatform platform = node.GetComponentInParent<MovingPlatform>();

        if (GUILayout.Button("Remove Node"))
        {
            if (platform.m_Nodes.Contains(node.transform))
            {
                platform.m_Nodes.Remove(node.transform);
                DestroyImmediate(node.gameObject);

                foreach (var item in platform.m_Nodes)
                {
                    item.gameObject.name = $"Node_{platform.m_Nodes.IndexOf(item) + 1}";
                }
            }
        }

    }

    //public void OnDestroy()
    //{
    //    if (Application.isEditor)
    //    {
    //        //if (((PlatformNode)target) == null)
    //    }
    //}
}
