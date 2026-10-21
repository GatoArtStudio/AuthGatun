using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace AuthGatun.Services;

public sealed class SingleInstanceService(string name) : IDisposable
{
    private const string ShowMessage = "show";
    private const int ConnectTimeoutMilliseconds = 2000;

    private readonly CancellationTokenSource _cancellation = new();
    private Mutex? _mutex;

    public bool TryAcquire()
    {
        // The Global prefix makes the mutex visible across login sessions; on Linux it is otherwise scoped to the terminal session.
        _mutex = new Mutex(true, $"Global\\{name}", out var createdNew);
        if (createdNew)
            return true;

        _mutex.Dispose();
        _mutex = null;
        return false;
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
        _mutex?.Dispose();
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
            }
        }
    }
}
