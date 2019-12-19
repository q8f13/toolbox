// pyramid fake hologram shader
// need forward, back, left, right - four rt as cameras around character
// use as postprocessing (another cam)
Shader "Unlit/PyramidHolo"
{
    Properties
    {
        _TexForward ("Texture", 2D) = "white" {}
        _TexBack ("Texture", 2D) = "white" {}
        _TexLeft ("Texture", 2D) = "white" {}
        _TexRight ("Texture", 2D) = "white" {}

        _Scale("Scale", float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            float2 TransformUV(float2 uv)
            {
                float u = uv.x;
                float v = uv.y;
                float l = 1.0 - sin(45);
                float t = u - 0.5;
                float sig = sign(t);
                float calc_u = lerp(0.5, cos(45)*sig, abs(t) * 2);
                float calc_v = lerp(l, 1, abs(t)*2);
                // float calc_v = lerp()
                u = lerp(0.5, calc_u, saturate(v));
                v = lerp(0, calc_v, saturate(v));
                float2x2 m = float2x2(cos(45), sin(45), -sin(45), cos(45));
                return mul(m, float2(u,v));
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _TexForward;
            sampler2D _TexBack;
            sampler2D _TexLeft;
            sampler2D _TexRight;
            float4 _TexForward_ST;
            float4 _TexBack_ST;
            float4 _TexLeft_ST;
            float4 _TexRight_ST;
            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _TexForward);
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed angle = 180 * 3.14 / 180.0;
                fixed angle_cw = 90 * 3.14 / 180.0;
                fixed angle2_ccw = -90 * 3.14 / 180.0;
                float2x2 m = float2x2(cos(angle), sin(angle), -sin(angle), cos(angle));
                float2x2 mcw = float2x2(cos(angle_cw), sin(angle_cw), -sin(angle_cw), cos(angle_cw));
                float2x2 mccw = float2x2(cos(angle2_ccw), sin(angle2_ccw), -sin(angle2_ccw), cos(angle2_ccw));
                fixed scale = _Scale;
                fixed4 col_fwd = tex2D(_TexForward, (i.uv * fixed2(scale, scale) + fixed2(-0.5, 0.0)));
                fixed4 col_left = tex2D(_TexLeft, (mul(mccw, i.uv * fixed2(scale, scale)) + fixed2(1.5, 0.0)));
                fixed4 col_right = tex2D(_TexRight, (mul(mcw, i.uv * fixed2(scale, scale)) + fixed2(-0.5, 2)));
                fixed4 col_back = tex2D(_TexBack, mul(m,i.uv*fixed2(scale, scale) + fixed2(-1.5, -2.0)));
                return fixed4(col_fwd.xyz, 1) + fixed4(col_back.xyz, 1) + fixed4(col_left.xyz, 1) + fixed4(col_right.xyz, 1);
            }
            ENDCG
        }
    }
}
