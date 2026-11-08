using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsSpanish
{
    private static Dictionary<string, string> SearchStrings() => new()
    {
        ["Search_Placeholder"] = "Buscar por nombre del servicio",
        ["Search_NoResults"] = "Ninguna clave coincide con tu búsqueda",
        ["Search_NoResultsHint"] = "Prueba con otro nombre.",
        ["Search_FavoriteTip"] = "Favorita",
        ["Search_MoveUp"] = "Subir",
        ["Search_MoveDown"] = "Bajar",
    };
}
