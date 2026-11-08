using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace AuthGatun.Services;

public sealed class SingleInstanceService(string name, string? lockDirectory = null) : IDisposable
{
    private const string ShowMessage = "show";
    private const int ConnectTimeoutMilliseconds = 2000;
    private const int RetryDelayMilliseconds = 200;

    private readonly CancellationTokenSource _cancellation = new();
    private FileStream? _lock;

    public bool TryAcquire()
    {
        // A lock on a file is released by the operating system when the process ends, even if it crashes, and it works
        // across login sessions. Named mutexes are not shared between processes in Native AOT builds on Linux.
        try
        {
            var directory = lockDirectory ?? LockDirectory();
            Directory.CreateDirectory(directory);

            _lock = new FileStream(
                Path.Combine(directory, name + ".lock"),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _lock = null;
            return false;
        }
    }

    public void NotifyRunningInstance()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", name, PipeDirection.Out);
            client.Connect(ConnectTimeoutMilliseconds);

            using var writer = new StreamWriter(client) { AutoFlush = true };
            writer.WriteLine(ShowMessage);
        }
        catch (Exception e) when (e is IOException or TimeoutException or UnauthorizedAccessException)
        {
            // The running instance is not reachable, nothing else can be done from here.
        }
    }

    public void StartListening(Action onShowRequested)
    {
        _ = Task.Run(() => ListenAsync(onShowRequested, _cancellation.Token));
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        _cancellation.Dispose();
        _lock?.Dispose();
    }

    private async Task ListenAsync(Action onShowRequested, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var server = new NamedPipeServerStream(
                    name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync(cancellationToken);

                using var reader = new StreamReader(server);
                if (await reader.ReadLineAsync(cancellationToken) == ShowMessage)
                    onShowRequested();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (IOException)
            {
                // A client disconnected too early, wait for the next one.
                await Task.Delay(RetryDelayMilliseconds, CancellationToken.None);
            }
        }
    }

    private static string LockDirectory()
    {
        var runtimeDirectory = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
        return !string.IsNullOrEmpty(runtimeDirectory) && Directory.Exists(runtimeDirectory)
            ? runtimeDirectory
            : Path.GetTempPath();
    }
}
