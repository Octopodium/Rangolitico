Shader "Custom/SpriteCharacter"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.0
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [Space(15)][Header(Sprite Config)][Space(10)]
        [KeywordEnum(one, two, three, four)] _Sprite("Current Sprite Being used", Float) = 0.0
        _Cutoff ("Cutoff", Range(0, 1)) = 0.5
        
    }
    SubShader
    {
        Tags {
            "Queue"="Geometry"
            "IgnoreProjector"="True"
            "RenderType"="Opaque"
            "PreviewType"="Plane"
        }
        Cull Back
        Stencil{
            Ref 1
            Comp Always
            Pass replace
        }

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alphatest:_Cutoff addshadow
        #pragma multi_compile _SPRITE_ONE _SPRITE_TWO _SPRITE_THREE _SPRITE_FOUR

        sampler2D _MainTex;

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
            half4 c;

            #if _SPRITE_ONE
                c = tex2D(_MainTex, IN.uv_MainTex);
            #elif _SPRITE_TWO
                c = tex2D(_MainTex, IN.uv2_MainTex);
            #elif _SPRITE_THREE
                c = tex2D(_MainTex, IN.uv3_MainTex);
            #elif _SPRITE_FOUR
                c = tex2D(_MainTex, IN.uv4_MainTex);
            #endif

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
