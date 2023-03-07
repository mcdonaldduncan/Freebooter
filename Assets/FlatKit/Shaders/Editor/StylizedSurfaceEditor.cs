using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FlatKit.StylizedSurface;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class StylizedSurfaceEditor : BaseShaderGUI {
    private Material _target;
    private MaterialProperty[] _properties;
    private int _celShadingNumSteps = 0;
    private AnimationCurve _gradient = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    private MaterialProperty QueueOffsetProp { get; set; }

    private static readonly Dictionary<string, bool> FoldoutStates =
        new Dictionary<string, bool> { { "Rendering options", false } };

    private static readonly Color HashColor = new Color(0.85023f, 0.85034f, 0.85045f, 0.85056f);
    private static readonly int ColorPropertyName = Shader.PropertyToID("_BaseColor");

    void DrawStandard(MaterialProperty property) {
        string displayName = property.displayName;
        // Remove everything in square brackets.
        displayName = Regex.Replace(displayName, @" ?\[.*?\]", string.Empty);
        Tooltips.map.TryGetValue(displayName, out string tooltip);
        var guiContent = new GUIContent(displayName, tooltip);
        if (property.type == MaterialProperty.PropType.Texture) {
            if (!property.name.Contains("_BaseMap")) {
                EditorGUILayout.Space(15);
            }

            materialEditor.TexturePropertySingleLine(guiContent, property);
        } else {
            materialEditor.ShaderProperty(property, guiContent);
        }
    }

    MaterialProperty FindProperty(string name) {
        return FindProperty(name, _properties);
    }

    bool HasProperty(string name) {
        return _target != null && _target.HasProperty(name);
    }

#if UNITY_2021_2_OR_NEWER
    [Obsolete("MaterialChanged has been renamed ValidateMaterial", false)]
#endif
    public override void MaterialChanged(Material material) { }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        this.materialEditor = materialEditor;
        _properties = properties;
        _target = materialEditor.target as Material;
        Debug.Assert(_target != null, "_target != null");

        if (_target.shader.name.Equals("FlatKit/Stylized Surface With Outline")) {
            EditorGUILayout.HelpBox(
                "'Stylized Surface with Outline' shader has been deprecated. Please use the outline section in the 'Stylized Surface' shader.",
                MessageType.Warning);
        }

        if (_target.IsKeywordEnabled("DR_OUTLINE_ON") && _target.IsKeywordEnabled("_ALPHATEST_ON")) {
            EditorGUILayout.HelpBox(
                "The 'Outline' and 'Alpha Clip' features are usually incompatible. The outline shader pass will not be using alpha clipping.",
                MessageType.Warning);
        }

        int originalIntentLevel = EditorGUI.indentLevel;
        int foldoutRemainingItems = 0;
        bool latestFoldoutState = false;

        foreach (MaterialProperty property in properties) {
            string displayName = property.displayName;

            if (displayName.Contains("[") && !displayName.Contains("FOLDOUT")) {
                EditorGUI.indentLevel += 1;
            }

            bool skipProperty = false;
            skipProperty |= displayName.Contains("[_CELPRIMARYMODE_SINGLE]") &&
                            !_target.IsKeywordEnabled("_CELPRIMARYMODE_SINGLE");
            skipProperty |= displayName.Contains("[_CELPRIMARYMODE_STEPS]") &&
                            !_target.IsKeywordEnabled("_CELPRIMARYMODE_STEPS");
            skipProperty |= displayName.Contains("[_CELPRIMARYMODE_CURVE]") &&
                            !_target.IsKeywordEnabled("_CELPRIMARYMODE_CURVE");
            skipProperty |= displayName.Contains("[DR_CEL_EXTRA_ON]") && !property.name.Equals("_CelExtraEnabled") &&
                            !_target.IsKeywordEnabled("DR_CEL_EXTRA_ON");
            skipProperty |= displayName.Contains("[DR_SPECULAR_ON]") && !property.name.Equals("_SpecularEnabled") &&
                            !_target.IsKeywordEnabled("DR_SPECULAR_ON");
            skipProperty |= displayName.Contains("[DR_RIM_ON]") && !property.name.Equals("_RimEnabled") &&
                            !_target.IsKeywordEnabled("DR_RIM_ON");
            skipProperty |= displayName.Contains("[DR_GRADIENT_ON]") && !property.name.Equals("_GradientEnabled") &&
                            !_target.IsKeywordEnabled("DR_GRADIENT_ON");
            skipProperty |= displayName.Contains("[_UNITYSHADOWMODE_MULTIPLY]") &&
                            !_target.IsKeywordEnabled("_UNITYSHADOWMODE_MULTIPLY");
            skipProperty |= displayName.Contains("[_UNITYSHADOWMODE_COLOR]") &&
                            !_target.IsKeywordEnabled("_UNITYSHADOWMODE_COLOR");
            skipProperty |= displayName.Contains("[DR_ENABLE_LIGHTMAP_DIR]") &&
                            !_target.IsKeywordEnabled("DR_ENABLE_LIGHTMAP_DIR");
            skipProperty |= displayName.Contains("[DR_OUTLINE_ON]") &&
                            !_target.IsKeywordEnabled("DR_OUTLINE_ON");
            skipProperty |= displayName.Contains("[_EMISSION]") &&
                            !_target.IsKeywordEnabled("_EMISSION");

            if (_target.IsKeywordEnabled("DR_ENABLE_LIGHTMAP_DIR") &&
                displayName.Contains("Override light direction")) {
                var dirPitch = _target.GetFloat("_LightmapDirectionPitch");
                var dirYaw = _target.GetFloat("_LightmapDirectionYaw");

                var dirPitchRad = dirPitch * Mathf.Deg2Rad;
                var dirYawRad = dirYaw * Mathf.Deg2Rad;

                var direction = new Vector4(Mathf.Sin(dirPitchRad) * Mathf.Sin(dirYawRad), Mathf.Cos(dirPitchRad),
                    Mathf.Sin(dirPitchRad) * Mathf.Cos(dirYawRad), 0.0f);
                _target.SetVector("_LightmapDirection", direction);
            }

            if (displayName.Contains("FOLDOUT")) {
                string foldoutName = displayName.Split('(', ')')[1];
                string foldoutItemCount = displayName.Split('{', '}')[1];
                foldoutRemainingItems = Convert.ToInt32(foldoutItemCount);
                if (!FoldoutStates.ContainsKey(property.name)) {
                    FoldoutStates.Add(property.name, false);
                }

                EditorGUILayout.Space();
                FoldoutStates[property.name] =
                    EditorGUILayout.Foldout(FoldoutStates[property.name], foldoutName);
                latestFoldoutState = FoldoutStates[property.name];
            }

            if (foldoutRemainingItems > 0) {
                skipProperty = skipProperty || !latestFoldoutState;
                EditorGUI.indentLevel += 1;
                --foldoutRemainingItems;
            }

            if (_target.IsKeywordEnabled("_CELPRIMARYMODE_STEPS") && displayName.Contains("[LAST_PROP_STEPS]")) {
                EditorGUILayout.HelpBox(
                    "This mode creates a step texture that control the light/shadow transition. To use:\n" +
                    "1. Set the number of steps (e.g. 3 means three steps between lit and shaded regions), \n" +
                    "2. Save the steps as a texture - 'Save Ramp Texture' button",
                    MessageType.Info);
                int currentNumSteps = _target.GetInt("_CelNumSteps");
                if (currentNumSteps != _celShadingNumSteps) {
                    if (GUILayout.Button("Save Ramp Texture")) {
                        _celShadingNumSteps = currentNumSteps;
                        PromptTextureSave(materialEditor, GenerateStepTexture, "_CelStepTexture", FilterMode.Point);
                    }
                }
            }

            if (_target.IsKeywordEnabled("_CELPRIMARYMODE_CURVE") && displayName.Contains("[LAST_PROP_CURVE]")) {
                EditorGUILayout.HelpBox(
                    "This mode uses arbitrary curves to control the light/shadow transition. How to use:\n" +
                    "1. Set shading curve (generally from 0.0 to 1.0)\n" +
                    "2. [Optional] Save the curve preset\n" +
                    "3. Save the curve as a texture.",
                    MessageType.Info);
                _gradient = EditorGUILayout.CurveField("Shading curve", _gradient);

                if (GUILayout.Button("Save Ramp Texture")) {
                    PromptTextureSave(materialEditor, GenerateCurveTexture, "_CelCurveTexture",
                        FilterMode.Trilinear);
                }
            }

            if (!skipProperty &&
                property.type == MaterialProperty.PropType.Color &&
                property.colorValue == HashColor) {
                property.colorValue = _target.GetColor(ColorPropertyName);
            }

            if (!skipProperty && property.name.Contains("_EmissionMap")) {
                EditorGUILayout.Space(10);
                bool emission = materialEditor.EmissionEnabledProperty();
                EditorGUILayout.Space(-15);
                EditorGUI.indentLevel += 1;
                if (emission) {
                    _target.EnableKeyword("_EMISSION");
                } else {
                    _target.DisableKeyword("_EMISSION");
                }
            }

            if (!skipProperty && property.name.Contains("_EmissionColor")) {
                EditorGUI.indentLevel += 1;
            }

            bool hideInInspector = (property.flags & MaterialProperty.PropFlags.HideInInspector) != 0;
            if (!hideInInspector && !skipProperty) {
                DrawStandard(property);
            }

            if (!skipProperty && property.name.Contains("_EmissionColor")) {
                EditorGUILayout.Space(15);
                EditorGUI.indentLevel -= 1;
                DrawTileOffset(materialEditor, FindProperty("_BaseMap"));
            }

            EditorGUI.indentLevel = originalIntentLevel;
        }

        EditorGUILayout.Space();
        FoldoutStates["Rendering options"] =
            EditorGUILayout.Foldout(FoldoutStates["Rendering options"], "Rendering options");
        if (FoldoutStates["Rendering options"]) {
            EditorGUI.indentLevel += 1;

            HandleUrpSettings(_target, materialEditor);

            QueueOffsetProp = FindProperty("_QueueOffset", _properties, false);
            DrawQueueOffsetField();

            materialEditor.EnableInstancingField();
        }

        if (_target.IsKeywordEnabled("_UNITYSHADOWMODE_NONE")) {
            _target.EnableKeyword("_RECEIVE_SHADOWS_OFF");
        } else {
            _target.DisableKeyword("_RECEIVE_SHADOWS_OFF");
        }

        // Toggle the outline pass. Disabling by name `Outline` doesn't work.
        _target.SetShaderPassEnabled("SRPDEFAULTUNLIT", _target.IsKeywordEnabled("DR_OUTLINE_ON"));

        /*
        if (HasProperty("_MainTex")) {
            TransferToBaseMap();
        }
        */
    }

    // Adapted from BaseShaderGUI.cs.
    private void HandleUrpSettings(Material material, MaterialEditor materialEditor) {
        bool alphaClip = false;
        if (material.HasProperty("_AlphaClip")) {
            alphaClip = material.GetFloat("_AlphaClip") >= 0.5;
        }

        if (alphaClip) {
            material.EnableKeyword("_ALPHATEST_ON");
        } else {
            material.DisableKeyword("_ALPHATEST_ON");
        }

        if (HasProperty("_Surface")) {
            EditorGUI.BeginChangeCheck();
            var surfaceProp = FindProperty("_Surface");
            EditorGUI.showMixedValue = surfaceProp.hasMixedValue;
            var surfaceType = (SurfaceType)surfaceProp.floatValue;
            EditorGUILayout.Separator();
            surfaceType = (SurfaceType)EditorGUILayout.EnumPopup("Surface Type", surfaceType);
            if (EditorGUI.EndChangeCheck()) {
                materialEditor.RegisterPropertyChangeUndo("Surface Type");
                surfaceProp.floatValue = (float)surfaceType;
            }

            if (surfaceType == SurfaceType.Opaque) {
                if (alphaClip) {
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                } else {
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    material.SetOverrideTag("RenderType", "Opaque");
                }

                material.renderQueue +=
                    material.HasProperty("_QueueOffset") ? (int)material.GetFloat("_QueueOffset") : 0;
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.SetShaderPassEnabled("ShadowCaster", true);
            } else // Transparent
            {
                BlendMode blendMode = (BlendMode)material.GetFloat("_Blend");

                // Specific Transparent Mode Settings
                switch (blendMode) {
                    case BlendMode.Alpha:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    case BlendMode.Premultiply:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    case BlendMode.Additive:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    case BlendMode.Multiply:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.EnableKeyword("_ALPHAMODULATE_ON");
                        break;
                }

                // General Transparent Material Settings
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_ZWrite", 0);
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                material.renderQueue +=
                    material.HasProperty("_QueueOffset") ? (int)material.GetFloat("_QueueOffset") : 0;
                material.SetShaderPassEnabled("ShadowCaster", false);
            }

            // DR: draw popup.
            if (surfaceType == SurfaceType.Transparent && HasProperty("_Blend")) {
                EditorGUI.BeginChangeCheck();
                var blendModeProp = FindProperty("_Blend");
                EditorGUI.showMixedValue = blendModeProp.hasMixedValue;
                var blendMode = (BlendMode)blendModeProp.floatValue;
                blendMode = (BlendMode)EditorGUILayout.EnumPopup("Blend Mode", blendMode);
                if (EditorGUI.EndChangeCheck()) {
                    materialEditor.RegisterPropertyChangeUndo("Blend Mode");
                    blendModeProp.floatValue = (float)blendMode;
                }
            }
        }

        // DR: draw popup.
        if (HasProperty("_Cull")) {
            EditorGUILayout.Separator();
            EditorGUI.BeginChangeCheck();
            var cullingProp = FindProperty("_Cull");
            EditorGUI.showMixedValue = cullingProp.hasMixedValue;
            var culling = (RenderFace)cullingProp.floatValue;
            culling = (RenderFace)EditorGUILayout.EnumPopup("Render Faces", culling);
            if (EditorGUI.EndChangeCheck()) {
                materialEditor.RegisterPropertyChangeUndo("Render Faces");
                cullingProp.floatValue = (float)culling;
                material.doubleSidedGI = (RenderFace)cullingProp.floatValue != RenderFace.Front;
            }
        }

        if (HasProperty("_AlphaClip")) {
            EditorGUILayout.Separator();
            EditorGUI.BeginChangeCheck();
            var alphaClipProp = FindProperty("_AlphaClip");
            EditorGUI.showMixedValue = alphaClipProp.hasMixedValue;
            var alphaClipEnabled = EditorGUILayout.Toggle("Alpha Clipping", alphaClipProp.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                alphaClipProp.floatValue = alphaClipEnabled ? 1 : 0;
            EditorGUI.showMixedValue = false;

            if (alphaClipProp.floatValue == 1 && HasProperty("_Cutoff")) {
                var alphaCutoffProp = FindProperty("_Cutoff");
                materialEditor.ShaderProperty(alphaCutoffProp, "Threshold", 1);
            }
        }
    }

    private void PromptTextureSave(MaterialEditor materialEditor, Func<Texture2D> generate, string propertyName,
        FilterMode filterMode) {
        var rampTexture = generate();
        var pngNameNoExtension = string.Format("{0}{1}-ramp", materialEditor.target.name, propertyName);
        var fullPath =
            EditorUtility.SaveFilePanel("Save Ramp Texture", "Assets", pngNameNoExtension, "png");
        if (fullPath.Length > 0) {
            SaveTextureAsPng(rampTexture, fullPath, filterMode);
            var loadedTexture = LoadTexture(fullPath);
            if (loadedTexture != null) {
                _target.SetTexture(propertyName, loadedTexture);
            } else {
                Debug.LogWarning("Could not save the texture. Make sure the destination is in the Assets folder.");
            }
        }
    }

    private Texture2D GenerateStepTexture() {
        int numSteps = _celShadingNumSteps;
        var t2d = new Texture2D(numSteps + 1, /*height=*/1, TextureFormat.R8, /*mipChain=*/false) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int i = 0; i < numSteps + 1; i++) {
            var color = Color.white * i / numSteps;
            t2d.SetPixel(i, 0, color);
        }

        t2d.Apply();
        return t2d;
    }

    private Texture2D GenerateCurveTexture() {
        const int width = 256;
        const int height = 1;
        var lut = new Texture2D(width, height, TextureFormat.R8, /*mipChain=*/false) {
            alphaIsTransparency = false,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Trilinear
        };

        for (float x = 0; x < width; x++) {
            float value = _gradient.Evaluate(x / width);
            for (float y = 0; y < height; y++) {
                var color = Color.white * value;
                lut.SetPixel(Mathf.CeilToInt(x), Mathf.CeilToInt(y), color);
            }
        }

        return lut;
    }

    private void SaveTextureAsPng(Texture2D texture, string fullPath, FilterMode filterMode) {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);
        AssetDatabase.Refresh();
        Debug.Log(string.Format("Texture saved as: {0}", fullPath));

        string pathRelativeToAssets = ConvertFullPathToAssetPath(fullPath);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(pathRelativeToAssets);
        if (importer != null) {
            importer.filterMode = filterMode;
            importer.textureType = TextureImporterType.SingleChannel;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            var textureSettings = new TextureImporterPlatformSettings {
                format = TextureImporterFormat.R8
            };
            importer.SetPlatformTextureSettings(textureSettings);
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        //22b5f7ed-989d-49d1-90d9-c62d76c3081a

        Debug.Assert(importer,
            string.Format("[FlatKit] Could not change import settings of {0} [{1}]",
                fullPath, pathRelativeToAssets));
    }

    private static Texture2D LoadTexture(string fullPath) {
        string pathRelativeToAssets = ConvertFullPathToAssetPath(fullPath);
        if (pathRelativeToAssets.Length == 0) {
            return null;
        }

        var loadedTexture = AssetDatabase.LoadAssetAtPath(pathRelativeToAssets, typeof(Texture2D)) as Texture2D;
        if (loadedTexture == null) {
            Debug.LogError(string.Format("[FlatKit] Could not load texture from {0} [{1}].", fullPath,
                pathRelativeToAssets));
            return null;
        }

        loadedTexture.filterMode = FilterMode.Point;
        loadedTexture.wrapMode = TextureWrapMode.Clamp;

        return loadedTexture;
    }

    private static string ConvertFullPathToAssetPath(string fullPath) {
        int count = (Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar).Length;
        return fullPath.Remove(0, count);
    }

