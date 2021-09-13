using System;
using System.Collections.Generic;

public class ObjFormatAnalyzer
{
    public struct Vector
    {
        public float X;
        public float Y;
        public float Z;
    }

    public struct FacePoint
    {
        public int VertexIndex;
        public int TextureIndex;
        public int NormalIndex;
    }

    public struct Face
    {
        public FacePoint[] Points;
        public bool IsQuad;
    }

    public Vector[] VertexArr;
    public Vector[] VertexNormalArr;
    public Vector[] VertexTextureArr;
    public Face[] FaceArr;
    
    public void Analyze(string content)
    {
        content = content.Replace('\r', ' ').Replace('\t', ' ');

        var lines = content.Split('\n');
        var vertexList = new List<Vector>();
        var vertexNormalList = new List<Vector>();
        var vertexTextureList = new List<Vector>();
        var faceList = new List<Face>();

        for (int i = 0; i < lines.Length; i++)
        {
            var currentLine = lines[i];

            if (currentLine.Contains("#") || currentLine.Length == 0)
            {
                continue;
            }

            if (currentLine.Contains("v "))
            {
                var splitInfo = currentLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                vertexList.Add(new Vector() { X = float.Parse(splitInfo[1]), Y = float.Parse(splitInfo[2]), Z = float.Parse(splitInfo[3]) });
            }
            else if (currentLine.Contains("vt "))
            {
                var splitInfo = currentLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                vertexTextureList.Add(new Vector() { X = splitInfo.Length > 1 ? float.Parse(splitInfo[1]) : 0, Y = splitInfo.Length > 2 ? float.Parse(splitInfo[2]) : 0, Z = splitInfo.Length > 3 ? float.Parse(splitInfo[3]) : 0 });
            }
            else if (currentLine.Contains("vn "))
            {
                var splitInfo = currentLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                vertexNormalList.Add(new Vector() { X = float.Parse(splitInfo[1]), Y = float.Parse(splitInfo[2]), Z = float.Parse(splitInfo[3]) });
            }
            else if (currentLine.Contains("f "))
            {
                Func<string, int> tryParse = (inArg) =>
                {
                    var outValue = -1;
                    return int.TryParse(inArg, out outValue) ? outValue : 0;
                };

                var splitInfo = currentLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var isQuad = splitInfo.Length > 4;
                var face1 = splitInfo[1].Split('/');
                var face2 = splitInfo[2].Split('/');
                var face3 = splitInfo[3].Split('/');
                var face4 = isQuad ? splitInfo[4].Split('/') : null;
                var face = new Face();
                face.Points = new FacePoint[4];
                face.Points[0] = new FacePoint() { VertexIndex = tryParse(face1[0]), TextureIndex = tryParse(face1[1]), NormalIndex = tryParse(face1[2]) };
                face.Points[1] = new FacePoint() { VertexIndex = tryParse(face2[0]), TextureIndex = tryParse(face2[1]), NormalIndex = tryParse(face2[2]) };
                face.Points[2] = new FacePoint() { VertexIndex = tryParse(face3[0]), TextureIndex = tryParse(face3[1]), NormalIndex = tryParse(face3[2]) };
                face.Points[3] = isQuad ? new FacePoint() { VertexIndex = tryParse(face4[0]), TextureIndex = tryParse(face4[1]), NormalIndex = tryParse(face4[2]) } : default(FacePoint);
                face.IsQuad = isQuad;

                faceList.Add(face);
            }
        }

        VertexArr = vertexList.ToArray();
        VertexNormalArr = vertexNormalList.ToArray();
        VertexTextureArr = vertexTextureList.ToArray();
        FaceArr = faceList.ToArray();
    }
}
