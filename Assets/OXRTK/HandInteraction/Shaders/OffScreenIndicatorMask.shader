Shader "OXRTK/OffScreenIndicatorMask" {
    Properties {
        _thickness ("thickness", Range(0, 1)) = 0.3
        _mag ("mag", Range(0, 1)) = 0.1
        _base ("base", Range(0, 1)) = 0.04
        _magnifier ("magnifier", Range(1, 10)) = 6
        _XMove ("XMove", Range(0, 1)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma target 3.0

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float, _thickness)
                UNITY_DEFINE_INSTANCED_PROP( float, _mag)
                UNITY_DEFINE_INSTANCED_PROP( float, _base)
                UNITY_DEFINE_INSTANCED_PROP( float, _magnifier)
                UNITY_DEFINE_INSTANCED_PROP( float, _XMove)
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

                float _XMove_var = UNITY_ACCESS_INSTANCED_PROP( Props, _XMove );
                float offset = (i.uv0.r+_XMove_var);
                float _magnifier_var = UNITY_ACCESS_INSTANCED_PROP( Props, _magnifier );
                
                float rightEnd = (_magnifier_var + 0.5);
                float leftEnd = (_magnifier_var - 0.5);
                
                float _mag_var = UNITY_ACCESS_INSTANCED_PROP( Props, _mag );
                float _base_var = UNITY_ACCESS_INSTANCED_PROP( Props, _base );
                float _thickness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _thickness );

                
                float sineShape = step(i.uv0.g, (cos(2.0 * 3.141592654 * fmod(clamp(max(1.0 - fmod(offset,1.0), fmod(offset,1.0)) * _magnifier_var, leftEnd, rightEnd) - rightEnd,1.0)) * (-1.0) *_mag_var + _mag_var+_base_var) * _thickness_var);
                
                fixed4 finalCol = fixed4(sineShape, sineShape, sineShape, sineShape);

                #ifdef UNITY_UI_ALPHACLIP
                clip (finalCol.a - 0.001);
                #endif

                return finalCol * 0.001;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
