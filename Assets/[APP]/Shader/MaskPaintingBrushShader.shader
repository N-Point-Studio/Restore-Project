Shader "Custom/MaskPaintingBrushShader"
{
    Properties
    {
        _PaintPosition("Paint Position (World)", Vector) = (0,0,0,0)
        _PaintDirection("Paint Direction", Vector) = (0,-1,0,0) 
        
        // --- PROPERTI ROTASI & BRUSH ---
        _ToolUp("Tool Up Direction", Vector) = (0,1,0,0) // Arah atas dari objek sikat
        _BrushTexture("Brush Texture", 2D) = "white" {}
        _Strength("Strength", Range(0, 1)) = 0.1
        // -------------------------------

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
                float3 _ToolUp; // Variabel penampung rotasi
                float _Radius;
                float _Hardness;
                float _NormalThreshold;
                float4 _CameraPosition; 
                float _Strength;
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

            // Memproyeksikan tekstur 2D sesuai arah objek sikat
            float GetBrushTextureMask(float3 worldPos, float3 paintPos, float3 paintDir, float3 toolUp, float radius)
            {
                float3 offset = worldPos - paintPos;

                // Gunakan toolUp asli dari objek sebagai patokan sumbu Y stempel
                float3 tangent = normalize(cross(toolUp, paintDir));
                float3 bitangent = cross(paintDir, tangent);

                float u = dot(offset, tangent);
                float v = dot(offset, bitangent);

                float2 brushUV = float2(u, v) / (radius * 2.0) + 0.5;

                // Potong batas luar agar tekstur brush tidak bocor/tiling
                if (brushUV.x < 0.0 || brushUV.x > 1.0 || brushUV.y < 0.0 || brushUV.y > 1.0)
                    return 0.0;

                float brushMask = SAMPLE_TEXTURE2D_LOD(_BrushTexture, sampler_BrushTexture, brushUV, 0).r;
                return brushMask;
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
                float distanceMask = GetDistanceMask(IN.worldPos, _PaintPosition.xyz, _Radius, _Hardness);
                float directionMask = GetDirectionMask(IN.normalWS, _PaintDirection, _NormalThreshold);
                float cameraMask = GetCameraFacingMask(IN.normalWS, IN.worldPos, _CameraPosition.xyz);
                
                // Masukkan parameter _ToolUp ke dalam fungsi
                float textureMask = GetBrushTextureMask(IN.worldPos, _PaintPosition.xyz, _PaintDirection, _ToolUp, _Radius);

                float finalPaint = distanceMask * directionMask * cameraMask * textureMask * _Strength;
                
                return float4(finalPaint, finalPaint, finalPaint, 1.0);
            }
            ENDHLSL
        }
    }
}