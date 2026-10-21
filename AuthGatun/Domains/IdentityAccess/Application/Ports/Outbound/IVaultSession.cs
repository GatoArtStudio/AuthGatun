namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface IVaultSession
{
    bool IsOpen { get; }
    bool Exists(string userName);
    void Create(string userName, string password);
    bool Open(string userName, string password);
    void Close();
}
