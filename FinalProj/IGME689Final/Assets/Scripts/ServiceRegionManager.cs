using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServiceRegionManager : MonoBehaviour
{
    [Header("References")]
    public FeatureBuildingBuilder buildingBuilder;
    public FeaturePOIParser poiParser;
    public GameObject poiParent;
    public Material regionMaterial;

    private Dictionary<int, List<LineStructure>> buildingsByPOI =
        new Dictionary<int, List<LineStructure>>();

    /// <summary>
    /// Main build pipeline to refresh Discrete Voronoi regions
    /// </summary>
    public IEnumerator RebuildServiceRegions(POIType targetType)
    {
        ClearRegions();

        // 1. Collect usable buildings
        List<LineStructure> buildings = GetUsableBuildings();
        Debug.Log("Collected buildings in RegionMgr: " + buildings.Count);

        // 2. Collect active POIs
        List<POIData> pois = GetActivePOIs();
        Debug.Log("Collected POI'S in RegionMgr");
        
        // 3. Assign nearest POI (by type)
        AssignBuildingsToPOIs(buildings, pois, targetType);
        Debug.Log("Assigned POI'S in RegionMgr");
        
        // Group buildings
        Dictionary<POIData, List<LineStructure>> grouped = GroupBuildingsByPOI(buildings);
        Debug.Log("Grouped buildings in RegionMgr!");
        
        // Build the mesh
        BuildRegionMeshes(grouped);
        Debug.Log("Build the mesh in RegionMgr!");
        yield return null;
    }

    /// <summary>
    /// Small cleanup between changes for optimization
    /// </summary>
    private void ClearRegions()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    private List<LineStructure> GetUsableBuildings()
    {
        return buildingBuilder
            .GetComponentsInChildren<LineStructure>()
            .Where(b => b.isUsable)
            .ToList();
    }

    private List<POIData> GetActivePOIs()
    {
        return poiParent
            .GetComponentsInChildren<POIData>()
            .Where(p => p.gameObject.activeSelf)
            .ToList();
    }

    private void AssignBuildingsToPOIs(
    List<LineStructure> buildings,
    List<POIData> pois,
    POIType targetType)
    {
        // Pre-filter POIs by type
        List<POIData> validPOIs = pois
            .Where(p => p.type == targetType)
            .ToList();

        if (validPOIs.Count == 0)
            return;

        // Looping through each building to find the nearest POI
        foreach (LineStructure b in buildings)
        {
            Vector2 buildingPos = b.GetCentroid2D();

            float bestDist = float.MaxValue;
            POIData closestPOI = null;

            // Checks each POI of a given type
            foreach (POIData poi in validPOIs)
            {
                Vector2 poiPos = new Vector2(poi.longitude, poi.latitude);
                float dist = Vector2.Distance(buildingPos, poiPos);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    closestPOI = poi;
                }
            }

            b.assignedPOI = closestPOI;
            //Debug.Log($"Assigned {b.assignedPOI.locationName} a poi of {closestPOI}");
        }
    }


    private Dictionary<POIData, List<LineStructure>> GroupBuildingsByPOI(
    List<LineStructure> buildings)
    {
        Dictionary<POIData, List<LineStructure>> map =
            new Dictionary<POIData, List<LineStructure>>();

        foreach (LineStructure b in buildings)
        {
            if (b.assignedPOI == null)
                continue;

            if (!map.ContainsKey(b.assignedPOI))
                map[b.assignedPOI] = new List<LineStructure>();

            map[b.assignedPOI].Add(b);
        }

        return map;
    }


    private void BuildRegionMeshes(
    Dictionary<POIData, List<LineStructure>> groups)
    {
        foreach (var kvp in groups)
        {
            POIData poi = kvp.Key;
            List<LineStructure> buildings = kvp.Value;

            if (buildings.Count < 3)
                continue;

            List<Vector3> centroids = new List<Vector3>();

            foreach (var b in buildings)
            {
                Vector2 c = b.GetCentroid2D();
                centroids.Add(new Vector3(c.x, poi.transform.position.y, c.y));
            }

            Mesh mesh = FeatureVoronoiPolygon.BuildHull(
                centroids,
                poi.transform.position);

            if (mesh == null)
                continue;

            GameObject regionGO = new GameObject($"Region_{poi.id}");
            regionGO.transform.SetParent(transform, false);
            regionGO.transform.position = poi.transform.position;

            var mf = regionGO.AddComponent<MeshFilter>();
            var mr = regionGO.AddComponent<MeshRenderer>();

            mf.sharedMesh = mesh;
            mr.sharedMaterial = CreateRegionMaterial(poi);
        }
    }



    private Material CreateRegionMaterial(POIData poi)
    {
        Material mat = new Material(regionMaterial);

        Color baseColor =
            poi.GetComponent<MeshRenderer>().material.color;

        mat.color = new Color(
            baseColor.r,
            baseColor.g,
            baseColor.b,
            0.35f);

        return mat;
    }



}
