Shader "Custom/MaskingShader"
{
    Properties
    {
        [NoScaleOffset] _MainTex("Mask Map", 2D) = "white" { }
        _HealthBar("Health Bar", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Zwrite Off
            Blend SrcAlpha OneMinusSrcAlpha

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

            sampler2D _MainTex;
            float _HealthBar;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float InverseLerp(float a, float b, float v){
                return (v - a) / (b - a);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // return float4(1, 0, 0, IN.uv.x);
                float healthBarMask = _HealthBar > IN.uv.x;
                // clip(healthBarMask - 0.5);
                // float tHealthColor = saturate(InverseLerp(0.2, 0.8, _HealthBar));

                // float3 healthBarColor = lerp(float3(1, 0, 0), float3(0, 1, 0), tHealthColor);
                float3 healthBarColor = tex2D(_MainTex, float2(_HealthBar, IN.uv.y));

                float flash = cos(_Time.y * 4) * 0.4 + 1;

                // float3 bgColor = float3(0,0,0);
                // float3 outColor = lerp(bgColor, healthBarColor, healthBarMask);
                
                // return float4(healthBarColor, healthBarMask);
                return float4(healthBarColor * flash, 1 );
            }
            ENDHLSL
        }
    }
}
