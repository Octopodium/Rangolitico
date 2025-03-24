Shader "Hidden/SombraCircular"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.5, 0.5, 0.5, 1)
        _Alpha ("Transparencia", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Cull Back
        ZWrite On
        Tags{
            "RenderType" = "Transparent"
            "Queue" = "transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _Color;
            float _Alpha;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                col.a = (tex2D(_MainTex, i.uv).a > 0) * _Alpha;
                col.rgb = _Color.rgb;
                return col;
            }
            ENDCG
        }
    }
}
