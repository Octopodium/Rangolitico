Shader "Cutout/XRayVIsion"
{
    Properties
    {
        _Color ("X-Ray Vision Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Texture", 2D) = "white" {}
        [KeywordEnum(one, two, three, four)] _UvSet("UV Set Being Rendered", Float) = 0.0
    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque" 
            "Queue" = "Geometry+1"
            "PreviewType" = "Plane"
        }

        Stencil{
            ref 0
            comp Equal
            Pass Keep
        }

        ZTest Greater
        AlphaToMask On

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

            half4 _Color;
            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half mask = tex2D(_MainTex, i.uv).a;
                return half4(_Color.rgb, mask);
            }
            ENDCG
        }
    }
}
