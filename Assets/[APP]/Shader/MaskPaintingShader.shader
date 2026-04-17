Shader "Custom/MaskPaintingShader"
{
    Properties
    {
        _PaintPosition("Paint Position (World)", Vector) = (0,0,0,0)
        _PaintDirection("Paint Direction", Vector) = (0,-1,0,0) 
        _Radius("Radius", Float) = 0.1
        _Hardness("Hardness", Float) = 0.5
        _NormalThreshold("Normal Threshold", Range(-1, 1)) = -0.2 
        
        // [BARU] Tambahkan properti untuk posisi kamera
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
                // [BARU] Deklarasi variabel
                float4 _CameraPosition; 
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
                float dist = distance(IN.worldPos, _PaintPosition.xyz);
                float draw = 1.0 - smoothstep(_Radius * _Hardness, _Radius, dist);

                float dotNormal = dot(IN.normalWS, -normalize(_PaintDirection));
                float normalMask = smoothstep(_NormalThreshold, _NormalThreshold + 0.1, dotNormal);

                // [BARU] Hitung arah kamera ke pixel dan pastikan permukaannya menghadap kamera
                float3 viewDir = normalize(_CameraPosition.xyz - IN.worldPos);
                float cameraDot = dot(IN.normalWS, viewDir);
                
                // Jika cameraDot > 0, berarti menghadap kamera. Jika < 0, membelakangi kamera.
                // Kita gunakan smoothstep tipis agar transisinya tidak terlalu kasar di pinggiran.
                float cameraFacingMask = smoothstep(-0.05, 0.05, cameraDot);

                // Kalikan dengan cameraFacingMask
                float finalPaint = draw * normalMask * cameraFacingMask;
                
                return float4(finalPaint, finalPaint, finalPaint, 1.0);
            }
            ENDHLSL
        }
    }
}