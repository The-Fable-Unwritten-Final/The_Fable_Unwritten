Shader "UI/InkWipe"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,1)
        _Progress ("Progress", Range(0,1)) = 0
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.2
        _Softness ("Edge Softness", Range(0,0.5)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            float4 _MainTex_ST;
            float4 _Color;
            float _Progress;
            float _NoiseStrength;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // 잉크 이미지
                half4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // Wipe 계산
                float wipe = uv.x;

                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv).r;
                wipe += (noise - 0.5) * _NoiseStrength;
                wipe = saturate(wipe);

                // Progress와의 거리 기반 계산
                float diff = wipe - _Progress;
                float wipeAlpha = smoothstep(_Softness, -_Softness, diff);

                half4 col = _Color;

                // 이미지 알파 × wipe
                col.a *= base.a * wipeAlpha;

                return col;
            }
            ENDHLSL
        }
    }
}
