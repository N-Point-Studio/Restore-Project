Shader "Custom/MaskPaintingShader"
{
    Properties
    {
        _PaintPosition("Paint Position (World)", Vector) = (0,0,0,0)
        _PaintDirection("Paint Direction", Vector) = (0,-1,0,0) 
        _Radius("Radius", Float) = 0.1
        _Hardness("Hardness", Float) = 0.5
        _NormalThreshold("Normal Threshold", Range(-1, 1)) = -0.2 
        
        _CameraPosition("Camera Position", Vector) = (0,0,0,0)
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
                float3 _PaintDirection;
                float _Radius;
                float _Hardness;
                float _NormalThreshold;
                float4 _CameraPosition; 
            CBUFFER_END

            // ==========================================
            // HELPER FUNCTIONS
            // ==========================================

            // 1. Menghitung mask berdasarkan jarak (radius & hardness)
            float GetDistanceMask(float3 worldPos, float3 paintPos, float radius, float hardness) 
            {
                float dist = distance(worldPos, paintPos);
                return 1.0 - smoothstep(radius * hardness, radius, dist);
            }

            // 2. Menghitung mask berdasarkan kecocokan arah normal dengan arah cat
            float GetDirectionMask(float3 normalWS, float3 paintDir, float threshold) 
            {
                float dotNormal = dot(normalWS, -normalize(paintDir));
                return smoothstep(threshold, threshold + 0.1, dotNormal);
            }

            // 3. Menghitung mask berdasarkan apakah permukaan menghadap kamera
            float GetCameraFacingMask(float3 normalWS, float3 worldPos, float3 cameraPos) 
            {
                float3 viewDir = normalize(cameraPos - worldPos);
                // Menggunakan logika cut-off tajam seperti sebelumnya
                return dot(normalWS, viewDir) > 0 ? 1.0 : 0.0; 
            }

            // ==========================================
            // MAIN FUNCTIONS
            // ==========================================

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
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target 
            {
                // Panggil masing-masing function
                float distanceMask = GetDistanceMask(IN.worldPos, _PaintPosition.xyz, _Radius, _Hardness);
                float directionMask = GetDirectionMask(IN.normalWS, _PaintDirection, _NormalThreshold);
                
                // Gunakan _WorldSpaceCameraPos dari Unity atau _CameraPosition bawaan properti kamu
                float cameraMask = GetCameraFacingMask(IN.normalWS, IN.worldPos, _CameraPosition.xyz);

                // Gabungkan semua mask
                float finalPaint = distanceMask * directionMask * cameraMask;
                
                return float4(finalPaint, finalPaint, finalPaint, 1.0);
            }
            ENDHLSL
        }
    }
}