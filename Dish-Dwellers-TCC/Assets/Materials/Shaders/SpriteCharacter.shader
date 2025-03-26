Shader "Custom/SpriteCharacter"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.0
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Sprite ("Sprite Channel", Range(0, 3)) = 0
        [HideInInspector]_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Cull Back
        Blend One OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alphatest:_Cutoff addshadow
        //#pragma dynamic_branch SPRITE_CHANNEL1 SPRITE_CHANNEL2 SPRITE_CHANNEL3

        sampler2D _MainTex;
        int _Sprite;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv2_MainTex;
            float2 uv3_MainTex;
            float2 uv4_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c;

            switch(_Sprite){

                case 0:
                    c = tex2D(_MainTex, IN.uv_MainTex);
                break;

                case 1:
                    c = tex2D(_MainTex, IN.uv2_MainTex);
                break;

                case 2:
                    c = tex2D(_MainTex, IN.uv3_MainTex);
                break;

                case 3:
                    c = tex2D(_MainTex, IN.uv4_MainTex);
                break;

            }

            o.Albedo = c.rgb;

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
