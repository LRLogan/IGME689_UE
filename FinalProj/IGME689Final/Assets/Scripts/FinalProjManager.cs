using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class FinalProjManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingPannel;
    [SerializeField] private FeatureRoadBuilder roadBuilder; 

    // Start is called before the first frame update
    void Start()
    {
        StartSimulation();
    }

    private void StartSimulation()
    {
        StartCoroutine(roadBuilder.QueryFeatureService(() =>
        {
            //lineArray = lineBuilder.lineArray;
            //AssignStartingData();
        }/*,loadingPannel.GetComponentInChildren<TextMeshProUGUI>()*/));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
