using System.Collections.Generic;
using UnityEngine;

public class LineStructure : MonoBehaviour
{
    [Range(0f, 1f)]
    public float colorValue;
    public int id;
    public string groupName;
    public float groundElevation, maxHeight;
    public Dictionary<int, float> averageCount;
    public LineRenderer[] lines;

    private void Start()
    {
        if (averageCount == null)
        {
            averageCount = new Dictionary<int, float>();
            for (int i = 1; i <= 24; i++)
                averageCount[i] = 0;
        }

        lines = GetComponentsInChildren<LineRenderer>();
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
}