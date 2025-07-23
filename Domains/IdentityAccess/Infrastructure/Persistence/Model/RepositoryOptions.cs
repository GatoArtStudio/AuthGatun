using AuthGatun.Domains.IdentityAccess.Infrastructure.Enums;

namespace AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence.Model;

public record RepositoryOptions(TypeRepository Type, string AppFolderPathData, string? DbHost, int? DbPort, string? DbDatabase, string? DbUser, string? DbPassword);