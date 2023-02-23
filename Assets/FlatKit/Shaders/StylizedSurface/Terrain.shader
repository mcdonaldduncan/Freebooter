Shader "FlatKit/Terrain" {
    Properties {
        /*---*/
        _Color ("Main Color", Color) = (1,1,1,1)
        
        [Space(10)]
        [KeywordEnum(None, Single, Steps, Curve)]_CelPrimaryMode("Cel Shading Mode", Float) = 1
        _ColorDim ("[_CELPRIMARYMODE_SINGLE]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _ColorDimSteps ("[_CELPRIMARYMODE_STEPS]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _ColorDimCurve ("[_CELPRIMARYMODE_CURVE]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _SelfShadingSize ("[_CELPRIMARYMODE_SINGLE]Self Shading Size", Range(0, 1)) = 0.5
        _ShadowEdgeSize ("[_CELPRIMARYMODE_SINGLE]Shadow Edge Size", Range(0, 0.5)) = 0.05
        _Flatness ("[_CELPRIMARYMODE_SINGLE]Localized Shading", Range(0, 1)) = 1.0
        
        [IntRange]_CelNumSteps ("[_CELPRIMARYMODE_STEPS]Number Of Steps", Range(1, 10)) = 3.0
        _CelStepTexture ("[_CELPRIMARYMODE_STEPS][LAST_PROP_STEPS]Cel steps", 2D) = "black" {}
        _CelCurveTexture ("[_CELPRIMARYMODE_CURVE][LAST_PROP_CURVE]Ramp", 2D) = "black" {}
        
        [Space(10)]
        [Toggle(DR_CEL_EXTRA_ON)] _CelExtraEnabled("Enable Extra Cel Layer", Int) = 0
        _ColorDimExtra ("[DR_CEL_EXTRA_ON]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _SelfShadingSizeExtra ("[DR_CEL_EXTRA_ON]Self Shading Size", Range(0, 1)) = 0.6
        _ShadowEdgeSizeExtra ("[DR_CEL_EXTRA_ON]Shadow Edge Size", Range(0, 0.5)) = 0.05
        _FlatnessExtra ("[DR_CEL_EXTRA_ON]Localized Shading", Range(0, 1)) = 1.0
        
        [Space(10)]
        [Toggle(DR_SPECULAR_ON)] _SpecularEnabled("Enable Specular", Int) = 0
        [HDR] _FlatSpecularColor("[DR_SPECULAR_ON]Specular Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _FlatSpecularSize("[DR_SPECULAR_ON]Specular Size", Range(0.0, 1.0)) = 0.1
        _FlatSpecularEdgeSmoothness("[DR_SPECULAR_ON]Specular Edge Smoothness", Range(0.0, 1.0)) = 0
        
        [Space(10)]
        [Toggle(DR_RIM_ON)] _RimEnabled("Enable Rim", Int) = 0
        [HDR] _FlatRimColor("[DR_RIM_ON]Rim Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _FlatRimLightAlign("[DR_RIM_ON]Light Align", Range(0.0, 1.0)) = 0
        _FlatRimSize("[DR_RIM_ON]Rim Size", Range(0, 1)) = 0.5
        _FlatRimEdgeSmoothness("[DR_RIM_ON]Rim Edge Smoothness", Range(0, 1)) = 0.5
        
        [Space(10)]
        [Toggle(DR_GRADIENT_ON)] _GradientEnabled("Enable Height Gradient", Int) = 0
        [HDR] _ColorGradient("[DR_GRADIENT_ON]Gradient Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _GradientCenterX("[DR_GRADIENT_ON]Center X", Float) = 0
        _GradientCenterY("[DR_GRADIENT_ON]Center Y", Float) = 0
        _GradientSize("[DR_GRADIENT_ON]Size", Float) = 10.0
        _GradientAngle("[DR_GRADIENT_ON]Gradient Angle", Range(0, 360)) = 0

        _LightContribution("[FOLDOUT(Advanced Lighting){4}]Light Color Contribution", Range(0, 1)) = 0

        // Used to provide light direction to cel shading if all light in the scene is baked.
        [Header(Override light direction)]
        [Toggle]_OverrideLightmapDir("[DR_ENABLE_LIGHTMAP_DIR]Enable", Int) = 0
        _LightmapDirectionPitch("Pitch", Range(0, 360)) = 0
        _LightmapDirectionYaw("Yaw", Range(0, 360)) = 0
        [HideInInspector] _LightmapDirection("Override Light Direction", Vector) = (0, 1, 0, 0)

        [KeywordEnum(None, Multiply, Color)] _UnityShadowMode ("[FOLDOUT(Unity Built-in Shadows){4}]Mode", Float) = 0
        _UnityShadowPower("[_UNITYSHADOWMODE_MULTIPLY]Power", Range(0, 1)) = 0.2
        _UnityShadowColor("[_UNITYSHADOWMODE_COLOR]Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _UnityShadowSharpness("Sharpness", Range(1, 10)) = 1.0
        
        /*---*/
        
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
    
        // From TerrainLit.hlsl
        [HideInInspector] [ToggleUI] _EnableHeightBlend("EnableHeightBlend", Float) = 0.0
        _HeightTransition("Height Transition", Range(0, 1.0)) = 0.0
        // Layer count is passed down to guide height-blend enable/disable, due
        // to the fact that heigh-based blend will be broken with multipass.
        [HideInInspector] [PerRendererData] _NumLayersCount ("Total Layer Count", Float) = 1.0
    }

    // -----------------------------------------------
    //_FLAT_KIT_BUILT_IN_BEGIN_
    SubShader {
        Tags {
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque"
        }

        CGPROGRAM
        #include "DustyroomStylizedLighting.cginc"
        #pragma surface surfTerrain DustyroomStylized vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows

        #pragma shader_feature_local __ _CELPRIMARYMODE_SINGLE _CELPRIMARYMODE_STEPS _CELPRIMARYMODE_CURVE
        #pragma shader_feature_local DR_CEL_EXTRA_ON
        #pragma shader_feature_local DR_GRADIENT_ON
        #pragma shader_feature_local DR_SPECULAR_ON
        #pragma shader_feature_local DR_RIM_ON
        #pragma shader_feature_local __ _UNITYSHADOWMODE_MULTIPLY _UNITYSHADOWMODE_COLOR

        #define _TEXTUREBLENDINGMODE_MULTIPLY

        #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE

        #pragma require interpolators15
        #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
        #pragma multi_compile_fog // needed because finalcolor oppresses fog code generation.
        #pragma target 3.0
        // needs more than 8 texcoords
        #pragma exclude_renderers gles

        // Not using normal map on the terrain.
        // #pragma multi_compile_local __ _NORMALMAP

        #define TERRAIN_STANDARD_SHADER
        #define TERRAIN_INSTANCED_PERPIXEL_NORMAL
        #define TERRAIN_SURFACE_OUTPUT SurfaceOutputDustyroom
        #include "DustyroomTerrainSplatmapCommon.cginc"

        half _Metallic0;
        half _Metallic1;
        half _Metallic2;
        half _Metallic3;

        half _Smoothness0;
        half _Smoothness1;
        half _Smoothness2;
        half _Smoothness3;

        void surfTerrain(Input IN, inout SurfaceOutputDustyroom o) {
            half4 c = SurfaceCore(IN.worldNormal, IN.worldPos, IN.lightDir, IN.viewDir);
            
            half4 splat_control;
            half weight;
            fixed4 mixedDiffuse;
            half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
            
            SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, o.Normal);
            o.Albedo = mixedDiffuse.rgb * c.rgb;
            o.Alpha = weight * c.a;
            
            o.Smoothness = mixedDiffuse.a;
            
            o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
        }

        ENDCG

        UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
        UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }

    Dependency "AddPassShader"    = "Hidden/TerrainEngine/Splatmap/Standard-AddPass"
    Dependency "BaseMapShader"    = "Hidden/TerrainEngine/Splatmap/Standard-Base"
    Dependency "BaseMapGenShader" = "Hidden/TerrainEngine/Splatmap/Standard-BaseGen"
    
    Fallback "Nature/Terrain/Diffuse"
    CustomEditor "StylizedSurfaceEditor"
}
