using System.Collections.Generic;
using System.Text;
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
