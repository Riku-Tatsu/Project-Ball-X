Shader "Custom/PlayerBall" {
	Properties {
		_Color ("Primary Color", Color) = (1,1,1,1)
		_SpecularPrimary ("Primary Reflectivity", Color) = (0.25,0.25,0.25,1)

		_ColorSecondary ("Secondary Color", Color) = (0.1,0.1,0.1,1)
		_SpecularSecondary ("Secondary Reflectivity", Color) = (0.5,0.5,0.5,0.75)

		_EmisInsideBase ("Emissive Base - Inside", Color) = (0.5,0.25,0,1)
		_EmisOutsideBase ("Emissive Base - Outside", Color) = (0.25,0.125,0,1)
		[HDR]_EmisInsideGlow ("Emissive Glow - Inside", Color) = (1,0.75,0.25,1)
		[HDR]_EmisOutsideGlow ("Emissive Glow - Outside", Color) = (0.5,0.375,0.125,1)
		_PulseSpeed ("Pulse Line Speed", Float) = 0.25
		_GlowLerp("Glow Lerp", Range(0,1)) = 0

		//needed because unity is stupid
		[HideInInspector][NoScaleOffset] _Zero ("LEAVE BLANK KTHX", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _Zero;
		float2 uv_Zero;

		struct Input {
			float4 color : COLOR;
			float3 worldPos;
			float3 worldNormal;
			float3 viewDir;
			float2 uv_Zero;
		};

		fixed4 _Color;
		fixed4 _SpecularPrimary;
		fixed4 _ColorSecondary;
		fixed4 _SpecularSecondary;
		
		fixed4 _EmisInsideBase;
		fixed4 _EmisOutsideBase;
		fixed4 _EmisInsideGlow;
		fixed4 _EmisOutsideGlow;
		float _PulseSpeed;
		fixed _GlowLerp;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input v, inout SurfaceOutputStandardSpecular o) {

			fixed fresnel = saturate(1.33 - length(v.viewDir - v.worldNormal));
			fixed pulseUV = 1 - length((v.uv_Zero - 0.5) * 2);

			fixed pulseLines = abs(sin((pulseUV + _Time.y * _PulseSpeed) * 4 * 3.1415));
			pulseLines *= pulseLines * pulseLines;
			pulseLines = lerp(0, 1, pulseLines);
			pulseLines = pulseLines * saturate(fresnel * 2) + saturate((fresnel - 0.125) * 2);
			pulseLines = pow(pulseLines, lerp(1, 2, _GlowLerp));

			fixed3 EmisColor = lerp(lerp(_EmisOutsideBase, _EmisOutsideGlow, _GlowLerp), lerp(_EmisInsideBase, _EmisInsideGlow, _GlowLerp), pulseLines);

			o.Albedo = lerp(_Color, _ColorSecondary, v.color.r);
			o.Specular = lerp(_SpecularPrimary.rgb, _SpecularSecondary.rgb, v.color.r);
			o.Smoothness = lerp(_SpecularPrimary.a, _SpecularSecondary.a, v.color.r);

			o.Occlusion = v.color.b;
			o.Emission = lerp(0, EmisColor, v.color.g) * v.color.b;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
