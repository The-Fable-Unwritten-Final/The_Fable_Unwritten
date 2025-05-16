Shader "Custom/UIGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,0,1)
        _GlowIntensity ("Glow Intensity", Range(0,5)) = 1
        _GlowThickness ("Glow Thickness", Range(0,0.1)) = 0.03
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowThickness;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 투명 부분(알파 0) 주변을 찾아 Glow 효과를 준다.
                float alpha = col.a;

                // 주변 4방향(좌,우,상,하) 알파 값을 샘플링해서 Glow 효과 계산
                float glow = 0.0;
                glow += tex2D(_MainTex, i.uv + float2(_GlowThickness, 0)).a;
                glow += tex2D(_MainTex, i.uv + float2(-_GlowThickness, 0)).a;
                glow += tex2D(_MainTex, i.uv + float2(0, _GlowThickness)).a;
                glow += tex2D(_MainTex, i.uv + float2(0, -_GlowThickness)).a;

                glow = glow * 0.25 * _GlowIntensity * (1 - alpha);

                fixed4 glowCol = _GlowColor * glow;

                // 기본 텍스처 컬러 + Glow 색상 더하기
                fixed4 finalCol = col + glowCol;
                finalCol.a = max(col.a, glowCol.a);

                return finalCol;
            }
            ENDCG
        }
    }
}
