Shader "Unlit/RepeatFix"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PatternImg ("Texture", 2D) = "white" {}
		_BlendOne ("Blend Method One", Range(0.0, 1.0)) = 0.0
		_BlendTwo ("Blend Method Two", Range(0.0, 1.0)) = 0.0
		_BlendThree ("Blend Method Three", Range(0.0, 1.0)) = 0.0
		_BlendThreeStep ("Blend Three Steps", float) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"}
		LOD 100

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
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			fixed4 hash4( fixed2 p ) { return frac(sin(fixed4( 1.0+dot(p,fixed2(37.0,17.0)),
                                              2.0+dot(p,fixed2(11.0,47.0)),
                                              3.0+dot(p,fixed2(41.0,29.0)),
                                              4.0+dot(p,fixed2(23.0,31.0))))*103.0); }

			sampler2D _PatternImg;

			float sum( fixed3 v ) { return v.x+v.y+v.z; }

			float4 textureNoTileThree( sampler2D samp, in fixed2 uv, float v)
			// float4 textureNoTileThree( in fixed2 uv, float v )
			{
				// sample variation pattern    
				float k = tex2D(_PatternImg, 0.005*uv ).x; // cheap (cache friendly) lookup    
				// float k = tex2D( _PatternImg, 0.005*x ).x; // cheap (cache friendly) lookup    
				
				// compute index    
				float index = k*8.0;
				float i = floor( index );
				float f = frac( index );

				// offsets for the different virtual patterns    
				float2 offa = sin(float2(3.0,7.0)*(i+0.0)); // can replace with any other hash    
				float2 offb = sin(float2(3.0,7.0)*(i+1.0)); // can replace with any other hash    

				// compute derivatives for mip-mapping    
				fixed2 dx = ddx(uv), dy = ddy(uv);
				// fixed2 dx = dFdx(x), dy = dFdy(x);
				
				// sample the two closest virtual patterns    
				float3 cola = tex2Dgrad( samp, uv + v * offa, dx, dy ).xyz;
				float3 colb = tex2Dgrad( samp, uv + v * offb, dx, dy ).xyz;

				// interpolate between the two virtual patterns    
				return float4(lerp( cola, colb, lerp(0.2,0.8,f-0.1*sum(cola-colb)) ), 1.0);
			}


			float4 textureNoTileTwo( sampler2D samp, in float2 uv )
			{
				float2 p = floor( uv );
				float2 f = frac( uv );
				
				// derivatives (for correct mipmapping)
				float2 _ddx = ddx( uv );
				float2 _ddy = ddy( uv );
				
				// voronoi contribution
				fixed4 va = fixed4( 0.0, 0.0, 0.0, 0.0 );
				float wt = 0.0;
				for( int j=-1; j<=1; j++ )
				for( int i=-1; i<=1; i++ )
				{
					fixed2 g = fixed2( float(i), float(j) );
					fixed4 o = hash4( p + g );
					fixed2 r = g - f + o.xy;
					float d = dot(r,r);
					float w = exp(-5.0*d );
					fixed4 c = tex2Dgrad( samp, uv + o.zw, _ddx, _ddy );
					va += w*c;
					wt += w;
				}
				
				// normalization
				return va/wt;
			}

			float4 textureNoTileOne( sampler2D samp, in float2 uv )
			{
				int2 iuv = int2( floor( uv ) );
				half2 fuv = frac( uv );

				// generate per-tile transform
				float4 ofa = hash4( iuv + int2(0,0) );
				float4 ofb = hash4( iuv + int2(1,0) );
				float4 ofc = hash4( iuv + int2(0,1) );
				float4 ofd = hash4( iuv + int2(1,1) );
				
				float2 _ddx = ddx( uv );
				// float2 ddx = dFdx( uv );
				float2 _ddy = ddy( uv );

				// transform per-tile uvs
				ofa.zw = sign( ofa.zw-0.5 );
				ofb.zw = sign( ofb.zw-0.5 );
				ofc.zw = sign( ofc.zw-0.5 );
				ofd.zw = sign( ofd.zw-0.5 );
				
				// uv's, and derivatives (for correct mipmapping)
				fixed2 uva = uv*ofa.zw + ofa.xy, ddxa = _ddx*ofa.zw, ddya = _ddy*ofa.zw;
				fixed2 uvb = uv*ofb.zw + ofb.xy, ddxb = _ddx*ofb.zw, ddyb = _ddy*ofb.zw;
				fixed2 uvc = uv*ofc.zw + ofc.xy, ddxc = _ddx*ofc.zw, ddyc = _ddy*ofc.zw;
				fixed2 uvd = uv*ofd.zw + ofd.xy, ddxd = _ddx*ofd.zw, ddyd = _ddy*ofd.zw;
					
				// fetch and blend
				float2 b = smoothstep( 0.25,0.75, fuv );
				
				return lerp( lerp( tex2Dgrad( samp, uva, ddxa, ddya ), 
								 tex2Dgrad( samp, uvb, ddxb, ddyb ), b.x ), 
							lerp( tex2Dgrad( samp, uvc, ddxc, ddyc ),
								 tex2Dgrad( samp, uvd, ddxd, ddyd ), b.x), b.y );
			}

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _BlendOne;
			float _BlendTwo;
			float _BlendThree;
			float _BlendThreeStep;
			
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
				// sample the texture
				fixed3 col_I = textureNoTileOne(_MainTex, i.uv).xyz;
				fixed3 col_II = textureNoTileTwo(_MainTex, i.uv).xyz;
				fixed3 col_III = textureNoTileThree(_MainTex, i.uv, _BlendThreeStep).xyz;
				// fixed3 cola = textureNoTileOne(_MainTex, i.uv).xyz;
				fixed3 colb = tex2D(_MainTex, i.uv).xyz;

				fixed3 col = lerp(colb, col_I, _BlendOne);
				col = lerp(col, col_II, _BlendTwo);
				col = lerp(col, col_III, _BlendThree);
				return fixed4(col, 1.0);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
			}
			ENDCG
		}
	}
}
