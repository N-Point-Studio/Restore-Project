Shader "Custom/PaintShaderWorldSpace"
{
    Properties
    {
        _PaintPosition("Paint Position (World)", Vector) = (0,0,0,0)
        _Radius("Radius", Float) = 0.1
        _Hardness("Hardness", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        // PENTING: Matikan culling karena proses unwrapping bisa membalikkan arah normal/face
        Cull Off
        ZTest Always
        ZWrite Off
        
        // Gunakan Additive Blending agar brush stroke menumpuk seperti spidol
        Blend One One 

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
                float3 worldPos : TEXCOORD0; // Kita oper WorldPos ke Fragment
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _PaintPosition;
                float _Radius;
                float _Hardness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // 1. Ambil posisi 3D ASLI di dunia nyata (sebelum dipipihkan)
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);

                // 2. TRIK UNWRAPPING: Ubah koordinat UV [0, 1] menjadi koordinat Layar [-1, 1]
                float2 uvClipSpace = IN.uv * 2.0 - 1.0;

                // Sesuaikan arah Y untuk Direct3D / Metal (Penting untuk Mac/iOS dev)
                #if UNITY_UV_STARTS_AT_TOP
                uvClipSpace.y = -uvClipSpace.y;
                #endif

                // Paksa vertex agar tergambar di layar secara 2D berdasarkan UV-nya
                OUT.positionHCS = float4(uvClipSpace, 0.0, 1.0);

                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                // Hitung jarak 3D antara piksel mesh ini dengan titik tengah brush (dari Raycast)
                float dist = distance(IN.worldPos, _PaintPosition.xyz);

                // Cek apakah masuk dalam radius brush (dengan hardness)
                float draw = 1.0 - smoothstep(_Radius * _Hardness, _Radius, dist);

                // Kembalikan warna putih di area yang di-brush. 
                // Karena 'Blend One One', ini otomatis tertambah ke RenderTexture!
                return float4(draw, draw, draw, 1.0);
            }

            ENDHLSL
        }
    }
}