using System;
using System.IO;
using System.Threading.Tasks;
using AuthGatun.Services;
using Xunit;

namespace AuthGatun.Tests.Services;

public class SingleInstanceServiceTest : IDisposable
{
    private readonly string _lockDirectory = Path.Combine(Path.GetTempPath(), "authgatun-lock-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_lockDirectory))
            Directory.Delete(_lockDirectory, true);
    }

    private SingleInstanceService NewInstance(string name) => new(name, _lockDirectory);

    private static string UniqueName() => "AuthGatun-test-" + Guid.NewGuid().ToString("N");

    [Fact]
    public void OnlyTheFirstInstanceAcquiresTheLock()
    {
        var name = UniqueName();
        using var first = NewInstance(name);
        using var second = NewInstance(name);

        Assert.True(first.TryAcquire());
        Assert.False(second.TryAcquire());
    }

    [Fact]
    public async Task SecondInstanceAsksTheFirstOneToShowItself()
    {
        var name = UniqueName();
        using var first = NewInstance(name);
        using var second = NewInstance(name);
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
        using var instance = NewInstance(UniqueName());

        instance.NotifyRunningInstance();
    }
}
