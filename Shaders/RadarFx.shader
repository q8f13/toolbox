// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/RadarFx"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1.0, 0.0, 0.0, 1.0)
        _FresnelColor("FresnelColor", Color) = (0.0, 1.0, 0.0, 1.0)
        _BodyWorldPosition("BodyWorldPosition", Vector) = (0.0, 0.0, 0.0)

        _Freq("Frequency", float) = 1.0
        _Wave("WaveLength", float) = 1.0
        _R("Radius", float) = 2.0

        _Bias("FresnelBias", float) = 0.2
        _Power("FresnelPower", float) = 1.0
        _I("FresnelI", float) = 1.0
        _Scale("Scale", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        // Blend Off
        Blend SrcAlpha OneMinusSrcAlpha
        // Blend SrcAlpha OneMinusSrcAlpha
        // Cull Off
        // ZWrite Off
        // ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            #include "qfboxShaderUtil.cginc"

            static float MYPI = 3.1415926;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };


            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 lpos : TEXCOORD1;
                // float4 wpos : TEXCOORD3;
                float3 normal : TEXCOORD2;
                float R:TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 _BodyWorldPosition;
            float4 _Color;

            float _Freq;
            float _Wave;
            float _R;

            // fresnel
            float _Bias;
            float _Scale;
            float _I;
            float _Power;
            fixed4 _FresnelColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lpos = v.vertex;

                // fresnel
                float3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 normWorld = normalize(mul(unity_ObjectToWorld, v.normal).xyz);
                o.R = CalcFresnel(_Bias, _Scale, _Power, posWorld, _WorldSpaceCameraPos.xyz, normWorld);

                o.normal = v.normal;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            inline float3 projectOnPlane( float3 vec, float3 normal )
            {
                return vec - normal * dot( vec, normal );
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;

                // frequency
                // consider using l.xyz instead of l.x / l.xz
                // but sphere surface may pop artefact circles due to face segments
                // so some scale on x axis may be useful
                // l.x *= 1.05;
                // l.y = 0.0;

                float val = 1.0 - (abs(-length(i.lpos.x) + _Time.w*_Freq) % _Wave / _Wave);
                col.xyz = lerp(_Color.xyz, _FresnelColor.xyz, i.R);
                col.a = val;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
