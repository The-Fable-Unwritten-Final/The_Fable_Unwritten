Shader "UI/BlurUI"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 4)) = 1
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "UIBlurSelfSafe"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _BlurSize;

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

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                // MainTex 크기에 기반해 texel 크기를 계산 (Safe 방식)
                float2 texel = float2(1.0 / 512.0, 1.0 / 512.0) * _BlurSize;

                float4 col = float4(0,0,0,0);
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-texel.x, 0));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texel.x, 0));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -texel.y));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, texel.y));

                col *= 0.25;

                float4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                col.a = main.a;

                return col;
            }
            ENDHLSL
        }
    }
}
