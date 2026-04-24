Shader "Custom/Paint"
{
    Properties
    {
        _PaintPositionWS("Paint Position", Vector) = (0, 0, 0, 0)
        _PaintNormalWS("Paint Normal", Vector) = (0, 0, 0, 0)
        _SpreadRadius("Spread Radius", Float) = 0
        _PaintColor("Paint Color", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "Universal Forward"
            // Penting: Matikan Cull agar UV unwrap tidak memotong face yang menghadap belakang di UV space
            Cull Off
            ZWrite Off
            ZTest Always
            
            // Blending: Tambahkan warna alpha kuas di atas coretan yang sudah ada
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 lightMapUV   : TEXCOORD1;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float3 _PaintPositionWS;
                float3 _PaintNormalWS;
                float _SpreadRadius;
                float4 _PaintColor;
            CBUFFER_END
            
            Varyings vert(Attributes i)
            {
                Varyings v;
                
                // Simpan posisi dan normal dunia aslinya untuk dicek di Fragment Shader
                v.positionWS = TransformObjectToWorld(i.positionOS.xyz);
                v.normalWS = TransformObjectToWorldNormal(i.normalOS);
                
                // PERBAIKAN: Petakan UV langsung ke Clip Space (-1 sampai 1)
                float2 uv = i.lightMapUV;
                
                // Sesuaikan posisi Y agar tidak terbalik antara Direct3D/OpenGL
                #if UNITY_UV_STARTS_AT_TOP
                uv.y = 1.0 - uv.y;
                #endif
                
                // Jangan gunakan View/Projection Matrix dari Kamera!
                v.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
                
                return v;
            }
            
            float4 frag(Varyings i) : SV_Target0
            {
                float dist = distance(i.positionWS, _PaintPositionWS);
                
                // Brush halus dengan radius
                float positionStrength = 1.0 - saturate(dist / _SpreadRadius);
                
                // Pastikan kita hanya mengecat face yang permukaannya searah dengan normal hantaman (mencegah cat tembus ke belakang)
                float facingStrength = dot(normalize(i.normalWS), normalize(_PaintNormalWS)) > 0.1 ? 1.0 : 0.0;
                
                // Alpha (opacity) dari cat
                float alpha = positionStrength * facingStrength;
                
                return float4(_PaintColor.rgb, alpha);
            }
            
            ENDHLSL
        }
    }
}