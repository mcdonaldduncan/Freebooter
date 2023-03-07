Shader "Hidden/Flat Kit/Terrain/Lit (Add Pass)"
{
    Properties
    {
        // Layer count is passed down to guide height-blend enable/disable, due
        // to the fact that heigh-based blend will be broken with multipass.
        [HideInInspector] [PerRendererData] _NumLayersCount ("Total Layer Count", Float) = 1.0

        // set by terrain engine
        [HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
        [HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}
        [HideInInspector] _Normal3("Normal 3 (A)", 2D) = "bump" {}
        [HideInInspector] _Normal2("Normal 2 (B)", 2D) = "bump" {}
        [HideInInspector] _Normal1("Normal 1 (G)", 2D) = "bump" {}
        [HideInInspector] _Normal0("Normal 0 (R)", 2D) = "bump" {}
        [HideInInspector][Gamma] _Metallic0("Metallic 0", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic1("Metallic 1", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic2("Metallic 2", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic3("Metallic 3", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _Mask3("Mask 3 (A)", 2D) = "grey" {}
        [HideInInspector] _Mask2("Mask 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Mask1("Mask 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Mask0("Mask 0 (R)", 2D) = "grey" {}
        [HideInInspector] _Smoothness0("Smoothness 0", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _Smoothness1("Smoothness 1", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _Smoothness2("Smoothness 2", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _Smoothness3("Smoothness 3", Range(0.0, 1.0)) = 1.0

        // used in fallback on old cards & base map
        [HideInInspector] _BaseMap("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _BaseColor("Main Color", Color) = (1,1,1,1)

		[HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}

		/* start CurvedWorld */
		//[CurvedWorldBendSettings] _CurvedWorldBendSettings("0|1|1", Vector) = (0, 0, 0, 0)
		/* end CurvedWorld */
    }

	HLSLINCLUDE

	#pragma multi_compile_fragment __ _ALPHATEST_ON

	ENDHLSL

    SubShader
    {
        Tags { "Queue" = "Geometry-99" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"}

        Pass
        {
            Name "TerrainAddLit"
            Tags { "LightMode" = "UniversalForwardOnly" }
            Blend One One
            HLSLPROGRAM
            #pragma target 3.0

            #pragma vertex SplatmapVert
            #pragma fragment SplatmapFragment_DSTRM

            // -------------------------------------
            // Flat Kit
            #pragma shader_feature_local __ _CELPRIMARYMODE_SINGLE _CELPRIMARYMODE_STEPS _CELPRIMARYMODE_CURVE
            #pragma shader_feature_local DR_CEL_EXTRA_ON
            #pragma shader_feature_local DR_GRADIENT_ON
            #pragma shader_feature_local DR_SPECULAR_ON
            #pragma shader_feature_local DR_RIM_ON
            #pragma shader_feature_local __ _UNITYSHADOWMODE_MULTIPLY _UNITYSHADOWMODE_COLOR

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #pragma shader_feature_local_fragment _TERRAIN_BLEND_HEIGHT
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _MASKMAP
            // Sample normal in pixel shader when doing instancing
            #pragma shader_feature_local _TERRAIN_INSTANCED_PERPIXEL_NORMAL
            #define TERRAIN_SPLAT_ADDPASS

            #define FLATKIT_TERRAIN 1

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
            #include "LibraryUrp/StylizedInput.hlsl"
            #include "LibraryUrp/Lighting_DR.hlsl"
            #include "LibraryUrp/TerrainLitPasses_DR.hlsl"
            
			/* start CurvedWorld */
			//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
			//#define CURVEDWORLD_BEND_ID_1
			//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
			//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
			//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"
			/* end CurvedWorld */

            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
