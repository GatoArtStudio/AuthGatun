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
        private readonly Dictionary<string, string> _recoveryCodes = new();
        private string? _current;

        public bool IsOpen => _current is not null;
        public bool Exists(string userName) => _passwords.ContainsKey(userName);

        public string Create(string userName, string password)
        {
            _passwords[userName] = password;
            _recoveryCodes[userName] = "RECOVERY-" + userName;
            _current = userName;
            return _recoveryCodes[userName];
        }

        public bool Open(string userName, string password)
        {
            _current = _passwords.TryGetValue(userName, out var stored) && stored == password ? userName : null;
            return IsOpen;
        }

        public bool OpenWithRecoveryCode(string userName, string recoveryCode)
        {
            _current = _recoveryCodes.TryGetValue(userName, out var stored) && stored == recoveryCode ? userName : null;
            return IsOpen;
        }

        public void ChangePassword(string newPassword) => _passwords[_current!] = newPassword;

        public string RegenerateRecoveryCode() => _recoveryCodes[_current!] = "NEW-" + _current;

        public void Close() => _current = null;
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

    private RegistrationResult Register(string name = "gato", string password = "Password-testing") =>
        new RegisterUserUseCase(_publisher, _vault, _repository, _hasher).Execute(name, password);

    private AuthenticatedUser RegisterUser(string name = "gato", string password = "Password-testing") =>
        Register(name, password).User;

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

    [Fact]
    public void RegisterReturnsTheRecoveryCode()
    {
        Assert.Equal("RECOVERY-gato", Register().RecoveryCode);
    }

    [Fact]
    public void ChangePasswordRequiresTheCurrentPasswordAndReplacesIt()
    {
        var user = RegisterUser();
        var changePassword = new ChangePasswordUseCase(_vault, _repository, _hasher);

        Assert.Throws<ArgumentException>(() => changePassword.Execute(user.Id, "wrong-password", "New-password-1"));

        changePassword.Execute(user.Id, "Password-testing", "New-password-1");

        var login = new LoginUseCase(_vault, _repository);
        Assert.Null(login.Execute("gato", "Password-testing"));
        Assert.NotNull(login.Execute("gato", "New-password-1"));
    }

    [Fact]
    public void RecoverAccessWithTheRecoveryCodeSetsANewPassword()
    {
        RegisterUser();
        var recover = new RecoverAccessUseCase(_vault, _repository, _hasher);

        Assert.Null(recover.Execute("gato", "wrong-code", "New-password-1"));
        Assert.NotNull(recover.Execute("gato", "RECOVERY-gato", "New-password-1"));
        Assert.NotNull(new LoginUseCase(_vault, _repository).Execute("gato", "New-password-1"));
    }

    [Fact]
    public void RegenerateRecoveryCodeInvalidatesTheOldOne()
    {
        RegisterUser();

        var code = new RegenerateRecoveryCodeUseCase(_vault).Execute();

        Assert.Equal("NEW-gato", code);
        Assert.False(_vault.OpenWithRecoveryCode("gato", "RECOVERY-gato"));
        Assert.True(_vault.OpenWithRecoveryCode("gato", code));
    }
}
