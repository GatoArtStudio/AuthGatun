using System.Globalization;
using System.Text;

namespace AuthGatun.Helpers;

public static class SearchTextNormalizer
{
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        var decomposed = text.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    public static bool Matches(string text, string normalizedQuery) =>
        normalizedQuery.Length == 0 || Normalize(text).Contains(normalizedQuery);
}
