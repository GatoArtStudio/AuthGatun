using System;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using AuthGatun.Domains.SharedKernel.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface IUserRepository : IRepository<Guid, User>
{
    User? FromName(string userName);
}