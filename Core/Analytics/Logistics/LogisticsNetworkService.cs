using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WitpCic.Core.Analytics.Logistics;

public sealed class LogisticsNetworkService : IDisposable
{
    private readonly SupplyNetworkGraph _graph = new();
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new();
    private Task? _bgTask;

    public SupplyNetworkGraph Graph => _graph;

    public LogisticsNetworkService(TimeSpan checkInterval)
    {
        _timer = new PeriodicTimer(checkInterval);
    }

    public void Start()
    {
        if (_bgTask != null) return;
        _bgTask = RunLoopAsync(_cts.Token);
    }

    private async Task RunLoopAsync(CancellationToken token)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(token))
            {
                _graph.EvaluateNetworkHealth();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _timer.Dispose();
        _cts.Dispose();
    }
}
