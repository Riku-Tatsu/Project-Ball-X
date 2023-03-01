Shader "Kamgam/BuiltIn/TriPlanar"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)

        _TopTex ("Top", 2D) = "white" {}

        _FrontTex("Front", 2D) = "white" {}

        _SideTex("Side", 2D) = "white" {}

        _BottomTex("Bottom", 2D) = "white" {}

        _Blend("Blend", Range(0,40)) = 5

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _TopTex;
        sampler2D _FrontTex;
        sampler2D _SideTex;
        sampler2D _BottomTex;

        float4 _TopTex_ST; 
        float4 _FrontTex_ST;
        float4 _SideTex_ST;
        float4 _BottomTex_ST;

        struct Input
        {
            float3 worldNormal;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        half _Blend;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        // Rotates the UVs by Rotation degrees.
        // Source: https://docs.unity3d.com/Packages/com.unity.shadergraph@15.0/manual/Rotate-Node.html
        void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
        {
            Rotation = Rotation * (3.1415926f / 180.0f);
            UV -= Center;
            float s = sin(Rotation);
            float c = cos(Rotation);
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix * 2 - 1;
            UV.xy = mul(UV.xy, rMatrix);
            UV += Center;
            Out = UV;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            ///*
            int signY = sign(IN.worldNormal.y);
            float3 p = pow(abs(IN.worldNormal), _Blend);
            float3 v = float3(1, 1, 1); 
            float3 blendNormal = p / dot(p, v);
            blendNormal.y *= signY;
            //*/
            //float3 blendNormal = IN.worldNormal;

            fixed4 top = tex2D(_TopTex, TRANSFORM_TEX(IN.worldPos.xz, _TopTex));
            fixed4 topColor = max(0, blendNormal.y * top);

            fixed4 bottom = tex2D(_BottomTex, TRANSFORM_TEX(IN.worldPos.xz, _BottomTex));
            fixed4 bottomColor = max(0, -blendNormal.y * bottom);

            fixed4 front = tex2D(_FrontTex, TRANSFORM_TEX(IN.worldPos.xy, _FrontTex));
            fixed4 frontColor = abs(blendNormal.z * front);

            float2 yz = 0;
            Unity_Rotate_Degrees_float(IN.worldPos.yz, 0.5, 270, yz); // rotate -90
            fixed4 side = tex2D(_SideTex, TRANSFORM_TEX(yz, _SideTex));
            fixed4 sideColor = abs(blendNormal.x * side);

            // Albedo comes from a texture tinted by color 
            fixed4 c = topColor + bottomColor + abs(frontColor) + abs(sideColor) * _Color;

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
