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

    private AuthenticatedUser RegisterUser(string name = "gato", string password = "Password-testing") =>
        new RegisterUserUseCase(_publisher, _repository, _hasher).Execute(name, password);

    [Fact]
    public void RegisterThenLoginReturnsSameUser()
    {
        var registered = RegisterUser();

        var logged = new LoginUseCase(_repository, _hasher).Execute("gato", "Password-testing");

        Assert.Equal(registered, logged);
    }

    [Fact]
    public void LoginWithWrongPasswordReturnsNull()
    {
        RegisterUser();

        Assert.Null(new LoginUseCase(_repository, _hasher).Execute("gato", "wrong-password"));
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
