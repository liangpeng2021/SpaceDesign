// Previously as PanUV and PanUV2
inline float2 UVPan( float2 uv , float2 panDir , float scaleX , float scaleY, float texModeScale )
{
    float2 result = (uv + length(panDir) * normalize(-panDir))*float2(scaleX,scaleY)/texModeScale + float2(0.5, 0.5);
    return result;
}

// Previously as Convert2UV() and Convert2UvNormal
inline float2 Pos2UV( float3 P , float3 O , float3 BT , float3 T , float ScaleX , float ScaleY )
{
    float3 vec = P-O;
    
    float3 pu = (dot(vec,T)/dot(T,T)) * T;
    float3 pv = (dot(vec,BT)/dot(BT,BT)) * BT;
    
    float u = (dot(vec,T) >= 0) ? (0.5 + length(pu)/ScaleX) : (0.5 - length(pu)/ScaleX);
    float v = (dot(vec,BT) >= 0) ? (0.5 + length(pv)/ScaleY) : (0.5 - length(pv)/ScaleY);
    
    return float2(u,v);
}

// Previously as BlurImage()
inline float3 TexBlur( float2 uv , sampler2D _MainTexAg , float radius , float resolution )
{
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

// Previously as VectorPlaneIntersect
inline float3 VecPlnIntrs( float3 n , float3 o , float3 d , float3 a )
{
    float t = -(n.x*o.x - n.x*a.x + n.y*o.y - n.y*a.y + n.z*o.z - n.z*a.z)/(n.x*d.x + n.y*d.y + n.z*d.z);
    
    float px = o.x + d.x*t;
    float py = o.y + d.y*t;
    float pz = o.z + d.z*t;
    
    return float3(px, py, pz);
}

// Previously as ProjectPtNormal
inline float3 ProjectPt2Normal( float3 O , float3 n , float3 P )
{
    float3 v = P-O;
    
    float dist = v.x*n.x + v.y*n.y + v.z*n.z;
    
    float3 projectedPt = P - dist*n;
    
    return projectedPt;
    
}