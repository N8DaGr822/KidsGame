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
            ThumbnailEmoji = memoryMatchDef.ThumbnailEmoji,
            LaunchTarget = memoryMatchDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute
        };

        var fishingDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.Fishing);
        var fishing = new Game
        {
            Title = fishingDef.Title,
            ThumbnailEmoji = fishingDef.ThumbnailEmoji,
            LaunchTarget = fishingDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute
        };

        var dressUpDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.DressUp);
        var dressUp = new Game
        {
            Title = dressUpDef.Title,
            ThumbnailEmoji = dressUpDef.ThumbnailEmoji,
            LaunchTarget = dressUpDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute
        };

        var mannersGardenDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.MannersGarden);
        var mannersGarden = new Game
        {
            Title = mannersGardenDef.Title,
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
