using System;
using System.Collections.Concurrent;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;

public class RepositoryFactory
{
    private readonly ConcurrentDictionary<TypeRepository, IUserRepository> _repositories = new();
    private readonly RepositoryOptions _options;
    private readonly SqlCipherVault _vault;

    public RepositoryFactory(RepositoryOptions options, SqlCipherVault vault)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _vault = vault ?? throw new ArgumentNullException(nameof(vault));
    }
    
    public IUserRepository CreateRepository()
    {
        if (_options.Type is TypeRepository.InMemory or TypeRepository.Default)
        {
            if (_repositories.TryGetValue(_options.Type, out var existingRepository))
            {
                return existingRepository;
            }
            
            // If no repository exists for the specified type, create a new one
            IUserRepository repository = new InMemoryRepository();
            _repositories.TryAdd(TypeRepository.Default, repository);
            _repositories.TryAdd(TypeRepository.InMemory, repository);
            return repository;
        }

        if (_options.Type == TypeRepository.SqLite)
        {
            if (_repositories.TryGetValue(_options.Type, out var existingRepository))
            {
                return existingRepository;
            }
            
            IUserRepository sqLiteRepository = new SqLiteRepository(_vault);
            _repositories.TryAdd(TypeRepository.SqLite, sqLiteRepository);
            return sqLiteRepository;
        }
        
        return _repositories[TypeRepository.Default];
    }
}