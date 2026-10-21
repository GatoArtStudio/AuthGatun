namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hashedPassword);
}