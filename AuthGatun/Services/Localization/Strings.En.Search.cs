using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsEnglish
{
    private static Dictionary<string, string> SearchStrings() => new()
    {
        ["Search_Placeholder"] = "Search by service name",
        ["Search_NoResults"] = "No keys match your search",
        ["Search_NoResultsHint"] = "Try a different name.",
        ["Search_FavoriteTip"] = "Favorite",
        ["Search_MoveUp"] = "Move up",
        ["Search_MoveDown"] = "Move down",
    };
}
