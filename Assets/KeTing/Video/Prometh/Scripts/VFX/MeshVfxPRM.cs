using prometheus;
using UnityEngine;

[ExecuteInEditMode]
public class MeshVfxPRM : MonoBehaviour
{
    [SerializeField] public Shader _bakeShader = null;
    [SerializeField] public RenderTexture _positionMap = null;
    [SerializeField] public RenderTexture _normalMap = null;
    [SerializeField] public RenderTexture _uvMap = null;
    [SerializeField] public RenderTexture _colorMap = null;
    // Buffers used to bake the point cloud
    (ComputeBuffer vertex,
     ComputeBuffer normal,
     ComputeBuffer uv,
     ComputeBuffer index) _bakeBuffer;

    // Objects for point cloud baking
    RenderBuffer[] _mrt = new RenderBuffer[3];
    Material _bakeMaterial;
    MeshPlayerPRM meshPlayerPRM;

    void Start()
    {
        if (enabled == false)
        {
            return;
        }
        meshPlayerPRM = GetComponent<MeshPlayerPRM>();
        meshPlayerPRM.vfxUpdateAction = UpdateVFX;
        if (!_bakeMaterial)
        {
            _bakeMaterial = new Material(_bakeShader) { hideFlags = HideFlags.DontSave };
        }
    }

    public void UpdateVFX(Mesh mesh, Texture2D texture)
    {
        if (enabled == false)
        {
            return;
        }
        if (_bakeBuffer.vertex == null)
        {
            InitVFXData(mesh);
        }

        if (_colorMap != null) Graphics.Blit(texture, _colorMap);

        // Bake the mesh into the point cloud attribute maps

        try
        {
            _bakeBuffer.vertex.SetData(mesh.vertices);
        }
        catch
        {
            Debug.LogError(" _bakeBuffer.vertex:" + _bakeBuffer.vertex.count + ",mesh.vertices:" + mesh.vertices.Length);
            CleanVFXData();
            return;
        }

        _bakeMaterial.SetBuffer("_VertexArray", _bakeBuffer.vertex);

        _bakeBuffer.normal.SetData(mesh.normals);
        _bakeMaterial.SetBuffer("_NormalArray", _bakeBuffer.normal);

        _bakeBuffer.uv.SetData(mesh.uv);
        _bakeMaterial.SetBuffer("_UVArray", _bakeBuffer.uv);

        _bakeMaterial.SetInt("_VertexCount", mesh.vertexCount);
        _bakeMaterial.SetVector("_TextureSize",
            new Vector2(_positionMap.width, _positionMap.height));

        _mrt[0] = _positionMap.colorBuffer;
        _mrt[1] = _normalMap.colorBuffer;
        _mrt[2] = _uvMap.colorBuffer;
        Graphics.SetRenderTarget(_mrt, _normalMap.depthBuffer);
        Graphics.Blit(null, _bakeMaterial, 0);    
    }

    public void InitVFXData(Mesh mesh)
    {
        var vcount = (mesh.vertexCount + (int)(mesh.vertexCount * 0.1f));
        var tcount = mesh.triangles.Length;

        _bakeBuffer = (
            vertex: new ComputeBuffer(vcount * 3, sizeof(float)),
            normal: new ComputeBuffer(vcount * 3, sizeof(float)),
            uv: new ComputeBuffer(vcount * 2, sizeof(float)),
            index: new ComputeBuffer(tcount * 3, sizeof(int))
        );
    }

    public void CleanVFXData()
    {
        if (_bakeBuffer.vertex != null)
        {
            _bakeBuffer.vertex.Dispose();
            _bakeBuffer.vertex = null;
        }
        if (_bakeBuffer.normal != null)
        {
            _bakeBuffer.normal.Dispose();
            _bakeBuffer.normal = null;
        }
        if (_bakeBuffer.uv != null)
        {
            _bakeBuffer.uv.Dispose();
            _bakeBuffer.uv = null;
        }
        if (_bakeBuffer.index != null)
        {
            _bakeBuffer.index.Dispose();
            _bakeBuffer.index = null;
        }
    }

    private void OnDestroy()
    {
        _positionMap.Release();
        _normalMap.Release();
        _uvMap.Release();
        _colorMap.Release();
    }
}
