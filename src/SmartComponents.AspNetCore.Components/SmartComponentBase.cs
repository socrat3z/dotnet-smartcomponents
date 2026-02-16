using Microsoft.AspNetCore.Components;
using System.Timers;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SmartComponents.AspNetCore.Components;

public enum ExecutionMode
{
    Manual,
    Automatic
}

public enum AutoTrigger
{
    Debounce,
    OnBlur
}

public class InferenceResult
{
    public TimeSpan Duration { get; set; }
    // Add other metadata later e.g. TokenUsage, Model
}

public abstract class SmartComponentBase : ComponentBase, IDisposable
{
    private CancellationTokenSource? _cts;
    private System.Timers.Timer? _debounceTimer; // Using System.Timers.Timer for debounce
    private readonly Queue<DateTime> _requestTimestamps = new();
    private readonly object _lock = new();

    [Parameter] public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.Manual;

    [Parameter] public AutoTrigger AutoTrigger { get; set; } = AutoTrigger.Debounce;

    [Parameter] public int DebounceMs { get; set; } = 500;

    [Parameter] public int MinIntervalMs { get; set; } = 1000;

    [Parameter] public int MaxRequestsPerMinute { get; set; } = 10;

    [Parameter] public EventCallback<InferenceResult> OnInferenceComplete { get; set; }

    [Parameter] public EventCallback<Exception> OnInferenceError { get; set; }

    protected bool IsLoading { get; private set; }
    protected bool HasError { get; private set; }
    protected string? ErrorMessage { get; private set; }

    protected async Task TriggerInferenceAsync(Func<CancellationToken, Task> inferenceAction, bool isAutoTrigger = false)
    {
        if (isAutoTrigger && ExecutionMode == ExecutionMode.Manual)
        {
            return;
        }

        if (isAutoTrigger && AutoTrigger == AutoTrigger.Debounce)
        {
            DebounceInference(inferenceAction);
            return;
        }

        await ExecuteInferenceAsync(inferenceAction);
    }

    private void DebounceInference(Func<CancellationToken, Task> inferenceAction)
    {
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();

        _debounceTimer = new System.Timers.Timer(DebounceMs);
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += async (sender, args) =>
        {
            await InvokeAsync(() => ExecuteInferenceAsync(inferenceAction));
        };
        _debounceTimer.Start();
    }

    protected async Task ExecuteInferenceAsync(Func<CancellationToken, Task> inferenceAction)
    {
        if (!CheckRateLimit())
        {
            // Rate limit exceeded
            // Might want to emit an error or just ignore
             await HandleErrorAsync(new InvalidOperationException("Rate limit exceeded."));
            return;
        }

        CancelCurrentInference();

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            var startTime = DateTime.UtcNow;
            await inferenceAction(token);
            var duration = DateTime.UtcNow - startTime;

            if (!token.IsCancellationRequested)
            {
                IsLoading = false;
                StateHasChanged();
                 await OnInferenceComplete.InvokeAsync(new InferenceResult { Duration = duration });
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
             IsLoading = false;
             StateHasChanged();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task HandleErrorAsync(Exception ex)
    {
        HasError = true;
        ErrorMessage = ex.Message;
        IsLoading = false;
        StateHasChanged();
        await OnInferenceError.InvokeAsync(ex);
    }

    private void CancelCurrentInference()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private bool CheckRateLimit()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            
            // Remove requests older than 1 minute
            while (_requestTimestamps.Count > 0 && (now - _requestTimestamps.Peek()).TotalMinutes >= 1)
            {
                _requestTimestamps.Dequeue();
            }

            if (_requestTimestamps.Count >= MaxRequestsPerMinute)
            {
                return false;
            }

            if (_requestTimestamps.Count > 0)
            {
                 var lastRequest = _requestTimestamps.Last();
                 if ((now - lastRequest).TotalMilliseconds < MinIntervalMs)
                 {
                     return false;
                 }
            }

            _requestTimestamps.Enqueue(now);
            return true;
        }
    }

    public void Dispose()
    {
        CancelCurrentInference();
        _debounceTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
