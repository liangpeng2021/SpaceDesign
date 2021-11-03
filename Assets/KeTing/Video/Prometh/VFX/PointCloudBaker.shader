Shader "Hidden/Fdvfx/PointCloudBaker"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    Buffer<float> _VertexArray;
    Buffer<float> _NormalArray;
    Buffer<float> _UVArray;

    float2 _TextureSize;
    uint _VertexCount;

    void Vertex(
        float4 vertex : POSITION,
        float2 uv : TEXCOORD0,
        out float4 outPosition : SV_Position,
        out float2 outUV : TEXCOORD0
    )
    {
        outPosition = UnityObjectToClipPos(vertex);
        outUV = uv;
    }

    void Fragment(
        float4 position : SV_Position,
        float2 uv : TEXCOORD0,
        out float4 outVertex : SV_Target0,
        out float4 outNormal : SV_Target1,
        out float4 outUV : SV_Target2
    )
    {
        uint2 uvi = uv * _TextureSize;
        uint index = uvi.x + uvi.y * _TextureSize.x;

        float px = 0;
        float py = 0;
        float pz = 0;

        float nx = 0;
        float ny = 0;
        float nz = 0;

        float u = 0;
        float v = 0;

        if (index * 3 + 2 < _VertexCount - 1) {
            px = _VertexArray[index * 3 + 0];
            py = _VertexArray[index * 3 + 1];
            pz = _VertexArray[index * 3 + 2];

            nx = _NormalArray[index * 3 + 0];
            ny = _NormalArray[index * 3 + 1];
            nz = _NormalArray[index * 3 + 2];

            u = _UVArray[index * 2 + 0];
            v = _UVArray[index * 2 + 1];
        }

        float alpha = index < _VertexCount;

        outVertex = float4(px, py, pz, alpha);
        outNormal = float4(nx, ny, nz, alpha);
        outUV = float4(u, v, 0, alpha);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
