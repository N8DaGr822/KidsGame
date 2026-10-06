using KidsGameLauncher.Models;

namespace KidsGameLauncher.Services;

/// <summary>
/// Ticks in the background for as long as a Kid profile is active,
/// adding elapsed time to that profile's daily usage total and flagging
/// when a parent-configured daily limit is reached. Driven by AppState's
/// session-boundary event rather than any one page's lifecycle, so it
/// keeps counting no matter which screen - or which iframe-hosted game -
/// is currently showing.
/// </summary>
public class PlayTimeTracker : IDisposable, IAsyncDisposable
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(15);

    private readonly AppState _state;
    private readonly AppDataService _data;
    private readonly TimeProvider _clock;
    private ITimer? _timer;
    private Profile? _trackedProfile;
    private DateTimeOffset _lastAccountedAt;
    private Task _pendingWork = Task.CompletedTask;
    private readonly Dictionary<(string ProfileId, DateTime Date), double> _partialSeconds = new();
    private int _generation;
    private bool _disposed;

    public bool LimitReached { get; private set; }
    public bool CheckingLimit { get; private set; }

    public event Action? OnChange;

    public PlayTimeTracker(AppState state, AppDataService data, TimeProvider? clock = null)
    {
        _state = state;
        _data = data;
        _clock = clock ?? TimeProvider.System;
        _state.OnChange += HandleProfileChanged;
        HandleProfileChanged();
    }

    private void HandleProfileChanged()
    {
        if (_disposed) return;
        var profile = _state.CurrentProfile is { Type: ProfileType.Kid } kid ? kid : null;
        StopTimer();
        LimitReached = false;
        CheckingLimit = profile is not null;
        OnChange?.Invoke();
        _pendingWork = ChangeProfileAsync(_pendingWork, profile, _clock.GetUtcNow(), ++_generation);
    }

    private async Task ChangeProfileAsync(Task previous, Profile? profile, DateTimeOffset at, int generation)
    {
        await previous;
        await AccountUntilAsync(at);
        _trackedProfile = profile;
        _lastAccountedAt = at;
        await CheckLimitAsync(at, generation);
    }

    private void Tick()
    {
        if (!_disposed) _pendingWork = TickAsync(_pendingWork, _clock.GetUtcNow(), _generation);
    }

    public Task FlushAsync()
    {
        Tick();
        return _pendingWork;
    }

    private async Task TickAsync(Task previous, DateTimeOffset at, int generation)
    {
        await previous;
        await AccountUntilAsync(at);
        await CheckLimitAsync(at, generation);
    }

    private async Task AccountUntilAsync(DateTimeOffset at)
    {
        if (_trackedProfile is not { } profile || at <= _lastAccountedAt) return;
        var cursor = _lastAccountedAt;
        _lastAccountedAt = at;
        var data = await _data.LoadAsync();

        // Screen Time Policy: Count elapsed time with a child active, including background/sleep.
        // Day Rollover: Split intervals at local midnight and retain subsecond time across switches.
        while (cursor < at)
        {
            var local = TimeZoneInfo.ConvertTime(cursor, _clock.LocalTimeZone);
            var date = local.Date;
            var midnight = date.AddDays(1);
            var nextDay = new DateTimeOffset(midnight, _clock.LocalTimeZone.GetUtcOffset(midnight));
            var end = at < nextDay ? at : nextDay;
            var key = (profile.Id, date);
            var elapsed = (end - cursor).TotalSeconds + _partialSeconds.GetValueOrDefault(key);
            var seconds = (int)Math.Min(int.MaxValue, Math.Floor(elapsed));
            _partialSeconds[key] = elapsed - seconds;
            var used = data.DailyUsageSeconds.TryGetValue(profile.Id, out var days)
                ? days.GetValueOrDefault(date.ToString("yyyy-MM-dd")) : 0;
            if (profile.DailyTimeLimitMinutes is > 0)
                seconds = (int)Math.Min(seconds, Math.Max(0L, profile.DailyTimeLimitMinutes.Value * 60L - used));
            if (seconds > 0) await _data.AddUsageSecondsAsync(profile.Id, seconds, date);
            cursor = end;
        }
    }

    private async Task CheckLimitAsync(DateTimeOffset at, int generation)
    {
        var data = await _data.LoadAsync();
        if (_disposed || generation != _generation) return;
        var date = TimeZoneInfo.ConvertTime(at, _clock.LocalTimeZone).Date;
        var used = _trackedProfile is { } profile && data.DailyUsageSeconds.TryGetValue(profile.Id, out var days)
            ? days.GetValueOrDefault(date.ToString("yyyy-MM-dd")) : 0;
        var limit = _trackedProfile?.DailyTimeLimitMinutes;
        LimitReached = limit is > 0 && used >= limit.Value * 60L;
        CheckingLimit = false;
        StopTimer();
        if (_trackedProfile is not null)
        {
            var untilLimit = limit is > 0 && !LimitReached ? limit.Value * 60L - used : TickInterval.TotalSeconds;
            var due = TimeSpan.FromSeconds(Math.Min(TickInterval.TotalSeconds, untilLimit));
            // Keep checking after lockout so the next local day unlocks without restarting the app.
            _timer = _clock.CreateTimer(_ => Tick(), null, due, Timeout.InfiniteTimeSpan);
        }
        OnChange?.Invoke();
    }

    private void StopTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _state.OnChange -= HandleProfileChanged;
        StopTimer();
        _pendingWork = ChangeProfileAsync(_pendingWork, null, _clock.GetUtcNow(), ++_generation);
    }

    public async ValueTask DisposeAsync()
    {
        Dispose();
        await _pendingWork;
    }
}
