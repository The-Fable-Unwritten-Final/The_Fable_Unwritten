// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Frame Shader Sprite"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
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
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 0

	}

	SubShader
	{
		LOD 0

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

		Cull Off
		Lighting Off
		ZWrite Off
		Blend [_Src] [_Dst]
		
		
		Pass
		{
		CGPROGRAM
			
			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				
			};
			
			uniform fixed4 _Color;
			uniform float _EnableExternalAlpha;
			uniform sampler2D _MainTex;
			uniform sampler2D _AlphaTex;
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
			uniform sampler2D _DistortionMaskTexture;
			uniform float2 _DistortionMaskScale;
			uniform float _DistortionMaskIntensity;
			uniform float _Opacity;
			uniform float _Mask_Power;
			uniform float _Mask_Multiply;
			uniform float4 _FillColor;
			uniform float _EmissiveIntensity;

			
			v2f vert( appdata_t IN  )
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				
				
				IN.vertex.xyz +=  float3(0,0,0) ; 
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				fixed4 alpha = tex2D (_AlphaTex, uv);
				color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}
			
			fixed4 frag(v2f IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

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
				float temp_output_888_0 = ( ( noises876 * _NoisesOpacityBoost ) * saturate( ( pow( ( tex2D( _MainTex, ( ( tex2D( _DistortionMaskTexture, ( texCoord849 * _DistortionMaskScale ) ).r * ( Distortion839 * _DistortionMaskIntensity ) ) + texCoord861 ) ).r * _Opacity ) , _Mask_Power ) * _Mask_Multiply ) ) );
				float4 lerpResult899 = lerp( _StrokeColor , float4( 0,0,0,0 ) , temp_output_888_0);
				float4 lerpResult895 = lerp( lerpResult899 , _FillColor , temp_output_888_0);
				
				fixed4 c = ( lerpResult895 * _EmissiveIntensity );
				c.rgb *= c.a;
				return c;
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
Node;AmplifyShaderEditor.CommentaryNode;808;-521.0283,2115.245;Inherit;False;2391;470;Flame Mask;13;874;871;867;866;865;864;862;861;858;853;887;906;907;;0,0,0,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;820;-473.0283,-14.58423;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;821;422.9717,241.4158;Inherit;False;819;windSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;822;38.97168,113.4158;Inherit;False;Property;_NoiseDistortion_Speed;NoiseDistortion_Speed;20;0;Create;True;0;0;0;False;0;False;-0.3,-0.3;0,0.05;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;824;422.9717,-14.58423;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;825;1062.972,113.4158;Inherit;False;Constant;_Float0;Float 0;8;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;827;1318.972,113.4158;Inherit;False;Property;_DistortionAmount;Distortion Amount;18;0;Create;True;0;0;0;False;0;False;1;-2.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;828;1062.972,-14.58423;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;829;1318.972,-14.58423;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;855;38.97168,1265.416;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;859;294.9717,1009.416;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;861;-89.02832,2163.245;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0.28,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;866;1062.972,2163.245;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;867;1446.972,2291.245;Inherit;False;Property;_Mask_Multiply;Mask_Multiply;4;0;Create;True;0;0;0;False;0;False;1;1.51;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;871;1446.972,2163.245;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;874;1702.972,2163.245;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;875;-473.0283,1265.416;Inherit;False;839;Distortion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;839;1574.972,-14.58423;Inherit;False;Distortion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;853;-473.0283,2163.245;Inherit;False;Constant;_Mask_Scale;Mask_Scale;2;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;858;-473.0283,2419.245;Inherit;False;Constant;_Mask_Offset;Mask_Offset;1;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;851;-473.0283,1393.416;Inherit;False;Property;_DistortionMaskIntensity;Distortion Mask Intensity;15;0;Create;True;0;0;0;False;0;False;1;-0.63;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;830;-404.5765,3348.445;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;831;-404.5765,3476.445;Inherit;False;Property;_Noise_02_Scale;Noise_02_Scale;12;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;832;-404.5765,2836.445;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;833;-404.5765,2964.445;Inherit;False;Property;_Noise_01_Scale;Noise_01_Scale;9;0;Create;True;0;0;0;False;0;False;0.8,0.8;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;834;-148.5762,3348.445;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;835;235.4241,3604.445;Inherit;False;819;windSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;836;-20.57591,3476.445;Inherit;False;Property;_Noise_02_Speed;Noise_02_Speed;13;0;Create;True;0;0;0;False;0;False;-0.2,0.4;0,-0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;837;-148.5762,2836.445;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;838;-20.57591,2964.445;Inherit;False;Property;_Noise_01_Speed;Noise_01_Speed;10;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;840;235.4241,3092.445;Inherit;False;819;windSpeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;841;235.4241,3348.445;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;842;491.4243,3604.445;Inherit;False;839;Distortion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;843;235.4241,2836.445;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;844;491.4243,3092.445;Inherit;False;839;Distortion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;845;491.4243,3348.445;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;846;491.4243,2836.445;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;850;1131.424,2836.445;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;852;1387.425,2964.445;Inherit;False;Property;_Noises_Power;Noises_Power;7;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;856;1643.425,2964.445;Inherit;False;Property;_Noises_Multiply;Noises_Multiply;6;0;Create;True;0;0;0;False;0;False;5;2.06;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;857;1387.425,2836.445;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;860;1643.425,2836.445;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;863;1899.425,2836.445;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;876;2155.424,2836.445;Inherit;False;noises;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;848;747.4243,2836.445;Inherit;True;Property;_Noise_01_Texture;Noise_01_Texture;8;1;[Header];Create;True;1;Header(Noise Texture 1);0;0;False;0;False;-1;None;cb0857c26faeebb449e28a5304450c7e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;847;747.4243,3348.445;Inherit;True;Property;_Noise_02_Texture;Noise_02_Texture;11;0;Create;True;0;0;0;False;1;Header(Noise Texture 2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;816;-1339.945,1260.762;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;817;-1083.946,1004.763;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;819;-827.9456,1004.763;Inherit;False;windSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;826;678.9717,-14.58423;Inherit;True;Property;_NoiseDistortion_Texture;NoiseDistortion_Texture;17;0;Create;True;0;0;0;False;1;Header(Distortion Texture);False;-1;None;33d4a85877826c14d83910d65644473a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;864;1062.972,2291.245;Inherit;False;Property;_Mask_Power;Mask_Power;3;0;Create;True;0;0;0;False;0;False;1;0.98;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;854;-89.02832,1009.416;Inherit;True;Property;_DistortionMaskTexture;Distortion Mask Texture;14;0;Create;True;0;0;0;False;1;Header(Distortion Mask Texture);False;-1;None;775f7c4cf85b8d94a8fd6219e5b0c006;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;815;-1339.945,1004.763;Inherit;False;Property;_WindAllSpeed;Wind All Speed;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;818;-473.0283,113.4158;Inherit;False;Property;_NoiseDistortion_Scale;NoiseDistortion_Scale;19;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;823;-217.0283,-14.58423;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;905;-266.6944,1004.524;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;904;-505.6941,1129.524;Inherit;False;Property;_DistortionMaskScale;Distortion Mask Scale;16;0;Create;True;0;0;0;False;0;False;1,1;3,3;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;849;-495.1526,999.1309;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;865;680.9174,2264.422;Inherit;True;Property;_Mask_Texture;Mask_Texture;5;0;Create;True;0;0;0;False;0;False;-1;None;48dc7bc18268b924c8348d314decc5da;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;862;389.8947,2297.5;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;887;463.4905,2190.969;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;888;2553.194,2125.707;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;898;2191.61,2065.74;Inherit;False;Property;_StrokeColor;Stroke Color;22;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0.2494924,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;899;2756.836,1920.964;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;895;3227.593,2067.395;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;897;2767.405,1721.232;Inherit;False;Property;_FillColor;Fill;21;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0.5689322,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;896;3317.265,1859.566;Inherit;False;Property;_EmissiveIntensity;Emissive Intensity;2;0;Create;True;0;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;900;3558.306,2052.553;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;689;3836.04,2082.142;Float;False;True;-1;2;ASEMaterialInspector;0;10;Custom/Frame Shader Sprite;0f8ba0101102bb14ebf021ddadce9b49;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;True;4;1;True;_Src;1;True;_Dst;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.RangedFloatNode;901;3712.678,2272.614;Inherit;False;Property;_Src;Src;23;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;902;3720.365,2367.101;Inherit;False;Property;_Dst;Dst;24;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;872;2351.572,2312.544;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;870;2123.598,2280.736;Inherit;False;876;noises;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;868;2071.032,2398.073;Inherit;False;Property;_NoisesOpacityBoost;Noises Opacity Boost;5;0;Create;True;0;0;0;False;1;Header(Noise Setting);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;906;980.1964,2430.976;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;907;803.1964,2459.976;Inherit;False;Property;_Opacity;Opacity;0;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
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
WireConnection;866;0;906;0
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
WireConnection;865;0;887;0
WireConnection;865;1;862;0
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
WireConnection;689;0;900;0
WireConnection;872;0;870;0
WireConnection;872;1;868;0
WireConnection;906;0;865;1
WireConnection;906;1;907;0
ASEEND*/
//CHKSM=40215841489A6DA7EA8CE93AF71233D77307A716