using System.Collections.Generic;
using System.Linq;

namespace AuthGatun.Services.Localization;

// Every area keeps its texts in Strings.Es.<Area>.cs so features can add texts without touching the same lines.
public static partial class StringsSpanish
{
    public static readonly IReadOnlyDictionary<string, string> Table = Merge(
        CommonStrings(), HomeStrings(), KeysStrings(), SettingsStrings(), ErrorsStrings(), EditStrings(), SearchStrings(), ParametersStrings(), PrivacyStrings(), ThemeStrings());

    private static Dictionary<string, string> Merge(params Dictionary<string, string>[] areas) =>
        areas.SelectMany(area => area).ToDictionary(pair => pair.Key, pair => pair.Value);
}
