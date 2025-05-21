// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Frame UI Shader Shape"
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

        [Toggle]_Invert("Invert", Float) = 1
        _EmissiveIntensity("Emissive Intensity", Float) = 1
        _Opacity("Opacity", Float) = 1
        [Enum(Line,0,Stripe,1,Square,2,Circle,3,Fade,4)][Header(Shape)]_ShapeType("Type", Float) = 2
        _Rotation("Rotation", Range( 0 , 360)) = 0
        _ShapeScaleX("Margin X", Float) = 0
        _ShapeScaleY("Margin Y", Float) = 0
        _ShapeOffsetX("Offset X", Float) = 0
        _ShapeOffsetY("Offset Y", Float) = 0
        [Header(Texture 1)]_OuterMask("Texture Sample 1", 2D) = "white" {}
        _InnerScale("Scale 1", Float) = 5
        _Texturesize3("Texture size 1", Vector) = (0,0,0,0)
        _TextureOffset3("Texture Offset 1", Vector) = (0,0,0,0)
        _SpeedTexture3("Speed Texture 1", Vector) = (0,0,0,0)
        [Header(Texture 2)]_OuterMask1("Texture Sample 2", 2D) = "white" {}
        _OuterScale("Scale 2", Float) = 5
        _Texturesize2("Texture size 2", Vector) = (0,0,0,0)
        _TextureOffset2("Texture Offset 2", Vector) = (0,0,0,0)
        _SpeedTexture2("Speed Texture 2", Vector) = (0,0,0,0)
        _InnerOffset("Offset", Float) = 0
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

            uniform float _Dst;
            uniform float _Src;
            uniform float4 _StrokeColor;
            uniform sampler2D _OuterMask;
            uniform float2 _SpeedTexture3;
            uniform float2 _Texturesize3;
            uniform float2 _TextureOffset3;
            uniform float _OuterScale;
            uniform float _ShapeOffsetX;
            uniform float _ShapeOffsetY;
            uniform float _Rotation;
            uniform float _ShapeScaleX;
            uniform float _ShapeScaleY;
            uniform float _ShapeType;
            uniform float _Invert;
            uniform sampler2D _OuterMask1;
            uniform float2 _SpeedTexture2;
            uniform float2 _Texturesize2;
            uniform float2 _TextureOffset2;
            uniform float _InnerScale;
            uniform float _InnerOffset;
            uniform float _Opacity;
            uniform float4 _FillColor;
            uniform float _EmissiveIntensity;

            
            v2f vert(appdata_t v )
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float cos831 = cos( 0.2 * _Time.y );
                float sin831 = sin( 0.2 * _Time.y );
                float2 rotator831 = mul( float2( 0,0 ) - float2( 0.5,0.5 ) , float2x2( cos831 , -sin831 , sin831 , cos831 )) + float2( 0.5,0.5 );
                

                v.vertex.xyz += float3( rotator831 ,  0.0 );

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

                float2 appendResult788 = (float2(_SpeedTexture3.x , _SpeedTexture3.y));
                float2 texCoord791 = IN.texcoord.xy * _Texturesize3 + _TextureOffset3;
                float2 panner789 = ( 1.0 * _Time.y * appendResult788 + texCoord791);
                float2 appendResult693 = (float2(_ShapeOffsetX , _ShapeOffsetY));
                float2 texCoord691 = IN.texcoord.xy * float2( 2,2 ) + float2( -1,-1 );
                float cos692 = cos( radians( _Rotation ) );
                float sin692 = sin( radians( _Rotation ) );
                float2 rotator692 = mul( texCoord691 - float2( 0,0 ) , float2x2( cos692 , -sin692 , sin692 , cos692 )) + float2( 0,0 );
                float2 temp_output_694_0 = ( appendResult693 + rotator692 );
                float LineShape713 = (temp_output_694_0).y;
                float2 appendResult698 = (float2(_ShapeScaleX , _ShapeScaleY));
                float temp_output_704_0 = (( ( appendResult698 + abs( temp_output_694_0 ) ) / ( float2( 1,1 ) + appendResult698 ) )).y;
                float StripeShape710 = temp_output_704_0;
                float lerpResult726 = lerp( LineShape713 , StripeShape710 , saturate( _ShapeType ));
                float SquareShape714 = max( temp_output_704_0 , (( ( appendResult698 + abs( temp_output_694_0 ) ) / ( float2( 1,1 ) + appendResult698 ) )).x );
                float lerpResult728 = lerp( lerpResult726 , SquareShape714 , saturate( ( _ShapeType - 1.0 ) ));
                float CircleShape716 = length( ( ( temp_output_694_0 / ( ( 1.0 - appendResult698 ) + float2( 1,1 ) ) ) * float2( 2,2 ) ) );
                float lerpResult733 = lerp( lerpResult728 , CircleShape716 , saturate( ( _ShapeType - 2.0 ) ));
                float lerpResult734 = lerp( lerpResult733 , 0.5 , saturate( ( _ShapeType - 3.0 ) ));
                float ShapeMask735 = lerpResult734;
                float temp_output_752_0 = ( ( 0.0 * _OuterScale ) + ( ( ( ShapeMask735 - 0.5 ) * _OuterScale ) + 0.5 ) );
                float lerpResult754 = lerp( temp_output_752_0 , ( 1.0 - temp_output_752_0 ) , _Invert);
                float OuterGradient756 = lerpResult754;
                float2 appendResult795 = (float2(_SpeedTexture2.x , _SpeedTexture2.y));
                float2 texCoord797 = IN.texcoord.xy * _Texturesize2 + _TextureOffset2;
                float2 panner796 = ( 1.0 * _Time.y * appendResult795 + texCoord797);
                float temp_output_740_0 = ( _InnerScale * 1.0 );
                float temp_output_745_0 = ( ( ( ( ShapeMask735 - 0.5 ) * temp_output_740_0 ) + 0.5 ) + ( temp_output_740_0 * 0.0 ) + _InnerOffset );
                float lerpResult747 = lerp( ( 1.0 - temp_output_745_0 ) , temp_output_745_0 , _Invert);
                float InnerGradient755 = lerpResult747;
                float temp_output_833_0 = ( ( saturate( ( tex2D( _OuterMask, panner789 ).r + OuterGradient756 ) ) * saturate( ( tex2D( _OuterMask1, panner796 ).r + InnerGradient755 ) ) ) * _Opacity );
                float4 lerpResult661 = lerp( _StrokeColor , float4( 0,0,0,0 ) , temp_output_833_0);
                float4 lerpResult663 = lerp( lerpResult661 , _FillColor , temp_output_833_0);
                

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
Node;AmplifyShaderEditor.CommentaryNode;718;-3128.937,1323.217;Inherit;False;1616.153;898.7643;;20;757;756;755;754;753;752;751;750;749;748;747;746;745;744;743;742;741;740;738;736;Resizing Shapes;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;686;-3307.813,-652.3983;Inherit;False;2194.727;997.1237;;31;717;716;715;714;713;712;711;710;709;708;707;706;705;704;703;702;701;700;699;698;697;696;695;694;693;692;691;690;689;688;687;Shapes;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;719;-3260.741,513.0762;Inherit;False;1721.574;728.4078;;17;758;735;734;733;732;731;730;729;728;727;726;725;724;723;722;721;720;Shape Switch;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;759;-522.6804,-258.4689;Inherit;False;1887.747;409.2518;;7;770;769;767;765;788;789;791;Dissolve Opacity;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;772;-542.5362,283.1729;Inherit;False;1887.747;409.2518;;7;781;780;779;778;795;796;797;Dissolve Opacity;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;720;-3213.229,716.6188;Inherit;False;Property;_ShapeType;Type;3;1;[Enum];Create;False;0;5;Line;0;Stripe;1;Square;2;Circle;3;Fade;4;0;False;1;Header(Shape);False;2;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;721;-2837.905,632.0763;Inherit;False;710;StripeShape;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;722;-2889.103,721.0648;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;723;-3025.618,812.3532;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;724;-2886.967,813.9173;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;726;-2610.192,671.1201;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;727;-3026.918,905.053;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;728;-2439.907,766.0762;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;729;-3029.686,1005.441;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;730;-2890.17,903.5669;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;732;-2892.937,1003.955;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;733;-2266.906,855.0763;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;734;-2066.352,949.664;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;735;-1827.352,940.1862;Inherit;False;ShapeMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;758;-2821.446,566.535;Inherit;False;713;LineShape;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;731;-2654.905,909.1764;Inherit;False;716;CircleShape;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;725;-2664.905,798.821;Inherit;False;714;SquareShape;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;688;-2964.059,-232.0654;Inherit;False;Property;_ShapeOffsetY;Offset Y;8;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;689;-2962.694,-307.0651;Inherit;False;Property;_ShapeOffsetX;Offset X;7;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;693;-2762.694,-280.3317;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;695;-2620.22,-424.5374;Inherit;False;Property;_ShapeScaleY;Margin Y;6;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;696;-2618.124,-501.6364;Inherit;False;Property;_ShapeScaleX;Margin X;5;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;697;-2383.634,-225.5229;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;698;-2418.124,-473.6366;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;699;-2219.509,-399.6772;Inherit;False;2;2;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;700;-2215.828,-268.354;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;701;-2257.827,-30.35908;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;702;-2067.41,-345.0771;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;703;-2101.832,115.3596;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;704;-1717.657,-482.4525;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;705;-1919.416,116.1378;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;706;-1531.772,-507.0999;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;707;-1760.428,75.7985;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;708;-1716.217,-413.456;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;710;-1360.867,-595.8704;Inherit;True;StripeShape;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;711;-1509.22,-458.456;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;712;-1640.498,76.01589;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;713;-1363.488,-152.6097;Inherit;True;LineShape;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;714;-1360.876,-375.2006;Inherit;True;SquareShape;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;715;-1508.329,72.8984;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;716;-1367.361,67.14069;Inherit;True;CircleShape;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;717;-1904.908,-345.0771;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;736;-2224.877,1736.553;Inherit;False;Property;_Invert;Invert;0;1;[Toggle];Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;740;-2831.215,1546.102;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;741;-2661.567,1633.727;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;743;-2438.364,1585.781;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;746;-2224.702,1381.278;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;747;-2043.878,1385.552;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;749;-3039.299,1954.414;Inherit;False;735;ShapeMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;750;-2814.854,1959.089;Inherit;True;CenterScale;-1;;33;d960bbb316065de47a9db2b86fdcf8b4;0;2;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;752;-2549.268,1934.714;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;753;-2402.969,2002.928;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;754;-2010.878,1937.553;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;756;-1817.291,1933.765;Inherit;True;OuterGradient;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;757;-2910.651,1401.411;Inherit;False;735;ShapeMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;744;-2648.188,1369.028;Inherit;True;CenterScale;-1;;34;d960bbb316065de47a9db2b86fdcf8b4;0;2;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;745;-2364.551,1388.027;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;742;-2466.946,1681.396;Inherit;False;Property;_InnerOffset;Offset;19;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;751;-2781.244,1815.613;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;662;1909.943,97.27531;Inherit;False;Property;_FillColor;Fill;20;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.05098036,0.5747563,0.972549,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;661;2054.991,493.2578;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;659;1672.833,562.5848;Inherit;False;Property;_StrokeColor;Stroke Color;21;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0.5347551,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;663;2339.363,251.8366;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;665;2692.743,315.2578;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;802;2007.027,-154.9067;Inherit;False;Property;_Dst;Dst;23;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;801;2008.67,-257.8357;Inherit;False;Property;_Src;Src;22;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;451;3071.588,250.4156;Float;False;True;-1;2;ASEMaterialInspector;0;3;Custom/Frame UI Shader Shape;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;True;True;4;1;True;_Src;1;True;_Dst;0;4;False;;1;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;True;True;True;True;True;0;True;_ColorMask;False;False;False;False;False;False;False;True;True;0;True;_Stencil;255;True;_StencilReadMask;255;True;_StencilWriteMask;0;True;_StencilComp;0;True;_StencilOp;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;0;True;unity_GUIZTestMode;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.ComponentMaskNode;709;-1596.071,-128.2535;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;687;-3284.111,-4.766;Inherit;False;Property;_Rotation;Rotation;4;0;Create;True;0;0;0;False;0;False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;690;-3010.11,1.234003;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;694;-2602.125,-142.4808;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;664;2566.076,477.6332;Inherit;False;Property;_EmissiveIntensity;Emissive Intensity;1;0;Create;True;0;0;0;False;0;False;1;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;832;2542.579,553.0086;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RotatorNode;831;2903.653,486.7689;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;0.2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;755;-1827.885,1409.041;Inherit;True;InnerGradient;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;692;-2855.962,-117.9595;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;691;-3155.36,-132.8662;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;2,2;False;1;FLOAT2;-1,-1;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;738;-3074.626,1545.629;Inherit;False;Property;_InnerScale;Scale 1;10;0;Create;False;0;0;0;False;0;False;5;23.44;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;748;-3014.973,1837.568;Inherit;False;Property;_OuterScale;Scale 2;15;0;Create;False;0;0;0;False;0;False;5;3.18;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;834;1584.786,271.7777;Inherit;False;Property;_Opacity;Opacity;2;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;833;1753.489,253.4442;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;778;544.6097,438.4799;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;780;261.4638,532.8448;Inherit;False;755;InnerGradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;779;764.2867,526.3567;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;769;281.3198,-8.796867;Inherit;False;756;OuterGradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;788;-198.7184,13.21823;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;792;-757.1517,-168.8011;Inherit;False;Property;_TextureOffset3;Texture Offset 1;12;0;Create;True;0;0;0;False;0;False;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;793;-765.9549,-320.8024;Inherit;False;Property;_Texturesize3;Texture size 1;11;0;Create;True;0;0;0;False;0;False;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;790;-749.6464,-3.582986;Inherit;False;Property;_SpeedTexture3;Speed Texture 1;13;0;Create;True;0;0;0;False;0;False;0,0;0,-0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;795;-242.2443,554.2382;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;796;-50.24453,394.2372;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;797;-340.6773,373.7795;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;799;-804.0165,220.2174;Inherit;False;Property;_Texturesize2;Texture size 2;16;0;Create;True;0;0;0;False;0;False;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;798;-795.2133,372.2188;Inherit;False;Property;_TextureOffset2;Texture Offset 2;17;0;Create;True;0;0;0;False;0;False;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;800;-787.7079,537.437;Inherit;False;Property;_SpeedTexture2;Speed Texture 2;18;0;Create;True;0;0;0;False;0;False;0,0;0,-0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;791;-297.1514,-167.2404;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;789;-1.715707,-168.7943;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;767;814.4048,-101.0502;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;765;649.8466,-122.4933;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;785;1413.24,360.489;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;770;305.3683,-206.4679;Inherit;True;Property;_OuterMask;Texture Sample 1;9;0;Create;False;0;0;0;False;1;Header(Texture 1);False;-1;None;69bed0415eef47e41907b5efd1ab1db7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;781;202.4686,333.1729;Inherit;True;Property;_OuterMask1;Texture Sample 2;14;0;Create;False;0;0;0;False;1;Header(Texture 2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;722;0;720;0
WireConnection;723;0;720;0
WireConnection;724;0;723;0
WireConnection;726;0;758;0
WireConnection;726;1;721;0
WireConnection;726;2;722;0
WireConnection;727;0;720;0
WireConnection;728;0;726;0
WireConnection;728;1;725;0
WireConnection;728;2;724;0
WireConnection;729;0;720;0
WireConnection;730;0;727;0
WireConnection;732;0;729;0
WireConnection;733;0;728;0
WireConnection;733;1;731;0
WireConnection;733;2;730;0
WireConnection;734;0;733;0
WireConnection;734;2;732;0
WireConnection;735;0;734;0
WireConnection;693;0;689;0
WireConnection;693;1;688;0
WireConnection;697;0;694;0
WireConnection;698;0;696;0
WireConnection;698;1;695;0
WireConnection;699;1;698;0
WireConnection;700;0;698;0
WireConnection;700;1;697;0
WireConnection;701;0;698;0
WireConnection;702;0;700;0
WireConnection;702;1;699;0
WireConnection;703;0;701;0
WireConnection;704;0;717;0
WireConnection;705;0;703;0
WireConnection;706;0;704;0
WireConnection;707;0;694;0
WireConnection;707;1;705;0
WireConnection;708;0;717;0
WireConnection;710;0;706;0
WireConnection;711;0;704;0
WireConnection;711;1;708;0
WireConnection;712;0;707;0
WireConnection;713;0;709;0
WireConnection;714;0;711;0
WireConnection;715;0;712;0
WireConnection;716;0;715;0
WireConnection;717;0;702;0
WireConnection;740;0;738;0
WireConnection;741;0;740;0
WireConnection;743;0;741;0
WireConnection;746;0;745;0
WireConnection;747;0;746;0
WireConnection;747;1;745;0
WireConnection;747;2;736;0
WireConnection;750;1;749;0
WireConnection;750;2;748;0
WireConnection;752;0;751;0
WireConnection;752;1;750;0
WireConnection;753;0;752;0
WireConnection;754;0;752;0
WireConnection;754;1;753;0
WireConnection;754;2;736;0
WireConnection;756;0;754;0
WireConnection;744;1;757;0
WireConnection;744;2;740;0
WireConnection;745;0;744;0
WireConnection;745;1;743;0
WireConnection;745;2;742;0
WireConnection;751;1;748;0
WireConnection;661;0;659;0
WireConnection;661;2;833;0
WireConnection;663;0;661;0
WireConnection;663;1;662;0
WireConnection;663;2;833;0
WireConnection;665;0;663;0
WireConnection;665;1;664;0
WireConnection;451;0;665;0
WireConnection;451;1;831;0
WireConnection;709;0;694;0
WireConnection;690;0;687;0
WireConnection;694;0;693;0
WireConnection;694;1;692;0
WireConnection;755;0;747;0
WireConnection;692;0;691;0
WireConnection;692;2;690;0
WireConnection;833;0;785;0
WireConnection;833;1;834;0
WireConnection;778;0;781;1
WireConnection;778;1;780;0
WireConnection;779;0;778;0
WireConnection;788;0;790;1
WireConnection;788;1;790;2
WireConnection;795;0;800;1
WireConnection;795;1;800;2
WireConnection;796;0;797;0
WireConnection;796;2;795;0
WireConnection;797;0;799;0
WireConnection;797;1;798;0
WireConnection;791;0;793;0
WireConnection;791;1;792;0
WireConnection;789;0;791;0
WireConnection;789;2;788;0
WireConnection;767;0;765;0
WireConnection;765;0;770;1
WireConnection;765;1;769;0
WireConnection;785;0;767;0
WireConnection;785;1;779;0
WireConnection;770;1;789;0
WireConnection;781;1;796;0
ASEEND*/
//CHKSM=8D1F285511DB4B40AD72353C9D2A790983BF11E2