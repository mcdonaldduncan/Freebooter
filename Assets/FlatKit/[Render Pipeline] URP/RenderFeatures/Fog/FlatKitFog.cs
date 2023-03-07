using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// TODO: Remove for URP 13.
// https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@13.1/manual/upgrade-guide-2022-1.html
#pragma warning disable CS0618

namespace FlatKit {
public class FlatKitFog : ScriptableRendererFeature {
    [Tooltip("To create new settings use 'Create > FlatKit > Fog Settings'.")]
    public FogSettings settings;

    [SerializeField, HideInInspector]
    private Material _effectMaterial;

    private BlitTexturePass _blitTexturePass;

    private RenderTargetHandle _fogTexture;

    private Texture2D _lutDepth;
    private Texture2D _lutHeight;

    private static readonly string FogShaderName = "Hidden/FlatKit/FogFilter";
    private static readonly int DistanceLut = Shader.PropertyToID("_DistanceLUT");
    private static readonly int Near = Shader.PropertyToID("_Near");
    private static readonly int Far = Shader.PropertyToID("_Far");
    private static readonly int UseDistanceFog = Shader.PropertyToID("_UseDistanceFog");
    private static readonly int UseDistanceFogOnSky = Shader.PropertyToID("_UseDistanceFogOnSky");
    private static readonly int DistanceFogIntensity = Shader.PropertyToID("_DistanceFogIntensity");
    private static readonly int HeightLut = Shader.PropertyToID("_HeightLUT");
    private static readonly int LowWorldY = Shader.PropertyToID("_LowWorldY");
    private static readonly int HighWorldY = Shader.PropertyToID("_HighWorldY");
    private static readonly int UseHeightFog = Shader.PropertyToID("_UseHeightFog");
    private static readonly int UseHeightFogOnSky = Shader.PropertyToID("_UseHeightFogOnSky");
    private static readonly int HeightFogIntensity = Shader.PropertyToID("_HeightFogIntensity");
    private static readonly int DistanceHeightBlend = Shader.PropertyToID("_DistanceHeightBlend");

    public override void Create() {
#if UNITY_EDITOR
        if (_effectMaterial == null) {
            AlwaysIncludedShaders.Add(BlitTexturePass.CopyEffectShaderName);
            AlwaysIncludedShaders.Add(FogShaderName);
        }
#endif

        if (settings == null) {
            return;
        }

        if (!CreateMaterials()) {
            return;
        }

        SetMaterialProperties();

        _blitTexturePass = new BlitTexturePass(_effectMaterial, useDepth: true, useNormals: false, useColor: false);
        _fogTexture.Init("_EffectTexture");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
#if UNITY_EDITOR
        if (renderingData.cameraData.isPreviewCamera) return;
        if (!settings.applyInSceneView && renderingData.cameraData.cameraType == CameraType.SceneView) return;
#endif

        SetMaterialProperties();
        _blitTexturePass.renderPassEvent = settings.renderEvent;
        renderer.EnqueuePass(_blitTexturePass);
    }

    private bool CreateMaterials() {
        if (_effectMaterial == null) {
            var effectShader = Shader.Find(FogShaderName);
            var blitShader = Shader.Find(BlitTexturePass.CopyEffectShaderName);
            if (effectShader == null || blitShader == null) return false;
            _effectMaterial = CoreUtils.CreateEngineMaterial(effectShader);
        }

        return _effectMaterial != null;
    }

    private void SetMaterialProperties() {
        if (_effectMaterial == null) {
            return;
        }

        UpdateDistanceLut();
        _effectMaterial.SetTexture(DistanceLut, _lutDepth);
        _effectMaterial.SetFloat(Near, settings.near);
        _effectMaterial.SetFloat(Far, settings.far);
        _effectMaterial.SetFloat(UseDistanceFog, settings.useDistance ? 1f : 0f);
        _effectMaterial.SetFloat(UseDistanceFogOnSky, settings.useDistanceFogOnSky ? 1f : 0f);
        _effectMaterial.SetFloat(DistanceFogIntensity, settings.distanceFogIntensity);

        UpdateHeightLut();
        _effectMaterial.SetTexture(HeightLut, _lutHeight);
        _effectMaterial.SetFloat(LowWorldY, settings.low);
        _effectMaterial.SetFloat(HighWorldY, settings.high);
        _effectMaterial.SetFloat(UseHeightFog, settings.useHeight ? 1f : 0f);
        _effectMaterial.SetFloat(UseHeightFogOnSky, settings.useHeightFogOnSky ? 1f : 0f);
        _effectMaterial.SetFloat(HeightFogIntensity, settings.heightFogIntensity);
        _effectMaterial.SetFloat(DistanceHeightBlend, settings.distanceHeightBlend);
    }

    private void UpdateDistanceLut() {
        if (settings.distanceGradient == null) return;

        if (_lutDepth != null) {
            DestroyImmediate(_lutDepth);
        }

        const int width = 256;
        const int height = 1;
        _lutDepth = new Texture2D(width, height, TextureFormat.RGBA32, /*mipChain=*/false) {
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear
        };

        //22b5f7ed-989d-49d1-90d9-c62d76c3081a

        for (float x = 0; x < width; x++) {
            Color color = settings.distanceGradient.Evaluate(x / (width - 1));
            for (float y = 0; y < height; y++) {
                _lutDepth.SetPixel(Mathf.CeilToInt(x), Mathf.CeilToInt(y), color);
            }
        }

        _lutDepth.Apply();
    }

    private void UpdateHeightLut() {
        if (settings.heightGradient == null) return;

        if (_lutHeight != null) {
            DestroyImmediate(_lutHeight);
        }

        const int width = 256;
        const int height = 1;
        _lutHeight = new Texture2D(width, height, TextureFormat.RGBA32, /*mipChain=*/false) {
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear
        };

        for (float x = 0; x < width; x++) {
            Color color = settings.heightGradient.Evaluate(x / (width - 1));
            for (float y = 0; y < height; y++) {
                _lutHeight.SetPixel(Mathf.CeilToInt(x), Mathf.CeilToInt(y), color);
            }
        }

        _lutHeight.Apply();
    }
}
}