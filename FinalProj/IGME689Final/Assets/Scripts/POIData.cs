using Esri.GameEngine.Geometry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;

/// <summary>
/// This class holds the data about a POI and should be attached to the POI prefab
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class POIData : MonoBehaviour
{
    // Data from query
    public float latitude, longitude;
    public int id, zipCode;
    public string locationName;
    public POIType type;

    // Other data
    public float altOffset = 50;
    public static Dictionary<int, POIData> idToPOI = new Dictionary<int, POIData>();

    // New structure data
    public List<LineStructure> assignedLines = new List<LineStructure>();
    public ArcGISPoint location;
    private LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = 10.0f;

        // Adding this POI to the global list
        idToPOI[id] = this;

        FeatureVoronoiPolygon mgr = FindObjectOfType<FeatureVoronoiPolygon>();
        if (mgr != null)
            mgr.pois.Add(this);
    }

    public void ClearAssignment()
    {
        assignedLines.Clear();

        if (lineRenderer == null)
            return;

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
    }


    public void DrawPolygon()
    {
        if (assignedLines.Count < 3)
            return;

        lineRenderer.enabled = true;
        List<Vector2> points2D = new List<Vector2>();

        foreach (var line in assignedLines)
        {
            Vector3 p = line.GetWorldCentroid();
            points2D.Add(new Vector2(p.x, p.z));
        }

        List<Vector2> hull = ConvexHull(points2D);

        lineRenderer.positionCount = hull.Count;
        for (int i = 0; i < hull.Count; i++)
        {
            lineRenderer.SetPosition(
                i,
                new Vector3(
                    hull[i].x,
                    transform.position.y,
                    hull[i].y
                )
            );
        }
        Debug.Log(
        $"[Polygon] POI {id} hull size: {lineRenderer.positionCount}"
        );

    }


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

    /// <summary>
    /// Clean way to get a POI by key
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static POIData GetPOIById(int id)
    {
        return idToPOI[id];
    }

    /// <summary>
    /// Additional finctionality when type is set / updated
    /// </summary>
    /// <param name="newType"></param>
    public void UpdateType(POIType newType)
    {
        type = newType;
        switch(newType)
        {
            case POIType.EMS:
                GetComponent<MeshRenderer>().material.color = Color.green;
                break;

            case POIType.Police:
                GetComponent<MeshRenderer>().material.color = Color.blue;
                break;

            case POIType.Fire:
                GetComponent<MeshRenderer>().material.color = Color.magenta;
                break;
        }
    }
}
