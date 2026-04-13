Shader "Custom/ExtendIsland"
{
    Properties
    {
        [MainTexture] _BaseMap ("Texture", 2D) = "white" {}
        _UVIslands ("Texture UV Islands", 2D) = "white" {}
        _OffsetUV ("UV Offset", Float) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ExtendIslandsPass"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

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

            TEXTURE2D(_UVIslands);
            SAMPLER(sampler_UVIslands);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseMap_TexelSize;
                float _OffsetUV;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                float4 island = SAMPLE_TEXTURE2D(_UVIslands, sampler_UVIslands, uv);

                // 🔥 offsets (8 arah)
                float2 texel = _BaseMap_TexelSize.xy * _OffsetUV;

                float2 offsets[8] =
                {
                    float2(-texel.x, 0),
                    float2(texel.x, 0),
                    float2(0, texel.y),
                    float2(0, -texel.y),
                    float2(-texel.x, texel.y),
                    float2(texel.x, texel.y),
                    float2(texel.x, -texel.y),
                    float2(-texel.x, -texel.y)
                };

                // kalau bukan island → extend
                if (island.z < 1.0)
                {
                    float4 extendedColor = color;

                    [unroll]
                    for (int j = 0; j < 8; j++)
                    {
                        float2 currentUV = uv + offsets[j];
                        float4 sampleCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, currentUV);

                        extendedColor = max(sampleCol, extendedColor);
                    }

                    color = extendedColor;
                }

                return color;
            }

            ENDHLSL
        }
    }
}