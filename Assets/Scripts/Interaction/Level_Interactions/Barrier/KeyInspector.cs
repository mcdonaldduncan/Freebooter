using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
#if UNITY_EDITOR
[CustomEditor(typeof(Key))]
public class KeyInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Key key = (Key)target;
        Barrier platform = key.GetComponentInParent<Barrier>();

        if (GUILayout.Button("Remove Key"))
        {
            if (platform.m_RequiredKeys.Contains(key))
            {
                platform.m_RequiredKeys.Remove(key);
                DestroyImmediate(key.gameObject);

                foreach (var item in platform.m_RequiredKeys)
                {
                    item.gameObject.name = $"Key_{platform.m_RequiredKeys.IndexOf(item) + 1}";
                }
            }
        }

    }
}
#endif