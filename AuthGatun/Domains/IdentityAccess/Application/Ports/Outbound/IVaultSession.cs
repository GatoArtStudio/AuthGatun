namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface IVaultSession
{
    bool IsOpen { get; }
    bool Exists(string userName);
    string Create(string userName, string password);
    bool Open(string userName, string password);
    bool OpenWithRecoveryCode(string userName, string recoveryCode);
    void ChangePassword(string newPassword);
    string RegenerateRecoveryCode();
    void Close();
}
