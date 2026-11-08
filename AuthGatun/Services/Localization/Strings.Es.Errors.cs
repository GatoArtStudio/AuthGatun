using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsSpanish
{
    private static Dictionary<string, string> ErrorsStrings() => new()
    {
            ["Error_Unexpected"] = "Algo salió mal, inténtalo de nuevo",
            ["Error_InvalidUserName"] = "El usuario no puede ser nulo o contener espacios",
            ["Error_InvalidPassword"] = "La contraseña no puede ser nula o contener espacios",
            ["Error_PasswordTooShort"] = "La nueva contraseña debe tener al menos 8 caracteres",
            ["Error_UserAlreadyExists"] = "Ese usuario ya existe, elige otro",
            ["Error_UserNotFound"] = "El usuario no existe",
            ["Error_KeyNotFound"] = "La clave ya no existe",
            ["Error_WrongCurrentPassword"] = "La contraseña actual es incorrecta",
            ["Error_BackupPasswordTooShort"] = "La contraseña del respaldo debe tener al menos 8 caracteres",
            ["Error_InvalidBackupPassword"] = "La contraseña del respaldo no puede ser nula o contener espacios",
            ["Error_WrongBackupPassword"] = "Contraseña del respaldo incorrecta o el archivo no es un respaldo válido",
            ["Error_InvalidRecoveryData"] = "El usuario y el código de recuperación no pueden estar vacíos",
            ["Error_SaveFailed"] = "No se pudieron guardar los datos",
            ["Error_QrNotFound"] = "No se encontró ningún código QR en la imagen",
            ["Error_QrNotOtpAuth"] = "El código QR no es una clave de verificación en dos pasos",
            ["Error_QrInvalidSecret"] = "El código QR no contiene una clave secreta válida",
            ["Error_QrUnsupportedParameters"] = "Esta clave usa ajustes que no se admiten (algoritmo, dígitos o periodo fuera de rango, o no es una clave por tiempo)",
            ["Error_ImageUnreadable"] = "No se pudo leer la imagen",
            ["Error_MigrationInvalidData"] = "El código QR de migración no es válido o está dañado",
            ["Error_MigrationNoSupportedKeys"] = "El código QR de migración no tiene claves compatibles (solo claves por tiempo con SHA1, SHA256 o SHA512)",
            ["Error_DuplicateKey"] = "Esa clave ya está guardada",
    };
}
