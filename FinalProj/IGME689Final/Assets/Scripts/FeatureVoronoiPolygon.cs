using System.Collections.Generic;
using UnityEngine;

public static class FeatureVoronoiPolygon
{
    public static Mesh BuildHull(
        List<Vector3> worldPoints,
        Vector3 origin)
    {
        if (worldPoints.Count < 3)
            return null;

        List<Vector2> points2D = new List<Vector2>();

        foreach (var p in worldPoints)
            points2D.Add(new Vector2(p.x - origin.x, p.z - origin.z));

        List<Vector2> hull2D = ComputeConvexHull(points2D);
        if (hull2D.Count < 3)
            return null;

        return TriangulateHull(hull2D);
    }

    // Monotonic Chain Convex Hull
    private static List<Vector2> ComputeConvexHull(List<Vector2> pts)
    {
        pts.Sort((a, b) =>
            a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));

        List<Vector2> hull = new List<Vector2>();

        // Lower hull
        foreach (var p in pts)
        {
            while (hull.Count >= 2 &&
                Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        // Upper hull
        int lowerCount = hull.Count + 1;
        for (int i = pts.Count - 1; i >= 0; i--)
        {
            Vector2 p = pts[i];
            while (hull.Count >= lowerCount &&
                Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        hull.RemoveAt(hull.Count - 1);
        return hull;
    }

    private static float Cross(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) -
               (b.y - a.y) * (c.x - a.x);
    }

    private static Mesh TriangulateHull(List<Vector2> hull)
    {
        Mesh mesh = new Mesh();

        Vector3[] verts = new Vector3[hull.Count];
        for (int i = 0; i < hull.Count; i++)
            verts[i] = new Vector3(hull[i].x, 0f, hull[i].y);

        int[] tris = new int[(hull.Count - 2) * 3];
        int t = 0;

        for (int i = 1; i < hull.Count - 1; i++)
        {
            tris[t++] = 0;
            tris[t++] = i;
            tris[t++] = i + 1;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
