Shader "Unlit/BolaDeFogo"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color1 ("Main Color", Color) = (1, 1, 1, 1)
        [HDR]_Color2 ("Accent Color", Color) = (0, 0, 0, 1)
        _Step ("Step", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color1;
            half4 _Color2;
            float _Step;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = float2(i.uv.x + _Time.x, i.uv.y + _Time.x);
                float mask = tex2D(_MainTex, uv).r;
                mask = step(mask, _Step);
                half4 finalCol = lerp(_Color1, _Color2, mask);

                UNITY_APPLY_FOG(i.fogCoord, finalCol);
                return finalCol;
            }
            ENDCG
        }
    }
}
