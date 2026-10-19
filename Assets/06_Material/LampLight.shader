Shader "Custom/LampLight"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.8, 0.7, 0.6, 1.0)
        _LampPosition ("Lamp Position (0-1)", Vector) = (0.85, 0.85, 0, 0)
        _LampBrightness ("Lamp Brightness", Range(0.5, 2.0)) = 1.5
        _LampFalloff ("Lamp Falloff", Range(0.1, 2.0)) = 0.8
        _ShadowIntensity ("Shadow Intensity", Range(0.0, 1.0)) = 0.4
        _LampColor ("Lamp Light Color", Color) = (1.0, 0.95, 0.8, 1.0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off

        Pass
        {
            Name "LampLight"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BaseColor;
            float4 _LampPosition;
            float _LampBrightness;
            float _LampFalloff;
            float _ShadowIntensity;
            float4 _LampColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 texColor = tex2D(_MainTex, uv);
                
                // 기본 색상
                float4 baseColor = _BaseColor * texColor;
                
                // 촛불 흔들림 효과
                // 여러 주파수를 조합하여 자연스러운 움직임 생성 (랜덤에 유사한 패턴)
                float flicker = sin(_Time.y * 3.5) * 0.5 + cos(_Time.y * 2.7) * 0.3 + sin(_Time.y * 1.3) * 0.2;
                float fluctuation = flicker * 0.1;  // ±0.1 범위로 더 흔들림
                float actualFalloff = _LampFalloff + fluctuation;
                
                // 밝기 변화 (촛불 깜빡임)
                float brightnessFlicker = sin(_Time.y * 2.3) * 0.5 + cos(_Time.y * 3.1) * 0.3;
                float brightnessVariation = 1.0 + brightnessFlicker * 0.2;  // 밝기 ±20% 변화
                
                // 램프 광원 위치까지의 거리 계산
                float2 lampPos = _LampPosition.xy;
                float2 towardLamp = uv - lampPos;
                float distance = length(towardLamp);
                
                // 거리에 기반한 밝기 계산 (가우시안 폴오프)
                float brightness = exp(-distance * distance / (actualFalloff * actualFalloff));
                brightness = pow(brightness, 1.2) * _LampBrightness * brightnessVariation;
                
                // 램프 쪽으로의 방향에 따른 추가 그래디언트
                // 우측 위쪽이 더 밝게, 좌측 아래는 어두운 음영
                float directionFactor = dot(normalize(lampPos - uv + float2(0.5, 0.5)), float2(0.707, 0.707));
                directionFactor = max(0.0, directionFactor);
                
                // 최종 조명 계산
                float finalBrightness = brightness + (directionFactor * _ShadowIntensity);
                finalBrightness = clamp(finalBrightness, 1.0 - _ShadowIntensity, 1.0 + brightness);
                
                // 램프 색상 적용 (따뜻한 톤)
                float3 lampLight = lerp(baseColor.rgb, _LampColor.rgb, brightness * 0.6);
                
                // 최종 색상
                float3 finalColor = baseColor.rgb * finalBrightness;
                finalColor = lerp(finalColor, lampLight, brightness * 0.3);
                
                return float4(finalColor, baseColor.a);
            }
            ENDCG
        }
    }
    
    FallBack "UI/Default"
}
