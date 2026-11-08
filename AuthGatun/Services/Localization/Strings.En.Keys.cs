using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsEnglish
{
    private static Dictionary<string, string> KeysStrings() => new()
    {
            ["Keys_Title"] = "New key",
            ["Keys_Hint"] = "Paste the secret key the service gives you when you enable two-step verification.",
            ["Keys_ServiceName"] = "Service name",
            ["Keys_ServiceNamePlaceholder"] = "Enter the service name",
            ["Keys_Secret"] = "Secret key",
            ["Keys_SecretPlaceholder"] = "Enter the TOTP key",
            ["Keys_Save"] = "Save",
            ["Keys_NotifyTitle"] = "New key",
            ["Keys_FillAll"] = "Please fill in all the fields",
            ["Keys_NoSpaces"] = "The TOTP key cannot contain spaces",
            ["Keys_NoUser"] = "No user is signed in",
            ["Keys_Saved"] = "Key saved",
            ["Keys_ScanFromClipboard"] = "Scan QR from clipboard",
            ["Keys_ScanFromFile"] = "Scan QR from an image file",
            ["Keys_DropHint"] = "or drop an image with the QR code here",
            ["Keys_ScanSuccess"] = "QR read. Review the name and save the key.",
            ["Keys_NoClipboardContent"] = "The clipboard has no image or otpauth link to read",
            ["Keys_FileTooLarge"] = "The image is too large (10 MB maximum)",
            ["Keys_PickerTitle"] = "Choose an image with a QR code",
    };
}
