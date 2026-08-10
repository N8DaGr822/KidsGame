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
public class PlayTimeTracker : IDisposable
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(15);

    private readonly AppState _state;
    private readonly AppDataService _data;
    private Timer? _timer;
    private string? _trackedProfileId;
    private bool _ticking;

    public bool LimitReached { get; private set; }

    public event Action? OnChange;

    public PlayTimeTracker(AppState state, AppDataService data)
    {
        _state = state;
        _data = data;
        _state.OnChange += HandleProfileChanged;
        HandleProfileChanged();
    }

    private void HandleProfileChanged()
    {
        var profile = _state.CurrentProfile;

        if (profile is null || profile.Type != ProfileType.Kid)
        {
            StopTimer();
            _trackedProfileId = null;
            LimitReached = false;
            OnChange?.Invoke();
            return;
        }

        if (profile.Id == _trackedProfileId)
            return;

        StopTimer();
        _trackedProfileId = profile.Id;
        LimitReached = false;
        OnChange?.Invoke();
        _timer = new Timer(_ => Tick(), null, TickInterval, TickInterval);
    }

    private void Tick()
    {
        // Timer callbacks can overlap a slow previous tick; skip rather
        // than queue, since a missed tick just gets picked up next time.
        if (_ticking) return;
        _ticking = true;

        _ = TickAsync();
    }

    private async Task TickAsync()
    {
        try
        {
            var profileId = _trackedProfileId;
            if (profileId is null) return;

            var totalSeconds = await _data.AddUsageSecondsAsync(profileId, (int)TickInterval.TotalSeconds);

            var profile = (await _data.GetProfilesAsync()).FirstOrDefault(p => p.Id == profileId);
            var limitMinutes = profile?.DailyTimeLimitMinutes;

            if (limitMinutes is > 0 && totalSeconds >= limitMinutes * 60)
            {
                LimitReached = true;
                StopTimer();
            }

            OnChange?.Invoke();
        }
        finally
        {
            _ticking = false;
        }
    }

    private void StopTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void Dispose()
    {
        _state.OnChange -= HandleProfileChanged;
        StopTimer();
    }
}
