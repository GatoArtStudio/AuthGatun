using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsEnglish
{
    private static Dictionary<string, string> HomeStrings() => new()
    {
            ["Home_Title"] = "Your codes",
            ["Home_Empty"] = "You have no keys yet",
            ["Home_EmptyHint"] = "Add them from the Keys tab.",
            ["Home_Copy"] = "Copy",
            ["Home_CopyCode"] = "Copy code",
            ["Home_Delete"] = "Delete",
            ["Home_CountdownTip"] = "Seconds before the code changes",
            ["Home_NotifyTitle"] = "TOTP keys",
            ["Home_KeyNotFound"] = "Key not found",
            ["Home_Copied"] = "Code copied. It will be cleared from the clipboard in 30 seconds.",
            ["Home_Deleted"] = "Key deleted",
    };
}
