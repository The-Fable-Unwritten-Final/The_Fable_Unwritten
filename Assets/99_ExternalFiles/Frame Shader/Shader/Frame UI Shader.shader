// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Frame UI Shader"
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

        _EmissiveIntensity("Emissive Intensity", Float) = 1
        _Opacity("Opacity", Float) = 1
        _TextureMain("Texture Main", 2D) = "white" {}
        [Header(Texture 1)]_TextureSample1("Texture Sample 1", 2D) = "white" {}
        _InnerScale1("Scale 1", Float) = 5
        _Texturesize1("Texture size 1", Vector) = (0,0,0,0)
        _TextureOffset1("Texture Offset 1", Vector) = (0,0,0,0)
        _SpeedTexture1("Speed Texture 1", Vector) = (0,0,0,0)
        [Header(Texture 2)]_TextureSample2("Texture Sample 2", 2D) = "white" {}
        _OuterScale1("Scale 2", Float) = 5
        _Texturesize2("Texture size 2", Vector) = (0,0,0,0)
        _TextureOffset2("Texture Offset 2", Vector) = (0,0,0,0)
        _SpeedTexture2("Speed Texture 2", Vector) = (0,0,0,0)
        _InnerOffset2("Offset", Float) = 0
        _FillColor("Fill", Color) = (0,0,0,0)
        _StrokeColor("Stroke Color", Color) = (1,0,0,0)
        [Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 0

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
            uniform float _OuterScale1;
            uniform sampler2D _TextureSample2;
            uniform float2 _SpeedTexture2;
            uniform float2 _Texturesize2;
            uniform float2 _TextureOffset2;
            uniform float _InnerOffset2;
            uniform sampler2D _TextureMain;
            uniform sampler2D _TextureSample1;
            uniform float2 _SpeedTexture1;
            uniform float2 _Texturesize1;
            uniform float2 _TextureOffset1;
            uniform float _InnerScale1;
            uniform float _Opacity;
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

                float2 appendResult408 = (float2(_SpeedTexture2.x , _SpeedTexture2.y));
                float2 texCoord407 = IN.texcoord.xy * _Texturesize2 + _TextureOffset2;
                float2 panner409 = ( 1.0 * _Time.y * appendResult408 + texCoord407);
                float temp_output_497_0 = ( ( (0) * _OuterScale1 ) + ( ( ( tex2D( _TextureSample2, panner409 ).r - 0.5 ) * _OuterScale1 ) + 0.5 ) + _InnerOffset2 );
                float lerpResult499 = lerp( temp_output_497_0 , ( 1.0 - temp_output_497_0 ) , 0.0);
                float Texture_2500 = lerpResult499;
                float2 texCoord449 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                float2 appendResult403 = (float2(_SpeedTexture1.x , _SpeedTexture1.y));
                float2 texCoord402 = IN.texcoord.xy * _Texturesize1 + _TextureOffset1;
                float2 panner404 = ( 1.0 * _Time.y * appendResult403 + texCoord402);
                float temp_output_484_0 = ( _InnerScale1 * 1.0 );
                float temp_output_489_0 = ( ( ( ( tex2D( _TextureSample1, panner404 ).r - 0.5 ) * temp_output_484_0 ) + 0.5 ) + ( temp_output_484_0 * (0) ) + _InnerOffset2 );
                float lerpResult491 = lerp( ( 1.0 - temp_output_489_0 ) , temp_output_489_0 , 0.0);
                float Texture_1492 = lerpResult491;
                float2 lerpResult476 = lerp( texCoord449 , float2( 0,0 ) , Texture_1492);
                float temp_output_685_0 = ( Texture_2500 * ( tex2D( _TextureMain, lerpResult476 ).a * _Opacity ) );
                float4 lerpResult661 = lerp( _StrokeColor , float4( 0,0,0,0 ) , temp_output_685_0);
                float4 lerpResult663 = lerp( lerpResult661 , _FillColor , temp_output_685_0);
                

                half4 color = ( lerpResult663 * _EmissiveIntensity );

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
Node;AmplifyShaderEditor.CommentaryNode;479;-2720.341,1090.023;Inherit;False;1616.153;898.7643;;18;500;499;498;497;496;495;493;492;491;490;489;488;487;486;485;484;483;482;Texture;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;483;-2602.229,1454.497;Inherit;False;-1;;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;484;-2430.164,1310.768;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;485;-2260.516,1398.393;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;487;-2037.314,1350.447;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;495;-2413.803,1723.756;Inherit;True;CenterScale;-1;;30;d960bbb316065de47a9db2b86fdcf8b4;0;2;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;496;-2321.695,1617.052;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;497;-2148.218,1699.381;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;498;-2001.918,1767.594;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;493;-2613.924,1602.236;Inherit;False;Property;_OuterScale1;Scale 2;9;0;Create;False;0;0;0;False;0;False;5;1.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;488;-2262.18,1134.635;Inherit;True;CenterScale;-1;;31;d960bbb316065de47a9db2b86fdcf8b4;0;2;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;499;-1665.122,1698.763;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;486;-2263.128,1506.236;Inherit;False;Property;_InnerOffset2;Offset;13;0;Create;False;0;0;0;False;0;False;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;489;-1953.728,1226.376;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;500;-1365.854,1726.897;Inherit;True;Texture 2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;491;-1502.753,1224.446;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;490;-1710.513,1136.393;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;492;-1319.352,1155.67;Inherit;True;Texture 1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;408;-3723.283,1955.536;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;409;-3504.828,1788.922;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;426;-4269.76,1831.279;Inherit;False;Property;_TextureOffset2;Texture Offset 2;11;0;Create;True;0;0;0;False;0;False;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;443;-4283.293,2030.835;Inherit;False;Property;_SpeedTexture2;Speed Texture 2;12;0;Create;True;0;0;0;False;0;False;0,0;0.01,0.05;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;407;-3961.876,1592.67;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;421;-4277.221,1627.422;Inherit;False;Property;_Texturesize2;Texture size 2;10;0;Create;True;0;0;0;False;0;False;0,0;2,2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;403;-3779.593,1285.311;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;404;-3587.593,1125.31;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;420;-4464.697,861.4595;Inherit;False;Property;_Texturesize1;Texture size 1;5;0;Create;True;0;0;0;False;0;False;0,0;2,2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;425;-4455.923,1018.073;Inherit;False;Property;_TextureOffset1;Texture Offset 1;6;0;Create;True;0;0;0;False;0;False;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;441;-4281.824,1260.901;Inherit;False;Property;_SpeedTexture1;Speed Texture 1;7;0;Create;True;0;0;0;False;0;False;0,0;-0.02,-0.05;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;402;-4170.462,812.4165;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;449;-869.303,1235.942;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;412;-864.4858,1411.297;Inherit;True;492;Texture 1;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;476;-521.1268,1130.659;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;451;1665.225,1203.651;Float;False;True;-1;2;ASEMaterialInspector;0;3;Custom/Frame UI Shader;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;True;True;4;1;True;_Src;1;True;_Dst;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;True;True;True;True;True;0;True;_ColorMask;False;False;False;False;False;False;False;True;True;0;True;_Stencil;255;True;_StencilReadMask;255;True;_StencilWriteMask;0;True;_StencilComp;0;True;_StencilOp;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;0;True;unity_GUIZTestMode;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.SamplerNode;400;-3178.939,1740.34;Inherit;True;Property;_TextureSample2;Texture Sample 2;8;0;Create;True;0;0;0;False;1;Header(Texture 2);False;-1;None;3128cd3ace6d4e64498af5e1c0954e06;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;687;36.18738,2003.013;Inherit;False;Property;_Src;Src;16;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;686;34.54431,2105.942;Inherit;False;Property;_Dst;Dst;17;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;482;-2673.576,1273.414;Inherit;False;Property;_InnerScale1;Scale 1;4;0;Create;False;0;0;0;False;0;False;5;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;354;-3167.969,1103.391;Inherit;True;Property;_TextureSample1;Texture Sample 1;3;0;Create;True;0;0;0;False;1;Header(Texture 1);False;-1;None;f24b8e4491f4ba0429ada2f51b84201f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;660;-134.987,1071.302;Inherit;True;Property;_TextureMain;Texture Main;2;0;Create;True;0;0;0;False;0;False;-1;None;6e29f189f1f6cb54da55fc379516d149;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;691;22.01331,1334.775;Inherit;False;Property;_Opacity;Opacity;1;0;Create;True;0;0;0;False;0;False;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;665;1442.83,1197.88;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;690;199.0133,1305.775;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;654;67.01603,1622.651;Inherit;True;500;Texture 2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;663;948.0508,1211.05;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;664;1212.825,1362.833;Inherit;False;Property;_EmissiveIntensity;Emissive Intensity;0;0;Create;True;0;0;0;False;0;False;1;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;662;571.6556,948.476;Inherit;False;Property;_FillColor;Fill;14;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.9902111,0.5990566,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;659;379.5488,1174.357;Inherit;False;Property;_StrokeColor;Stroke Color;15;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0.7175808,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;661;770.2023,1490.205;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;685;360.2534,1524.905;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
WireConnection;484;0;482;0
WireConnection;485;0;484;0
WireConnection;485;1;483;0
WireConnection;487;0;485;0
WireConnection;495;1;400;1
WireConnection;495;2;493;0
WireConnection;496;0;483;0
WireConnection;496;1;493;0
WireConnection;497;0;496;0
WireConnection;497;1;495;0
WireConnection;497;2;486;0
WireConnection;498;0;497;0
WireConnection;488;1;354;1
WireConnection;488;2;484;0
WireConnection;499;0;497;0
WireConnection;499;1;498;0
WireConnection;489;0;488;0
WireConnection;489;1;487;0
WireConnection;489;2;486;0
WireConnection;500;0;499;0
WireConnection;491;0;490;0
WireConnection;491;1;489;0
WireConnection;490;0;489;0
WireConnection;492;0;491;0
WireConnection;408;0;443;1
WireConnection;408;1;443;2
WireConnection;409;0;407;0
WireConnection;409;2;408;0
WireConnection;407;0;421;0
WireConnection;407;1;426;0
WireConnection;403;0;441;1
WireConnection;403;1;441;2
WireConnection;404;0;402;0
WireConnection;404;2;403;0
WireConnection;402;0;420;0
WireConnection;402;1;425;0
WireConnection;476;0;449;0
WireConnection;476;2;412;0
WireConnection;451;0;665;0
WireConnection;400;1;409;0
WireConnection;354;1;404;0
WireConnection;660;1;476;0
WireConnection;665;0;663;0
WireConnection;665;1;664;0
WireConnection;690;0;660;4
WireConnection;690;1;691;0
WireConnection;663;0;661;0
WireConnection;663;1;662;0
WireConnection;663;2;685;0
WireConnection;661;0;659;0
WireConnection;661;2;685;0
WireConnection;685;0;654;0
WireConnection;685;1;690;0
ASEEND*/
//CHKSM=E7006C5916D28E86987E7287F00696032650FA63