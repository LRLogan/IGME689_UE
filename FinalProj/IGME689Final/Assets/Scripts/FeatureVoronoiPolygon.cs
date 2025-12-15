using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FeatureVoronoiPolygon : MonoBehaviour
{
    public List<POIData> pois;
    public List<LineStructure> buildings;
    public List<POIData> poiTypeEMS = new List<POIData>();
    public List<POIData> poiTypePol = new List<POIData>();
    public List<POIData> poiTypeFir = new List<POIData>();

    /// <summary>
    /// Special first time load to do additional set up
    /// </summary>
    /// <param name="type"></param>
    public void FirstLoadVoronoi(POIType type)
    {
        SeperatePOIs();
        GenerateVoronoi(type);
    }

    public void GenerateVoronoi(POIType type)
    {
        foreach (POIData poi in pois)
            poi.ClearAssignment();

        List<POIData> selectedPois = type switch
        {
            POIType.EMS => poiTypeEMS,
            POIType.Police => poiTypePol,
            POIType.Fire => poiTypeFir,
            _ => null
        };
        Debug.Log($"SelectedPois count: {selectedPois.Count}");
        if (selectedPois == null || selectedPois.Count == 0)
            return;
        Debug.Log($"Buildings count {buildings.Count}");
        foreach (LineStructure building in buildings)
            building.AssignToNearestPOI(selectedPois);

        // DRAW AFTER ASSIGNMENT
        foreach (POIData poi in selectedPois)
            poi.DrawPolygon();
    }


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
        Debug.Log($"Counts in POI lists {poiTypeEMS.Count}, {poiTypePol.Count},, {poiTypeFir.Count}");
    }

}
