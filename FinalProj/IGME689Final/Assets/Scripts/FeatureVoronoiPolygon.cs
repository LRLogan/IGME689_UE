using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

/// <summary>
/// Manages Voronoi-style service regions by assigning buildings to nearest POIs
/// and drawing region outlines per POIType.
/// </summary>
public class FeatureVoronoiPolygon : MonoBehaviour
{
    [Header("Runtime Data")]
    public List<POIData> pois;
    public List<LineStructure> buildings;

    [Header("POI Type Buckets")]
    public List<POIData> poiTypeEMS = new();
    public List<POIData> poiTypePol = new();
    public List<POIData> poiTypeFir = new();

    // World positions of buildings (copied from Transforms)
    private NativeArray<float3> buildingPositions;

    // World positions of POIs (subset per type)
    private NativeArray<float3> poiPositions;

    // Output: index of nearest POI per building
    private NativeArray<int> assignmentResults;

    private int buildingCapacity;
    private int poiCapacity;

    /// <summary>
    /// Special first-time setup entry point.
    /// </summary>
    public void FirstLoadVoronoi(POIType type)
    {
        SeperatePOIs();
        StartCoroutine(GenerateVoronoi(type));
    }

    /// <summary>
    /// Called by UI (dropdown, etc.) to rebuild regions asynchronously.
    /// </summary>
    public void RebuildVoronoiAsync(POIType type)
    {
        StopAllCoroutines();
        StartCoroutine(GenerateVoronoi(type));
    }

    /// <summary>
    /// Main Voronoi pipeline
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private IEnumerator GenerateVoronoi(POIType type)
    {
        Debug.Log(
         $"[Voronoi] Start GenerateVoronoi | Type={type} | Buildings={buildings.Count} | POIs={pois.Count}"
        );

        // Clear previous assignments AND visuals
        foreach (POIData poi in pois)
            poi.ClearAssignment();

        // Select active POI list
        List<POIData> selectedPois = type switch
        {
            POIType.EMS => poiTypeEMS,
            POIType.Police => poiTypePol,
            POIType.Fire => poiTypeFir,
            _ => null
        };

        if (selectedPois == null || selectedPois.Count == 0)
            yield break;

        // Ensure persistent buffers are large enough
        EnsureBuildingCapacity(buildings.Count);
        EnsurePOICapacity(selectedPois.Count);

        // Copy Unity data into temp NativeArrays (main thread only)
        for (int i = 0; i < buildings.Count; i++)
            buildingPositions[i] = buildings[i].GetWorldCentroid();

        for (int i = 0; i < selectedPois.Count; i++)
        {
            Vector3 p = selectedPois[i].transform.position;
            poiPositions[i] = new float3(p.x, 0f, p.z);

        }

        // Schedule parallel assignment job
        AssignNearestPOIJob job = new AssignNearestPOIJob
        {
            BuildingPositions = buildingPositions,
            POIPositions = poiPositions,
            Results = assignmentResults
        };

        JobHandle handle = job.Schedule(buildings.Count, 128);

        float startTime = Time.realtimeSinceStartup;

        while (!handle.IsCompleted)
            yield return null;

        handle.Complete();

        Debug.Log(
            $"[Voronoi] Assignment job completed in {Time.realtimeSinceStartup - startTime:F3}s"
        );


        // Apply results back to Unity objects (main thread)
        for (int i = 0; i < buildings.Count; i++)
        {
            int poiIndex = assignmentResults[i];
            if (poiIndex < 0 || poiIndex >= selectedPois.Count)
                continue;

            POIData poi = selectedPois[poiIndex];
            LineStructure building = buildings[i];

            building.assignedPOI = poi;
            poi.assignedLines.Add(building);
        }

        // Draw region outlines 
        Debug.Log("[Voronoi] Drawing polygons");

        foreach (POIData poi in selectedPois)
            poi.DrawPolygon();
        Debug.Log("[Voronoi] Region rebuild complete");

    }

    /// <summary>
    /// Computes nearest POI index for each building.
    /// Pure math only — no Unity objects.
    /// // Helper class to help optimization of computing clocest POI
    /// Makes use of running parallel jobs (threads) to take weight off of the main thread
    /// </summary>
    private struct AssignNearestPOIJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> BuildingPositions;
        [ReadOnly] public NativeArray<float3> POIPositions;

        public NativeArray<int> Results;

        public void Execute(int index)
        {
            float minDistSq = float.MaxValue;
            int closestIndex = -1;

            float3 buildingPos = BuildingPositions[index];

            for (int i = 0; i < POIPositions.Length; i++)
            {
                float distSq = math.distancesq(buildingPos, POIPositions[i]);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closestIndex = i;
                }
            }

            Results[index] = closestIndex;
        }
    }

    private void EnsureBuildingCapacity(int count)
    {
        if (!buildingPositions.IsCreated || buildingCapacity < count)
        {
            DisposeBuildingBuffers();

            buildingPositions = new NativeArray<float3>(count, Allocator.Persistent);
            assignmentResults = new NativeArray<int>(count, Allocator.Persistent);
            buildingCapacity = count;
        }
    }

    private void EnsurePOICapacity(int count)
    {
        if (!poiPositions.IsCreated || poiCapacity < count)
        {
            if (poiPositions.IsCreated)
                poiPositions.Dispose();

            poiPositions = new NativeArray<float3>(count, Allocator.Persistent);
            poiCapacity = count;
        }
    }

    private void DisposeBuildingBuffers()
    {
        if (buildingPositions.IsCreated)
            buildingPositions.Dispose();

        if (assignmentResults.IsCreated)
            assignmentResults.Dispose();
    }

    private void OnDestroy()
    {
        DisposeBuildingBuffers();

        if (poiPositions.IsCreated)
            poiPositions.Dispose();
    }

    /// <summary>
    /// Seperated POI's by type
    /// </summary>
    private void SeperatePOIs()
    {
        poiTypeEMS.Clear();
        poiTypePol.Clear();
        poiTypeFir.Clear();

        foreach (var poi in pois)
        {
            switch (poi.type)
            {
                case POIType.EMS:
                    poiTypeEMS.Add(poi);
                    break;
                case POIType.Police:
                    poiTypePol.Add(poi);
                    break;
                case POIType.Fire:
                    poiTypeFir.Add(poi);
                    break;
            }
        }
    }
}
