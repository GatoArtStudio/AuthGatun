namespace AuthGatun.Domains.IdentityAccess.Application;

public enum IdentityAccessError
{
    InvalidUserName,
    InvalidPassword,
    PasswordTooShort,
    UserAlreadyExists,
    UserNotFound,
    KeyNotFound,
    WrongCurrentPassword,
    BackupPasswordTooShort,
    InvalidBackupPassword,
    WrongBackupPassword,
    InvalidRecoveryData,
    SaveFailed,
    QrNotFound,
    QrNotOtpAuth,
    QrInvalidSecret,
    QrUnsupportedParameters,
    ImageUnreadable
}
