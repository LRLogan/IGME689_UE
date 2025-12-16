using System.Collections.Generic;
using UnityEngine;

public class LineStructure : MonoBehaviour
{
    [Range(0f, 1f)]
    public float colorValue;
    public int id;
    public string groupName;
    public float groundElevation, maxHeight;
    public LineRenderer[] lines;
    public bool isUsable = true; // If not usable that means the structure is underwater in this simulation
    public POIData assignedPOI = null;
    public List<Vector3[]> worldRings = new List<Vector3[]>();


    private void Start()
    {
        lines = GetComponentsInChildren<LineRenderer>();
    }

    public Vector3 GetWorldCentroid()
    {
        Vector3[] verts = GetWorldFootprint();
        if (verts == null || verts.Length == 0)
            return transform.position;

        Vector3 sum = Vector3.zero;
        foreach (var v in verts)
            sum += v;

        return sum / verts.Length;
    }

    public Vector2 GetCentroid2D()
    {
        Vector3[] verts = GetWorldFootprint();
        if (verts == null || verts.Length == 0)
            return Vector2.zero;

        Vector2 sum = Vector2.zero;

        foreach (Vector3 v in verts)
            sum += new Vector2(v.x, v.z);

        return sum / verts.Length;
    }


    public Vector3[] GetWorldFootprint()
    {
        if (worldRings == null || worldRings.Count == 0)
            return null;

        // Return the outer ring
        return worldRings[0];
    }

}