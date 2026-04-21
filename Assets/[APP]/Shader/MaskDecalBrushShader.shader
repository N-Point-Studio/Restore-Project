Shader "Custom/MaskDecalBrushShader"
{
    Properties
    {
        [Header(Brush Texture Properties)]
        [NoScaleOffset] _BrushTexture("Brush Texture", 2D) = "white" {}
        _BrushScale("Brush Scale", Range(0, 1)) = 0.5
        _BrushStrength("Brush Strength", Range(0, 1)) = 0.5

        [Header(Painting Properties)]
        _BrushPosition("Brush Position (World)", Vector) = (0, 0, 0, 0)
        _PaintingColor("Painting Color", Color) = (1, 1, 1, 1)
        _PaintDirection("Paint Direction (Normal)", Vector) = (0, -1, 0, 0) 
        _ToolDirection("Tool Direction", Vector) = (0, 1, 0, 0)

        [Header(Mask Properties)]
        _MaskTexture("Mask Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Cull Off
            ZTest Always
            ZWrite Off
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
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            TEXTURE2D(_BrushTexture);
            SAMPLER(sampler_BrushTexture);

            TEXTURE2D(_MaskTexture);
            SAMPLER(sampler_MaskTexture);

            CBUFFER_START(UnityPerMaterial)
                float _BrushScale;
                float _BrushStrength;
                float3 _BrushPosition; 
                float4 _PaintingColor;
                float3 _PaintDirection; 
                float4 _ToolDirection;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                float2 uvClipSpace = IN.uv * 2.0 - 1.0;
                #if UNITY_UV_STARTS_AT_TOP
                uvClipSpace.y = -uvClipSpace.y;
                #endif
                
                OUT.positionHCS = float4(uvClipSpace, 0.0, 1.0);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 GetBrushUV(float3 worldPos)
            {
                float3 offset = worldPos - _BrushPosition;

                float3 tangent = normalize(cross(_PaintDirection, _ToolDirection));
                float3 bitangent = cross(_PaintDirection, tangent);

                float2 planarUV;
                planarUV.x = dot(offset, tangent);
                planarUV.y = dot(offset, bitangent);

                planarUV /= _BrushScale;
                float2 brushUV = planarUV + 0.5;

                float isInsideBrushX = step(0.0, brushUV.x) * step(brushUV.x, 1.0);
                float isInsideBrushY = step(0.0, brushUV.y) * step(brushUV.y, 1.0);
                float isInside = isInsideBrushX * isInsideBrushY;

                float depth = abs(dot(offset, _PaintDirection));
                float depthFade = step(depth, _BrushScale); 

                half brushAlpha = SAMPLE_TEXTURE2D(_BrushTexture, sampler_BrushTexture, brushUV).r;

                brushAlpha *= isInside * depthFade * _BrushStrength;

                return _PaintingColor * brushAlpha;
            }
            
            // half4 frag(Varyings IN) : SV_Target
            // {
            //     half4 brush = GetBrushUV(IN.worldPos);

            //     // sample mask (pakai UV mesh)
            //     float mask = SAMPLE_TEXTURE2D(_MaskTexture, sampler_MaskTexture, IN.uv).g;

            //     // normal facing check
            //     float facingStrength = dot(normalize(IN.normalWS), normalize(_PaintDirection)) > 0.1 ? 1.0 : 0.0;

            //     // apply mask
            //     brush *= mask;

            //     // return brush * facingStrength;
            //     return lerp(0, brush * facingStrength, mask);
            // }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 brush = GetBrushUV(IN.worldPos);
                half mask = SAMPLE_TEXTURE2D(_MaskTexture, sampler_MaskTexture, IN.uv).g;
                float facingStrength = dot(normalize(IN.normalWS), normalize(_PaintDirection)) > 0.1 ? 1.0 : 0.0;
                return GetBrushUV(IN.worldPos) * facingStrength * mask;
            }
            ENDHLSL
        }
    }
}