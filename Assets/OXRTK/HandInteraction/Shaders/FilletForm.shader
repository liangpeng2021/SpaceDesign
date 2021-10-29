Shader "OXRTK/FilletForm" {
    Properties {
        _Thickness ("Thickness", Range(0, 1)) = 0.1437185
        _filletScale ("filletScale", Range(0, 0.5)) = 0.1581197
        _Color ("Color", Color) = (1,1,1,1)
        _opacity ("opacity", Range(0, 1)) = 1
        [MaterialToggle] _solid ("solid", Float ) = 1
        _MainTex ("MainTex", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    	
    	_Stencil ("Stencil ID", Float) = 0
    	_ZTestMode("ZTest", Float) = 4
    }
    SubShader {
        Tags {
            // "IgnoreProjector"="True"
            /*"Queue"="Transparent-1"
            "RenderType"="Transparent"*/
        	"Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
                // "LightMode"="Always"
            }
	        Blend SrcAlpha OneMinusSrcAlpha
            
            ZWrite Off
            ZTest [_ZTestMode]

        	Stencil {
                Ref [_Stencil]
                Pass Replace
                // Pass Keep
            }
        	
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
           
            #pragma target 3.0
            
            float FrameWorldH( float2 uv , float3 oscale , float thickness , float filletSize , float isSolid )
            {
	            float sp = oscale.x/oscale.y;
	            
	            float d = filletSize * min(oscale.x, oscale.y);
	            
	            float2 lbLocal = float2(d, d)/float2(oscale.x, oscale.y);
	            float2 rbLocal = float2(oscale.x-d, d)/float2(oscale.x, oscale.y);
	            float2 ltLocal = float2(d, oscale.y-d)/float2(oscale.x, oscale.y);
	            float2 rtLocal = float2(oscale.x-d, oscale.y-d)/float2(oscale.x, oscale.y);
	            
	            
	            float leftBottom = step(uv.x, lbLocal.x) * step(uv.y, lbLocal.y);
	            float rightBottom = step(rbLocal.x, uv.x) * step(uv.y, rbLocal.y);
	            float leftTop = step(uv.x, ltLocal) * step(ltLocal.y, uv.y);
	            float rightTop = step(rtLocal.x, uv.x) * step(rtLocal.y, uv.y);
	            
	            float onCorner = leftBottom + rightBottom + leftTop + rightTop;
	            
	            float lr,tb,sides; 
	            
	            float2 coeff = sp>1? float2(sp,1) : float2(1,1/sp);
	            
	            
	            float lbFillet = leftBottom==0? 0:
            		step(distance(uv*coeff, lbLocal*coeff), filletSize);
	            
	            float rbFillet = rightBottom==0? 0:
            		step(distance(uv*coeff, rbLocal*coeff), filletSize);
	            
	            float ltFillet = leftTop==0? 0:
            			step(distance(uv*coeff, ltLocal*coeff), filletSize);
	            
	            float rtFillet = rightTop==0? 0:
            		step(distance(uv*coeff, rtLocal*coeff), filletSize);
	            
	            float fillet = lbFillet + rbFillet + ltFillet + rtFillet;
	            
	            //////// Calculate Void Area when needed
	            if(isSolid==1)
	            {
	            
            		lr = sp>1?
                 		(onCorner!=0? 0:(step(uv.x*sp, thickness) + step(1-thickness/sp, uv.x))) 
                 		:
                 		(onCorner!=0? 0:(step(uv.x, thickness) + step(1-thickness, uv.x)));
	            
            		tb = sp>1?
            		(onCorner!=0? 0:(step(uv.y, thickness) + step(1-thickness, uv.y))) 
                 		:
                 		(onCorner!=0? 0:(step(uv.y/sp, thickness) + step(1-thickness*sp, uv.y)));
	            
            		sides = lr+tb;
	            
            		////////////////
            		float lbFilletVoid = leftBottom==0? 0:
            		step(distance(uv*coeff, lbLocal*coeff), filletSize-thickness*(sp>1? 1:coeff));
	            
            		float rbFilletVoid = rightBottom==0? 0:
            		step(distance(uv*coeff, rbLocal*coeff), filletSize-thickness*(sp>1? 1:coeff));
	            
            		float ltFilletVoid = leftTop==0? 0:
            			step(distance(uv*coeff, ltLocal*coeff), filletSize-thickness*(sp>1? 1:coeff));
	            
            		float rtFilletVoid = rightTop==0? 0:
            							step(distance(uv*coeff, rtLocal*coeff), filletSize-thickness*(sp>1? 1:coeff));
	            
            		float fVoid = lbFilletVoid + rbFilletVoid + ltFilletVoid + rtFilletVoid;
            		fillet -= fVoid;
	            }
	            else
	            {	
            		sides = onCorner!=0? 0:1;
	            }
	            ////////
	            
	            float final = clamp(sides + fillet,0,1);
	            
	            return final;
            }
            
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float, _Thickness)
                UNITY_DEFINE_INSTANCED_PROP( float, _filletScale)
                UNITY_DEFINE_INSTANCED_PROP( float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP( float, _opacity)
                UNITY_DEFINE_INSTANCED_PROP( fixed, _solid)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                float3 recipObjScale = float3( length(unity_WorldToObject[0].xyz), length(unity_WorldToObject[1].xyz), length(unity_WorldToObject[2].xyz) );
                float3 objScale = 1.0/recipObjScale;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                float3 recipObjScale = float3( length(unity_WorldToObject[0].xyz), length(unity_WorldToObject[1].xyz), length(unity_WorldToObject[2].xyz) );
                float3 objScale = 1.0/recipObjScale;
                float _Thickness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Thickness );
                float _filletScale_var = UNITY_ACCESS_INSTANCED_PROP( Props, _filletScale );
                float _solid_var = UNITY_ACCESS_INSTANCED_PROP( Props, _solid );
                float finalForm = FrameWorldH( i.uv0 , objScale , _Thickness_var , _filletScale_var , _solid_var );
                clip(finalForm - 0.5);

                float4 _Color_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Color );
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 emissive = (_Color_var.rgb*finalForm*i.vertexColor.rgb*_MainTex_var.rgb);
                float3 finalColor = emissive;
                float _opacity_var = UNITY_ACCESS_INSTANCED_PROP( Props, _opacity );
                return fixed4(finalColor,(i.vertexColor.a*_opacity_var));
            }
            ENDCG
        }
    }
}