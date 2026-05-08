Shader "Custom/PaintShaderWorldSpace_WithRayCulling"
{
    Properties
    {
        _PaintPosition("Paint Position (World)", Vector) = (0,0,0,0)
        // TAMBAHKAN INI: Arah tembakan raycast dari kamera
        _PaintDirection("Paint Direction", Vector) = (0,-1,0,0) 
        _Radius("Radius", Float) = 0.1
        _Hardness("Hardness", Float) = 0.5
        // Ubah range threshold menjadi lebih longgar
        _NormalThreshold("Normal Threshold", Range(-1, 1)) = -0.2 
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Cull Off
        ZTest Always
        ZWrite Off
        Blend One One 

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _PaintPosition;
                float3 _PaintDirection; // Variabel baru
                float _Radius;
                float _Hardness;
                float _NormalThreshold;
            CBUFFER_END

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

                float2 uvClipSpace = IN.uv * 2.0 - 1.0;
                #if UNITY_UV_STARTS_AT_TOP
                uvClipSpace.y = -uvClipSpace.y;
                #endif
                OUT.positionHCS = float4(uvClipSpace, 0.0, 1.0);
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target {
                // 1. Kalkulasi Jarak 3D
                float dist = distance(IN.worldPos, _PaintPosition.xyz);
                float draw = 1.0 - smoothstep(_Radius * _Hardness, _Radius, dist);

                // 2. REVISI KALKULASI NORMAL
                // Bandingkan normal permukaan dengan arah TERBALIK dari sorotan kamera/raycast.
                // Jika raycast menembak ke depan (Z+), kita cek permukaan yang menghadap ke belakang (Z-).
                float dotNormal = dot(IN.normalWS, -normalize(_PaintDirection));

                // Jika dotNormal = 1 (menghadap tegak lurus ke kamera)
                // Jika dotNormal = 0 (menghadap ke samping/serong, tapi masih terlihat)
                // Jika dotNormal = -1 (menghadap membelakangi kamera)
                
                float normalMask = smoothstep(_NormalThreshold, _NormalThreshold + 0.1, dotNormal);

                float finalPaint = draw * normalMask;
                return float4(finalPaint, finalPaint, finalPaint, 1.0);
            }
            ENDHLSL
        }
    }
}