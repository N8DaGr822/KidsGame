using KidsGameLauncher.Models;

namespace KidsGameLauncher.Services;

/// <summary>
/// Tracks which profile is active for the current session only. This is
/// deliberately NOT persisted - every launch of the app starts back at
/// the profile picker, which is the behavior you want on a shared kid's
/// tablet (walking away shouldn't leave the app parked on a game).
/// </summary>
public class AppState
{
    public Profile? CurrentProfile { get; private set; }

    public event Action? OnChange;

    public void SetCurrentProfile(Profile? profile)
    {
        CurrentProfile = profile;
        OnChange?.Invoke();
    }

    public void ClearCurrentProfile() => SetCurrentProfile(null);
}
