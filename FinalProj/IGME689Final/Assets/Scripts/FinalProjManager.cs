using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Layers;
using Esri.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class FinalProjManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingPannel;
    [SerializeField] private FeatureRoadBuilder roadBuilder;
    [SerializeField] private FeatureBuildingBuilder buildingBuilder;
    [SerializeField] private ArcGISMapComponent mapComponent;

    // Water settings
    [SerializeField] private GameObject waterAsset;
    private float curAlt, prevAlt;  // Trackers used for both interpilation and detection optimization

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartSimulation());
    }

    private IEnumerator StartSimulation()
    {
        // Wait until the map has fully initialized
        while (!mapComponent || !mapComponent.HasSpatialReference())
            yield return null;

        Debug.Log("Starting simulation");
        StartCoroutine(roadBuilder.QueryFeatureService(() =>
        {
            //lineArray = lineBuilder.lineArray;
            //AssignStartingData();
        }/*,loadingPannel.GetComponentInChildren<TextMeshProUGUI>()*/));
        StartCoroutine(buildingBuilder.LoadOrBuild());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Change the height of the water. Pass in a alt to raise / lower water
    /// </summary>
    /// <param name="newHeight"></param>
    public void ChangeWaterAlt(float newHeight)
    {
        Vector3 curPos = new Vector3();
        curPos = waterAsset.transform.position;
        curPos.y = newHeight;
        waterAsset.transform.position = curPos;

        // Update the current height tracker 
        curAlt = newHeight;
    }
}
