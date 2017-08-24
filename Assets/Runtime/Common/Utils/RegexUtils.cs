using System.Text.RegularExpressions;

public static class RegexUtils
{
    /// <summary>
    /// Get index of the last character in the match.
    /// </summary>
    public static int GetEndIndex (this Match match)
    {
        return match.Index + match.Length - 1;
    }
}
