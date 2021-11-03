Shader "NormalShader"{
    Properties
    {
        _Color("Base Color",color) = (1,1,1,1)
        _MainTex("Base(RGB)",2D) = "white"{}
    }
    Subshader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        pass
        {
            Cull off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;

            struct v2f
            {
                float4 pos : POSITION;
                float4 uv   : TEXCOORD;
                float4 col :COLOR;
            };
            v2f vert(appdata_base v)
            {
                v2f  o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.col.xyz = v.normal * 0.5 + 0.5;
                o.col.w = 1.0;
                return o;
            }
            half4 frag(v2f i) :COLOR
            {
                half4 h = i.col;
                return h;
            }
            ENDHLSL
        }


    }
    FallBack "Diffuse"
}