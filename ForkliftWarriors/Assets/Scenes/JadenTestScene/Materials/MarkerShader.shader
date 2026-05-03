Shader "Custom/markershader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _FlipUV("Flip Vertical UV (0 = off, 1 = on)", Range(0,1)) = 0
        _TwoSided("Two Sided (0 = off, 1 = on)", Range(0,1)) = 0
        [Enum(Off,Front,Back)] _CullMode("Cull Mode (use Off for two-sided)", Int) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        // Control face culling using the property below. Set to 0 (Off) for two-sided rendering.
        Cull [_CullMode]
        // Transparent objects should not write depth
        ZWrite Off

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _FlipUV;
                float _TwoSided;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample texture and apply base color
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                // Optionally flip the vertical UV coordinate based on _FlipUV.
                float uvY = lerp(IN.uv.y, 1.0 - IN.uv.y, _FlipUV);

                // Create a vertical gradient: top (uvY = 1) -> clear (alpha = 0), bottom (uvY = 0) -> opaque (alpha = 1)
                half gradientAlpha = saturate(1.0 - uvY);

                // Multiply alpha by gradient and ensure it respects base color alpha
                color.a = color.a * gradientAlpha;

                return color;
            }
            ENDHLSL
        }
    }
}