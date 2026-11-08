using System;

namespace AuthGatun.Domains.IdentityAccess.Application.Models;

public record TotpCodeModel(
    Guid Id,
    string ServiceName,
    string Code,
    int SecondsRemaining,
    int PeriodSeconds,
    bool IsFavorite = false,
    int SortOrder = 0);
