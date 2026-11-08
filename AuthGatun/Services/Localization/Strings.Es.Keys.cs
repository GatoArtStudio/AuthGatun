using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsSpanish
{
    private static Dictionary<string, string> KeysStrings() => new()
    {
            ["Keys_Title"] = "Nueva clave",
            ["Keys_Hint"] = "Pega la clave secreta que te da el servicio al activar la verificación en dos pasos.",
            ["Keys_ServiceName"] = "Nombre del servicio",
            ["Keys_ServiceNamePlaceholder"] = "Ingresa el nombre del servicio",
            ["Keys_Secret"] = "Clave secreta",
            ["Keys_SecretPlaceholder"] = "Ingresa la clave TOTP",
            ["Keys_Save"] = "Guardar",
            ["Keys_NotifyTitle"] = "Nueva clave",
            ["Keys_FillAll"] = "Por favor, rellena todos los campos",
            ["Keys_NoSpaces"] = "La clave TOTP no debe contener espacios",
            ["Keys_NoUser"] = "Aún no hay ningún usuario con la sesión iniciada",
            ["Keys_Saved"] = "Clave guardada con éxito",
            ["Keys_ScanFromClipboard"] = "Escanear QR del portapapeles",
            ["Keys_ScanFromFile"] = "Escanear QR desde un archivo de imagen",
            ["Keys_DropHint"] = "o suelta aquí una imagen con el código QR",
            ["Keys_ScanSuccess"] = "QR leído. Revisa el nombre y guarda la clave.",
            ["Keys_NoClipboardContent"] = "El portapapeles no tiene una imagen ni un enlace otpauth para leer",
            ["Keys_FileTooLarge"] = "La imagen es demasiado grande (máximo 10 MB)",
            ["Keys_PickerTitle"] = "Elige una imagen con un código QR",
    };
}
