using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsSpanish
{
    private static Dictionary<string, string> ParametersStrings() => new()
    {
            ["Parameters_Hint"] = "{0} · {1} dígitos · {2} s",
            ["Parameters_MigrationResult"] = "Migración terminada: {0} importadas, {1} omitidas (duplicadas o no compatibles)",
            ["Parameters_MigrationNothing"] = "No se importó nada: {0} omitidas (duplicadas o no compatibles)",
    };
}
