// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Projector/TatooProjector"
 {
     Properties
     {
         _ShadowTex ("Cookie", 2D) = "white" { TexGen ObjectLinear }
         _Color ("Color", Color) = (1,1,1,1)
         _SpeedX ("SpeedX", float) = 0.0
         _SpeedZ ("SpeedZ", float) = 0.0
        _ColorScale ("Color Scale", float) = 1.0
     }
    
     Subshader
     {
         Tags { "RenderType"="Transparent"  "Queue"="Transparent+100"}
         Pass
         {
             ZWrite Off
             Offset -1, -1
    
             Fog { Mode Off }
    
             ColorMask RGB
             Blend SrcAlpha OneMinusSrcAlpha
            //  Blend OneMinusSrcAlpha SrcAlpha
            // BlendOp Add
 
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma fragmentoption ARB_fog_exp2
             #pragma fragmentoption ARB_precision_hint_fastest
             #include "UnityCG.cginc"
            
             struct v2f
             {
                 float4 pos      : SV_POSITION;
                 float4 uv       : TEXCOORD0;
             };
            
            sampler2D _ShadowTex;
            float4 _ShadowTex_ST;
            float4x4 unity_Projector;
            float4 _Color;
            float _ColorScale;

            float _SpeedX;
            float _SpeedZ;
            
             v2f vert(appdata_tan v)
             {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                //  float4 uv_pre = mul (unity_Projector, v.vertex); 
                // float4x4 mat = unity_Projector;
                // mat[3] += _ShadowTex_ST.z;
                // mat[11] += _ShadowTex_ST.w;
                // float4x1 tri = float4x1(_SpeedX * _Time.y, 0, _SpeedZ * _Time.y, 1);
                // float4x4 mat = mul(tri, unity_Projector);
                // mat[13] += _ShadowTex_ST.z;
                // mat[15] += _ShadowTex_ST.w;
                // o.uv = mul(unity_CameraProjection, v.vertex); 
                // o.uv = mul (unity_Projector, worldPos); 
                o.uv = mul (unity_Projector, v.vertex); 
                // o.uv = mul (unity_Projector, offset_pos); 
                // o.uv = mul(tri, o.uv);
                // o.uv = mul (unity_Projector, v.vertex); 
                // o.uv = mul(unity_Projector, float4(v.vertex.x + _SpeedX * _Time.y, v.vertex.z + _SpeedZ * _Time.y, v.vertex.zw)); return o;
                return o;
             }
            
             half4 frag (v2f i) : COLOR
             {
                // float2 offset = _ShadowTex_ST.zw;
                float2 offset = float2(_SpeedX * _Time.y, _SpeedZ * _Time.y);
                half4 tex = tex2Dproj(_ShadowTex, float4(i.uv.xy * _ShadowTex_ST.xy + offset, i.uv.zw));
                //  half4 tex = tex2Dproj(_ShadowTex, i.uv);
                tex.a = 1-tex.a;
                if (i.uv.w < 0)
                {
                    tex = float4(0,0,0,1);
                }
                // tex = clamp(tex, (1.0 - _Filter), 1.0);
                float val = saturate(tex.r * _ColorScale);
                fixed4 col = fixed4(_Color.rgb, _Color.a * val);
                // fixed4 col = fixed4((fixed3(1.0, 1.0, 1.0) - tex.rgb), val);
                return col;
                //  return tex;
             }
             ENDCG
        
         }
     }
 }