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
    public float altOffset = 50;
    public static Dictionary<int, POIData> idToPOI = new Dictionary<int, POIData>();

    // Start is called before the first frame update
    void Start()
    {
        // Adding this POI to the global list
        idToPOI[id] = this;
    }

    /// <summary>
    /// Clean way to get a POI by key
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static POIData GetPOIById(int id)
    {
        return idToPOI[id];
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
