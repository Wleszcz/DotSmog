using System.Collections.Concurrent;

namespace DotSmog.service;

public class TransferQueueProcessor
{
    private readonly ConcurrentQueue<string> _transferQueue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly SemaphoreSlim _rateLimiter;
    private readonly TokenService _tokenService;

    public TransferQueueProcessor( int maxConcurrentRequests)
    {
        _rateLimiter = new SemaphoreSlim(maxConcurrentRequests); // Ograniczenie zapytań
        _tokenService = new TokenService();
        Task.Run(() => ProcessQueueAsync());
    }

    public void EnqueueTransfer(string stationId)
    {
        _transferQueue.Enqueue(stationId);
        _signal.Release();
    }

    private async Task ProcessQueueAsync()
    {
        while (true)
        {
            await _signal.WaitAsync();

            while (_transferQueue.TryDequeue(out var stationId))
            {
                await _rateLimiter.WaitAsync(); // Ograniczenie równoległości
                try
                {
                    Console.WriteLine($"Przetwarzanie transferu dla: {stationId}");
                    await _tokenService.TransferTo(stationId);
                    Console.WriteLine($"Transfer zakończony dla: {stationId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas transferu dla {stationId}: {ex.Message}");
                }
                finally
                {
                    _rateLimiter.Release(); // Zwolnienie ograniczenia
                }

                // Opcjonalne opóźnienie między zapytaniami
                await Task.Delay(500); // 500 ms
            }
        }
    }
}
