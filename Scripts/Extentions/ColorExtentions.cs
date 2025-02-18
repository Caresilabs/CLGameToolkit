using System.Globalization;
using UnityEngine;

public static class ColorExtentions
{
    public static Color HDRToSDR(this Color color)
    {
        var intensity = Mathf.Pow(2f, (color.r + color.g + color.b) / 3f);
        return new Color(Mathf.LinearToGammaSpace(color.r * intensity), Mathf.LinearToGammaSpace(color.g * intensity), Mathf.LinearToGammaSpace(color.b * intensity));
    }

    public static Color Alpha(this Color color, float alpha = 0f)
    {
        Color newColor = color;
        newColor.a = alpha;
        return newColor;
    }

    public static Color Normalized(this Color color)
    {
        var magnitude = (float)System.Math.Sqrt(color.r * color.r + color.g * color.g + color.b * color.b);
        color.r /= magnitude;
        color.g /= magnitude;
        color.b /= magnitude;
        return color;
    }

    public static Color ShiftSaturation(this Color color, float amount)
    {
        float h, s, l;
        Color.RGBToHSV(color, out h, out s, out l);

        s = Mathf.Clamp01(s + amount);
        return Color.HSVToRGB(h, s, l);
    }

    public static string ToHex(this Color color)
    {
        return string.Format(CultureInfo.InvariantCulture.NumberFormat, "#{0:X2}{1:X2}{2:X2}",
            (byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255),
            (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255),
            (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255));
    }
}
