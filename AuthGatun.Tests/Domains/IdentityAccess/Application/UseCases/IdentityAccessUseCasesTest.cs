using System;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Application.UseCases;
using AuthGatun.Domains.IdentityAccess.Domain.Event;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Application.UseCases;

public class IdentityAccessUseCasesTest
{
    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => "hashed-" + password;
        public bool Verify(string password, string hashedPassword) => hashedPassword == Hash(password);
    }

    private sealed class FakeVaultSession : IVaultSession
    {
        private readonly Dictionary<string, string> _passwords = new();
        public bool IsOpen { get; private set; }
        public bool Exists(string userName) => _passwords.ContainsKey(userName);

        public void Create(string userName, string password)
        {
            _passwords[userName] = password;
            IsOpen = true;
        }

        public bool Open(string userName, string password)
        {
            IsOpen = _passwords.TryGetValue(userName, out var stored) && stored == password;
            return IsOpen;
        }

        public void Close() => IsOpen = false;
    }

    private sealed class FakePublisher : IDomainEventPublisher
    {
        public List<IDomainEvent> Published { get; } = [];
        public void Publish(IDomainEvent @event) => Published.Add(@event);
    }

    private sealed class FakeTotpCodeGenerator : ITotpCodeGenerator
    {
        public string Generate(string secretKey) => "code-" + secretKey;
    }

    private readonly InMemoryRepository _repository = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeVaultSession _vault = new();

    private AuthenticatedUser RegisterUser(string name = "gato", string password = "Password-testing") =>
        new RegisterUserUseCase(_publisher, _vault, _repository, _hasher).Execute(name, password);

    [Fact]
    public void RegisterThenLoginReturnsSameUser()
    {
        var registered = RegisterUser();

        var logged = new LoginUseCase(_vault, _repository).Execute("gato", "Password-testing");

        Assert.Equal(registered, logged);
    }

    [Fact]
    public void LoginWithWrongPasswordReturnsNull()
    {
        RegisterUser();

        Assert.Null(new LoginUseCase(_vault, _repository).Execute("gato", "wrong-password"));
        Assert.False(_vault.IsOpen);
    }

    [Fact]
    public void LogoutClosesTheVault()
    {
        RegisterUser();

        new LogoutUseCase(_vault).Execute();

        Assert.False(_vault.IsOpen);
    }

    [Fact]
    public void RegisterDuplicatedUserThrows()
    {
        RegisterUser();

        Assert.Throws<ArgumentException>(() => RegisterUser());
    }

    [Fact]
    public void NewKeyIsListedAndGeneratesTotpCode()
    {
        var user = RegisterUser();
        new NewKeyUseCase(_publisher, _repository).Execute(new NewKeyRequest(user.Id, "github", "SECRET"));

        var keys = new ReadKeysUseCase(_repository).Execute(user.Id);

        var key = Assert.Single(keys);
        Assert.Equal("github", key.ServiceName);
        Assert.Equal("code-SECRET", new GenerateTotpCodeUseCase(_repository, new FakeTotpCodeGenerator()).Execute(key.Id));
    }

    [Fact]
    public void DeleteKeyRemovesItAndReportsMissingKeys()
    {
        var user = RegisterUser();
        new NewKeyUseCase(_publisher, _repository).Execute(new NewKeyRequest(user.Id, "github", "SECRET"));
        var keyId = new ReadKeysUseCase(_repository).Execute(user.Id)[0].Id;
        var delete = new DeleteKeyUseCase(_publisher, _repository);

        Assert.True(delete.Execute(keyId));
        Assert.False(delete.Execute(keyId));
        Assert.Empty(new ReadKeysUseCase(_repository).Execute(user.Id));
    }
}
