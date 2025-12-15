using UnityEngine;

public class TESTVoronoiManager : MonoBehaviour
{
    void Start()
    {
        GenerateVoronoi();
    }

    public void GenerateVoronoi()
    {
        TESTPOIData[] pois = FindObjectsOfType<TESTPOIData>();
        TESTLineStructure[] lines = FindObjectsOfType<TESTLineStructure>();

        // Clear old assignments
        foreach (var poi in pois)
            poi.ClearAssignments();

        // Assign each LineStructure to nearest POI
        foreach (var line in lines)
            line.AssignToNearestPOI(pois);

        // Draw polygons
        foreach (var poi in pois)
            poi.DrawPolygon();
    }
}
