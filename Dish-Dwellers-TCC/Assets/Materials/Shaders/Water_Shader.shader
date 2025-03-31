Shader "Custom/Water_Shader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NormalTex ("Normal Textute", 2D) = "bump" {}
        _NormalTex2 ("Normal Texture2", 2D) = "bump" {}

        _ScaleNoise ("Noise Scale", Range(0, 1)) = 0.5
        _WaveSpeed ("Wave Speed", Range(0, 1)) = 0.5
        _WaveAmplitude ("Wave Amplitude", Range(0, 1)) = 0.5
        _NormalStrength ("Normal Strength", Range(0, 1)) = 0.5

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha vertex:vert

        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NoiseTex;
        sampler2D _NormalTex;
        sampler2D _NormalTex2;

         struct Input
        {
            float2 uv_NormalTex;
        };

        float _ScaleNoise;
        float _WaveSpeed;
        float _WaveAmplitude;
        float _NormalStrength;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void vert(inout appdata_full v)
        {
            float2 noiseUV = float2((v.texcoord.xy + _Time * _WaveSpeed) * _ScaleNoise);
            float noiseV = tex2Dlod(_NoiseTex, float4(noiseUV, 0, 0)).x * _WaveAmplitude;

            v.vertex = v.vertex + float4(0, noiseV, 0, 0);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            float normalUVX = IN.uv_NormalTex.x + sin(_Time) * 5;
            float normalUVY = IN.uv_NormalTex.y + sin(_Time) * 5;

            float2 normalUV1 = float2(normalUVX, IN.uv_NormalTex.y);
            float2 normalUV2 = float2(IN.uv_NormalTex.x, normalUVY);

            o.Normal = UnpackNormal((tex2D(_NormalTex, normalUV1) * tex2D(_NormalTex2, normalUV2)) * _NormalStrength);
        }
        ENDCG
    }
    FallBack "Diffuse"
}