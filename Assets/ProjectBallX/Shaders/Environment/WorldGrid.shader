Shader "Custom/WorldGrid"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Cutoff ("Texture Cutoff", Range(0,1)) = 0.25
		_NoiseTex ("Cutoff Noise", 2D) = "grey" {}
		_NoiseSize ("Noise Resolution", Float) = 64.0
		_HeightBtm ("Fade Start", Float) = -1.0
		_HeightTop ("Fade Height", Float) = 5.0
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
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _Cutoff;
			float _HeightBtm;
			float _HeightTop;
			sampler2D _NoiseTex;
			float _NoiseSize;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f v) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, v.uv);

				fixed heightFade = saturate((v.worldPos.y - _HeightBtm) / _HeightTop) * saturate(col.a - _Cutoff) * _Color.a;
				float2 screenUV = frac((v.screenPos.xy / v.screenPos.w) * _ScreenParams.xy * (1.0 / _NoiseSize));
				fixed noise = tex2D(_NoiseTex, screenUV);

				clip(heightFade - noise * 0.5);
				return _Color;
			}
			ENDCG
		}
	}
}
