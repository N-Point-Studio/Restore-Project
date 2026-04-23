Shader "Custom/MaskShader"
{
    Properties
    {
        // BARU: Tambahkan properti Mask Texture di sini
        _MaskTexture("Mask Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normal: NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                
                // PERBAIKAN: Ubah jadi float3 karena TransformObjectToWorld mengembalikan nilai float3 (x,y,z)
                float3 worldPos : TEXCOORD2; 
            };

            // BARU: Deklarasi Texture dan Sampler agar bisa dibaca oleh HLSL
            TEXTURE2D(_MaskTexture);
            SAMPLER(sampler_MaskTexture);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normal = TransformObjectToWorldNormal(IN.normal);
                
                float2 uvClipSpace = IN.uv * 2.0 - 1.0;
                #if UNITY_UV_STARTS_AT_TOP
                uvClipSpace.y = -uvClipSpace.y;
                #endif
                
                OUT.positionHCS = float4(uvClipSpace, 0.0, 1.0);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half maskValue = SAMPLE_TEXTURE2D(_MaskTexture, sampler_MaskTexture, IN.uv).g;
                return half4(1.0, 1.0, 1.0, 1.0) * maskValue;
            }
            ENDHLSL
        }
    }
}