Shader "Unlit/NewUnlitShader"
{
    Properties
    { 

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


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            


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


            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = 
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 customColor;
                customColor = float4(IN.uv, 1);
                return customColor;
            }
            ENDHLSL
        }
    }
}