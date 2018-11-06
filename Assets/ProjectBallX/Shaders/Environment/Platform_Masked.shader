Shader "Custom/Platform_Masked" {
	Properties {
		_MainTex ("Channel Mask", 2D) = "white" {}
		_NormalTex("Normal Map", 2D) = "bump" {}

		_Color ("Color Zero", Color) = (0.25,0.25,0.25,1)
		_Specular ("Specular Zero", Color) = (0.1,0.1,0.1,0.5)

		_ColorRed ("Color Red", Color) = (0.5,0.25,0.25,1)
		_SpecularRed ("Specular Red", Color) = (0.1,0.1,0.1,0.5)

		_ColorGreen ("Color Green", Color) = (0.25,0.5,0.25,1)
		_SpecularGreen ("Specular Green", Color) = (0.1,0.1,0.1,0.5)
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		fixed4 _Color;
		fixed4 _Specular;
		fixed4 _ColorRed;
		fixed4 _SpecularRed;
		fixed4 _ColorGreen;
		fixed4 _SpecularGreen;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input v, inout SurfaceOutputStandardSpecular o) {
			
			fixed4 mask = tex2D (_MainTex, v.uv_MainTex);
			mask.rg = max(mask.rg, v.color.rg);
			mask.b = min(mask.b, v.color.b);
			float3 normal = UnpackNormal(tex2D(_NormalTex, v.uv_MainTex));

			fixed3 diffuse = lerp(lerp(_Color, _ColorRed, mask.r), _ColorGreen, mask.g);
			fixed4 specular = lerp(lerp(_Specular, _SpecularRed, mask.r), _SpecularGreen, mask.g);

			float normalFix = 1 - saturate((1 - normal.b) / 0.01);

			//o.Emission = lerp(normal, float3(0,0,1), normalFix);

			o.Albedo = diffuse * mask.b;
			o.Normal = lerp(normal, float3(0,0,1), normalFix);
			
			o.Specular = specular.rgb * mask.b;
			o.Smoothness = specular.a;

			o.Occlusion = mask.b;

			o.Emission = diffuse * mask.b;

		}
		ENDCG
	}
	FallBack "Diffuse"
}
