Shader "Unlit/SkyGradient"
{
	Properties
	{
		_Color ("Top Color", Color) = (0.5,0.6,1,1)
		_ColorMid ("Horizon Color", Color) = (0.65,0.9,1,1)
		_ColorBtm ("Bottom Color", Color) = (0.85,0.75,0.6,1)
		_SunSize ("Sun Size", Range(0.02, 1)) = 0.1
		_SunSpread ("Sun Spread", Range(0.01, 0.2)) = 0.02
		_CloudTex ("Cloud Texture", 2D) = "black" {}
		_CloudColor ("Cloud Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		LOD 100

		Pass
		{
			Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			fixed4 _Color;
			fixed4 _ColorMid;
			fixed4 _ColorBtm;
			fixed _SunSize;
			fixed _SunSpread;
			sampler2D _CloudTex;
			fixed4 _CloudColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 dir : TEXCOORD0;
			};

			struct v2f
			{
				float3 dir : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.dir = v.dir;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed skyTop = (i.dir.y + 1) * 0.5;
				fixed skyBtm = saturate(-1 * (i.dir.y - 0.05));
				skyBtm = saturate(skyBtm / 0.02);

				fixed btmLerp = saturate(i.dir.y * 20);
				fixed3 finalSky = lerp(_Color, _ColorMid, saturate(1 - abs(i.dir.y) * 2));
				finalSky = lerp(_ColorBtm, finalSky, btmLerp);

				float3 sunVec = _WorldSpaceLightPos0 - i.dir;
				float dist = 1 - length(sunVec);

				fixed sunGradient = saturate(dist);
				fixed sunSpot = saturate((sunGradient - (1 - _SunSize)) / _SunSpread);


				float2 skyUV = atan2(i.dir.x, i.dir.z).xx;
				skyUV.x = (skyUV.x + 3.14159) / (3.14159 * 2);
				skyUV.y = (i.dir.y + 1) * 0.5;

				fixed timePan = _Time.y * 0.01;
				fixed timeRot = _Time.y * 0.075;

				fixed3 skyTex = tex2D(_CloudTex, skyUV + fixed2(timePan, 0));
				fixed3 skyTex2 = tex2D(_CloudTex, skyUV + fixed2(timePan * 0.5, timePan * 0.39714));

				float sinX = sin (timeRot);
            	float cosX = cos (timeRot);
            	float2x2 rotMx = float2x2( cosX, -sinX, sinX, cosX);
				fixed2 capUV_CW = mul(i.dir.xz, rotMx);

				sinX = sin (timeRot * 0.5);
            	cosX = cos (timeRot * 0.5);
				rotMx = float2x2( cosX, -sinX, sinX, cosX);
				fixed2 capUV_CCW = mul(i.dir.xz, rotMx);

				fixed3 skyCaps = tex2D(_CloudTex, capUV_CW * fixed2(0.25, 0.5));
				fixed3 skyCaps2 = tex2D(_CloudTex, capUV_CCW * fixed2(0.25, 0.5));

				fixed3 cloudSides = max(skyTex, skyTex2);
				fixed3 cloudCaps = max(skyCaps, skyCaps2);
				fixed capLerp = 1 - ((abs(i.dir.y) - 0.5) / 0.35);
				capLerp = 1 - saturate((abs(i.dir.y) - 0.2) * 1.25);

				fixed3 finalClouds = lerp(cloudCaps, cloudSides, capLerp);
				finalClouds = smoothstep(0, finalClouds, _CloudColor.a * 0.5) / max(0.01, _CloudColor.a);
				//finalClouds *= lerp(_LightColor0.rgb, _LightColor0.rgb * 0.25, sunGradient);

				fixed3 finalColor = max(finalSky, sunSpot);//lerp(finalSky, sunSpot * _LightColor0.rgb, sunSpot);
				finalColor = lerp(finalColor, _CloudColor.rgb, finalClouds * btmLerp);

				

				return fixed4(finalColor,1);
			}
			ENDCG
		}
	}
}
