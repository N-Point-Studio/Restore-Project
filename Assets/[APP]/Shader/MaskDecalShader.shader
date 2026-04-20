Shader "Custom/MaskDecalShader"
{
    Properties
    {
        [Header(Main Texture Properties)]
        [MainColor] _BaseColor("Base Color", Color) = (0, 0, 0, 0)
        _MainTexture("Main Texture", 2D) = "Black" {}

        [Header(Brush Texture Properties)]
        [NoScaleOffset] _BrushTexture("Brush Texture", 2D) = "white" {}
        _BrushScale("Brush Scale", Range(0, 1)) = 0.5
        _BrushStrength("Brush Strength", Range(0, 1)) = 0.5
        _BrushHardness("Brush Hardness", Range(0, 1)) = 0.5

        [Header(Painting Properties)]
        _BrushPosition("Brush Position (UV)", Vector) = (0.5, 0.5, 0, 0)
        _PaintingColor("Painting Color", Color) = (1, 1, 1, 1)
        _PaintDirection("Paint Direction", Vector) = (0, -1, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Cull Off
            Blend One One

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float lightMapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            TEXTURE2D(_MainTexture);
            SAMPLER(sampler_MainTexture);

            TEXTURE2D(_BrushTexture);
            SAMPLER(sampler_BrushTexture);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _MainTexture_ST;

                float _BrushScale;
                float _BrushStrength;

                float2 _BrushPosition;
                float4 _PaintingColor;
                float3 _PaintDirection;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTexture);
                return OUT;
            }

            half4 GetBrushUV(float2 uv){
                float2 centeredUV = uv - _BrushPosition;
                centeredUV += 0.5;
                centeredUV -= 0.5;
                
                centeredUV /= _BrushScale;
                float2 brushUV = centeredUV + 0.5;

                half4 brush = SAMPLE_TEXTURE2D(_BrushTexture, sampler_BrushTexture, brushUV).r;

                float isInsideBrushX = step(0.0, brushUV.x) * step(brushUV.x, 1.0);
                float isInsideBrushY = step(0.0, brushUV.y) * step(brushUV.y, 1.0);
                brush.a *= isInsideBrushX * isInsideBrushY * _BrushStrength;

                return brush * _PaintingColor;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTexture, sampler_MainTexture, IN.uv) * _BaseColor;

                half4 brushColor = GetBrushUV(IN.uv);

                half3 finalColor = lerp(color.rgb, brushColor.rgb, brushColor.a);

                return half4(finalColor, color.a);
            }
            ENDHLSL
        }
    }
}