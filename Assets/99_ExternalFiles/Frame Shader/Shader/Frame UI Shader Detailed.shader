// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Frame UI Shader Detailed"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _TextureMask("Texture Mask", 2D) = "white" {}
        _Opacity("Opacity", Float) = 1
        _WindAllSpeed("Wind All Speed", Float) = 1
        _EmissiveIntensity("Emissive Intensity", Float) = 1
        _Mask_Power("Mask_Power", Float) = 1
        _Mask_Multiply("Mask_Multiply", Float) = 1
        [Header(Noise Setting)]_NoisesOpacityBoost("Noises Opacity Boost", Float) = 1
        _Noises_Multiply("Noises_Multiply", Float) = 5
        _Noises_Power("Noises_Power", Float) = 1
        [Header(Header(Noise Texture 1))]_Noise_01_Texture("Noise_01_Texture", 2D) = "white" {}
        _Noise_01_Scale("Noise_01_Scale", Vector) = (0.8,0.8,0,0)
        _Noise_01_Speed("Noise_01_Speed", Vector) = (0.5,0.5,0,0)
        [Header(Noise Texture 2)]_Noise_02_Texture("Noise_02_Texture", 2D) = "white" {}
        _Noise_02_Scale("Noise_02_Scale", Vector) = (1,1,0,0)
        _Noise_02_Speed("Noise_02_Speed", Vector) = (-0.2,0.4,0,0)
        [Header(Distortion Mask Texture)]_DistortionMaskTexture("Distortion Mask Texture", 2D) = "white" {}
        _DistortionMaskIntensity("Distortion Mask Intensity", Float) = 1
        _DistortionMaskScale("Distortion Mask Scale", Vector) = (1,1,0,0)
        [Header(Distortion Texture)]_NoiseDistortion_Texture("NoiseDistortion_Texture", 2D) = "white" {}
        _DistortionAmount("Distortion Amount", Float) = 1
        _NoiseDistortion_Scale("NoiseDistortion_Scale", Vector) = (1,1,0,0)
        _NoiseDistortion_Speed("NoiseDistortion_Speed", Vector) = (-0.3,-0.3,0,0)
        _FillColor("Fill", Color) = (0,0,0,0)
        _StrokeColor("Stroke Color", Color) = (1,0,0,0)
        [Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 1

    }

    SubShader
    {
		LOD 0

        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Stencil
        {
        	Ref [_Stencil]
        	ReadMask [_StencilReadMask]
        	WriteMask [_StencilWriteMask]
        	Comp [_StencilComp]
        	Pass [_StencilOp]
        }


        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend [_Src] [_Dst]
        ColorMask [_ColorMask]

        
        Pass
        {
            Name "Default"
        CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityShaderVariables.cginc"


            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
                
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            uniform float _Src;
            uniform float _Dst;
            uniform float4 _StrokeColor;
            uniform sampler2D _Noise_01_Texture;
            uniform float _WindAllSpeed;
            uniform float2 _Noise_01_Speed;
            uniform float2 _Noise_01_Scale;
            uniform sampler2D _NoiseDistortion_Texture;
            uniform float2 _NoiseDistortion_Speed;
            uniform float2 _NoiseDistortion_Scale;
            uniform float _DistortionAmount;
            uniform sampler2D _Noise_02_Texture;
            uniform float2 _Noise_02_Speed;
            uniform float2 _Noise_02_Scale;
            uniform float _Noises_Power;
            uniform float _Noises_Multiply;
            uniform float _NoisesOpacityBoost;
            uniform sampler2D _TextureMask;
            uniform sampler2D _DistortionMaskTexture;
            uniform float2 _DistortionMaskScale;
            uniform float _DistortionMaskIntensity;
            uniform float _Opacity;
            uniform float _Mask_Power;
            uniform float _Mask_Multiply;
            uniform float4 _FillColor;
            uniform float _EmissiveIntensity;

            
            v2f vert(appdata_t v )
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                

                v.vertex.xyz +=  float3( 0, 0, 0 ) ;

                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = v.texcoord;
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN ) : SV_Target
            {
                //Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
                //The incoming alpha could have numerical instability, which makes it very sensible to
                //HDR color transparency blend, when it blends with the world's texture.
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;

                float windSpeed819 = ( _WindAllSpeed * _Time.y );
                float2 texCoord832 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                float2 panner843 = ( windSpeed819 * _Noise_01_Speed + ( texCoord832 * _Noise_01_Scale ));
                float2 texCoord820 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                float2 panner824 = ( windSpeed819 * _NoiseDistortion_Speed + ( texCoord820 * _NoiseDistortion_Scale ));
                float Distortion839 = ( ( tex2D( _NoiseDistortion_Texture, panner824 ).r * 0.1 ) * _DistortionAmount );
                float2 texCoord830 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                float2 panner841 = ( windSpeed819 * _Noise_02_Speed + ( texCoord830 * _Noise_02_Scale ));
                float noises876 = saturate( ( pow( ( tex2D( _Noise_01_Texture, ( panner843 + Distortion839 ) ).r * tex2D( _Noise_02_Texture, ( panner841 + Distortion839 ) ).r ) , _Noises_Power ) * _Noises_Multiply ) );
                float2 texCoord849 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                float2 texCoord861 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                float temp_output_888_0 = ( ( noises876 * _NoisesOpacityBoost ) * saturate( ( pow( ( tex2D( _TextureMask, ( ( tex2D( _DistortionMaskTexture, ( texCoord849 * _DistortionMaskScale ) ).r * ( Distortion839 * _DistortionMaskIntensity ) ) + texCoord861 ) ).r * _Opacity ) , _Mask_Power ) * _Mask_Multiply ) ) );
                float4 lerpResult899 = lerp( _StrokeColor , float4( 0,0,0,0 ) , temp_output_888_0);
                float4 lerpResult895 = lerp( lerpResult899 , _FillColor , temp_output_888_0);
                

                half4 color = ( lerpResult895 * _EmissiveIntensity );

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.rgb *= color.a;

                return color;
            }
        ENDCG
        }
    }
    CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.CommentaryNode;804;-1387.945,956.7626;Inherit;False;786;417;Register Wind Speed;4;819;817;816;815;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;805;-681.0283,-158.5842;Inherit;False;2502.5;663.612;Heat Haze;12;839;829;828;827;826;825;824;823;822;821;820;818;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;806;-500.5765,2724.445;Inherit;False;2997.113;1074.221;Noises;25;876;863;860;857;856;852;850;848;847;846;845;844;843;842;841;840;838;837;836;835;834;833;832;831;830;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;807;-521.0283,961.4158;Inherit;False;980;550;Distortion Mask;8;875;859;855;854;851;849;905;904;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;808;-521.0283,2115.245;Inherit;False;2391;470;Flame Mask;10;874;871;867;866;865;864;862;861;858;853;;0,0,0,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;820;-473.0283,-14.58423;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;821;422.9717,241.4158;Inherit;False;819;windSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;822;38.97168,113.4158;Inherit;False;Property;_NoiseDistortion_Speed;NoiseDistortion_Speed;21;0;Create;True;0;0;0;False;0;False;-0.3,-0.3;0,-0.3;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;824;422.9717,-14.58423;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;825;1062.972,113.4158;Inherit;False;Constant;_Float0;Float 0;8;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;827;1318.972,113.4158;Inherit;False;Property;_DistortionAmount;Distortion Amount;19;0;Create;True;0;0;0;False;0;False;1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;828;1062.972,-14.58423;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;829;1318.972,-14.58423;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;855;38.97168,1265.416;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;859;294.9717,1009.416;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;861;-89.02832,2163.245;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0.28,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;866;1062.972,2163.245;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;867;1446.972,2291.245;Inherit;False;Property;_Mask_Multiply;Mask_Multiply;5;0;Create;True;0;0;0;False;0;False;1;1.79;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;871;1446.972,2163.245;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;874;1702.972,2163.245;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;875;-473.0283,1265.416;Inherit;False;839;Distortion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;839;1574.972,-14.58423;Inherit;False;Distortion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;853;-473.0283,2163.245;Inherit;False;Constant;_Mask_Scale;Mask_Scale;2;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;858;-473.0283,2419.245;Inherit;False;Constant;_Mask_Offset;Mask_Offset;1;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;851;-473.0283,1393.416;Inherit;False;Property;_DistortionMaskIntensity;Distortion Mask Intensity;16;0;Create;True;0;0;0;False;0;False;1;-0.09;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;830;-404.5765,3348.445;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;831;-404.5765,3476.445;Inherit;False;Property;_Noise_02_Scale;Noise_02_Scale;13;0;Create;True;0;0;0;False;0;False;1,1;5,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;832;-404.5765,2836.445;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;833;-404.5765,2964.445;Inherit;False;Property;_Noise_01_Scale;Noise_01_Scale;10;0;Create;True;0;0;0;False;0;False;0.8,0.8;5,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;834;-148.5762,3348.445;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;835;235.4241,3604.445;Inherit;False;819;windSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;836;-20.57591,3476.445;Inherit;False;Property;_Noise_02_Speed;Noise_02_Speed;14;0;Create;True;0;0;0;False;0;False;-0.2,0.4;0,-0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;837;-148.5762,2836.445;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;838;-20.57591,2964.445;Inherit;False;Property;_Noise_01_Speed;Noise_01_Speed;11;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,-0.3;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;840;235.4241,3092.445;Inherit;False;819;windSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;841;235.4241,3348.445;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;842;491.4243,3604.445;Inherit;False;839;Distortion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;843;235.4241,2836.445;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;844;491.4243,3092.445;Inherit;False;839;Distortion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;845;491.4243,3348.445;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;846;491.4243,2836.445;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;850;1131.424,2836.445;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;856;1643.425,2964.445;Inherit;False;Property;_Noises_Multiply;Noises_Multiply;7;0;Create;True;0;0;0;False;0;False;5;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;857;1387.425,2836.445;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;860;1643.425,2836.445;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;863;1899.425,2836.445;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;876;2155.424,2836.445;Inherit;False;noises;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;848;747.4243,2836.445;Inherit;True;Property;_Noise_01_Texture;Noise_01_Texture;9;1;[Header];Create;True;1;Header(Noise Texture 1);0;0;False;0;False;-1;None;dc566e4c9381aea4687dd7e7d7678ed6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;847;747.4243,3348.445;Inherit;True;Property;_Noise_02_Texture;Noise_02_Texture;12;0;Create;True;0;0;0;False;1;Header(Noise Texture 2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;816;-1339.945,1260.762;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;817;-1083.946,1004.763;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;819;-827.9456,1004.763;Inherit;False;windSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;826;678.9717,-14.58423;Inherit;True;Property;_NoiseDistortion_Texture;NoiseDistortion_Texture;18;0;Create;True;0;0;0;False;1;Header(Distortion Texture);False;-1;None;33d4a85877826c14d83910d65644473a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;864;1062.972,2291.245;Inherit;False;Property;_Mask_Power;Mask_Power;4;0;Create;True;0;0;0;False;0;False;1;1.27;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;854;-89.02832,1009.416;Inherit;True;Property;_DistortionMaskTexture;Distortion Mask Texture;15;0;Create;True;0;0;0;False;1;Header(Distortion Mask Texture);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;815;-1339.945,1004.763;Inherit;False;Property;_WindAllSpeed;Wind All Speed;2;0;Create;True;0;0;0;False;0;False;1;0.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;818;-473.0283,113.4158;Inherit;False;Property;_NoiseDistortion_Scale;NoiseDistortion_Scale;20;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;823;-217.0283,-14.58423;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;905;-266.6944,1004.524;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;904;-505.6941,1129.524;Inherit;False;Property;_DistortionMaskScale;Distortion Mask Scale;17;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;849;-495.1526,999.1309;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;862;389.8947,2297.5;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;888;2553.194,2125.707;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;899;2756.836,1920.964;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;895;3227.593,2067.395;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;897;2767.405,1721.232;Inherit;False;Property;_FillColor;Fill;22;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;900;3558.306,2052.553;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;872;2351.572,2312.544;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;870;2123.598,2280.736;Inherit;False;876;noises;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;868;2071.032,2398.073;Inherit;False;Property;_NoisesOpacityBoost;Noises Opacity Boost;6;0;Create;True;0;0;0;False;1;Header(Noise Setting);False;1;3.92;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;906;3836.04,2082.142;Float;False;True;-1;2;ASEMaterialInspector;0;3;Custom/Frame UI Shader Detailed;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;True;True;4;1;True;_Src;1;True;_Dst;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;True;True;True;True;True;0;True;_ColorMask;False;False;False;False;False;False;False;True;True;0;True;_Stencil;255;True;_StencilReadMask;255;True;_StencilWriteMask;0;True;_StencilComp;0;True;_StencilOp;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;0;True;unity_GUIZTestMode;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.SamplerNode;865;680.9174,2264.422;Inherit;True;Property;_Mask_Texture;Mask_Texture;5;0;Create;True;0;0;0;False;0;False;-1;None;48dc7bc18268b924c8348d314decc5da;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0,0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;907;461.0694,2091.676;Inherit;True;Property;_TextureMask;Texture Mask;0;0;Create;True;0;0;0;False;0;False;None;cb59170e71f56d440a43367fee94c3a7;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;896;3317.265,1859.566;Inherit;False;Property;_EmissiveIntensity;Emissive Intensity;3;0;Create;True;0;0;0;False;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;852;1387.425,2964.445;Inherit;False;Property;_Noises_Power;Noises_Power;8;0;Create;True;0;0;0;False;0;False;1;1.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;898;2153.397,1865.86;Inherit;False;Property;_StrokeColor;Stroke Color;23;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0.5151776,0.1839622,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;901;3844.952,2490.131;Inherit;False;Property;_Src;Src;24;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;902;3852.639,2584.618;Inherit;False;Property;_Dst;Dst;25;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;908;1005.546,2510.65;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;909;828.5461,2539.65;Inherit;False;Property;_Opacity;Opacity;1;0;Create;True;0;0;0;False;0;False;1;0.33;0;0;0;1;FLOAT;0
WireConnection;824;0;823;0
WireConnection;824;2;822;0
WireConnection;824;1;821;0
WireConnection;828;0;826;1
WireConnection;828;1;825;0
WireConnection;829;0;828;0
WireConnection;829;1;827;0
WireConnection;855;0;875;0
WireConnection;855;1;851;0
WireConnection;859;0;854;1
WireConnection;859;1;855;0
WireConnection;861;0;853;0
WireConnection;861;1;858;0
WireConnection;866;0;908;0
WireConnection;866;1;864;0
WireConnection;871;0;866;0
WireConnection;871;1;867;0
WireConnection;874;0;871;0
WireConnection;839;0;829;0
WireConnection;834;0;830;0
WireConnection;834;1;831;0
WireConnection;837;0;832;0
WireConnection;837;1;833;0
WireConnection;841;0;834;0
WireConnection;841;2;836;0
WireConnection;841;1;835;0
WireConnection;843;0;837;0
WireConnection;843;2;838;0
WireConnection;843;1;840;0
WireConnection;845;0;841;0
WireConnection;845;1;842;0
WireConnection;846;0;843;0
WireConnection;846;1;844;0
WireConnection;850;0;848;1
WireConnection;850;1;847;1
WireConnection;857;0;850;0
WireConnection;857;1;852;0
WireConnection;860;0;857;0
WireConnection;860;1;856;0
WireConnection;863;0;860;0
WireConnection;876;0;863;0
WireConnection;848;1;846;0
WireConnection;847;1;845;0
WireConnection;817;0;815;0
WireConnection;817;1;816;0
WireConnection;819;0;817;0
WireConnection;826;1;824;0
WireConnection;854;1;905;0
WireConnection;823;0;820;0
WireConnection;823;1;818;0
WireConnection;905;0;849;0
WireConnection;905;1;904;0
WireConnection;862;0;859;0
WireConnection;862;1;861;0
WireConnection;888;0;872;0
WireConnection;888;1;874;0
WireConnection;899;0;898;0
WireConnection;899;2;888;0
WireConnection;895;0;899;0
WireConnection;895;1;897;0
WireConnection;895;2;888;0
WireConnection;900;0;895;0
WireConnection;900;1;896;0
WireConnection;872;0;870;0
WireConnection;872;1;868;0
WireConnection;906;0;900;0
WireConnection;865;0;907;0
WireConnection;865;1;862;0
WireConnection;908;0;865;1
WireConnection;908;1;909;0
ASEEND*/
//CHKSM=D8167643C3B4283C787613F476479637493DF247