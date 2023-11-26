using UnityEngine;

public static class StringExtentions
{
  
    public static string Lerp(string start, string end, float t)
    {
        int maxLength = Mathf.Max(start.Length, end.Length);
        string lerpedString = "";

        for (int i = 0; i < maxLength; i++)
        {
            char startChar = (i < start.Length) ? start[i] : ' ';
            char endChar = (i < end.Length) ? end[i] : ' ';

            lerpedString += (char)Mathf.Lerp(startChar, endChar, t);
        }

        return lerpedString;
    }

}

