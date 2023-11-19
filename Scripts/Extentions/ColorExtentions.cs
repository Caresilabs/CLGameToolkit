using UnityEngine;

public static class ColorExtentions
{
    public static Color HDRToSDR(this Color color)
    {
        var intensity = Mathf.Pow(2f, (color.r + color.g + color.b) / 3f);
        return new Color(Mathf.LinearToGammaSpace(color.r * intensity), Mathf.LinearToGammaSpace(color.g * intensity), Mathf.LinearToGammaSpace(color.b * intensity));
    }

}

