using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsEnglish
{
    private static Dictionary<string, string> EditStrings() => new()
    {
        ["Edit_MenuItem"] = "Edit",
        ["Edit_Title"] = "Edit key",
        ["Edit_NameLabel"] = "Service name",
        ["Edit_SecretWatermark"] = "New secret key (leave empty to keep it)",
        ["Edit_Save"] = "Save",
        ["Edit_Cancel"] = "Cancel",
        ["Edit_Saved"] = "Key updated",
        ["Edit_NameRequired"] = "The service name cannot be empty",
        ["Edit_NotifyTitle"] = "Edit key",
    };
}
