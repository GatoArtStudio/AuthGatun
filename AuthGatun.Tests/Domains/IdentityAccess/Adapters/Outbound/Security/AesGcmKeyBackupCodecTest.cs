using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Security;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Adapters.Outbound.Security;

public class AesGcmKeyBackupCodecTest
{
    private const string Password = "Backup-password";
    private readonly AesGcmKeyBackupCodec _codec = new();
    private readonly List<BackupKey> _keys = [new("github", "JBSWY3DPEHPK3PXP"), new("gitlab", "MFRGGZDFMZTWQ2LK")];

    [Fact]
    public void DecodeReturnsTheEncodedKeys()
    {
        var decoded = _codec.Decode(_codec.Encode(_keys, Password), Password);

        Assert.Equal(_keys, decoded);
    }

    [Fact]
    public void DecodeReturnsTheParametersOfEachKey()
    {
        List<BackupKey> keys =
        [
            new("a", "JBSWY3DPEHPK3PXP", new TotpParametersModel("SHA256", 8, 60)),
            new("b", "MFRGGZDFMZTWQ2LK", TotpParametersModel.Default)
        ];

        Assert.Equal(keys, _codec.Decode(_codec.Encode(keys, Password), Password));
    }

    [Fact]
    public void DecodeOldPayloadWithoutParametersReturnsKeysWithoutThem()
    {
        var decoded = _codec.Decode(EncodeRaw("""{"Version":1,"Keys":[{"ServiceName":"github","SecretKey":"JBSWY3DPEHPK3PXP"}]}"""), Password);

        Assert.Equal([new BackupKey("github", "JBSWY3DPEHPK3PXP")], decoded);
    }

    [Fact]
    public void DecodePartialParametersFillsTheDefaults()
    {
        var decoded = _codec.Decode(EncodeRaw("""{"Version":1,"Keys":[{"ServiceName":"a","SecretKey":"JBSWY3DPEHPK3PXP","Digits":8}]}"""), Password);

        Assert.Equal(new TotpParametersModel("SHA1", 8, 30), Assert.Single(decoded!).Parameters);
    }

    [Fact]
    public void DecodeInvalidParametersReturnsNull()
    {
        Assert.Null(_codec.Decode(EncodeRaw("""{"Version":1,"Keys":[{"ServiceName":"a","SecretKey":"JBSWY3DPEHPK3PXP","Algorithm":"MD5"}]}"""), Password));
    }

    private static byte[] EncodeRaw(string json)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintext = Encoding.UTF8.GetBytes(json);
        var cipher = new byte[plaintext.Length];
        var tag = new byte[16];
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(Password)) { Salt = salt, DegreeOfParallelism = 1, MemorySize = 65536, Iterations = 4 };
        var magic = "AGB1"u8.ToArray();
        using var aes = new AesGcm(argon2.GetBytes(32), 16);
        aes.Encrypt(nonce, plaintext, cipher, tag, (byte[])[.. magic, .. salt]);
        return [.. magic, .. salt, .. nonce, .. tag, .. cipher];
    }

    [Fact]
    public void EncodedBackupDoesNotContainPlainText()
    {
        var text = Encoding.Latin1.GetString(_codec.Encode(_keys, Password));

        Assert.DoesNotContain("github", text);
        Assert.DoesNotContain("JBSWY3DPEHPK3PXP", text);
    }

    [Fact]
    public void DecodeWithWrongPasswordReturnsNull()
    {
        Assert.Null(_codec.Decode(_codec.Encode(_keys, Password), "Another-password"));
    }

    [Fact]
    public void DecodeTamperedBackupReturnsNull()
    {
        var backup = _codec.Encode(_keys, Password);
        backup[^1] ^= 0xFF;

        Assert.Null(_codec.Decode(backup, Password));
    }

    [Fact]
    public void DecodeUnknownFormatReturnsNull()
    {
        Assert.Null(_codec.Decode(Encoding.UTF8.GetBytes("this is not a backup file at all, just text"), Password));
    }
}
