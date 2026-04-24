Shader "Custom/PainterShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _PainterColor("Painter Color", Color) = (1,0,0,1)

        _PainterPosition("Painter Position", Vector) = (0,0,0,0)
        _Radius("Radius", Float) = 1.0
        _Hardness("Hardness", Float) = 0.5
        _Strength("Strength", Float) = 1.0

        _PrepareUV("Prepare UV", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "TexturePainterPass"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

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

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _PainterColor;
                float3 _PainterPosition;
                float _Radius;
                float _Hardness;
                float _Strength;
                float _PrepareUV;
            CBUFFER_END

            // 🔥 Mask function (sama kayak punya lo)
            float Mask(float3 position, float3 center, float radius, float hardness)
            {
                float dist = distance(position, center);
                float inner = radius * hardness;
                float falloff = smoothstep(inner, radius, dist);
                return 1.0 - falloff;
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                // world position
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldPos = worldPos;

                // UV
                OUT.uv = IN.uv;

                // fullscreen quad (sama logic lama lo)
                float2 clipPos = IN.uv * 2.0 - 1.0;
                clipPos.y *= _ProjectionParams.x;

                OUT.positionHCS = float4(clipPos, 0.0, 1.0);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // debug UV mode
                if (_PrepareUV > 0.5)
                {
                    return float4(0, 0, 1, 1);
                }

                // base texture
                float4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                // mask
                float m = Mask(IN.worldPos, _PainterPosition, _Radius, _Hardness);

                // strength
                float edge = m * _Strength;

                // blend
                float4 finalColor = lerp(baseColor, _PainterColor, edge);

                return finalColor;
            }

            ENDHLSL
        }
    }
}