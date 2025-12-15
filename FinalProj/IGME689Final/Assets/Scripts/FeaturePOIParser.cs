using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public class FeaturePOIParser : MonoBehaviour
{
    private string baseURL =
        "https://services1.arcgis.com/8cuieNI8NbqQZQVJ/arcgis/rest/services/Emergency_Services/FeatureServer";

    [SerializeField] private GameObject POIobj;
    [SerializeField] private GameObject POIParent;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator QueryPOIFeatures(Action onComplete)
    {
        // Sends requests to each of the 3 desired layers
        for(int i = 0; i < 3; i++)
        {
            string url = $"{baseURL}/{i}/query/?where=1=1&outFields=*&returnGeometry=true&f=geojson";

            Debug.Log($"Requesting POI records layer: {i}");

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                // Checks for bad request
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Query failed");
                    yield break;
                }

                // Downloads data into string JSON format
                string json = request.downloadHandler.text;
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("Empty response from FeatureServer.");
                    yield break;
                }

                // Gets the entire JSON obj
                JObject root;
                try
                {
                    root = JObject.Parse(json);
                }
                catch (Exception)
                {
                    Debug.LogError("Failed to parse GeoJSON");
                    yield break;
                }

                JArray features = (JArray)root["features"];
                if (features == null)
                {
                    Debug.LogWarning("No features returned.");
                    yield break;
                }

                foreach (JToken feature in features)
                {
                    JToken geometry = feature["geometry"];
                    JToken properties = feature["properties"];

                    if (geometry == null || properties == null)
                        continue;

                    // Ensure geometry is a point
                    if (geometry["type"]?.ToString() != "Point")
                        continue;

                    JArray coords = geometry["coordinates"] as JArray;
                    if (coords == null || coords.Count < 2)
                        continue;

                    // GeoJSON order: [longitude, latitude]
                    float longitude = coords[0].Value<float>();
                    float latitude = coords[1].Value<float>();

                    // Instantiate POI object
                    GameObject poi = Instantiate(POIobj, POIParent.transform);

                    POIData data = poi.GetComponent<POIData>();

                    // Assign queried values
                    ArcGISLocationComponent locComponent = poi.GetComponent<ArcGISLocationComponent>();
                    locComponent.Position = new ArcGISPoint(longitude, latitude, data.altOffset, ArcGISSpatialReference.WGS84());
                    data.longitude = longitude;
                    data.latitude = latitude;

                    data.id = properties["OBJECTID"]?.Value<int>() ?? -1;
                    data.locationName = properties["NAME"]?.ToString() ?? "Unknown";

                    // ZIP_CODE comes as string in the dataset
                    if (int.TryParse(properties["ZIP_CODE"]?.ToString(), out int zip))
                        data.zipCode = zip;
                    else
                        data.zipCode = -1;

                    // Assigning the type based off of the current layer
                    switch (i)
                    {
                        case 0:
                            data.UpdateType(POIType.EMS);
                            break;

                        case 1:
                            data.UpdateType(POIType.Police);
                            break;

                        case 2:
                            data.UpdateType(POIType.Fire);
                            break;
                    }
                }
            }
        } // END QUERY LOOP

        onComplete?.Invoke();
    }
}
