namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Out;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hashedPassword);
}