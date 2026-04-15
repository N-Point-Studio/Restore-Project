Shader "Custom/PaintShader"
{
    Properties
    {
        _MainTex("Base Mask", 2D) = "black" {}
        _BrushTex("Brush Texture", 2D) = "white" {}
        _PaintPosition("Paint Position", Vector) = (0,0,0,0)
        _Radius("Radius", Float) = 0.2
        _Hardness("Hardness", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
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
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_BrushTex);
            SAMPLER(sampler_BrushTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _PaintPosition;
                float _Radius;
                float _Hardness;
            CBUFFER_END

            float mask(float3 position, float3 center, float radius, float hardness)
            {
                float dist = distance(center, position);
                return 1 - smoothstep(radius * hardness, radius, dist);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = IN.uv;
                OUT.worldPos = worldPos;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 oldMask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // 🔥 UV relatif ke posisi klik
                float2 brushUV = (IN.uv - _PaintPosition.xy) / _Radius + 0.5;

                // 🔥 cek apakah masih dalam area brush
                float inside = step(0.0, brushUV.x) * step(0.0, brushUV.y) *
                            step(brushUV.x, 1.0) * step(brushUV.y, 1.0);

                float brush = SAMPLE_TEXTURE2D(_BrushTex, sampler_BrushTex, brushUV).r * inside;

                // hardness (optional)
                brush = pow(brush, _Hardness);

                // 🔥 gabung ke mask lama
                float finalMask = saturate(oldMask.r + brush);

                return float4(finalMask, finalMask, finalMask, 1);
            }

            // half4 frag(Varyings IN) : SV_Target
            // {
            //     // 🔹 ambil mask lama
            //     float4 oldMask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
            //     float4 brushMask = SAMPLE_TEXTURE2D(_BrushTex, sampler_BrushTex, IN.uv);

            //     float4 final = lerp(oldMask, float4(1,1,1,1), brushMask);

            //     float draw = pow(saturate(1 - distance(IN.uv, _PaintPosition.xy)), 100);

            //     float dist = distance(IN.uv, _PaintPosition.xy);
            //     float4 drawColor = float4(1, 1, 1, 1) * draw;

            //     return saturate(oldMask + drawColor);

            //     // 🔹 hitung brush baru
            //     // float dist = distance(IN.uv, _PaintPosition.xy);
            //     // float brush = saturate(1.0 - dist / _Radius);
            //     // brush = pow(brush, _Hardness);

            //     // // 🔥 accumulate
            //     // float finalMask = saturate(oldMask + brush);

            //     // return float4(finalMask, finalMask, finalMask, 1);
            // }

            ENDHLSL
        }
    }
}