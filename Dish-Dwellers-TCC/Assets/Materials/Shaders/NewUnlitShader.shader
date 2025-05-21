Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        [MainTexture]_MainTex ("Main Texture", 2D) = "white" {} 
        _Color ("Color", Color) = (1, 1, 1, 1)
        _FlowTex ("Flow Texture", 2D) = "black" {}
        _Tilling ("Tilling", Float) = 1.0 
        [Toggle]_DisplayUV ("Display UV as color", Float) = 0.0
    }

    SubShader
    {
        Tags 
        {
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalRenderPipeline" 
        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _DISPLAYUV_ON


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            
            #include "Flow.cginc"


            struct Attributes
            {
                float4 positionOS   : POSITION;                 
                float3 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 uv : TEXCOORD0;
                float4 positionWS : TEXCOORD1;
            };            

            float _Tilling;
            half4 _Color;

            TEXTURE2D(_FlowTex);
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_FlowTex);
            SAMPLER(sampler_MainTex);


            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                //OUT.positionWS = 
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                /*
                half3 flowTex = SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, IN.uv).rga * 2 - 1;
                float3 flowUV = FlowUV(IN.uv, flowTex, _Time.y + flowTex.z);
                */
                // Sample of the flow texture data:
                float2 flowVector = SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, IN.uv).rg * 2 - 1;
                float noise = SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, IN.uv).a;

                // UV distortion:
                float3 flowUV1 = FlowUV(IN.uv, flowVector, _Time.y, noise);
                float3 pericles = FlowUV(IN.uv, flowVector, _Time.y, noise + 0.5);

                //Tilling:
                half4 col = 0;
                #if _DISPLAYUV_ON
                    col = float4(frac(flowUV1.rg * _Tilling), 0, 1) * flowUV1.z;
                    col += float4(frac(pericles.rg * _Tilling), 0, 1) * pericles.z;
                #else
                    col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, flowUV1.rg) * flowUV1.z;
                    col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pericles.rg) * pericles.z;
                #endif
                return col * _Color;
            }
            ENDHLSL
        }
    }
}