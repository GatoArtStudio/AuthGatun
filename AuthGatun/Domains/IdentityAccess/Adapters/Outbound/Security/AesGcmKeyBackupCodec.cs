using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using Konscious.Security.Cryptography;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Security;

public partial class AesGcmKeyBackupCodec : IKeyBackupCodec
{
    private const int SaltSize = 16;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 4;
    private const int MemorySize = 65536;
    private const int DegreeOfParallelism = 1;
    private const int FormatVersion = 1;
    private static readonly byte[] Magic = "AGB1"u8.ToArray();

    public byte[] Encode(IReadOnlyList<BackupKey> keys, string password)
    {
        var payload = new BackupPayload(FormatVersion, keys.Select(k => new BackupEntry(k.ServiceName, k.SecretKey)).ToList());
        var plaintext = JsonSerializer.SerializeToUtf8Bytes(payload, BackupJsonContext.Default.BackupPayload);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plaintext.Length];
        var tag = new byte[TagSize];
        var key = DeriveKey(password, salt);

        try
        {
            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, plaintext, cipher, tag, Header(salt));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
            CryptographicOperations.ZeroMemory(plaintext);
        }

        return [.. Magic, .. salt, .. nonce, .. tag, .. cipher];
    }

    public IReadOnlyList<BackupKey>? Decode(byte[] data, string password)
    {
        var headerSize = Magic.Length + SaltSize + NonceSize + TagSize;
        if (data.Length <= headerSize || !data.AsSpan(0, Magic.Length).SequenceEqual(Magic))
            return null;

        var salt = data[Magic.Length..(Magic.Length + SaltSize)];
        var nonce = data[(Magic.Length + SaltSize)..(Magic.Length + SaltSize + NonceSize)];
        var tag = data[(Magic.Length + SaltSize + NonceSize)..headerSize];
        var cipher = data[headerSize..];
        var plaintext = new byte[cipher.Length];
        var key = DeriveKey(password, salt);

        try
        {
            using var aes = new AesGcm(key, TagSize);
            aes.Decrypt(nonce, cipher, tag, plaintext, Header(salt));

            var payload = JsonSerializer.Deserialize(plaintext, BackupJsonContext.Default.BackupPayload);
            if (payload is null || payload.Version != FormatVersion)
                return null;

            return payload.Keys.Select(e => new BackupKey(e.ServiceName, e.SecretKey)).ToList();
        }
        catch (Exception e) when (e is CryptographicException or JsonException)
        {
            return null;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    private static byte[] Header(byte[] salt) => [.. Magic, .. salt];

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        try
        {
            var argon2 = new Argon2id(passwordBytes)
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                MemorySize = MemorySize,
                Iterations = Iterations
            };
            return argon2.GetBytes(KeySize);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    internal sealed record BackupEntry(string ServiceName, string SecretKey);

    internal sealed record BackupPayload(int Version, List<BackupEntry> Keys);

    [JsonSerializable(typeof(BackupPayload))]
    internal sealed partial class BackupJsonContext : JsonSerializerContext;
}
