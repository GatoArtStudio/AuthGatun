namespace AuthGatun.Domains.IdentityAccess.Application.Models;

public record RegistrationResult(AuthenticatedUser User, string RecoveryCode);
