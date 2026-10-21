using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Services;
using JetBrains.Annotations;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Infrastructure.Services;

[TestSubject(typeof(Argon2PasswordHasher))]
public class Argon2PasswordHasherTest
{
    private readonly IPasswordHasher _hasher = new Argon2PasswordHasher(); 
    private const string Password = "Password-testing";

    [Fact]
    public void HashDifferentFromPassword()
    {
        string hashPassword = _hasher.Hash(Password);
        
        Assert.True(Password != hashPassword);
    }

    [Fact]
    public void VerificationOfCorrectPassword()
    {
        string hashPasswordStore = _hasher.Hash(Password);
        
        Assert.True(_hasher.Verify(Password, hashPasswordStore));
    }

    [Fact]
    public void VerificationOfIncorrectPassword()
    {
        string hashToVerify = _hasher.Hash(Password + "2");
        
        Assert.False(_hasher.Verify(Password, hashToVerify));
    }
}