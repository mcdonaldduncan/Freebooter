using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(PlatformBase))]
public class BaseInspector : Editor
{
    public SerializedProperty
        maxSpeed_Prop,
        maxForce_Prop,
        Type_Prop,
        lerpScale_Prop,
        damping_Prop,
        curve_Prop;
        
    

    private void OnEnable()
    {
        maxForce_Prop = serializedObject.FindProperty("m_MaxForce");
        maxSpeed_Prop = serializedObject.FindProperty("m_MaxSpeed");
        Type_Prop = serializedObject.FindProperty("m_TranslationType");
        curve_Prop = serializedObject.FindProperty("m_AnimationCurve");
        lerpScale_Prop = serializedObject.FindProperty("m_LerpScale");
        damping_Prop = serializedObject.FindProperty("m_Damping");
    }

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        serializedObject.Update();

        PlatformBase _base = (PlatformBase)target;
        TranslationType type = _base.m_TranslationType;
        AnimationCurve curve = _base.m_AnimationCurve;

        EditorGUILayout.PropertyField(Type_Prop);

        switch (type)
        {
            case TranslationType.LINEAR:
                break;
            case TranslationType.CURVE:
                EditorGUILayout.CurveField("Animation Curve", curve);
                EditorGUILayout.PropertyField(lerpScale_Prop);
                break;
            case TranslationType.STEERING:
                EditorGUILayout.LabelField("Caution, steering can be unpredictable");
                EditorGUILayout.LabelField("Node delay will not be applied for steering");
                EditorGUILayout.PropertyField(maxSpeed_Prop);
                EditorGUILayout.PropertyField(maxForce_Prop);
                break;
            case TranslationType.DAMP:
                EditorGUILayout.PropertyField(damping_Prop);
                break;
            default:
                break;
        }
        _base.m_AnimationCurve = curve;
        serializedObject.ApplyModifiedProperties();
    }
}
#endif