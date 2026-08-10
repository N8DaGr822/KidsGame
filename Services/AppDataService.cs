using System.Text.Json;
using KidsGameLauncher.Data;
using KidsGameLauncher.Models;
using Microsoft.JSInterop;

namespace KidsGameLauncher.Services;

/// <summary>
/// Owns all persisted app state: profiles, the game catalog, and which
/// games each kid profile can access. Backed by browser localStorage
/// for v1 - small data volume (a handful of profiles/games), no server,
/// works fully offline. If this ever needs multi-device sync, swap this
/// service's internals for a real database without touching the pages
/// that consume it.
/// </summary>
public class AppDataService
{
    private const string StorageKey = "kgl_appdata";
    private readonly IJSRuntime _js;
    private AppData? _cache;

    public AppDataService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<AppData> LoadAsync()
    {
        if (_cache is not null)
            return _cache;

        var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);

        if (string.IsNullOrWhiteSpace(json))
        {
            _cache = SeedDefaultData();
            await SaveAsync();
            return _cache;
        }

        _cache = JsonSerializer.Deserialize<AppData>(json) ?? SeedDefaultData();
        return _cache;
    }

    public async Task SaveAsync()
    {
        if (_cache is null) return;
        var json = JsonSerializer.Serialize(_cache);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    // ---- Profiles ----

    public async Task<List<Profile>> GetProfilesAsync()
    {
        var data = await LoadAsync();
        return data.Profiles;
    }

    public async Task AddProfileAsync(Profile profile)
    {
        var data = await LoadAsync();
        data.Profiles.Add(profile);
        data.ProfileGameAccess[profile.Id] = new List<string>();
        await SaveAsync();
    }

    public async Task UpdateProfileAsync(Profile profile)
    {
        var data = await LoadAsync();
        var index = data.Profiles.FindIndex(p => p.Id == profile.Id);
        if (index >= 0) data.Profiles[index] = profile;
        await SaveAsync();
    }

    public async Task DeleteProfileAsync(string profileId)
    {
        var data = await LoadAsync();
        data.Profiles.RemoveAll(p => p.Id == profileId);
        data.ProfileGameAccess.Remove(profileId);
        data.DailyUsageSeconds.Remove(profileId);
        await SaveAsync();
    }

    // ---- Games ----

    public async Task<List<Game>> GetAllGamesAsync()
    {
        var data = await LoadAsync();
        return data.Games;
    }

    public async Task<List<Game>> GetGamesForProfileAsync(string profileId)
    {
        var data = await LoadAsync();

        var profile = data.Profiles.FirstOrDefault(p => p.Id == profileId);
        if (profile is null) return new List<Game>();

        // Admins see the full enabled catalog; kids see only their
        // assigned + catalog-enabled games.
        if (profile.Type == ProfileType.Admin)
            return data.Games.Where(g => g.IsCatalogEnabled).ToList();

        var allowedIds = data.ProfileGameAccess.TryGetValue(profileId, out var ids)
            ? ids
            : new List<string>();

        return data.Games
            .Where(g => g.IsCatalogEnabled && allowedIds.Contains(g.Id))
            .ToList();
    }

    public async Task AddGameAsync(Game game)
    {
        var data = await LoadAsync();
        data.Games.Add(game);
        await SaveAsync();
    }

    public async Task UpdateGameAsync(Game game)
    {
        var data = await LoadAsync();
        var index = data.Games.FindIndex(g => g.Id == game.Id);
        if (index >= 0) data.Games[index] = game;
        await SaveAsync();
    }

    public async Task DeleteGameAsync(string gameId)
    {
        var data = await LoadAsync();
        data.Games.RemoveAll(g => g.Id == gameId);
        foreach (var access in data.ProfileGameAccess.Values)
            access.Remove(gameId);
        await SaveAsync();
    }

    // ---- Access control ----

    public async Task<List<string>> GetAccessListAsync(string profileId)
    {
        var data = await LoadAsync();
        return data.ProfileGameAccess.TryGetValue(profileId, out var ids)
            ? ids
            : new List<string>();
    }

    public async Task SetGameAccessAsync(string profileId, string gameId, bool allowed)
    {
        var data = await LoadAsync();
        if (!data.ProfileGameAccess.TryGetValue(profileId, out var ids))
        {
            ids = new List<string>();
            data.ProfileGameAccess[profileId] = ids;
        }

        if (allowed && !ids.Contains(gameId))
            ids.Add(gameId);
        else if (!allowed)
            ids.Remove(gameId);

        await SaveAsync();
    }

    // ---- Play history ----

    public async Task AddPlayHistoryAsync(PlayHistoryEntry entry)
    {
        var data = await LoadAsync();
        data.PlayHistory.Add(entry);
        await SaveAsync();
    }

    public async Task<List<PlayHistoryEntry>> GetPlayHistoryAsync(string profileId)
    {
        var data = await LoadAsync();
        return data.PlayHistory.Where(h => h.ProfileId == profileId).ToList();
    }

    // ---- Play time tracking ----

    // Adds to today's usage bucket for a profile and returns the new
    // running total for today, so callers can check it against a limit
    // without a second round trip.
    public async Task<int> AddUsageSecondsAsync(string profileId, int seconds)
    {
        var data = await LoadAsync();
        var dateKey = TodayKey();

        if (!data.DailyUsageSeconds.TryGetValue(profileId, out var days))
        {
            days = new Dictionary<string, int>();
            data.DailyUsageSeconds[profileId] = days;
        }

        days.TryGetValue(dateKey, out var current);
        var updated = current + seconds;
        days[dateKey] = updated;

        await SaveAsync();
        return updated;
    }

    public async Task<int> GetTodayUsageSecondsAsync(string profileId)
    {
        var data = await LoadAsync();
        return data.DailyUsageSeconds.TryGetValue(profileId, out var days)
               && days.TryGetValue(TodayKey(), out var seconds)
            ? seconds
            : 0;
    }

    // Oldest first, one entry per day, zero-filled for days with no usage.
    public async Task<List<(DateTime Date, int Seconds)>> GetUsageHistoryAsync(string profileId, int dayCount)
    {
        var data = await LoadAsync();
        var byDate = data.DailyUsageSeconds.TryGetValue(profileId, out var days) ? days : new Dictionary<string, int>();

        var result = new List<(DateTime, int)>();
        for (var i = dayCount - 1; i >= 0; i--)
        {
            var date = DateTime.Now.Date.AddDays(-i);
            result.Add((date, byDate.TryGetValue(date.ToString("yyyy-MM-dd"), out var seconds) ? seconds : 0));
        }

        return result;
    }

    // Lets a parent grant bonus time today without waiting for the
    // midnight rollover or raising the permanent daily limit.
    public async Task ResetTodayUsageAsync(string profileId)
    {
        var data = await LoadAsync();
        if (data.DailyUsageSeconds.TryGetValue(profileId, out var days))
            days[TodayKey()] = 0;
        await SaveAsync();
    }

    private static string TodayKey() => DateTime.Now.ToString("yyyy-MM-dd");

    // ---- Seed data ----

    private static AppData SeedDefaultData()
    {
        var admin = new Profile
        {
            Name = "Parent",
            AvatarEmoji = "🔒",
            ColorHex = "#8892A6",
            Type = ProfileType.Admin
        };

        var kid = new Profile
        {
            Name = "Buddy",
            AvatarEmoji = "🦖",
            ColorHex = "#4ECDC4",
            Type = ProfileType.Kid
        };

        var memoryMatchDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.MemoryMatch);
        var memoryMatch = new Game
        {
            Title = memoryMatchDef.Title,
            ThumbnailImagePath = memoryMatchDef.ThumbnailImagePath,
            ThumbnailEmoji = memoryMatchDef.ThumbnailEmoji,
            LaunchTarget = memoryMatchDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute
        };

        var fishingDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.Fishing);
        var fishing = new Game
        {
            Title = fishingDef.Title,
            ThumbnailImagePath = fishingDef.ThumbnailImagePath,
            ThumbnailEmoji = fishingDef.ThumbnailEmoji,
            LaunchTarget = fishingDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute
        };

        var dressUpDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.DressUp);
        var dressUp = new Game
        {
            Title = dressUpDef.Title,
            ThumbnailImagePath = dressUpDef.ThumbnailImagePath,
            ThumbnailEmoji = dressUpDef.ThumbnailEmoji,
            LaunchTarget = dressUpDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute
        };

        var mannersGardenDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.MannersGarden);
        var mannersGarden = new Game
        {
            Title = mannersGardenDef.Title,
            ThumbnailImagePath = mannersGardenDef.ThumbnailImagePath,
            ThumbnailEmoji = mannersGardenDef.ThumbnailEmoji,
            LaunchTarget = mannersGardenDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute
        };

        // Externally-built (React/Vite), loaded via iframe rather than as a
        // routed component - see wwwroot/games/crown-and-banner. It's a
        // turn-based strategy game, a different audience than the other
        // built-ins here, so it's seeded into the catalog but not
        // auto-granted to the default kid profile; grant it per-kid from
        // the admin panel.
        var crownAndBanner = new Game
        {
            Title = "Crown & Banner",
            ThumbnailImagePath = "games/crown-and-banner/assets/units/griffin.png",
            ThumbnailEmoji = "👑",
            LaunchTarget = "games/crown-and-banner/index.html",
            LaunchMode = GameLaunchMode.ExternalIframe
        };

        var data = new AppData();
        data.Profiles.Add(admin);
        data.Profiles.Add(kid);
        data.Games.Add(memoryMatch);
        data.Games.Add(fishing);
        data.Games.Add(dressUp);
        data.Games.Add(mannersGarden);
        data.Games.Add(crownAndBanner);
        data.ProfileGameAccess[admin.Id] = new List<string>();
        data.ProfileGameAccess[kid.Id] = new List<string> { memoryMatch.Id, fishing.Id, dressUp.Id, mannersGarden.Id };

        return data;
    }
}
