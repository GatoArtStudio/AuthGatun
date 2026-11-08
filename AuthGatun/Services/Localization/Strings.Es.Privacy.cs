using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsSpanish
{
    private static Dictionary<string, string> PrivacyStrings() => new()
    {
        ["Privacy_Show"] = "Mostrar",
        ["Privacy_Hide"] = "Ocultar",
    };
}
