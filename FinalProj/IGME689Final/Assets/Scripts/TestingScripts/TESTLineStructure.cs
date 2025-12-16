using UnityEngine;

public class TESTLineStructure : MonoBehaviour
{
    public TESTPOIData assignedPOI;

    public void AssignToNearestPOI(TESTPOIData[] pois)
    {
        float minDist = float.MaxValue;
        TESTPOIData closest = null;

        foreach (var poi in pois)
        {
            float d = Vector3.Distance(transform.position, poi.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = poi;
            }
        }

        assignedPOI = closest;
        closest.assignedLines.Add(this);
    }
}
