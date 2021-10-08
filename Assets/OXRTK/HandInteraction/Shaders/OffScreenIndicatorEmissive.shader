Shader "OXRTK/OffScreenIndicatorEmissive" {
    Properties {
        _thickness ("thickness", Range(0, 1)) = 0.3
        _mag ("mag", Range(0, 1)) = 0.4
        _base ("base", Range(0, 1)) = 0.9
        _magnifier ("magnifier", Range(1, 10)) = 1.6
        [HideInInspector] _XMove ("XMove", Range(0, 1)) = 0
        _GStrength ("GStrength", Range(0, 10)) = 4
        _Gradient ("Gradient", Range(0, 1)) = 1
        _EmissionPow ("EmissionPow", Range(1, 5)) = 2
        [HDR]_EmissionPower ("EmissionPower", Color) = (0, 1, 1, 1)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma target 3.0
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float, _thickness)
                UNITY_DEFINE_INSTANCED_PROP( float, _mag)
                UNITY_DEFINE_INSTANCED_PROP( float, _base)
                UNITY_DEFINE_INSTANCED_PROP( float, _magnifier)
                UNITY_DEFINE_INSTANCED_PROP( float, _XMove)
                UNITY_DEFINE_INSTANCED_PROP( float, _GStrength)
                UNITY_DEFINE_INSTANCED_PROP( float, _Gradient)
                UNITY_DEFINE_INSTANCED_PROP( float, _EmissionPow)
                UNITY_DEFINE_INSTANCED_PROP( float4, _EmissionPower)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                
                float4 _EmissionPower_var = UNITY_ACCESS_INSTANCED_PROP( Props, _EmissionPower );
                float _XMove_var = UNITY_ACCESS_INSTANCED_PROP( Props, _XMove );
                float offset = fmod(i.uv0.r+_XMove_var,1.0);
                float _magnifier_var = UNITY_ACCESS_INSTANCED_PROP( Props, _magnifier );
                float _GStrength_var = UNITY_ACCESS_INSTANCED_PROP( Props, _GStrength );
                float _Gradient_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Gradient );
                float _EmissionPow_var = UNITY_ACCESS_INSTANCED_PROP( Props, _EmissionPow );
                float _mag_var = UNITY_ACCESS_INSTANCED_PROP( Props, _mag );
                float _base_var = UNITY_ACCESS_INSTANCED_PROP( Props, _base );
                float _thickness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _thickness );
                
                float waveShape = ((cos(2.0*3.141592654*fmod(clamp(max(1.0 - offset, offset) * _magnifier_var, _magnifier_var - 0.5, _magnifier_var + 0.5) - _magnifier_var + 0.5, 1.0))*(-1.0)*_mag_var)+_mag_var+_base_var) * _thickness_var;
                float opacity = step(i.uv0.g, waveShape).r;
                float emission = (waveShape - pow(i.uv0.g * _GStrength_var, _Gradient_var)) * _EmissionPow_var * opacity;
                float3 emissive = (_EmissionPower_var.rgb * emission);
                float3 finalColor = emissive;
                return fixed4(finalColor, opacity);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
