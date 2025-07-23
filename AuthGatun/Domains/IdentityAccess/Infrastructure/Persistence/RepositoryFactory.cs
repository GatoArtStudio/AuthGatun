using System;
using System.Collections.Concurrent;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Enums;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence.Model;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence.Repositories;

namespace AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence;

public class RepositoryFactory
{
    private static RepositoryFactory? _instance;
    private static readonly object Lock = new object();
    
    private readonly ConcurrentDictionary<TypeRepository, IUserRepository> _repositories = new();
    private readonly RepositoryOptions _options;

    private RepositoryFactory(RepositoryOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }
    
    public static RepositoryFactory GetInstance()
    {
        return GetInstance(null);
    }

    public static RepositoryFactory GetInstance(RepositoryOptions? options)
    {
        if (_instance == null)
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    if (options == null)
                        throw new ArgumentNullException(nameof(options), "Repository options cannot be null.");
                    
                    _instance = new RepositoryFactory(options);
                }
            }
        }

        return _instance;
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
            
            IUserRepository sqLiteRepository = new SqLiteRepository(_options);
            _repositories.TryAdd(TypeRepository.SqLite, sqLiteRepository);
            return sqLiteRepository;
        }
        
        return _repositories[TypeRepository.Default];
    }
}