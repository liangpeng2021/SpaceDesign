Shader "OXRTK/TorchLightOld" {
    Properties {
        _Hei ("Hei", Range(0, 1)) = 1
        _ProjectionRangeNM ("ProjectionRangeNM", Float ) = 1
        _ProjectionRange ("ProjectionRange", Float ) = 1
        _MainTexAg ("MainTexAg", 2D) = "white" {}
        _radiusCoeff ("radiusCoeff", Float ) = 7
        _resolution ("resolution", Float ) = 1000
        _absRing ("absRing", Float ) = 0.2
        _innerAbsRing ("innerAbsRing", Float ) = 0.2
        _Ramp ("Ramp", 2D) = "white" {}
        _innerRamp ("innerRamp", 2D) = "white" {}
        [Toggle(UseTex)] _UseTex ("UseTex", Float ) = 0
        _InnerEmission ("InnerEmission", Float ) = 1
        _Emission ("Emission", Float ) = 1
        _MainTexNP ("MainTexNP", 2D) = "white" {}
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
            #include "cginc/Oxrtk.cginc"
            #pragma multi_compile_fwdbase
            #pragma target 3.0
            #pragma shader_feature UseTex
            
            uniform float4 _probe;
            uniform float4 _probe2;
            uniform float4 _inputDir;
            uniform float4 _inputDir2;
            
            float2 Convert2Uv( float3 P , float3 O , float3 BT , float3 T , float ScaleX , float ScaleY ){
            float3 vec = P-O;
            
            float3 pu = (dot(vec,T)/dot(T,T)) * T;
            float3 pv = (dot(vec,BT)/dot(BT,BT)) * BT;
            
            float u = (dot(vec,T) >= 0) ? (0.5 + length(pu)/ScaleX) : (0.5 - length(pu)/ScaleX);
            float v = (dot(vec,BT) >= 0) ? (0.5 + length(pv)/ScaleY) : (0.5 - length(pv)/ScaleY);
            
            return float2(u,v);
            }
            
            float3 ProjectPtNormal( float3 O , float3 n , float3 P ){
            float3 v = P-O;
            
            float dist = v.x*n.x + v.y*n.y + v.z*n.z;
            
            float3 projectedPt = P - dist*n;
            
            return projectedPt;
            
            }
            
            float2 Convert2UvNormal( float3 P , float3 O , float3 BT , float3 T , float ScaleX , float ScaleY ){
            float3 vec = P-O;
            
            float3 pu = (dot(vec,T)/dot(T,T)) * T;
            float3 pv = (dot(vec,BT)/dot(BT,BT)) * BT;
            
            float u = (dot(vec,T) >= 0) ? (0.5 + length(pu)/ScaleX) : (0.5 - length(pu)/ScaleX);
            float v = (dot(vec,BT) >= 0) ? (0.5 + length(pv)/ScaleY) : (0.5 - length(pv)/ScaleY);
            
            return float2(u,v);
            }
            
            float2 PanUV( float2 uv , float2 panDir ){
            float2 result = (uv + length(panDir) * normalize(-panDir) + float2(0.5, 0.5));
            return result;
            }
            
            float2 PanUV2( float2 uv , float2 panDir ){
            float2 result = (uv + length(panDir) * normalize(-panDir) + float2(0.5, 0.5));
            return result;
            }
            
            float3 BlurImage( float2 uv , sampler2D _MainTexAg , float radius , float resolution ){
            float4 sum = float4(0.0, 0.0, 0.0, 0.0);
            float2 tc = uv;
            
            float4 t = _Time;
                            
            float hstep = 1.86876 * sin(abs(t.g / 5.6786) + 0.15);
            float vstep = 2.24134 * sin(abs(t.g / 6.1565));
            
            //blur radius in pixels
            float blur = radius/resolution/4;
            
            // fixed4 col = tex2D(_MainTex, i.uv);
            
            sum += tex2D(_MainTexAg, float2(tc.x - 4.0*blur*hstep, tc.y - 4.0*blur*vstep)) * 0.0162162162;
            sum += tex2D(_MainTexAg, float2(tc.x - 3.0*blur*hstep, tc.y - 3.0*blur*vstep)) * 0.0540540541;
            sum += tex2D(_MainTexAg, float2(tc.x - 2.0*blur*hstep, tc.y - 2.0*blur*vstep)) * 0.1216216216;
            sum += tex2D(_MainTexAg, float2(tc.x - 1.0*blur*hstep, tc.y - 1.0*blur*vstep)) * 0.1945945946;
            
            sum += tex2D(_MainTexAg, float2(tc.x - 4.0*blur*hstep, tc.y + 4.0*blur*vstep)) * 0.0162162162;
            sum += tex2D(_MainTexAg, float2(tc.x - 3.0*blur*hstep, tc.y + 3.0*blur*vstep)) * 0.0540540541;
            sum += tex2D(_MainTexAg, float2(tc.x - 2.0*blur*hstep, tc.y + 2.0*blur*vstep)) * 0.1216216216;
            sum += tex2D(_MainTexAg, float2(tc.x - 1.0*blur*hstep, tc.y + 1.0*blur*vstep)) * 0.1945945946;
            
            // sum += tex2D(_MainTex, float2(tc.x, tc.y)) * 0.2270270270;
            
            sum += tex2D(_MainTexAg, float2(tc.x + 1.0*blur*hstep, tc.y - 1.0*blur*vstep)) * 0.1945945946;
            sum += tex2D(_MainTexAg, float2(tc.x + 2.0*blur*hstep, tc.y - 2.0*blur*vstep)) * 0.1216216216;
            sum += tex2D(_MainTexAg, float2(tc.x + 3.0*blur*hstep, tc.y - 3.0*blur*vstep)) * 0.0540540541;
            sum += tex2D(_MainTexAg, float2(tc.x + 4.0*blur*hstep, tc.y - 4.0*blur*vstep)) * 0.0162162162;
            
            sum += tex2D(_MainTexAg, float2(tc.x + 1.0*blur*hstep, tc.y + 1.0*blur*vstep)) * 0.1945945946;
            sum += tex2D(_MainTexAg, float2(tc.x + 2.0*blur*hstep, tc.y + 2.0*blur*vstep)) * 0.1216216216;
            sum += tex2D(_MainTexAg, float2(tc.x + 3.0*blur*hstep, tc.y + 3.0*blur*vstep)) * 0.0540540541;
            sum += tex2D(_MainTexAg, float2(tc.x + 4.0*blur*hstep, tc.y + 4.0*blur*vstep)) * 0.0162162162;
            
            return sum;
            }
            
            uniform sampler2D _MainTexAg; uniform float4 _MainTexAg_ST;
            uniform sampler2D _Ramp; uniform float4 _Ramp_ST;
            uniform sampler2D _innerRamp; uniform float4 _innerRamp_ST;
            float3 VectorPlaneIntersect( float3 n , float3 o , float3 d , float3 a ){
            float t = -(n.x*o.x - n.x*a.x + n.y*o.y - n.y*a.y + n.z*o.z - n.z*a.z)/(n.x*d.x + n.y*d.y + n.z*d.z);
            
            float px = o.x + d.x*t;
            float py = o.y + d.y*t;
            float pz = o.z + d.z*t;
            
            return float3(px, py, pz);
            }
            
            uniform sampler2D _MainTexNP; uniform float4 _MainTexNP_ST;
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float, _Hei)
                UNITY_DEFINE_INSTANCED_PROP( float, _ProjectionRangeNM)
                UNITY_DEFINE_INSTANCED_PROP( float, _ProjectionRange)
                UNITY_DEFINE_INSTANCED_PROP( float, _radiusCoeff)
                UNITY_DEFINE_INSTANCED_PROP( float, _resolution)
                UNITY_DEFINE_INSTANCED_PROP( float, _absRing)
                UNITY_DEFINE_INSTANCED_PROP( float, _innerAbsRing)
                UNITY_DEFINE_INSTANCED_PROP( fixed, _UseTex)
                UNITY_DEFINE_INSTANCED_PROP( float, _InnerEmission)
                UNITY_DEFINE_INSTANCED_PROP( float, _Emission)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                
                float3 recipObjScale = float3( length(unity_WorldToObject[0].xyz), length(unity_WorldToObject[1].xyz), length(unity_WorldToObject[2].xyz) );
                float3 objScale = 1.0/recipObjScale;
                
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                float3 recipObjScale = float3( length(unity_WorldToObject[0].xyz), length(unity_WorldToObject[1].xyz), length(unity_WorldToObject[2].xyz) );
                float3 objScale = 1.0/recipObjScale;
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                
                float3 finalColor = float3(0,0,0);

                /// for probe 1
                [unroll]
                for (int handIndex = 0; handIndex < 2; handIndex++)
                {
                    float4 _probe_var = handIndex==0? _probe : _probe2; 
                    float3 normalLocal = _probe_var.rgb;
                    float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
    
                    // Instance properties
                    // float _UseTex_var = UNITY_ACCESS_INSTANCED_PROP( Props, _UseTex );
                    float _Hei_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Hei );
                    float _ProjectionRangeNM_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ProjectionRangeNM );
                    float _radiusCoeff_var = UNITY_ACCESS_INSTANCED_PROP( Props, _radiusCoeff );
                    float _resolution_var = UNITY_ACCESS_INSTANCED_PROP( Props, _resolution );
                    float _ProjectionRange_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ProjectionRange );
                    float _innerAbsRing_var = UNITY_ACCESS_INSTANCED_PROP( Props, _innerAbsRing );
                    float _InnerEmission_var = UNITY_ACCESS_INSTANCED_PROP( Props, _InnerEmission );
                    float _absRing_var = UNITY_ACCESS_INSTANCED_PROP( Props, _absRing );
                    float _Emission_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Emission );
    
    
                    float3 probePlnProj = ProjectPtNormal( objPos.rgb , i.normalDir , _probe_var.rgb ); // probePlnProj - Project the probe pt to the mesh through normal direction
                    float probeVerticalDistance = distance(probePlnProj,_probe_var.rgb); // probeVerticalDistance - Calculate the distance between pt and mesh
                    float2 parallaxUV = PanUV2( ((sign(dot((_probe_var.rgb-probePlnProj),i.normalDir))*(probeVerticalDistance/objScale.b))*(_Hei_var - 1.0)*mul(tangentTransform, viewDirection).xy + i.uv0).rg , Convert2UvNormal( probePlnProj , objPos.rgb , i.bitangentDir , i.tangentDir , objScale.r , objScale.b ) ); // Parallax effect - Pan the UV texture along the normal direction
                    float4 parallaxInTex = tex2D(_MainTexNP,TRANSFORM_TEX(parallaxUV, _MainTexNP)); // parallaxInTex - Applied the paned UV to the texture
    
                    float OpacityDistNorm = (1.0 - (clamp(probeVerticalDistance,0.0,_ProjectionRangeNM_var)/_ProjectionRangeNM_var)); // OpacityDistNorm - Distance parameter to affect opacity of normal projection
                    float4 _inputDir_var = handIndex==0? _inputDir:_inputDir2;
                    float3 rvsInputDir = (_inputDir_var.rgb*(-1.0)); // rvsInputDir - Reversed input direction
                    float opacityAngular = saturate((dot(i.normalDir,rvsInputDir)*1.333333+-0.3333333)); // OpacityAngular - Angular Opacity Control between the mesh and the input direction
                    float3 probeMeshProj = VectorPlaneIntersect( i.normalDir , _probe_var.rgb , rvsInputDir , i.posWorld.rgb ); // probeMeshProj - Calculate the intersection point between a vector and a plane
                    
                    float OpacityDistDir = (1.0 - (clamp(probeVerticalDistance,0.0,_ProjectionRange_var)/_ProjectionRange_var)); // OpacityDistDir - Distance parameter to affect opacity of directional projection
    
                    float opacityDist = distance(_probe_var.rgb,i.posWorld.rgb); // opacityDist
    
                    float opacityDistLimit = clamp((opacityDist*sign(dot((_probe_var.rgb-i.posWorld.rgb),i.normalDir))),(-1.0),1.0); // opacityDistLimit
    
    
                    
                    #if UseTex              
                    {
    
                        float3 emissive = ((((parallaxInTex.rgb*OpacityDistNorm)
                                            +(opacityAngular*BlurImage( PanUV( i.uv0 , Convert2Uv( probeMeshProj , objPos.rgb , i.bitangentDir , i.tangentDir , objScale.r , objScale.g ) ) , _MainTexAg , (distance(probeMeshProj,_probe_var.rgb)*_radiusCoeff_var) , _resolution_var )*OpacityDistDir*(1.0 - opacityDistLimit))))
                                            );
                            
                        
                        finalColor += emissive;
                    }
                    #endif
    
                    #if !UseTex
                    {
                        float2 haloNrmRampUV = float2(sin(saturate((opacityDist*_innerAbsRing_var))),0.0); // haloNrmRampUV - construct the UV for halo edge section - normal projection
                        float4 haloNrmRamp = tex2D(_innerRamp,TRANSFORM_TEX(haloNrmRampUV, _innerRamp)); // haloNrmRamp - Halo mode normal projection ring pixel
                        float backOpacity = abs(opacityDistLimit); // backOpacity - Behind Ojbect Opacity Output - step
                        
                        float2 haloDirRampUV = float2(sin(saturate((distance(probeMeshProj,i.posWorld.rgb)*_absRing_var))),0.0); // haloDirRampUV- construct the UV for halo edge section - dir projection
                        float4 haloDirRamp = tex2D(_Ramp,TRANSFORM_TEX(haloDirRampUV, _Ramp)); // haloDirRamp - Halo mode direction projection ring pixel
                        float3 emissive = (
                                            +(((haloNrmRamp.rgb*OpacityDistNorm*backOpacity*_InnerEmission_var)
                                            +(haloDirRamp.rgb*opacityAngular*backOpacity*_Emission_var*OpacityDistDir))));
                            
                        
                        finalColor += emissive;
                    }
                    #endif
                }
                
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
}