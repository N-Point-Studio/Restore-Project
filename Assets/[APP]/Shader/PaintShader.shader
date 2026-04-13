Shader "Custom/PaintMask"
{
    Properties
    {
        _MainTex("Base Mask", 2D) = "black" {}
        _PaintPosition("Paint Position", Vector) = (0,0,0,0)
        _Radius("Radius", Float) = 0.01
        _Hardness("Hardness", Float) = 0.1
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
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _PaintPosition;
                float _Radius;
                float _Hardness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 🔹 ambil mask lama
                float oldMask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).r;

                // 🔹 hitung brush baru
                float dist = distance(IN.uv, _PaintPosition.xy);
                float brush = saturate(1.0 - dist / _Radius);
                brush = pow(brush, _Hardness);

                // 🔥 accumulate
                float finalMask = saturate(oldMask + brush);

                return float4(finalMask, finalMask, finalMask, 1);
            }

            ENDHLSL
        }
    }
}