// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "shuiliu3"
{
	Properties
	{
		_MainTexture1("Main Texture", 2D) = "white" {}
		_TextureSample1("Texture Sample 0", 2D) = "white" {}
		_DistortTexture1("Distort Texture", 2D) = "bump" {}
		[HDR]_TintColor1("Tint Color", Color) = (1,0.4196078,0,1)
		_Speed1("Speed", Float) = 0
		_UVDistortIntensity1("UV Distort Intensity", Range( 0 , 0.04)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "UnityStandardUtils.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
			};

			uniform float4 _TintColor1;
			uniform sampler2D _MainTexture1;
			uniform float _Speed1;
			uniform float _UVDistortIntensity1;
			uniform sampler2D _DistortTexture1;
			uniform float4 _DistortTexture1_ST;
			uniform sampler2D _TextureSample1;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				float mulTime3 = _Time.y * _Speed1;
				float2 panner4 = ( mulTime3 * float2( -1,-1 ) + float2( 0,0 ));
				float2 uv07 = i.ase_texcoord.xy * float2( 1,1 ) + panner4;
				float2 uv_DistortTexture1 = i.ase_texcoord.xy * _DistortTexture1_ST.xy + _DistortTexture1_ST.zw;
				float3 tex2DNode9 = UnpackScaleNormal( tex2D( _DistortTexture1, uv_DistortTexture1 ), _UVDistortIntensity1 );
				float mulTime2 = _Time.y * _Speed1;
				float2 panner5 = ( mulTime2 * float2( 1,0.5 ) + float2( 0,0 ));
				float2 uv08 = i.ase_texcoord.xy * float2( 1,1 ) + panner5;
				
				
				finalColor = ( _TintColor1 * ( tex2D( _MainTexture1, ( float3( uv07 ,  0.0 ) + tex2DNode9 ).xy ) * tex2D( _TextureSample1, ( float3( uv08 ,  0.0 ) + tex2DNode9 ).xy ) ) );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=17700
0;0;1920;1019;1277.476;514.322;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;1;-2439.342,233.4319;Float;False;Property;_Speed1;Speed;4;0;Create;True;0;0;False;0;0;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;2;-1761.326,664.7101;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;3;-1683.003,62.80302;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;4;-1489.123,-76.88995;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,-1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;5;-1533.191,537.095;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,0.5;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1955.927,280.8602;Float;False;Property;_UVDistortIntensity1;UV Distort Intensity;5;0;Create;True;0;0;False;0;0;0.04;0;0.04;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-1311.793,528.8093;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-1264.817,-78.32233;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;9;-1597.317,264.6855;Inherit;True;Property;_DistortTexture1;Distort Texture;2;0;Create;True;0;0;False;0;-1;None;0bebe40e9ebbecc48b8e9cfea982da7e;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;10;-1017.685,15.56063;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-1052.746,623.1883;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;12;-896.0479,-7.522364;Inherit;True;Property;_MainTexture1;Main Texture;0;0;Create;True;0;0;False;0;-1;None;cd460ee4ac5c1e746b7a734cc7cc64dd;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;13;-889.2264,176.3279;Inherit;True;Property;_TextureSample1;Texture Sample 0;1;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-569.6248,68.92202;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;14;-891.1586,-239.2981;Float;False;Property;_TintColor1;Tint Color;3;1;[HDR];Create;True;0;0;False;0;1,0.4196078,0,1;1.064,0.4172549,0,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-340.8248,-12.50097;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;100;1;shuiliu3;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;0
WireConnection;2;0;1;0
WireConnection;3;0;1;0
WireConnection;4;1;3;0
WireConnection;5;1;2;0
WireConnection;8;1;5;0
WireConnection;7;1;4;0
WireConnection;9;5;6;0
WireConnection;10;0;7;0
WireConnection;10;1;9;0
WireConnection;11;0;8;0
WireConnection;11;1;9;0
WireConnection;12;1;10;0
WireConnection;13;1;11;0
WireConnection;15;0;12;0
WireConnection;15;1;13;0
WireConnection;16;0;14;0
WireConnection;16;1;15;0
WireConnection;0;0;16;0
ASEEND*/
//CHKSM=B506B4D2A8CC6ACD1B1D37B07FBE14AFBE71241B