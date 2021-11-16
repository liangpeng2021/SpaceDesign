// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ZH/DoubleSideDisplay"{
    Properties{
        _MainColor("MainColor",Color) = (1,1,1,1)
        _MainTexture("MainTexture",2D) = "white"{}
    }
    SubShader
    {
        Tags{"Queue" = "Transparent" "RenderType" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull off //关建行

        pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTexture;
            float4 _MainColor;
            struct v2f
            {
                float4 pos:POSITION;
                float4 uv:TEXCOORD;
            };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            half4 frag(v2f i):COLOR
            {
                half4 c = tex2D(_MainTexture,i.uv)*_MainColor;
                return c;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
