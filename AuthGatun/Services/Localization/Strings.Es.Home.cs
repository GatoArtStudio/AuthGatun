using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsSpanish
{
    private static Dictionary<string, string> HomeStrings() => new()
    {
            ["Home_Title"] = "Tus códigos",
            ["Home_Empty"] = "Aún no tienes claves",
            ["Home_EmptyHint"] = "Agrégalas desde la pestaña Claves.",
            ["Home_Copy"] = "Copiar",
            ["Home_CopyCode"] = "Copiar código",
            ["Home_Delete"] = "Eliminar",
            ["Home_CountdownTip"] = "Segundos antes de que cambie el código",
            ["Home_NotifyTitle"] = "Claves TOTP",
            ["Home_KeyNotFound"] = "Clave no encontrada",
            ["Home_Copied"] = "Código copiado. Se borrará del portapapeles en 30 segundos.",
            ["Home_Deleted"] = "Clave eliminada con éxito",
    };
}
