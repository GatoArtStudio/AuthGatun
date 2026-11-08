namespace AuthGatun.Domains.IdentityAccess.Application;

public enum IdentityAccessError
{
    InvalidUserName,
    InvalidPassword,
    PasswordTooShort,
    UserAlreadyExists,
    UserNotFound,
    WrongCurrentPassword,
    BackupPasswordTooShort,
    InvalidBackupPassword,
    WrongBackupPassword,
    InvalidRecoveryData,
    SaveFailed
}
