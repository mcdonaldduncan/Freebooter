using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Barrier))]
public class BarrierInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Barrier barrier = (Barrier)target;

        if (GUILayout.Button("Add Key"))
        {
            GameObject temp = barrier.AddKey();
            Selection.activeGameObject = temp;
        }

        if (GUILayout.Button("Remove Key"))
        {
            Key temp = barrier.m_RequiredKeys[barrier.m_RequiredKeys.Count - 1];
            barrier.m_RequiredKeys.RemoveAt(barrier.m_RequiredKeys.Count - 1);
            DestroyImmediate(temp.gameObject);
        }


        if (GUILayout.Button("Add Segment"))
        {
            GameObject temp = barrier.AddSegment();
            Selection.activeGameObject = temp;
        }
    }
}
