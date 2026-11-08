using System.Collections.Generic;

namespace AuthGatun.Services.Localization;

public static partial class StringsEnglish
{
    private static Dictionary<string, string> ErrorsStrings() => new()
    {
            ["Error_Unexpected"] = "Something went wrong, please try again",
            ["Error_InvalidUserName"] = "The username cannot be empty or contain spaces",
            ["Error_InvalidPassword"] = "The password cannot be empty or contain spaces",
            ["Error_PasswordTooShort"] = "The new password must have at least 8 characters",
            ["Error_UserAlreadyExists"] = "That user already exists, choose another one",
            ["Error_UserNotFound"] = "The user does not exist",
            ["Error_WrongCurrentPassword"] = "The current password is incorrect",
            ["Error_BackupPasswordTooShort"] = "The backup password must have at least 8 characters",
            ["Error_InvalidBackupPassword"] = "The backup password cannot be empty or contain spaces",
            ["Error_WrongBackupPassword"] = "Wrong backup password or the file is not a valid backup",
            ["Error_InvalidRecoveryData"] = "The username and the recovery code cannot be empty",
            ["Error_SaveFailed"] = "The data could not be saved",
            ["Error_QrNotFound"] = "No QR code was found in the image",
            ["Error_QrNotOtpAuth"] = "The QR code is not a two-step verification key",
            ["Error_QrInvalidSecret"] = "The QR code does not contain a valid secret key",
            ["Error_QrUnsupportedParameters"] = "This key uses settings that are not supported yet (only 6 digits, 30 seconds and SHA1)",
            ["Error_ImageUnreadable"] = "The image could not be read",
    };
}
