using System;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using AuthGatun.Domains.SharedKernel.Domain.Repository;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Out;

public interface IUserRepository : IRepository<Guid, User>
{
    User? FromName(string userName);
}