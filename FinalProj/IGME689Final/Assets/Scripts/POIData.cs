using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class holds the data about a POI and should be attached to the POI prefab
/// </summary>
public class POIData : MonoBehaviour
{
    // Data from query
    public float latitude, longitude;
    public int id, zipCode;
    public string locationName;
    public POIType type;

    // Other data
    public int altOffset = 50;
    [SerializeField] Material EMSMaterial, PoMaterial, FireMaterial;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Additional finctionality when type is set / updated
    /// </summary>
    /// <param name="newType"></param>
    public void UpdateType(POIType newType)
    {
        switch(newType)
        {
            case POIType.EMS:
                GetComponent<MeshRenderer>().material.color = Color.green;
                break;

            case POIType.Police:
                GetComponent<MeshRenderer>().material.color = Color.blue;
                break;

            case POIType.Fire:
                GetComponent<MeshRenderer>().material.color = Color.magenta;
                break;
        }
    }
}
