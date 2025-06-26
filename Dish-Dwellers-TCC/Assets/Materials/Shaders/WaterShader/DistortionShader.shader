Shader "Unlit/DistortionShader"
{
    Properties
    {
        [Header(Texture Configuration)]
        [Space(15)]

        [MainTexture]_MainTex ("Main Texture", 2D) = "white" {} 
        _FlowTex ("Flow Texture", 2D) = "black" {}
        _NormalMap ("Normal Map", 2D) = "black" {}
        [Toggle]_DisplayUV ("Display UV as color", Float) = 0.0

        _Color ("Color", Color) = (1, 1, 1, 1)
        _DeepColor ("DeepColor", Color) = (0, 0, 0, 1)

        _Tilling ("Tilling", Range(0.0, 10)) = 1.0 
        _DistortionTilling ("Tilling for the distortion texture", Range(0.0, 10)) = 1.0


        [Space(20)]

        [Header(Animation Configuration)]
        [Space(15)]

        _UJump ("U Jump per phase", Range(-0.25, 0.25))  = 0.25
        _VJump ("V Jump per phase", Range(-0.25, 0.25))  = 0.25

        _Speed ("Animation Speed", Float) = 1.0
        _DistortionStrength("Strength of the distortion applied", Float) = 1.0

        [Space(20)]

        [Header(Water Configuration)]
        _DepthOffset ("Shalow water offset", Range(-10, 10)) = 0.0
        _TransitionIntensity ("Intensity of the transition of deep to shallow water", Range(1, 20)) = 1
        
    }

    SubShader
    {
        Tags 
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"  
            "RenderPipeline" = "UniversalRenderPipeline" 
        }

        //Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _DISPLAYUV_ON


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Flow.cginc"
            #include "LookingTroughWater.cginc"


            struct Attributes
            {
                float4 positionOS : POSITION;          
                float3 uv : TEXCOORD0;
                half3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 positionWS : TEXCOORD2;
                float2 uv : TEXCOORD0;
                half3 normal : TEXCOORD1;
            };            

            float _Tilling;
            float _DistortionTilling;

            half4 _Color, _DeepColor;

            float _UJump, _VJump;
            float _Speed;
            float _DistortionStrength;

            float _DepthOffset;
            float _TransitionIntensity;

            TEXTURE2D(_FlowTex);
            TEXTURE2D(_MainTex);
            TEXTURE2D(_CameraOpaqueTexture);

            SAMPLER(sampler_FlowTex);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_CameraOpaqueTexture);


            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS);
                OUT.positionHCS = positions.positionCS;
                OUT.uv = positions.positionWS.xz;
                OUT.positionWS = positions.positionWS;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // TEXTURE DISTORTION :
                // Sample of the flow texture data:
                float2 flowVector = SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, IN.uv * _DistortionTilling).rg * 2 - 1;
                flowVector *= _DistortionStrength;
                float noise = SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, IN.uv * _DistortionTilling).a;

                float2 jump = float2(_UJump, _VJump);

                float time = _Time.y * _Speed + noise;

                // UV distortion:
                float3 flowUV1 = FlowUV(IN.uv, flowVector, jump, _DistortionTilling, time, 0.0);
                float3 pericles = FlowUV(IN.uv, flowVector, jump, _DistortionTilling, time, 0.5);

                //Tilling:
                half4 col = 0;
                #if _DISPLAYUV_ON
                    col = float4(frac(flowUV1.rg * _Tilling), 0, 1) * flowUV1.z;
                    col += float4(frac(pericles.rg * _Tilling), 0, 1) * pericles.z;
                #else
                    col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, frac(flowUV1.rg * _Tilling)) * flowUV1.z;
                    col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, frac(pericles.rg * _Tilling)) * pericles.z;
                #endif

                half4 surfaceCol = saturate(lerp( _DeepColor, _Color, col.r));

                
                
                // Depth texture sampling:
                float2 projUV = IN.positionHCS.xy / _ScaledScreenParams.xy;
                
                #if UNITY_REVERSED_Z
                real depth = SampleSceneDepth(projUV);
                #else
                real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif

                //Reconstructing worldSpacePosition from depth:
                float3 worldPos = ComputeWorldSpacePosition(projUV, depth, UNITY_MATRIX_I_VP);
                
                /*
                uint scale = 10;
                uint3 worldIntPos = uint3(abs(worldPos.xyz * scale));
                bool white = (worldIntPos.x & 1) ^ (worldIntPos.y & 1) ^ (worldIntPos.z & 1);
                half4 color = white ? half4(1,1,1,1) : half4(0,0,0,1);
                */

                #if UNITY_REVERSED_Z
                    if(depth < 0.0001){
                        return _DeepColor + surfaceCol;
                    }
                #else
                    if(depth > 0.9999){
                        return _DeepColor + surfaceCol;
                    }
                #endif
                
                float compDepth = saturate(1 - ((IN.positionWS.y - (worldPos.y + _DepthOffset)) / _TransitionIntensity));
                half4 color = lerp( _DeepColor, _Color, compDepth * compDepth);

                //float3 opaqueUV = FlowUV(projUV, flowVector, jump, 1, time, 0.0);
                //half4 opaque = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, frac(opaqueUV.rg * _Tilling)) * opaqueUV.z;
                //return opaque + 0.1;

                return saturate(color + surfaceCol);
            }
            ENDHLSL
        }
    }
}