Shader "OXRTK/BoundingBoxBorder" {
    Properties {
        _BorderWidth ("BorderWidth", Range(0, 0.5)) = 0.05
        _BorderOpacity ("BorderOpacity", Range(0, 1)) = 0.25
        _MainColor ("MainColor", Color) = (1,1,1,1)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _Stencil ("Stencil ID", Float) = 1

    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            Stencil {
                Ref [_Stencil]
                Pass Replace
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma target 3.0
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float, _BorderWidth)
                UNITY_DEFINE_INSTANCED_PROP( float4, _MainColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _BorderOpacity)
            
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
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );

                float4 _MainColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _MainColor );
                float _BorderOpacity_var = UNITY_ACCESS_INSTANCED_PROP( Props, _BorderOpacity );
                float _BorderWidth_var = UNITY_ACCESS_INSTANCED_PROP( Props, _BorderWidth );
                float frameCol = (1.0 - step((step(i.uv0.r,_BorderWidth_var)+(1.0 - step(i.uv0.r,(1.0 - _BorderWidth_var)))+step(i.uv0.g,_BorderWidth_var)+(1.0 - step(i.uv0.g,(1.0 - _BorderWidth_var)))),0.0));
                float3 emissive = (2.0*_MainColor_var.rgb*_BorderOpacity_var*frameCol);
           
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,frameCol);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
}