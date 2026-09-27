using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphabetManager : MonoBehaviour
{
    string[] colorCodes = { "BDBAFF", "FFF997", "B7FFED", "B6F2FF", "FBB5FF" };
    public SpriteRenderer bg;
    private void Start()
    {
        bg.color = HexToColor(colorCodes[Random.Range(0, colorCodes.Length)]);
    }
    Color HexToColor(string hex)
    {
        // Remove the '#' from the beginning
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        // Parse hex color to integer
        int hexInt = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);

        // Extract individual color components
        float r = ((hexInt >> 16) & 0xFF) / 255.0f;
        float g = ((hexInt >> 8) & 0xFF) / 255.0f;
        float b = (hexInt & 0xFF) / 255.0f;

        // Create Color object
        Color color = new Color(r, g, b);

        return color;
    }
}
