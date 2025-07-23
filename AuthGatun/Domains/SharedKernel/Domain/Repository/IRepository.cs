using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AuthGatun.Domains.SharedKernel.Domain.Repository;

public interface IRepository<TId, TEntity>
{
    TEntity? Read([NotNull] TId id);
    bool Delete([NotNull] TId id);
    bool Save([NotNull] TEntity entity);
    List<TEntity> ReadAll();
}