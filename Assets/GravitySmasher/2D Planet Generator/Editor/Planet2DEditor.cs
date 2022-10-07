using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

[CustomEditor(typeof(Planet2D))]

public class Planet2DEditor : Editor
{
    Planet2D main;
    Editor planetEditor;

    void OnEnable()
    {
        main = (Planet2D)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DisplaySettingsEditor(main.planetSettings, ref main.planetSettingsFoldout, ref planetEditor);

        if (GUILayout.Button("Create Planet"))
        {
            main.CreatePlanet2D();
        }
        main.filePath = EditorGUILayout.TextField("Relative Path from Asset + FileName", main.filePath);
        if (GUILayout.Button("Export As PNG"))
        {
            main.ExportAsPNG();
        }
    }

    void DisplaySettingsEditor(Object settings, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();
                }
            }
        }
    }
}
