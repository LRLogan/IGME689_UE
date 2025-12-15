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


    public void AssignToNearestPOI(List<POIData> pois)
    {
        if (pois == null || pois.Count == 0)
            return;

        float minDist = float.MaxValue;
        POIData closest = null;

        Vector3 buildingPos = GetWorldCentroid();

        foreach (var poi in pois)
        {
            float d = Vector3.Distance(buildingPos, poi.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = poi;
            }
        }

        assignedPOI = closest;
        closest.assignedLines.Add(this);
        Debug.Log($"Assigned POI of {this.name} to {assignedPOI}");
    }


    /// <summary>
    /// Updates congestionValue and applies a color gradient across all child LineRenderers.
    /// Takes into account traffic volume scaled between 1–500.
    /// </summary>
    public void UpdateCValAndGrad(float newVal)
    {
        if (lines == null || lines.Length == 0)
            lines = GetComponentsInChildren<LineRenderer>();

        if (lines.Length == 0) return;

        if (newVal > 0)
        {
            // Normalize traffic count (1–500) into 0–1 range
            float normalizedVal = Mathf.InverseLerp(1f, 500f, newVal);
            colorValue = Mathf.Clamp01(normalizedVal);

            // Map congestion value (0 = green, 1 = red)
            Color color = Color.Lerp(Color.green, Color.red, colorValue);

            ApplyGradient(color);
        }
        else
        {
            ApplyGradient(Color.white);
        }
    }

    private void ApplyGradient(Color color)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );

        foreach (var lr in lines)
        {
            if (lr != null)
                lr.colorGradient = gradient;
        }
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