using System.Collections.Generic;

namespace AuthGatun.Domains.IdentityAccess.Application.Models;

public record ScanResultModel(IReadOnlyList<ScannedKeyModel> Keys, int Unsupported = 0);
