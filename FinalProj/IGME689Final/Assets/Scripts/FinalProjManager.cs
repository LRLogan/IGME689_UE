using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Layers;
using Esri.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum POIType{
    EMS,
    Police,
    Fire
}

public class FinalProjManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingPannel;
    [SerializeField] private FeatureRoadBuilder roadBuilder;
    [SerializeField] private FeatureBuildingBuilder buildingBuilder;
    [SerializeField] private FeaturePOIParser POIParser;
    [SerializeField] private ArcGISMapComponent mapComponent;
    [SerializeField] private FeatureVoronoiPolygon polygonMgr;

    private bool buildingFootprintsDone = false, POIsDone = false, roadsDone = false;

    // Water settings
    [SerializeField] private GameObject waterAsset;
    private float curAlt, prevAlt;  // Trackers used for both interpilation and detection optimization

    // UI elements
    [SerializeField] private TMP_Dropdown poiDropdown;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartSimulation());


        poiDropdown.gameObject.SetActive(false);
        poiDropdown.onValueChanged.AddListener(OnDropdownChanged);
        poiDropdown.value = 1;
        poiDropdown.RefreshShownValue();
    }

    private IEnumerator StartSimulation()
    {
        // Wait until the map has fully initialized
        while (!mapComponent || !mapComponent.HasSpatialReference())
            yield return null;
        
        Debug.Log("Starting simulation");
        StartCoroutine(roadBuilder.QueryFeatureService(() =>
        {
            Debug.Log("Roads done");
            roadsDone = true;
            //lineArray = lineBuilder.lineArray;
        }/*,loadingPannel.GetComponentInChildren<TextMeshProUGUI>()*/));
        StartCoroutine(buildingBuilder.LoadOrBuild(()=>
        {
            buildingFootprintsDone = true;

            // POPULATE BUILDINGS HERE
            polygonMgr.buildings = buildingBuilder
                .GetComponentsInChildren<LineStructure>()
                .Where(b => b.isUsable)
                .ToList();

            Debug.Log("Buildings done");
            StartCoroutine(POIParser.QueryPOIFeatures(() =>
            {
                POIsDone = true;
                Debug.Log("POI's done");
            }));
        }));

        
        // Waiting for part 1 set up
        while (!buildingFootprintsDone || !POIsDone || !roadsDone)
            yield return null;
        
        Debug.Log("Part 2 set up");
        polygonMgr.FirstLoadVoronoi(POIType.Police); // Hard coded for now
        Debug.Log("Set up complete");
        poiDropdown.gameObject.SetActive(true);
    }

    private void OnDropdownChanged(int index)
    {
        switch (index)
        {
            case 0:
                polygonMgr.GenerateVoronoi(POIType.EMS);
                break;

            case 1:
                polygonMgr.GenerateVoronoi(POIType.Police);
                break;

            case 2:
                polygonMgr.GenerateVoronoi(POIType.Fire);
                break;
        }
    }

    /// <summary>
    /// Change the height of the water. Pass in a alt to raise / lower water
    /// </summary>
    /// <param name="newHeight"></param>
    private void ChangeWaterAlt(float newHeight)
    {
        Vector3 curPos = new Vector3();
        curPos = waterAsset.transform.position;
        curPos.y = newHeight;
        waterAsset.transform.position = curPos;

        // Update the current height tracker 
        curAlt = newHeight;
    }

    public void OnAdvance()
    {

    }

    public void OnRetreat()
    {

    }
}
