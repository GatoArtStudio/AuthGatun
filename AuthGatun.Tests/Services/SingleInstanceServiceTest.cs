using System;
using System.Threading.Tasks;
using AuthGatun.Services;
using Xunit;

namespace AuthGatun.Tests.Services;

public class SingleInstanceServiceTest
{
    private static string UniqueName() => "AuthGatun-test-" + Guid.NewGuid().ToString("N");

    [Fact]
    public void OnlyTheFirstInstanceAcquiresTheLock()
    {
        var name = UniqueName();
        using var first = new SingleInstanceService(name);
        using var second = new SingleInstanceService(name);

        Assert.True(first.TryAcquire());
        Assert.False(second.TryAcquire());
    }

    [Fact]
    public async Task SecondInstanceAsksTheFirstOneToShowItself()
    {
        var name = UniqueName();
        using var first = new SingleInstanceService(name);
        using var second = new SingleInstanceService(name);
        var shown = new TaskCompletionSource();

        Assert.True(first.TryAcquire());
        first.StartListening(() => shown.TrySetResult());
        Assert.False(second.TryAcquire());

        second.NotifyRunningInstance();

        await shown.Task.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void NotifyingWithoutARunningInstanceDoesNotThrow()
    {
        using var instance = new SingleInstanceService(UniqueName());

        instance.NotifyRunningInstance();
    }
}
