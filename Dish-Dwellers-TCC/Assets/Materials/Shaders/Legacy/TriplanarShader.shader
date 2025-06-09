Shader "Custom/TriplanarShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _Size ("Size", Range(0, 3)) = 1.0

        _OffsetX_YX ("Offset X (YX)", Range(-3, 3)) = 0.0
        _OffsetX_XZ ("Offset X (XZ)", Range(-3, 3)) = 0.0
        _OffsetY_YX ("Offset Y (YX)", Range(-3, 3)) = 0.0
        _OffsetY_YZ ("Offset Y (YZ)", Range(-3, 3)) = 0.0
        _OffsetZ_YZ ("Offset Z (YZ)", Range(-3, 3)) = 0.0
        _OffsetZ_XZ ("Offset Z (XZ)", Range(-3, 3)) = 0.0


        [Toggle] _InverterX ("Invert X", Float) = 0
        [Toggle] _InverterY ("Invert Y", Float) = 0
        [Toggle] _InverterZ ("Invert Z", Float) = 0
        [Toggle] _Girar90Graus ("Girar 90 Graus", Float) = 0
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal; // Normal in world space
            float3 worldPos;   // Position in world space
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        bool _InverterX, _InverterY, _InverterZ;
        bool _Girar90Graus;

        float _Size;

        float _OffsetX_YX;
        float _OffsetX_XZ;
        float _OffsetY_YX;
        float _OffsetY_YZ;
        float _OffsetZ_YZ;
        float _OffsetZ_XZ;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 nx = IN.worldNormal;
            float3 wpos = IN.worldPos;

            if (_InverterX > 0) wpos.x = -wpos.x;
            if (_InverterY > 0) wpos.y = -wpos.y;
            if (_InverterZ > 0) wpos.z = -wpos.z;

            if (_Girar90Graus > 0) {
                float3x3 rot = float3x3(1, 0, 0,
                                        0, 0, -1,
                                        0, 1, 0);
                wpos = mul(rot, wpos);
                nx = mul(rot, nx);
            }

            float x = wpos.x;
            float y = wpos.y;
            float z = wpos.z;

            fixed4 xc = tex2D(_MainTex, float2(y+_OffsetY_YX,x+_OffsetX_YX) * _Size);
            fixed4 yc = tex2D(_MainTex, float2(y+_OffsetY_YZ,z+_OffsetZ_YZ) * _Size);
            fixed4 zc = tex2D(_MainTex, float2(x+_OffsetX_XZ,z+_OffsetZ_XZ) * _Size);

            o.Albedo = (abs(nx.z) * xc + abs(nx.x) * yc + abs(nx.y) * zc) * _Color;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
