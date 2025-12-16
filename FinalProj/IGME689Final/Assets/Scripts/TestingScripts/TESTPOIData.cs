using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TESTPOIData : MonoBehaviour
{
    public List<TESTLineStructure> assignedLines = new List<TESTLineStructure>();

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = 0.2f;
    }

    public void ClearAssignments()
    {
        assignedLines.Clear();
    }

    public void DrawPolygon()
    {
        if (assignedLines.Count < 3)
            return;

        // Convert assigned positions to 2D (XZ)
        List<Vector2> points2D = new List<Vector2>();
        foreach (var line in assignedLines)
        {
            Vector3 p = line.transform.position;
            points2D.Add(new Vector2(p.x, p.z));
        }

        // Compute convex hull
        List<Vector2> hull = ConvexHull(points2D);

        // Convert back to 3D
        lineRenderer.positionCount = hull.Count;
        for (int i = 0; i < hull.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(hull[i].x, transform.position.y, hull[i].y));
        }
    }

    // Gift wrapping (Jarvis march)
    private List<Vector2> ConvexHull(List<Vector2> points)
    {
        List<Vector2> hull = new List<Vector2>();

        Vector2 start = points[0];
        foreach (var p in points)
            if (p.x < start.x)
                start = p;

        Vector2 current = start;

        while (true)
        {
            hull.Add(current);
            Vector2 next = points[0];

            foreach (var p in points)
            {
                if (p == current) continue;

                float cross =
                    (next.x - current.x) * (p.y - current.y) -
                    (next.y - current.y) * (p.x - current.x);

                if (next == current || cross < 0 ||
                   (cross == 0 && Vector2.Distance(current, p) > Vector2.Distance(current, next)))
                {
                    next = p;
                }
            }

            current = next;
            if (current == start)
                break;
        }

        return hull;
    }
}
