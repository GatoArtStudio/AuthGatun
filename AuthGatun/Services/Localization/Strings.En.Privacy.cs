using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsEnglish
{
    private static Dictionary<string, string> PrivacyStrings() => new()
    {
        ["Privacy_Show"] = "Show",
        ["Privacy_Hide"] = "Hide",
    };
}
