using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsSpanish
{
    private static Dictionary<string, string> EditStrings() => new()
    {
        ["Edit_MenuItem"] = "Editar",
        ["Edit_Title"] = "Editar clave",
        ["Edit_NameLabel"] = "Nombre del servicio",
        ["Edit_SecretWatermark"] = "Nueva clave secreta (déjala vacía para conservarla)",
        ["Edit_Save"] = "Guardar",
        ["Edit_Cancel"] = "Cancelar",
        ["Edit_Saved"] = "Clave actualizada",
        ["Edit_NameRequired"] = "El nombre del servicio no puede estar vacío",
        ["Edit_NotifyTitle"] = "Editar clave",
    };
}
