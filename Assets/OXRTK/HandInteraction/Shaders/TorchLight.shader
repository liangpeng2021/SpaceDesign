// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "OXRTK/TorchLight" {
    Properties {
        _Hei ("Hei", Range(0, 1)) = 1
        _ProjectionRangeNM ("ProjectionRangeNM", Float ) = 1
        _ProjectionRange ("ProjectionRange", Float ) = 1
        
        _MainTexAg ("MainTexAg", 2D) = "white" {}
        _MainTexNP ("MainTexNP", 2D) = "white" {}
        
        _DirOpacity ("Directional Map Opacity", Range(0,1)) = 0.5
        _NormOpacity ("Normal Map Opacity", Range(0,1)) = 0.5

        [HideInInspector]_radiusCoeff ("radiusCoeff", Float ) = 7
        [HideInInspector]_resolution ("resolution", Float ) = 1000
        _absRing ("absRing", Float ) = 0.2
        _innerAbsRing ("innerAbsRing", Float ) = 0.2
        _Ramp ("Ramp", 2D) = "white" {}
        _innerRamp ("innerRamp", 2D) = "white" {}
        [Toggle(UseTex)] _UseTex ("UseTex", Float ) = 0
        [HideInInspector][Toggle(BackAtten)] _BackAtten ("Use Back Attenuation", Float ) = 1
        [Toggle(IsTouchSrf)] _IsTouchSrf ("Is a touch surface", Float ) = 1
        
        _InnerEmission ("InnerEmission", Float ) = 1
        _Emission ("Emission", Float ) = 1
        _texModeScaleAg ("Texture Mode Scale-Angular Projection", Float ) = .15
        _texModeScaleNp ("Texture Mode Scale-Nromal Projection", Float ) = .17
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
            #pragma shader_feature BackAtten
            #pragma shader_feature IsTouchSrf
            
            uniform float4 _probe;
            uniform float4 _probe2;
            uniform float4 _inputDir;
            uniform float4 _inputDir2;

            uniform float4 _probe_ind;
            uniform float4 _probe2_ind;
            uniform float4 _inputDir_ind;
            uniform float4 _inputDir2_ind;
            
            uniform sampler2D _MainTexAg; uniform float4 _MainTexAg_ST;
            uniform sampler2D _Ramp; uniform float4 _Ramp_ST;
            uniform sampler2D _innerRamp; uniform float4 _innerRamp_ST;
            
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
                UNITY_DEFINE_INSTANCED_PROP( fixed, _BackAtten)
                UNITY_DEFINE_INSTANCED_PROP( fixed, _IsTouchSrf)
            
                
                UNITY_DEFINE_INSTANCED_PROP( float, _InnerEmission)
                UNITY_DEFINE_INSTANCED_PROP( float, _Emission)
                UNITY_DEFINE_INSTANCED_PROP( float, _texModeScaleAg)
                UNITY_DEFINE_INSTANCED_PROP( float, _texModeScaleNp)
                UNITY_DEFINE_INSTANCED_PROP( float, _DirOpacity)
                UNITY_DEFINE_INSTANCED_PROP( float, _NormOpacity)
               
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            
            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
            };

            VertexOutput vert (VertexInput v)
            {
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
            
            float4 frag(VertexOutput i) : COLOR
            {
                UNITY_SETUP_INSTANCE_ID( i );
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                float3 recipObjScale = float3( length(unity_WorldToObject[0].xyz), length(unity_WorldToObject[1].xyz), length(unity_WorldToObject[2].xyz) );
                float3 objScale = 1.0/recipObjScale;
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                
                float3 finalColor = fixed3(0,0,0);

                float _UseTex_var = UNITY_ACCESS_INSTANCED_PROP( Props, _UseTex );
                float _Hei_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Hei );
                float _ProjectionRangeNM_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ProjectionRangeNM );

                float _radiusCoeff_var = UNITY_ACCESS_INSTANCED_PROP( Props, _radiusCoeff );
                float _resolution_var = UNITY_ACCESS_INSTANCED_PROP( Props, _resolution );
                float _ProjectionRange_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ProjectionRange );

                float _innerAbsRing_var = UNITY_ACCESS_INSTANCED_PROP( Props, _innerAbsRing );

                float _InnerEmission_var = UNITY_ACCESS_INSTANCED_PROP( Props, _InnerEmission );
                float _absRing_var = UNITY_ACCESS_INSTANCED_PROP( Props, _absRing );
                float _Emission_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Emission );
                float _texModeScale_var = UNITY_ACCESS_INSTANCED_PROP( Props, _texModeScaleAg );
                float _texModeScaleNp_var = UNITY_ACCESS_INSTANCED_PROP( Props, _texModeScaleNp );
                
                float _DirOpacity_var = UNITY_ACCESS_INSTANCED_PROP( Props, _DirOpacity );
                float _NormOpacity_var = UNITY_ACCESS_INSTANCED_PROP( Props, _NormOpacity );
                
                [unroll]
                for (int handIndex = 0; handIndex < 2; handIndex++)
                {
                    float4 _probe_var = float4(0,0,0,0);
                    float4 _inputDir_var = float4(0,0,0,0);
                    

#if IsTouchSrf
                    _probe_var = handIndex==0? _probe : _probe2; 
                    _inputDir_var = handIndex==0? _inputDir:_inputDir2;
                    
                    // _inputDir_var = float4(0,0,1,0); // For testing
                    // i.bitangentDir , i.tangentDir
                    
#elif !IsTouchSrf
                   
                    _probe_var.xyz =  (handIndex == 0) ? objPos.xyz + ((i.tangentDir) * _probe_ind.x + (i.bitangentDir) * _probe_ind.y + -i.normalDir * _probe_ind.z)
                                                       : objPos.xyz + ((i.tangentDir) * _probe2_ind.x + (i.bitangentDir) * _probe2_ind.y + -i.normalDir * _probe2_ind.z);
                    
                    _inputDir_var = handIndex==0? _inputDir_ind : _inputDir2_ind;
#endif

                    float3 normalLocal = _probe_var.rgb;
                    float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
#if UseTex     
                    
                    float3 probePlnProj = ProjectPt2Normal( objPos.rgb , i.normalDir , _probe_var.rgb ); // probePlnProj - Project the probe pt to the mesh through normal direction
                    float probeVerticalDistance = distance(probePlnProj,_probe_var.rgb); // probeVerticalDistance - Calculate the distance between pt and mesh

                                         
                    float3 rvsInputDir = (_inputDir_var.rgb*(-1.0)); // rvsInputDir - Reversed input direction
                    float OpacityAngular = saturate((dot(i.normalDir,rvsInputDir)*1.333333+-0.3333333)); // OpacityAngular - Angular Opacity Control between the mesh and the input direction
                    // float3 probeMeshProj = VecPlnIntrs( i.normalDir , _probe_var.rgb , rvsInputDir , i.posWorld.rgb ); // probeMeshProj - Calculate the intersection point between a vector and a plane
                    
                    float probeMeshDist = distance(_probe_var.rgb, i.posWorld.rgb);
                    float opacityDistLimit = clamp(probeMeshDist*sign(dot(_probe_var.rgb - i.posWorld.rgb, i.normalDir)), -1.0, 1.0); // opacityDistLimit

                    // Opacity on distance
    #if _BackAtten
                    float OpacityDistNorm = (1.0 - (clamp(probeVerticalDistance,0.0,_ProjectionRangeNM_var)/_ProjectionRangeNM_var)); // OpacityDistNorm - Distance parameter to affect opacity of normal projection
                    float OpacityDistDir = (1.0 - (clamp(probeVerticalDistance,0.0,_ProjectionRange_var)/_ProjectionRange_var)); // OpacityDistDir - Distance parameter to affect opacity of directional projection
                    
    #elif !_BackAtten
                    float OpacityDistNorm = opacityDistLimit>0 ? (1.0 - (clamp(probeVerticalDistance, 0.0, _ProjectionRangeNM_var)/_ProjectionRangeNM_var)) : 1; // OpacityDistNorm - Distance parameter to affect opacity of normal projection
                    float OpacityDistDir = opacityDistLimit>0 ? (1.0 - (clamp(probeVerticalDistance, 0.0, _ProjectionRange_var)/_ProjectionRange_var)): 1; // OpacityDistDir - Distance parameter to affect opacity of directional projection

    #endif
                    //
                    
                    float3 probeMeshProj = opacityDistLimit>=0? VecPlnIntrs( i.normalDir , _probe_var.rgb , rvsInputDir , i.posWorld.rgb ) : probePlnProj; // probeMeshProj - Calculate the intersection point between a vector and a plane

                    float2 parallaxedUV = sign(dot((_probe_var.rgb-probePlnProj),i.normalDir))*(probeVerticalDistance/* / objScale.b*/)
                                         *(opacityDistLimit>=0?(_Hei_var - 1.0):0)*mul(tangentTransform, viewDirection).xy
                                         + UVPan( i.uv0 , Pos2UV( probePlnProj , objPos.rgb , i.bitangentDir , i.tangentDir , objScale.r , objScale.g ) ,
                                             objScale.r , objScale.g, /*objScale.b, */_texModeScaleNp_var );

                    float4 parallaxInTex = tex2D(_MainTexNP,TRANSFORM_TEX(parallaxedUV.rg, _MainTexNP)); // parallaxInTex - Applied the paned UV to the texture
                    
                    float3 emissive = (((parallaxInTex.rgb * OpacityDistNorm) * _NormOpacity_var // Double check this opacity
                                      + _DirOpacity_var * (OpacityAngular*TexBlur( UVPan( i.uv0 , Pos2UV( probeMeshProj , objPos.rgb , i.bitangentDir , i.tangentDir , objScale.r , objScale.g ) , objScale.r , objScale.g, _texModeScale_var ) , _MainTexAg , (distance(probeMeshProj,_probe_var.rgb)*_radiusCoeff_var) , _resolution_var )*OpacityDistDir*(1.0 - opacityDistLimit))))
                                        ;

                    finalColor += emissive;
                    
#elif !UseTex
                    
                    float3 probePlnProj = ProjectPt2Normal( objPos.rgb , i.normalDir , _probe_var.rgb ); // probePlnProj - Project the probe pt to the mesh through normal direction
                    float probeVerticalDistance = distance(probePlnProj,_probe_var.rgb); // probeVerticalDistance - Calculate the distance between pt and mesh
                    float2 parallaxedUV = ((sign(dot((_probe_var.rgb-probePlnProj),i.normalDir))*(probeVerticalDistance/objScale.b))*(_Hei_var - 1.0)*mul(tangentTransform, viewDirection).xy + UVPan( i.uv0 , Pos2UV( probePlnProj , objPos.rgb , i.bitangentDir , i.tangentDir , objScale.r , objScale.g ) , objScale.r , objScale.g, /*objScale.b, */_texModeScale_var ));
                    float4 parallaxInTex = tex2D(_MainTexNP,TRANSFORM_TEX(parallaxedUV.rg, _MainTexNP)); // parallaxInTex - Applied the paned UV to the texture
                    
                    // float OpacityDistNorm = (1.0 - (clamp(probeVerticalDistance,0.0,_ProjectionRangeNM_var)/_ProjectionRangeNM_var)); // OpacityDistNorm - Distance parameter to affect opacity of normal projection
                    
                    float3 rvsInputDir = (_inputDir_var.rgb*(-1.0)); // rvsInputDir - Reversed input direction
                    float OpacityAngular = saturate((dot(i.normalDir,rvsInputDir)*1.333333+-0.3333333)); // OpacityAngular - Angular Opacity Control between the mesh and the input direction
                    
                    // float OpacityDistDir = (1.0 - (clamp(probeVerticalDistance,0.0,_ProjectionRange_var)/_ProjectionRange_var)); // OpacityDistDir - Distance parameter to affect opacity of directional projection
                    float probeMeshDist = distance(_probe_var.rgb,i.posWorld.rgb);
                    float opacityDistLimit = clamp((probeMeshDist*sign(dot((_probe_var.rgb-i.posWorld.rgb),i.normalDir))),(-1.0),1.0); // opacityDistLimit

                    // Opacity on distance 
    #if _BackAtten

                    float OpacityDistNorm = (1.0 - (clamp(probeVerticalDistance,0.0,_ProjectionRangeNM_var)/_ProjectionRangeNM_var)); // OpacityDistNorm - Distance parameter to affect opacity of normal projection
                    float OpacityDistDir = (1.0 - (clamp(probeVerticalDistance,0.0,_ProjectionRange_var)/_ProjectionRange_var)); // OpacityDistDir - Distance parameter to affect opacity of directional projection
                    
    #elif !_BackAtten

                    float OpacityDistNorm = opacityDistLimit>0 ? (1.0 - (clamp(probeVerticalDistance,0.0,_ProjectionRangeNM_var)/_ProjectionRangeNM_var)) : 1; // OpacityDistNorm - Distance parameter to affect opacity of normal projection
                    float OpacityDistDir = opacityDistLimit>0 ? (1.0 - (clamp(probeVerticalDistance,0.0,_ProjectionRange_var)/_ProjectionRange_var)): 1; // OpacityDistDir - Distance parameter to affect opacity of directional projection     

    #endif

                    float3 probeMeshProj = opacityDistLimit>=0? VecPlnIntrs( i.normalDir , _probe_var.rgb , rvsInputDir , i.posWorld.rgb ) : probePlnProj; // probeMeshProj - Calculate the intersection point between a vector and a plane

                    float haloNrmDdist = opacityDistLimit>=0? probeMeshDist : distance(probePlnProj, i.posWorld.rgb);
                
                    float2 haloNrmRampUV = float2(sin(saturate((haloNrmDdist*_innerAbsRing_var))),0.0); // haloNrmRampUV - construct the UV for halo edge section - normal projection
                    float4 haloNrmRamp = tex2D(_innerRamp,TRANSFORM_TEX(haloNrmRampUV, _innerRamp)); // haloNrmRamp - Halo mode normal projection ring pixel

                    // float backOpacity = abs(opacityDistLimit); // backOpacity - Behind Ojbect Opacity Output - step
                    
                    float2 haloDirRampUV = float2(sin(saturate((distance(probeMeshProj,i.posWorld.rgb)*_absRing_var))),0.0); // haloDirRampUV- construct the UV for halo edge section - dir projection
                    float4 haloDirRamp = tex2D(_Ramp,TRANSFORM_TEX(haloDirRampUV, _Ramp)); // haloDirRamp - Halo mode direction projection ring pixel


                    float3 emissive = (haloNrmRamp.rgb * OpacityDistNorm /** backOpacity*/ * _InnerEmission_var * _NormOpacity_var
                                    + (haloDirRamp.rgb * OpacityAngular /** backOpacity*/ * _Emission_var * OpacityDistDir * _DirOpacity_var));
                    finalColor += emissive;
                    
#endif
                }
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
}