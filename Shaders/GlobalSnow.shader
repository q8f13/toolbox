Shader "Custom/GlobalSnow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SnowTex ("Snow Texture", 2D) = "white" {}
		_DebugColor ("DebugColor", Color) = (1,1,1,1)

		_SnowUVTop ("Snow Topdown UV Top", Vector) = (0,0,0)
		_SnowUVBottom ("Snow Topdown UV Bottom", Vector) = (0,0,0)

		[PowerSlider(1.0)]_DotLimit("Dot Limit", Range(0.0, 1.0)) = 1.0

		[PowerSlider(10.0)]_Repeatance("Repeatance", Range(0.0, 10.0)) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		ZTest always Cull Off ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				half2 uv_depth : TEXCOORD1;
				// float3 worldPos : TEXCOORD1;
				float4 interpolate_ray : TEXCOORD2;
			};

			sampler2D _MainTex;
			sampler2D _SnowTex;
			float4 _SnowTex_ST;
			fixed4 _DebugColor;
			float4 _MainTex_ST;
			float4x4 _ViewToWorld;

			float4 _FrustrumPoints;
			float _CameraFar;

			float4x4 _FrustumCornersRay;

			float _Repeatance;
			float _DotLimit; 

			// sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;

			// 1. Take the screen space position uv and find where it lies on the rectangle on the far clip plane. This is a ray from the camera to the far clip plane.
			// 2. Then we multiply the z position by the depth to find where that point lies on a ray. We have to multiple the x and y coordinates by depth if it's perspective.
			// The _FrustrumPoints are calculated in the code
			float3 CalcVS(float2 screenPos, float depth)
			{
				float2 lerpVal = screenPos;
				float3 ray = float3(
				_FrustrumPoints.x * lerpVal.x + _FrustrumPoints.y * (1 - lerpVal.x),
				_FrustrumPoints.z * lerpVal.y + _FrustrumPoints.w * (1 - lerpVal.y),
				_CameraFar); // I think _ProjectionParams.z will also work
			
				#ifdef PERSPECTIVE
					float3 posVS = float3(ray.xy * depth, ray.z * -depth);
				#else
					float3 posVS = float3(ray.xy, ray.z * -depth);
				#endif
				return posVS;
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv_depth = TRANSFORM_TEX(v.uv, _MainTex);

				#if UNITY_UV_STARTS_AT_TOP
				if(_MainTex_ST.y < 0)
					o.uv_depth.y = 1 - o.uv_depth.y;
				#endif

				int index = 0;
				if(o.uv.x < 0.5 && o.uv.y < 0.5){
					index = 0;
				}else if(o.uv.x > 0.5 && o.uv.y < 0.5){
					index = 1;
				}else if(o.uv.x > 0.5 && o.uv.y > 0.5){
					index = 2;
				}else{
					index = 3;
				}

				#if UNITY_UV_STARTS_AT_TOP
				if(_MainTex_ST.y < 0)
					index = 3 - index;
				#endif

				o.interpolate_ray = _FrustumCornersRay[index];

				// o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				// o.scrPos = ComputeScreenPos(o.vertex);
				// UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal;
				// float4x4 view_transpose = transpose(UNITY_MATRIX_V);
				float depth;
				// float3 up_world_view = mul(unity_WorldToObject, float3(0,1,0));
				// float3 up_world_view = float3(0,1,0);
				DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv),depth, normal);
				// float3 normal = DecodeFloatRG(tex2D(_CameraDepthNormalsTexture, i.uv).xy);
				// float3 normal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv));
				// float3 world_normal = normalize(mul(view_transpose, float4(normal, 0)).xyz);
				float3 worldPos = _WorldSpaceCameraPos + depth * i.interpolate_ray.xyz;
				float3 world_normal = normalize(mul(_ViewToWorld, float4(normal, 0)).xyz);
				float up_val = saturate(dot(world_normal, float3(0,1.0,0))) * _DotLimit;
				// float depth_limit = (1.0 - saturate(clamp(depth, 0.0, 100)));
				float2 snow_uv = worldPos.xz;
				fixed4 snow_col = tex2D(_SnowTex, snow_uv);
				// fixed4 col = tex2D(_MainTex, i.uv) * (1.0 - up_val) + snow_col * up_val;
				fixed4 col = lerp(tex2D(_MainTex, i.uv) , snow_col , up_val);
				// fixed4 col = fixed4(snow_uv.x, 0, snow_uv.y, 1.0);
				// fixed3 col_single = fixed3(_DebugColor.xyz * up_depth_limit);
				// fixed4 col = fixed4(col_single,1.0);
				return col;
			}
			ENDCG
		}
	}

	Fallback Off
}
