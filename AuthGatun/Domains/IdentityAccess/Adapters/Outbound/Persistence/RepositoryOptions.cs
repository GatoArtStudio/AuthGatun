
namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;

public record RepositoryOptions(TypeRepository Type, string AppFolderPathData, string? DbHost, int? DbPort, string? DbDatabase, string? DbUser, string? DbPassword);