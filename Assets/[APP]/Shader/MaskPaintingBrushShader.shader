Shader "Custom/MaskPaintingBrushShader"
{
    Properties
    {
        _BrushTexture("Brush Texture", 2D) = "white" {}
        // [BARU] Properti untuk memutar brush
        _BrushRotation("Brush Rotation (Degrees)", Range(0, 360)) = 0 

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

            TEXTURE2D(_BrushTexture);
            SAMPLER(sampler_BrushTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _PaintPosition;
                float3 _PaintDirection;
                float _Radius;
                float _Hardness;
                float _NormalThreshold;
                float4 _CameraPosition; 
                float _BrushRotation; // [BARU]
            CBUFFER_END

            // ==========================================
            // HELPER FUNCTIONS
            // ==========================================

            float GetDistanceMask(float3 worldPos, float3 paintPos, float radius, float hardness) 
            {
                float dist = distance(worldPos, paintPos);
                return 1.0 - smoothstep(radius * hardness, radius, dist);
            }

            float GetDirectionMask(float3 normalWS, float3 paintDir, float threshold) 
            {
                float dotNormal = dot(normalWS, -normalize(paintDir));
                return smoothstep(threshold, threshold + 0.1, dotNormal);
            }

            float GetCameraFacingMask(float3 normalWS, float3 worldPos, float3 cameraPos) 
            {
                float3 viewDir = normalize(cameraPos - worldPos);
                return dot(normalWS, viewDir) > 0 ? 1.0 : 0.0; 
            }

            // [BARU] 4. Membuat UV lokal untuk Brush agar menempel dan berputar sesuai Normal
            float2 GetBrushUV(float3 worldPos, float3 paintPos, float3 normalWS, float radius, float rotationDeg) 
            {
                // Jarak dari titik tengah cat
                float3 offset = worldPos - paintPos;

                // Membangun sumbu lokal (Tangent dan Bitangent) berdasarkan arah Normal
                // Menggunakan trik "Up Vector" agar cross product tidak error saat normal menghadap tepat ke atas/bawah
                float3 up = abs(normalWS.y) > 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);
                float3 right = normalize(cross(up, normalWS));
                float3 forward = cross(normalWS, right);

                // Proyeksikan posisi 3D ke sumbu 2D brush kita
                float2 localUV = float2(dot(offset, right), dot(offset, forward));

                // Aplikasikan rotasi tambahan pada UV
                float rad = radians(rotationDeg);
                float s, c;
                sincos(rad, s, c);
                localUV = float2(
                    localUV.x * c - localUV.y * s,
                    localUV.x * s + localUV.y * c
                );

                // Normalisasikan ukuran UV agar 0..1 berdasarkan radius
                localUV /= (radius * 2.0);
                return localUV + 0.5;
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
                // [BARU] Dapatkan UV Brush yang sudah diproyeksikan dan dirotasi
                float2 brushUV = GetBrushUV(IN.worldPos, _PaintPosition.xyz, IN.normalWS, _Radius, _BrushRotation);
                
                // [BARU] Sample Teksturnya (mengambil channel .r, asumsikan tekstur grayscale/hitam putih)
                float brushTexMask = SAMPLE_TEXTURE2D(_BrushTexture, sampler_BrushTexture, brushUV).r;

                // Ambil sisa mask
                float distanceMask = GetDistanceMask(IN.worldPos, _PaintPosition.xyz, _Radius, _Hardness);
                float directionMask = GetDirectionMask(IN.normalWS, _PaintDirection, _NormalThreshold);
                float cameraMask = GetCameraFacingMask(IN.normalWS, IN.worldPos, _CameraPosition.xyz);

                // Kalikan semua mask, termasuk Brush Texture
                float finalPaint = distanceMask * directionMask * cameraMask * brushTexMask;
                
                return float4(finalPaint, finalPaint, finalPaint, 1.0);
            }
            ENDHLSL
        }
    }
}