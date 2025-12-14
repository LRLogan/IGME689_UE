using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public struct SerializableVec2
{
    public float x;
    public float y;

    public SerializableVec2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public SerializableVec2(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }
}


[System.Serializable]
public class BuildingCache
{
    public List<BuildingRecord> buildings;
}

[System.Serializable]
public class BuildingRecord
{
    public int id;
    public string name;
    public float groundElevation;
    public float heightRoof;
    public List<List<SerializableVec2>> rings;

}

/// <summary>
/// Very similar to FeatureRoadBuilder
/// </summary>
public class FeatureBuildingBuilder : MonoBehaviour
{
    [Header("ArcGIS")]
    public ArcGISMapComponent mapComponent;

    [Tooltip("NYC Building Footprints FeatureServer /0/query")]
    public string featureServiceUrl =
        "https://services2.arcgis.com/IsDCghZ73NgoYoz5/arcgis/rest/services/NYC_Building_Footprint/FeatureServer/0/query";

    [Header("Rendering")]
    public Material lineMaterial;
    public float lineWidth = 1.0f;
    public float heightOffset = 0f;

    [Header("Cache")]
    public string cacheFileName = "nyc_buildings.json";

    private string CachePath =>
        Path.Combine(Application.streamingAssetsPath, cacheFileName);

    private void Start()
    {
      
    }

    public IEnumerator LoadOrBuild()
    {
        // If cached data exists, load locally (no API calls)
        if (File.Exists(CachePath))
        {
            string json = File.ReadAllText(CachePath);
            BuildingCache cache = JsonConvert.DeserializeObject<BuildingCache>(json);
            BuildFromCache(cache);
            Debug.Log("Finished loading building data from cache");
            yield break;
        }

        // Otherwise query ArcGIS once and build cache
        yield return QueryAndCache();
    }

    private IEnumerator QueryAndCache()
    {
        List<BuildingRecord> records = new List<BuildingRecord>();

        int offset = 0;
        const int pageSize = 2000;
        bool hasMore = true;

        while (hasMore)
        {
            string url =
                $"{featureServiceUrl}?f=geojson" +
                "&where=1=1" +
                "&outFields=FID,NAME,HEIGHTROOF,GROUNDELEV" +
                "&returnGeometry=true" +
                "&outSR=4326" +
                $"&resultOffset={offset}" +
                $"&resultRecordCount={pageSize}";

            Debug.Log($"Requesting BUILDING records {offset}–{offset + 2000}");

            using UnityWebRequest req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(req.error);
                yield break;
            }

            JObject root = JObject.Parse(req.downloadHandler.text);
            JArray features = (JArray)root["features"];

            if (features == null || features.Count == 0)
                break;

            foreach (JToken f in features)
            {
                var props = f["properties"];
                var geom = f["geometry"];

                if (geom?["type"]?.ToString() != "Polygon")
                    continue;

                records.Add(ParseBuilding(props, geom));
            }

            hasMore = features.Count == pageSize;
            offset += pageSize;
        }

        // Persist normalized data locally (one-time cost)
        BuildingCache cache = new BuildingCache { buildings = records };
        File.WriteAllText(CachePath, JsonConvert.SerializeObject(cache, Formatting.Indented));

        BuildFromCache(cache);
        Debug.Log("Finished building building data from API");
    }

    private BuildingRecord ParseBuilding(JToken props, JToken geom)
    {
        BuildingRecord record = new BuildingRecord
        {
            id = props["FID"].Value<int>(),
            name = props["NAME"]?.ToString() ?? "",
            groundElevation = props["GROUNDELEV"]?.Value<float>() ?? 0f,
            heightRoof = props["HEIGHTROOF"]?.Value<float>() ?? 0f,
            rings = new List<List<SerializableVec2>>()
        };

        // Extract polygon rings (lon/lat pairs)
        foreach (JArray ring in geom["coordinates"])
        {
            List<SerializableVec2> pts = new List<SerializableVec2>();

            foreach (JArray p in ring)
                pts.Add(new SerializableVec2(
                    p[0].Value<float>(),
                    p[1].Value<float>()
                ));

            record.rings.Add(pts);
        }


        return record;
    }

    private void BuildFromCache(BuildingCache cache)
    {
        foreach (var b in cache.buildings)
        {
            GameObject parent = new GameObject($"Building_{b.id}");
            parent.transform.parent = transform;
            //Debug.Log($"Building Building from cache {b.id}");      // There are 1082694 buildings

            // Attach shared LineStructure used by roads/buildings
            LineStructure data = parent.AddComponent<LineStructure>();
            data.id = b.id;
            data.groupName = b.name;
            data.groundElevation = b.groundElevation;
            data.maxHeight = b.heightRoof;

            foreach (var ring in b.rings)
                DrawPolygon(ring, parent.transform);
        }
    }

    private void DrawPolygon(List<SerializableVec2> ring, Transform parent)
    {
        if (ring.Count < 2) return;

        Vector3[] points = new Vector3[ring.Count];

        for (int i = 0; i < ring.Count; i++)
        {
            Vector2 v = ring[i].ToVector2();

            ArcGISPoint geo =
                new ArcGISPoint(v.x, v.y, heightOffset, ArcGISSpatialReference.WGS84());


            points[i] = mapComponent.GeographicToEngine(geo);
        }

        GameObject go = new GameObject("BuildingOutline");
        go.transform.parent = parent;

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.positionCount = points.Length;
        lr.SetPositions(points);
        lr.loop = true;
        lr.widthMultiplier = lineWidth;
        lr.useWorldSpace = true;
    }
}
