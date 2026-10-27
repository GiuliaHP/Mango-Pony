Shader "MangoPony/ToonMaster"
{
    Properties
    {
        [Header(Base Surface)]
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.85, 0.55, 0.32, 1.0)
        _ShadeColor ("Shadow Tint", Color) = (0.55, 0.32, 0.20, 1.0)

        [Header(Cel Shading Parameters)]
        _CelThreshold ("Cel Threshold", Range(0.0, 1.0)) = 0.45
        _CelSmoothness ("Cel Smoothness", Range(0.001, 0.5)) = 0.05
        _RimColor ("Rim Light Color", Color) = (1.0, 0.95, 0.85, 0.4)
        _RimPower ("Rim Power", Range(0.5, 10.0)) = 3.5

        [Header(Face Markings Layer)]
        _FaceMarkTex ("Face Mark Texture", 2D) = "black" {}
        _FaceMarkColor ("Face Mark Color", Color) = (0.98, 0.98, 0.98, 1.0)
        _FaceMarkIntensity ("Face Mark Intensity", Range(0.0, 1.0)) = 1.0

        [Header(Leg Markings Layer)]
        _LegMarkGlobalTex ("Leg Mark Global Mask", 2D) = "black" {}
        _LegMarkFLTex ("Leg Mark Front-Left Mask", 2D) = "black" {}
        _LegMarkFRTex ("Leg Mark Front-Right Mask", 2D) = "black" {}
        _LegMarkBLTex ("Leg Mark Back-Left Mask", 2D) = "black" {}
        _LegMarkBRTex ("Leg Mark Back-Right Mask", 2D) = "black" {}
        _LegMarkColor ("Leg Mark Color", Color) = (0.98, 0.98, 0.98, 1.0)
        _LegMarkIntensity ("Leg Mark Intensity", Range(0.0, 1.0)) = 1.0
        [Toggle] _UseIndividualLegs ("Use Individual Leg Masks", Float) = 0.0

        [Header(Body Patterns Layer)]
        _BodyPatternTex ("Body Pattern Mask", 2D) = "black" {}
        _BodyPatternColor ("Body Pattern Color", Color) = (0.95, 0.95, 0.95, 1.0)
        _BodyPatternIntensity ("Body Pattern Intensity", Range(0.0, 1.0)) = 1.0

        [Header(Coat Nuances Layer)]
        _CoatNuanceTex ("Coat Nuance Mask", 2D) = "black" {}
        _CoatNuanceColor ("Coat Nuance Color", Color) = (0.92, 0.75, 0.55, 1.0)
        _CoatNuanceIntensity ("Coat Nuance Intensity", Range(0.0, 1.0)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            // URP Keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float2 uv           : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_FaceMarkTex);
            SAMPLER(sampler_FaceMarkTex);

            TEXTURE2D(_LegMarkGlobalTex);
            SAMPLER(sampler_LegMarkGlobalTex);
            TEXTURE2D(_LegMarkFLTex);
            SAMPLER(sampler_LegMarkFLTex);
            TEXTURE2D(_LegMarkFRTex);
            SAMPLER(sampler_LegMarkFRTex);
            TEXTURE2D(_LegMarkBLTex);
            SAMPLER(sampler_LegMarkBLTex);
            TEXTURE2D(_LegMarkBRTex);
            SAMPLER(sampler_LegMarkBRTex);

            TEXTURE2D(_BodyPatternTex);
            SAMPLER(sampler_BodyPatternTex);

            TEXTURE2D(_CoatNuanceTex);
            SAMPLER(sampler_CoatNuanceTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _ShadeColor;
                float _CelThreshold;
                float _CelSmoothness;
                float4 _RimColor;
                float _RimPower;

                float4 _FaceMarkColor;
                float _FaceMarkIntensity;

                float4 _LegMarkColor;
                float _LegMarkIntensity;
                float _UseIndividualLegs;

                float4 _BodyPatternColor;
                float _BodyPatternIntensity;

                float4 _CoatNuanceColor;
                float _CoatNuanceIntensity;
            CBUFFER_END

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs vertexInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInputs.positionCS;
                output.positionWS = vertexInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            half4 LitPassFragment(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);

                // 1. Couche de base (Albedo)
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                half3 finalAlbedo = baseTex.rgb * _BaseColor.rgb;

                // 2. Couche Dégradé / Nuance (Pommelé, Pangaré, Isabelle)
                half nuanceMask = SAMPLE_TEXTURE2D(_CoatNuanceTex, sampler_CoatNuanceTex, uv).r * _CoatNuanceIntensity;
                finalAlbedo = lerp(finalAlbedo, _CoatNuanceColor.rgb, nuanceMask);

                // 3. Couche Motif corporel (Appaloosa, Tobiano, Capé)
                half patternMask = SAMPLE_TEXTURE2D(_BodyPatternTex, sampler_BodyPatternTex, uv).r * _BodyPatternIntensity;
                finalAlbedo = lerp(finalAlbedo, _BodyPatternColor.rgb, patternMask);

                // 4. Couche Marques aux jambes (Balzanes)
                half legMask = 0.0;
                if (_UseIndividualLegs > 0.5)
                {
                    half maskFL = SAMPLE_TEXTURE2D(_LegMarkFLTex, sampler_LegMarkFLTex, uv).r;
                    half maskFR = SAMPLE_TEXTURE2D(_LegMarkFRTex, sampler_LegMarkFRTex, uv).r;
                    half maskBL = SAMPLE_TEXTURE2D(_LegMarkBLTex, sampler_LegMarkBLTex, uv).r;
                    half maskBR = SAMPLE_TEXTURE2D(_LegMarkBRTex, sampler_LegMarkBRTex, uv).r;
                    legMask = saturate(maskFL + maskFR + maskBL + maskBR);
                }
                else
                {
                    legMask = SAMPLE_TEXTURE2D(_LegMarkGlobalTex, sampler_LegMarkGlobalTex, uv).r;
                }
                legMask *= _LegMarkIntensity;
                finalAlbedo = lerp(finalAlbedo, _LegMarkColor.rgb, legMask);

                // 5. Couche Marque en tête (Étoile, Liste, etc.)
                half faceMask = SAMPLE_TEXTURE2D(_FaceMarkTex, sampler_FaceMarkTex, uv).r * _FaceMarkIntensity;
                finalAlbedo = lerp(finalAlbedo, _FaceMarkColor.rgb, faceMask);

                // 6. Éclairage Toon / Cel-Shading
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half NdotL = saturate(dot(normalWS, mainLight.direction));

                // Demi-Lambert adouci avec seuil cel
                half halfLambert = NdotL * 0.5 + 0.5;
                half cel = smoothstep(_CelThreshold - _CelSmoothness, _CelThreshold + _CelSmoothness, halfLambert);
                half shadow = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
                cel *= shadow;

                // Teinte d'ombrage stylisée
                half3 litAlbedo = lerp(_ShadeColor.rgb * finalAlbedo, finalAlbedo, cel);
                half3 diffuseLighting = litAlbedo * mainLight.color;

                // Lumière ambiante / GI
                half3 ambient = SampleSH(normalWS) * finalAlbedo * 0.5;

                // Rim Light stylisé
                half NdotV = 1.0 - saturate(dot(normalWS, viewDirWS));
                half rim = pow(NdotV, _RimPower) * cel;
                half3 rimLighting = _RimColor.rgb * (rim * _RimColor.a);

                half3 finalColor = diffuseLighting + ambient + rimLighting;
                return half4(finalColor, _BaseColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
            };

            float3 _LightDirection;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}

