using System;
using UnityEngine;

public static class StringUtils
{
    /// <summary>
    /// Whether compared string is literally-equal (independent of case).
    /// </summary>
    public static bool LEquals (this string content, string comparedString)
    {
        Debug.Assert(content != null);
        return content.Equals(comparedString, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Attempts to extract content between the specified matches (on first occurence).
    /// </summary>
    public static string GetBetween (this string content, string startMatchString, string endMatchString)
    {
        Debug.Assert(content != null);
        if (content.Contains(startMatchString) && content.Contains(endMatchString))
        {
            var startIndex = content.IndexOf(startMatchString) + startMatchString.Length;
            var endIndex = content.IndexOf(endMatchString, startIndex);
            return content.Substring(startIndex, endIndex - startIndex);
        }
        else return null;
    }

    /// <summary>
    /// Attempts to extract content before the specified match (on first occurence).
    /// </summary>
    public static string GetBefore (this string content, string matchString)
    {
        Debug.Assert(content != null);
        if (content.Contains(matchString))
        {
            var endIndex = content.IndexOf(matchString);
            return content.Substring(0, endIndex);
        }
        else return null;
    }

    /// <summary>
    /// Attempts to extract content after the specified match (on last occurence).
    /// </summary>
    public static string GetAfter (this string content, string matchString)
    {
        Debug.Assert(content != null);
        if (content.Contains(matchString))
        {
            var startIndex = content.LastIndexOf(matchString) + matchString.Length;
            if (content.Length <= startIndex) return string.Empty;
            return content.Substring(startIndex);
        }
        else return null;
    }

    /// <summary>
    /// Splits the string using new line symbol as a separator.
    /// </summary>
    public static string[] SplitByNewLine (this string content)
    {
        if (string.IsNullOrEmpty(content)) return null;

        return content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
    }
}

