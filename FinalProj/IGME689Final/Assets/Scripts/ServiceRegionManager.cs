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
    public void RebuildServiceRegions(POIType targetType)
    {
        // 1. Collect usable buildings
        List<LineStructure> buildings = GetUsableBuildings();

        // 2. Collect active POIs
        List<POIData> pois = GetActivePOIs();

        // 3. Assign nearest POI (by type)
        AssignBuildingsToPOIs(buildings, pois, targetType);

        // Group buildings
        Dictionary<POIData, List<LineStructure>> grouped = GroupBuildingsByPOI(buildings);

        // Build the mesh
        BuildRegionMeshes(grouped);
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


    private void BuildRegionMeshes(Dictionary<POIData, List<LineStructure>> groupes)
    {
        foreach (var kvp in groupes)
        {
            POIData poi = kvp.Key;
            List<LineStructure> assignedBuildings = kvp.Value;

            GameObject regionGO = new GameObject($"Region_{poi.id}");
            regionGO.transform.parent = transform;

            MeshFilter mf = regionGO.AddComponent<MeshFilter>();
            MeshRenderer mr = regionGO.AddComponent<MeshRenderer>();

            mr.material = CreateRegionMaterial(poi);
            mf.mesh = FeatureVoronoiPolygon.Build(assignedBuildings);
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
