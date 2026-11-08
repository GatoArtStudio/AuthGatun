using System;
using System.Linq;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Security;
using AuthGatun.Domains.IdentityAccess.Application;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Application.UseCases;
using AuthGatun.Domains.IdentityAccess.Domain.Event;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;
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
        public string Generate(string secretKey, TotpParameters parameters) =>
            "code-" + secretKey + "-" + parameters.Digits;
        public int RemainingSeconds(TotpParameters parameters) => parameters.PeriodSeconds / 2;
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

        Assert.Equal(IdentityAccessError.UserAlreadyExists, Assert.Throws<IdentityAccessException>(() => RegisterUser()).Error);
    }

    [Fact]
    public void NewKeyIsListedAndGeneratesTotpCode()
    {
        var user = RegisterUser();
        new NewKeyUseCase(_publisher, _repository).Execute(new NewKeyRequest(user.Id, "github", "SECRET"));

        var keys = new ReadKeysUseCase(_repository).Execute(user.Id);

        var key = Assert.Single(keys);
        Assert.Equal("github", key.ServiceName);
        Assert.Equal("code-SECRET-6", new GenerateTotpCodeUseCase(_repository, new FakeTotpCodeGenerator()).Execute(key.Id));
    }

    [Fact]
    public void ReadTotpCodesReturnsCodeAndRemainingTimeForEachKey()
    {
        var user = RegisterUser();
        new NewKeyUseCase(_publisher, _repository).Execute(new NewKeyRequest(user.Id, "github", "SECRET"));

        var codes = new ReadTotpCodesUseCase(_repository, new FakeTotpCodeGenerator()).Execute(user.Id);

        var code = Assert.Single(codes);
        Assert.Equal("github", code.ServiceName);
        Assert.Equal("code-SECRET-6", code.Code);
        Assert.Equal(15, code.SecondsRemaining);
        Assert.Equal(30, code.PeriodSeconds);
    }

    private (AuthenticatedUser User, Guid KeyId) UserWithKey()
    {
        var user = RegisterUser();
        new NewKeyUseCase(_publisher, _repository).Execute(new NewKeyRequest(user.Id, "github", "SECRET"));
        return (user, new ReadKeysUseCase(_repository).Execute(user.Id)[0].Id);
    }

    [Fact]
    public void UpdateKeyRenamesServiceAndKeepsSecretWhenNull()
    {
        var (user, keyId) = UserWithKey();

        new UpdateKeyUseCase(_publisher, _repository).Execute(new UpdateKeyRequest(user.Id, keyId, "gitlab", null));

        Assert.Equal("gitlab", Assert.Single(new ReadKeysUseCase(_repository).Execute(user.Id)).ServiceName);
        Assert.Equal("code-SECRET-6", new GenerateTotpCodeUseCase(_repository, new FakeTotpCodeGenerator()).Execute(keyId));
    }

    [Fact]
    public void UpdateKeyReplacesSecretWhenProvided()
    {
        var (user, keyId) = UserWithKey();

        new UpdateKeyUseCase(_publisher, _repository).Execute(new UpdateKeyRequest(user.Id, keyId, "github", "NEWSECRET"));

        Assert.Equal("code-NEWSECRET-6", new GenerateTotpCodeUseCase(_repository, new FakeTotpCodeGenerator()).Execute(keyId));
    }

    [Fact]
    public void UpdateKeyReportsMissingKey()
    {
        var (user, _) = UserWithKey();

        var error = Assert.Throws<IdentityAccessException>(() =>
            new UpdateKeyUseCase(_publisher, _repository).Execute(new UpdateKeyRequest(user.Id, Guid.NewGuid(), "x", null)));

        Assert.Equal(IdentityAccessError.KeyNotFound, error.Error);
    }

    [Fact]
    public void UpdateKeyRejectsBlankName()
    {
        var (user, keyId) = UserWithKey();

        Assert.Throws<ArgumentException>(() =>
            new UpdateKeyUseCase(_publisher, _repository).Execute(new UpdateKeyRequest(user.Id, keyId, "  ", null)));
        Assert.Equal("github", Assert.Single(new ReadKeysUseCase(_repository).Execute(user.Id)).ServiceName);
    }

    [Fact]
    public void ReadTotpCodesUsesTheParametersOfEachKey()
    {
        var user = RegisterUser();
        var newKey = new NewKeyUseCase(_publisher, _repository);
        newKey.Execute(new NewKeyRequest(user.Id, "slow", "SECRET", new TotpParametersModel("SHA256", 8, 60)));
        newKey.Execute(new NewKeyRequest(user.Id, "fast", "OTHER"));

        var codes = new ReadTotpCodesUseCase(_repository, new FakeTotpCodeGenerator()).Execute(user.Id);

        var slow = codes.Single(c => c.ServiceName == "slow");
        Assert.Equal("code-SECRET-8", slow.Code);
        Assert.Equal(60, slow.PeriodSeconds);
        Assert.Equal(30, slow.SecondsRemaining);
        Assert.Equal(30, codes.Single(c => c.ServiceName == "fast").PeriodSeconds);
    }

    [Fact]
    public void NewKeyCanRejectDuplicates()
    {
        var user = RegisterUser();
        var newKey = new NewKeyUseCase(_publisher, _repository);
        newKey.Execute(new NewKeyRequest(user.Id, "github", "SECRET"));

        var error = Assert.Throws<IdentityAccessException>(() =>
            newKey.Execute(new NewKeyRequest(user.Id, "github", "SECRET", RejectDuplicate: true)));

        Assert.Equal(IdentityAccessError.DuplicateKey, error.Error);
        newKey.Execute(new NewKeyRequest(user.Id, "github", "SECRET"));
        Assert.Equal(2, new ReadKeysUseCase(_repository).Execute(user.Id).Count);
    }

    [Fact]
    public void ExportAndImportCarryTheParameters()
    {
        var source = RegisterUser("source");
        var target = RegisterUser("target");
        var expected = new TotpParametersModel("SHA512", 8, 45);
        new NewKeyUseCase(_publisher, _repository).Execute(new NewKeyRequest(source.Id, "github", "SECRET", expected));
        var codec = new AesGcmKeyBackupCodec();
        var backup = new ExportKeysUseCase(_repository, codec).Execute(source.Id, "Backup-password");

        new ImportKeysUseCase(_repository, codec).Execute(target.Id, backup, "Backup-password");

        Assert.Equal(expected, Assert.Single(new ReadKeysUseCase(_repository).Execute(target.Id)).Parameters);
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

        Assert.Equal(
            IdentityAccessError.WrongCurrentPassword,
            Assert.Throws<IdentityAccessException>(() => changePassword.Execute(user.Id, "wrong-password", "New-password-1")).Error);

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

    [Fact]
    public void ExportedKeysCanBeImportedByAnotherUserWithoutDuplicates()
    {
        var source = RegisterUser("gato");
        var target = RegisterUser("perro");
        new NewKeyUseCase(_publisher, _repository).Execute(new NewKeyRequest(source.Id, "github", "SECRET"));
        new NewKeyUseCase(_publisher, _repository).Execute(new NewKeyRequest(source.Id, "gitlab", "OTHER"));
        var codec = new AesGcmKeyBackupCodec();
        var backup = new ExportKeysUseCase(_repository, codec).Execute(source.Id, "Backup-password");
        var import = new ImportKeysUseCase(_repository, codec);

        var first = import.Execute(target.Id, backup, "Backup-password");
        var second = import.Execute(target.Id, backup, "Backup-password");

        Assert.Equal(new ImportResult(2, 0), first);
        Assert.Equal(new ImportResult(0, 2), second);
        Assert.Equal(2, new ReadKeysUseCase(_repository).Execute(target.Id).Count);
    }

    [Fact]
    public void ImportWithWrongPasswordThrows()
    {
        var user = RegisterUser();
        var codec = new AesGcmKeyBackupCodec();
        var backup = new ExportKeysUseCase(_repository, codec).Execute(user.Id, "Backup-password");

        var error = Assert.Throws<IdentityAccessException>(() =>
            new ImportKeysUseCase(_repository, codec).Execute(user.Id, backup, "wrong-password"));
        Assert.Equal(IdentityAccessError.WrongBackupPassword, error.Error);
    }

    [Fact]
    public void ExportRequiresAStrongEnoughPassword()
    {
        var user = RegisterUser();

        var error = Assert.Throws<IdentityAccessException>(() =>
            new ExportKeysUseCase(_repository, new AesGcmKeyBackupCodec()).Execute(user.Id, "short"));
        Assert.Equal(IdentityAccessError.BackupPasswordTooShort, error.Error);
    }
}
