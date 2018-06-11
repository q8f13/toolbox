Shader "Custom/CelShadingForwardWorldYClip" {
	Properties {
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_MaskY("MaskY", float) = 0
	}
	SubShader {
		Tags {
			"RenderType" = "Opaque"
		}

		ZWrite On
		Cull Off
		//ZTest GEqual

		LOD 200

		CGPROGRAM
			#pragma surface surf CelShadingForward
			#pragma target 3.0

		half4 LightingCelShadingForward(SurfaceOutput s, half3 lightDir, half atten) {
			half NdotL = dot(s.Normal, lightDir);
			if (NdotL <= 0.0) 
				NdotL = 0;
			else if(NdotL < 0.85)
				NdotL = 0.85;
			else NdotL = 1;
			smoothstep(0, 0.025f, NdotL);
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten * 2);
			c.a = s.Alpha;
			return c;
		}

		sampler2D _MainTex;
		fixed4 _Color;
		float _MaskY;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			clip(_MaskY - IN.worldPos.y);
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG

	}
	FallBack "Diffuse"
}
