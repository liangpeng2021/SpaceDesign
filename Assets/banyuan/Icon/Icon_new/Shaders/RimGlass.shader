
Shader "CloudShaodw/RimGlass"
{
	Properties
	{
		_FrostTex ("Fross Texture", 2D) = "white" {}
		//_FrostIntensity ("Frost Intensity", Range(0.0, 1.0)) = 0.5
		_Diffuse ("Diffuse", Color) = (1, 1, 1, 1)
		_DiffuseStrength("Diffuse Strength",Float) = 1
		_Specular ("Specular", Color) = (1, 1, 1, 1)
		_Gloss ("Gloss", Range(8.0, 256)) = 20
		_Cubemap ("Reflection Cubemap", Cube) = "_Skybox" {}
		_BlurStrength("Blur Strength",Float) = 1
		_CubemapReflectionStren("Cubemap Reflection Strength",Float) = 3
		// _Light1Pos("Light1 Pos", Vector) = (0,0,0,0)
		// _Light1Color ("_Light1Color", Color) = (1, 1, 1, 1)
		// _light1Range("Range",Float) = 1
		_Emission ("Emission", 2D) = "black" {}
        _EmissStr ("Emiss Strength", Range(0,20)) = 1
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0,20)) = 1
		
	}
	SubShader
	{
		Tags { "LightMode"="ForwardBase"  "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 100
		//Blend One One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			fixed4 _Diffuse;
			fixed4 _Specular;
			float _Gloss;
			samplerCUBE _Cubemap;
			float _DiffuseStrength;
			float _BlurStrength;
			float _CubemapReflectionStren;
			// float4 _Light1Pos;
			// fixed4 _Light1Color;
			// float _light1Range;

			sampler2D _Emission;
			half _EmissStr;
			half _RimPower;
			fixed4 _RimColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uvfrost : TEXCOORD0;
				float4 uvgrab : TEXCOORD1; 
				float3 worldNormal : TEXCOORD2; 
				float3 worldPos : TEXCOORD3;
				fixed3 worldRefl : TEXCOORD4;
				float4 vertex : SV_POSITION;
			};

			sampler2D _FrostTex;
			float4 _FrostTex_ST;

			//float _FrostIntensity;

			sampler2D _GrabBlurTexture_0;
			sampler2D _GrabBlurTexture_1;
			sampler2D _GrabBlurTexture_2;
			sampler2D _GrabBlurTexture_3;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvfrost = TRANSFORM_TEX(v.uv, _FrostTex);
				o.uvgrab = ComputeGrabScreenPos(o.vertex);
				o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 worldViewDir = UnityWorldSpaceViewDir(o.worldPos);
				o.worldRefl = reflect(-worldViewDir, o.worldNormal);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 surfSmooth = tex2D(_FrostTex, i.uvfrost);
				

				half4 refraction;

				half4 ref00 = tex2Dproj(_GrabBlurTexture_0, i.uvgrab);
				half4 ref01 = tex2Dproj(_GrabBlurTexture_1, i.uvgrab);
				half4 ref02 = tex2Dproj(_GrabBlurTexture_2, i.uvgrab);
				half4 ref03 = tex2Dproj(_GrabBlurTexture_3, i.uvgrab);

				float step00 = smoothstep(0.75, 1.00, surfSmooth.a * _BlurStrength);
				float step01 = smoothstep(0.5, 0.75, surfSmooth.a * _BlurStrength);
				float step02 = smoothstep(0.05, 0.5, surfSmooth.a * _BlurStrength);
				float step03 = smoothstep(0.00, 0.05, surfSmooth.a * _BlurStrength);

				refraction = lerp(ref03, lerp( lerp( lerp(ref03, ref02, step02), ref01, step01), ref00, step00), step03);

				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);

				// Compute diffuse term
				fixed3 diffuse = _LightColor0.rgb * surfSmooth.rgb*_Diffuse.rgb * max(0, dot(worldNormal, worldLightDir));

				// float dis = distance(_Light1Pos, i.worldPos.xyz);
				// fixed3 light1D = _Light1Color.rgb * _Diffuse.rgb * max(0, dot(worldNormal, _Light1Pos - i.worldPos.xyz)) * (dis/_light1Range);

				// Get the view direction in world space
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				// Get the half direction in world space
				fixed3 halfDir = normalize(worldLightDir + viewDir);
				// Compute specular term
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(worldNormal, halfDir)), _Gloss);
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				fixed3 reflection = texCUBE(_Cubemap, i.worldRefl).rgb;

				fixed3 emiss = tex2D(_Emission, i.uvfrost);

				fixed rim = 1.0 - saturate(dot (normalize(viewDir), worldNormal));

				fixed3 result = (ambient + diffuse + specular)*_DiffuseStrength + refraction*0.5 + reflection*_CubemapReflectionStren;
				result += emiss*_EmissStr;
				result += pow(rim, _RimPower) * _RimColor;
				//result = reflection;
				
				return fixed4(result, surfSmooth.a);
			}
			ENDCG
		}

		}

	
	FallBack "Mobile/Diffuse"
}
 