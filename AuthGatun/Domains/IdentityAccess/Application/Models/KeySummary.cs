using System;

namespace AuthGatun.Domains.IdentityAccess.Application.Models;

public record KeySummary(
    Guid Id,
    string ServiceName,
    TotpParametersModel? Parameters = null,
    bool IsFavorite = false,
    int SortOrder = 0);
