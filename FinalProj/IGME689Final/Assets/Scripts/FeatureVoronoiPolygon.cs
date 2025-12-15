using System.Collections.Generic;
using UnityEngine;

public static class FeatureVoronoiPolygon
{
    public static Mesh Build(List<LineStructure> buildings)
    {
        List<CombineInstance> combines = new List<CombineInstance>();

        foreach (LineStructure b in buildings)
        {
            Vector3[] footprint = b.GetWorldFootprint();
            if (footprint == null || footprint.Length < 3)
                continue;

            Mesh footprintMesh = TriangulateConvexPolygon(footprint);

            CombineInstance ci = new CombineInstance
            {
                mesh = footprintMesh,
                transform = Matrix4x4.identity
            };

            combines.Add(ci);
        }

        Mesh regionMesh = new Mesh();
        regionMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        regionMesh.CombineMeshes(combines.ToArray(), true, false);
        regionMesh.RecalculateNormals();
        regionMesh.RecalculateBounds();

        return regionMesh;
    }

    private static Mesh TriangulateConvexPolygon(Vector3[] verts)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verts;

        List<int> triangles = new List<int>();

        // Triangle fan from vertex 0
        for (int i = 1; i < verts.Length - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        mesh.triangles = triangles.ToArray();
        return mesh;
    }
}
