Shader "OXRTK/Standard/ModelHand" {
    Properties {
        _RimPower ("RimPower", Range(0, 4)) = 1.4
        [HideInInspector]_TriggerPos ("TriggerPos", Vector) = (0,0,0,0)
        [HideInInspector]_WavePow ("WavePow", Range(1, 200)) = 75
        _TriggerHighlightPower ("TriggerHighlightPower", Range(0, 10)) = 1
        _OpacityTex ("OpacityTex", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        [Toggle] _IsWhite("Is White", Float) = 0
        [Toggle] _BlackSeeThrough("Black ST", Float) = 0
        [Toggle] _UseRim("Use Rim", Float) = 0
        
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", Float) = 0

        [Toggle] _ZWrite ("ZWrite", Float) = 0
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
            // Blend SrcAlpha OneMinusSrcAlpha
            // Blend One One
            Blend [_SrcBlend] [_DstBlend]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma target 3.0
            uniform sampler2D _OpacityTex; uniform float4 _OpacityTex_ST;
            
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float, _RimPower)
                UNITY_DEFINE_INSTANCED_PROP( float4, _TriggerPos)
                UNITY_DEFINE_INSTANCED_PROP( float, _WavePow)
                UNITY_DEFINE_INSTANCED_PROP( float, _TriggerHighlightPower)
                UNITY_DEFINE_INSTANCED_PROP( float, _IsWhite)
                UNITY_DEFINE_INSTANCED_PROP( float, _BlackSeeThrough)
                UNITY_DEFINE_INSTANCED_PROP( float, _UseRim)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                
                float4 _TriggerPos_var = UNITY_ACCESS_INSTANCED_PROP( Props, _TriggerPos );
                float _WavePow_var = UNITY_ACCESS_INSTANCED_PROP( Props, _WavePow );
                float3 viewDirN = normalize(viewDirection);
                float3 col = _IsWhite? float3(.7, .7, .7) : float3(abs(viewDirN.r) * 0.47, (abs(viewDirN.g) * 0.25), abs(viewDirN.b) * 0.7);
                float _TriggerHighlightPower_var = UNITY_ACCESS_INSTANCED_PROP( Props, _TriggerHighlightPower );
                float _RimPower_var = UNITY_ACCESS_INSTANCED_PROP( Props, _RimPower );
                float3 emissive = _UseRim?
                    ((pow((1.0 - saturate(abs(distance(_TriggerPos_var.rgb,i.posWorld.rgb)))),_WavePow_var)*col*_TriggerHighlightPower_var)+(min(distance(i.posWorld.rgb,objPos.rgb),
                     pow((1.0 - max(min(dot(normalize(viewDirection),i.normalDir),1.0),0.0)),_RimPower_var))*(10.0*pow((1.0 - max(min(dot(normalize(viewDirection),i.normalDir),1.0),0.0)),
                         _RimPower_var)*col)))
                    :
                    pow(1.0 - saturate(abs(distance(_TriggerPos_var.rgb,i.posWorld.rgb))),_WavePow_var) * col * _TriggerHighlightPower_var
                ;
                float3 finalColor = emissive;
                float4 _OpacityTex_var = tex2D(_OpacityTex,TRANSFORM_TEX(i.uv0, _OpacityTex));
                return fixed4(finalColor, _BlackSeeThrough ? min(_OpacityTex_var.r, finalColor.r) : _OpacityTex_var.r);
            }
            ENDCG
        }
    }
}
