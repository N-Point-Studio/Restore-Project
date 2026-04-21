Shader "Custom/MaskShader"
{
    Properties
    {
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
                float worldPos : TEXCOORD2;
            };

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
                return (1,1,1,1);
            }
            ENDHLSL
        }
    }
}
