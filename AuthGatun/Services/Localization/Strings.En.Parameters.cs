using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsEnglish
{
    private static Dictionary<string, string> ParametersStrings() => new()
    {
            ["Parameters_Hint"] = "{0} · {1} digits · {2} s",
            ["Parameters_MigrationResult"] = "Migration finished: {0} imported, {1} skipped (duplicates or unsupported)",
            ["Parameters_MigrationNothing"] = "Nothing was imported: {0} skipped (duplicates or unsupported)",
    };
}
