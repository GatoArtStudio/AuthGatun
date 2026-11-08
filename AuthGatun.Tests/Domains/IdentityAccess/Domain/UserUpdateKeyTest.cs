using System;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using AuthGatun.Domains.IdentityAccess.Domain.Model;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Domain;

public class UserUpdateKeyTest
{
    [Fact]
    public void UpdateKeyReplacesElementKeepingIdentityFavoriteAndOrder()
    {
        var user = new User(Guid.NewGuid(), new UserName("gato"), new Password("hashed-password"), true);
        var key = new Key(Guid.NewGuid(), user.Id, "github", "SECRET", null, true, 3);
        user.AddKey(key);

        Assert.True(user.UpdateKey(key.Id, "gitlab", "OTHER"));

        var updated = Assert.Single(user.Keys);
        Assert.Equal(key.Id, updated.Id);
        Assert.Equal("gitlab", updated.ServiceName);
        Assert.Equal("OTHER", updated.SecretKey);
        Assert.True(updated.IsFavorite);
        Assert.Equal(3, updated.SortOrder);
        Assert.Equal(key.Parameters, updated.Parameters);
    }

    [Fact]
    public void UpdateKeyReturnsFalseForUnknownKey()
    {
        var user = new User(Guid.NewGuid(), new UserName("gato"), new Password("hashed-password"), true);
        Assert.False(user.UpdateKey(Guid.NewGuid(), "x", null));
    }
}
