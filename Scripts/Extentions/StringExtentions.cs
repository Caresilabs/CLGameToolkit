using System.Text.RegularExpressions;
using System;
using UnityEngine;

public static class StringExtentions
{
    private const int ASCII_TEXT_MIN = 32;
    private const int ASCII_TEXT_MAX = 125;

    public static string Lerp(string start, string end, float t)
    {
        int maxLength = Mathf.Max(start.Length, end.Length);
        string lerpedString = "";

        if (t == 1f) return end;

        for (int i = 0; i < maxLength; i++)
        {
            char startChar = (i < start.Length) ? start[i] : (char)((i * 421) % 125); // Pseudo random start
            char endChar = (i < end.Length) ? end[i] : ' ';

            lerpedString += (char)Mathf.Lerp(
                Mathf.Clamp(startChar, ASCII_TEXT_MIN, ASCII_TEXT_MAX),
                Mathf.Clamp(endChar, ASCII_TEXT_MIN, ASCII_TEXT_MAX),
                t);
        }

        return lerpedString;
    }

    public static int CountWords(this string line)
    {
        int wordCount = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == ' ' || i == line.Length - 1)
                wordCount++;
        }

        return wordCount;
    }

    public static string StripMarkupUnsafe(string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }

    public static string FindNonEmpty(string first, string second)
    {
        if (!string.IsNullOrEmpty(first))
            return first;

        if (!string.IsNullOrEmpty(second))
            return second;

        return null;
    }

}

