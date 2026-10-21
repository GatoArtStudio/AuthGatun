using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface IKeyBackupCodec
{
    byte[] Encode(IReadOnlyList<BackupKey> keys, string password);

    // Returns null when the data is not a backup, was tampered with or the password is wrong.
    IReadOnlyList<BackupKey>? Decode(byte[] data, string password);
}
