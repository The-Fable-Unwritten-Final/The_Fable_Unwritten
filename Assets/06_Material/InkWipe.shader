Shader "UI/InkWipe"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise", 2D) = "white" {}
        _Color ("Color", Color) = (0.698, 0.447, 0.239, 1)
        _Progress ("Progress", Range(0,1)) = 0
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.2
        _Softness ("Edge Softness", Range(0,0.5)) = 0.1
        
        // 조명 효과 프로퍼티
        _LampPosition ("Lamp Position (0-1)", Vector) = (0.85, 0.85, 0, 0)
        _LampBrightness ("Lamp Brightness", Range(0.5, 2.0)) = 1.5
        _LampFalloff ("Lamp Falloff", Range(0.1, 2.0)) = 0.8
        _ShadowIntensity ("Shadow Intensity", Range(0.0, 1.0)) = 0.4
        _LampColor ("Lamp Light Color", Color) = (1.0, 0.95, 0.8, 1.0)
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
        BlendOp Add
        Cull Off
        ZWrite Off
        
        // 알파가 거의 0이면 완전히 투명하게 처리
        AlphaToMask On

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
            float4 _LampPosition;
            float _LampBrightness;
            float _LampFalloff;
            float _ShadowIntensity;
            float4 _LampColor;

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

                // Wipe 계산 (좌상단에서 우하단으로)
                float wipe = (uv.x + (1.0 - uv.y)) * 0.5;

                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv).r;
                wipe += (noise - 0.5) * _NoiseStrength;
                wipe = saturate(wipe);

                // Progress와의 거리 기반 계산
                float diff = wipe - _Progress;
                float wipeAlpha = smoothstep(_Softness, -_Softness, diff);

                // 촛불 흔들림 효과 (LampLight와 동일)
                float flicker = sin(_Time.y * 3.5) * 0.5 + cos(_Time.y * 2.7) * 0.3 + sin(_Time.y * 1.3) * 0.2;
                float fluctuation = flicker * 0.1;  // ±0.1 범위로 흔들림
                float actualFalloff = _LampFalloff + fluctuation;
                
                // 밝기 변화 (촛불 깜빡임)
                float brightnessFlicker = sin(_Time.y * 2.3) * 0.5 + cos(_Time.y * 3.1) * 0.3;
                float brightnessVariation = 1.0 + brightnessFlicker * 0.2;  // 밝기 ±20% 변화

                // 조명 효과 적용
                float2 lampPos = _LampPosition.xy;
                float2 towardLamp = uv - lampPos;
                float distance = length(towardLamp);
                
                // 거리에 기반한 밝기 계산 (흔들림 적용)
                float brightness = exp(-distance * distance / (actualFalloff * actualFalloff));
                brightness = pow(brightness, 1.2) * _LampBrightness * brightnessVariation;
                
                // 방향 팩터 (음영 효과)
                float directionFactor = dot(normalize(lampPos - uv + float2(0.5, 0.5)), float2(0.707, 0.707));
                directionFactor = max(0.0, directionFactor);
                
                // 최종 조명 계산
                float finalBrightness = brightness + (directionFactor * _ShadowIntensity);
                finalBrightness = clamp(finalBrightness, 1.0 - _ShadowIntensity, 1.0 + brightness);
                
                // 기본 색상에 조명 적용
                half4 col = _Color;
                col.rgb *= finalBrightness;
                
                // 램프 색상 혼합
                col.rgb = lerp(col.rgb, _LampColor.rgb, brightness * 0.3);

                // 이미지 알파 × wipe
                col.a *= base.a * wipeAlpha;

                return col;
            }
            ENDHLSL
        }
    }
}
