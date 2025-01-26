// Made with Amplify Shader Editor v1.9.8.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water"
{
	Properties
	{
		_Color("Color", Color) = (0,0,0,0)
		_Smoothness("Smoothness", Float) = 0
		_Metallic("Metallic", Float) = 0
		_NormalMap("NormalMap", 2D) = "bump" {}
		_TimeScale("TimeScale", Float) = 0
		_Vector1("Vector1", Vector) = (0,0,0,0)
		_Vector2("Vector2", Vector) = (0,0,0,0)
		_Strength("Strength", Float) = 1
		_Wavelength("Wavelength", Float) = 0.2
		_Period("Period", Float) = 1
		_Amplitude("Amplitude", Float) = 0.1
		_DistanceFalloff("DistanceFalloff", Float) = 0
		_PourPosition("PourPosition", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.5
		#define ASE_VERSION 19801
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _Period;
		uniform float3 _PourPosition;
		uniform float _Wavelength;
		uniform float _Amplitude;
		uniform float _DistanceFalloff;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _TimeScale;
		uniform float2 _Vector1;
		uniform float2 _Vector2;
		uniform float _Strength;
		uniform float4 _Color;
		uniform float _Metallic;
		uniform float _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_positionOS = v.vertex.xyz;
			float3 temp_output_55_0 = ( _PourPosition - ase_positionOS );
			float temp_output_56_0 = length( temp_output_55_0 );
			float temp_output_40_0 = ( ( _Time.y / _Period ) + ( temp_output_56_0 / _Wavelength ) );
			float temp_output_52_0 = ( _Amplitude * ( 1.0 / ( ( temp_output_56_0 * _DistanceFalloff ) + 1.0 ) ) );
			v.vertex.xyz += ( sin( temp_output_40_0 ) * float3(0,1,0) * temp_output_52_0 );
			v.vertex.w = 1;
			float3 ase_normalOS = v.normal.xyz;
			float3 normalizeResult59 = normalize( ( ase_normalOS + ( cos( temp_output_40_0 ) * temp_output_55_0 * temp_output_52_0 * ( 1.0 / _Wavelength ) ) ) );
			v.normal = normalizeResult59;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float temp_output_18_0 = ( _TimeScale * _Time.y );
			float3 break25 = ( UnpackNormal( tex2D( _NormalMap, ( uv_NormalMap + ( temp_output_18_0 * _Vector1 ) ) ) ) + UnpackNormal( tex2D( _NormalMap, ( uv_NormalMap + ( temp_output_18_0 * _Vector2 ) ) ) ) );
			float3 appendResult28 = (float3(( break25.x * _Strength ) , ( break25.y * _Strength ) , ( break25.z * -1.0 )));
			o.Normal = appendResult28;
			o.Albedo = _Color.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
}
/*ASEBEGIN
Version=19801
Node;AmplifyShaderEditor.PosVertexDataNode;32;-720,1008;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;64;-720,800;Inherit;False;Property;_PourPosition;PourPosition;12;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;13;-1760,784;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-1760,704;Inherit;False;Property;_TimeScale;TimeScale;4;0;Create;True;0;0;0;False;0;False;0;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;55;-416,848;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-1616,432;Inherit;True;Property;_NormalMap;NormalMap;3;0;Create;True;0;0;0;False;0;False;None;20d12a1712149664a9bfa845e47156a9;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1520,736;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;17;-1488,976;Inherit;False;Property;_Vector2;Vector2;6;0;Create;True;0;0;0;False;0;False;0,0;-1,-0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;16;-1504,848;Inherit;False;Property;_Vector1;Vector1;5;0;Create;True;0;0;0;False;0;False;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LengthOpNode;56;-224,912;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;160,1248;Inherit;False;Property;_DistanceFalloff;DistanceFalloff;11;0;Create;True;0;0;0;False;0;False;0;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;30;-304,688;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-272,768;Inherit;False;Property;_Period;Period;9;0;Create;True;0;0;0;False;0;False;1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1376,560;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-1152,704;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-1136,864;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;368,1120;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-256,1024;Inherit;False;Property;_Wavelength;Wavelength;8;0;Create;True;0;0;0;False;0;False;0.2;-0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;46;-80,720;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;38;-16,912;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;10;-864,480;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-848,672;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;49;544,1120;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-704,368;Inherit;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;8;-688,608;Inherit;True;Property;_TextureSample1;Texture Sample 0;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleDivideOpNode;50;672,1104;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;336,1008;Inherit;False;Property;_Amplitude;Amplitude;10;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;40;176,816;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-336,480;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CosOpNode;51;448,720;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;880,992;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;63;416,512;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;25;-192,416;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;22;-240,288;Inherit;False;Property;_Strength;Strength;7;0;Create;True;0;0;0;False;0;False;1;3.55;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;1072,544;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalVertexDataNode;58;896,256;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-48,320;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-48,416;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-16,528;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;43;448,832;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;34;800,832;Inherit;False;Constant;_Vector0;Vector 0;8;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;57;1232,304;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;2;-544,-80;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.1206239,0.2690177,0.3584904,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;4;-496,112;Inherit;False;Property;_Metallic;Metallic;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-528,192;Inherit;False;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;0;False;0;False;0;0.36;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;28;144,384;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;1088,720;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;59;1376,224;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1632,-128;Float;False;True;-1;3;AmplifyShaderEditor.MaterialInspector;0;0;Standard;Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;55;0;64;0
WireConnection;55;1;32;0
WireConnection;18;0;15;0
WireConnection;18;1;13;0
WireConnection;56;0;55;0
WireConnection;9;2;7;0
WireConnection;19;0;18;0
WireConnection;19;1;16;0
WireConnection;20;0;18;0
WireConnection;20;1;17;0
WireConnection;48;0;56;0
WireConnection;48;1;47;0
WireConnection;46;0;30;0
WireConnection;46;1;36;0
WireConnection;38;0;56;0
WireConnection;38;1;35;0
WireConnection;10;0;9;0
WireConnection;10;1;19;0
WireConnection;11;0;9;0
WireConnection;11;1;20;0
WireConnection;49;0;48;0
WireConnection;6;0;7;0
WireConnection;6;1;10;0
WireConnection;8;0;7;0
WireConnection;8;1;11;0
WireConnection;50;1;49;0
WireConnection;40;0;46;0
WireConnection;40;1;38;0
WireConnection;21;0;6;0
WireConnection;21;1;8;0
WireConnection;51;0;40;0
WireConnection;52;0;45;0
WireConnection;52;1;50;0
WireConnection;63;1;35;0
WireConnection;25;0;21;0
WireConnection;53;0;51;0
WireConnection;53;1;55;0
WireConnection;53;2;52;0
WireConnection;53;3;63;0
WireConnection;26;0;25;0
WireConnection;26;1;22;0
WireConnection;27;0;25;1
WireConnection;27;1;22;0
WireConnection;29;0;25;2
WireConnection;43;0;40;0
WireConnection;57;0;58;0
WireConnection;57;1;53;0
WireConnection;28;0;26;0
WireConnection;28;1;27;0
WireConnection;28;2;29;0
WireConnection;33;0;43;0
WireConnection;33;1;34;0
WireConnection;33;2;52;0
WireConnection;59;0;57;0
WireConnection;0;0;2;5
WireConnection;0;1;28;0
WireConnection;0;3;4;0
WireConnection;0;4;3;0
WireConnection;0;11;33;0
WireConnection;0;12;59;0
ASEEND*/
//CHKSM=06E34EAD1DC1CA02AB386477C831EAE8576B0F45