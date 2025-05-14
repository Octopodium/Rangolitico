Shader "Custom/Triplanar"
{
    /*
    Properties
    {
        [Header(Color Configuration)][Space(10)]
        _Color ("Color", Color) = (1,1,1,1)

        [Space(15)]
        [Header(Texture Configuration)][Space(10)]
        _TopTex ("Top Texture", 2D) = "white" {}
        _SideTex ("Side Texture", 2D) = "white" {}
        _Scale ("Scale of the the Top Texture", Vector) = (1, 1, 1, 1)

        [Toggle]_InvertX ("Invert the X Axis", float) = 0.0
        [Toggle]_InvertY ("Invert the Y Axis", float) = 0.0
        [Toggle]_InvertZ ("Invert the Z Axis", float) = 0.0
        

        [Sapce(15)]
        [Header(Material Configuration)][Space(10)]
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        HLSLPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #pragma shader_feature _INVERTX_ON
        #pragma shader_feature _INVERTY_ON
        #pragma shader_feature _INVERTZ_ON

        sampler2D _TopTex;
        sampler2D _SideTex;

        struct Input
        {
            float3 worldNormal;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        half4 _Color;
        float4 _Scale;

        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void WeightedBlend(in float3 wnormal, out float3 blend_weights){
            blend_weights = abs(wnormal) -0.2;
            blend_weights *= 7;
            blend_weights = pow(blend_weights, 3);
            blend_weights = max(0, blend_weights);
            blend_weights /= dot(blend_weights, 1);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 tiledWorldPos = frac(float3(IN.worldPos.x * _Scale.x, IN.worldPos.y * _Scale.y, IN.worldPos.z * _Scale.z));

            #if _INVERTX_ON
                tiledWorldPos.x *= -1;
            #endif

            #if _INVERTY_ON
                tiledWorldPos.y *= -1;
            #endif

            #if _INVERTZ_ON
                tiledWorldPos.z *= -1;
            #endif



            half4 col1 = tex2D(_TopTex, tiledWorldPos.xz);
            half4 col2 = tex2D(_SideTex, tiledWorldPos.xy);
            half4 col3 = tex2D(_SideTex, tiledWorldPos.zy);

            float3 weight;
            WeightedBlend(IN.worldNormal, weight);

            half4 finalCol = col1 * weight.g + col3 * weight.r + col2 * weight.b;
            finalCol *= _Color;

            
            // Metallic and smoothness come from slider variables
            o.Albedo = finalCol;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDHLSL
    }
    FallBack "Diffuse"
    */
}