#if !UNITY_2020_3_OR_NEWER
    private new void DrawQueueOffsetField() {
        GUIContent queueSlider = new GUIContent("     Priority",
            "Determines the chronological rendering order for a Material. High values are rendered first.");
        const int queueOffsetRange = 50;
        MaterialProperty queueOffsetProp = FindProperty("_QueueOffset", _properties, false);
        if (queueOffsetProp == null) return;
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = queueOffsetProp.hasMixedValue;
        var queue = EditorGUILayout.IntSlider(queueSlider, (int) queueOffsetProp.floatValue, -queueOffsetRange,
            queueOffsetRange);
        if (EditorGUI.EndChangeCheck())
            queueOffsetProp.floatValue = queue;
        EditorGUI.showMixedValue = false;

        _target.renderQueue = (int)RenderQueue.Transparent + queue;
    }
#endif

    private void TransferToBaseMap() {
        var baseMapProp = FindProperty("_MainTex");
        var baseColorProp = FindProperty("_Color");
        _target.SetTexture("_BaseMap", baseMapProp.textureValue);
        var baseMapTiling = baseMapProp.textureScaleAndOffset;
        _target.SetTextureScale("_BaseMap", new Vector2(baseMapTiling.x, baseMapTiling.y));
        _target.SetTextureOffset("_BaseMap", new Vector2(baseMapTiling.z, baseMapTiling.w));
        _target.SetColor("_BaseColor", baseColorProp.colorValue);
    }

    private void TransferToMainTex() {
        var baseMapProp = FindProperty("_BaseMap");
        var baseColorProp = FindProperty("_BaseColor");
        _target.SetTexture("_MainTex", baseMapProp.textureValue);
        var baseMapTiling = baseMapProp.textureScaleAndOffset;
        _target.SetTextureScale("_MainTex", new Vector2(baseMapTiling.x, baseMapTiling.y));
        _target.SetTextureOffset("_MainTex", new Vector2(baseMapTiling.z, baseMapTiling.w));
        _target.SetColor("_Color", baseColorProp.colorValue);
    }
}