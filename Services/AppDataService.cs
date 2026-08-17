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
            EnsureBuiltInCatalogEntries(_cache);
            EnsureExternalCatalogEntries(_cache);
            await SaveAsync();
            return _cache;
        }

        _cache = JsonSerializer.Deserialize<AppData>(json) ?? SeedDefaultData();
        var changed = ApplyBuiltInCatalogArt(_cache);
        changed |= RemoveStaleGameEntries(_cache);
        changed |= EnsureBuiltInCatalogEntries(_cache);
        changed |= EnsureExternalCatalogEntries(_cache);
        if (changed)
        {
            await SaveAsync();
        }

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
        data.GardenItems.Remove(profileId);
        data.GameDifficultyOverrides.Remove(profileId);
        data.GameBests.Remove(profileId);
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

    // Same as SetGameAccessAsync but for many games in one localStorage
    // round trip - used by "Select all" / "Clear all" on a filtered list,
    // so granting a search result group doesn't take one write per game.
    public async Task SetGameAccessBulkAsync(string profileId, IReadOnlyCollection<string> gameIds, bool allowed)
    {
        var data = await LoadAsync();
        if (!data.ProfileGameAccess.TryGetValue(profileId, out var ids))
        {
            ids = new List<string>();
            data.ProfileGameAccess[profileId] = ids;
        }

        foreach (var gameId in gameIds)
        {
            if (allowed && !ids.Contains(gameId))
                ids.Add(gameId);
            else if (!allowed)
                ids.Remove(gameId);
        }

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

    // Adds a catalog entry for every built-in game that isn't in the
    // catalog yet (matched by LaunchTarget), so a parent never has to
    // manually "Add game to catalog" just to make a shipped game show up
    // as an option - new entries aren't granted to any profile, they just
    // become visible to check on/off. Runs on every load (not just first
    // install) so a game added to BuiltInGames.All after a parent's data
    // already existed still shows up automatically next time they open
    // the catalog, instead of only benefiting fresh installs.
    private static bool EnsureBuiltInCatalogEntries(AppData data)
    {
        var existingTargets = data.Games.Select(g => g.LaunchTarget).ToHashSet();
        var changed = false;

        foreach (var builtIn in BuiltInGames.All)
        {
            if (existingTargets.Contains(builtIn.LaunchTarget)) continue;

            data.Games.Add(new Game
            {
                Title = builtIn.Title,
                ThumbnailImagePath = builtIn.ThumbnailImagePath,
                ThumbnailEmoji = builtIn.ThumbnailEmoji,
                LaunchTarget = builtIn.LaunchTarget,
                LaunchMode = GameLaunchMode.InternalRoute,
                MinAge = builtIn.MinAge,
                MaxAge = builtIn.MaxAge
            });
            changed = true;
        }

        return changed;
    }

    // One-time cleanup: "Inland Hauler" was renamed to "Highway Hauler"
    // (its ITP branding removed) shortly after it was first added, moving
    // wwwroot/games/inland-hauler -> highway-hauler. Anyone who loaded the
    // app in that narrow window would have synced a catalog entry
    // pointing at the now-deleted old path - drop it here so
    // EnsureExternalCatalogEntries's rename below doesn't just add the
    // new one alongside a permanently dead duplicate.
    private static bool RemoveStaleGameEntries(AppData data) =>
        data.Games.RemoveAll(g => g.LaunchTarget == "games/inland-hauler/index.html") > 0;

    private record ExternalGameDef(string Title, string ThumbnailImagePath, string ThumbnailEmoji, string LaunchTarget, int? MinAge, int? MaxAge);

    // Iframe-hosted games (self-contained HTML, not routed Razor
    // components) - not in BuiltInGames.All, so they don't benefit from
    // EnsureBuiltInCatalogEntries above. Small enough to list directly
    // here rather than a separate file. None are auto-granted to any
    // profile - same reasoning as Crown & Banner in SeedDefaultData: a
    // parent decides per-kid via the admin panel.
    private static readonly ExternalGameDef[] ExternalGames =
    {
        new("Crown & Banner", "games/crown-and-banner/assets/units/griffin.png", "👑", "games/crown-and-banner/index.html", 7, 13),
        new("Highway Hauler", "", "🚚", "games/highway-hauler/index.html", 8, 14),
        new("Truck Repair Bay", "", "🔧", "games/truck-repair-bay/index.html", 7, 13),
    };

    // Same reasoning as EnsureBuiltInCatalogEntries, for the iframe-hosted
    // games instead of routed components - runs on every load so a game
    // added here after a parent's data already existed still shows up
    // automatically. Checks by LaunchTarget, so this is a no-op for
    // Crown & Banner on a fresh install (SeedDefaultData already added it)
    // and only backfills it for pre-existing installs.
    private static bool EnsureExternalCatalogEntries(AppData data)
    {
        var existingTargets = data.Games.Select(g => g.LaunchTarget).ToHashSet();
        var changed = false;

        foreach (var ext in ExternalGames)
        {
            if (existingTargets.Contains(ext.LaunchTarget)) continue;

            data.Games.Add(new Game
            {
                Title = ext.Title,
                ThumbnailImagePath = ext.ThumbnailImagePath,
                ThumbnailEmoji = ext.ThumbnailEmoji,
                LaunchTarget = ext.LaunchTarget,
                LaunchMode = GameLaunchMode.ExternalIframe,
                MinAge = ext.MinAge,
                MaxAge = ext.MaxAge
            });
            changed = true;
        }

        return changed;
    }

    private static bool ApplyBuiltInCatalogArt(AppData data)
    {
        var changed = false;
        var builtInsByTarget = BuiltInGames.All.ToDictionary(g => g.LaunchTarget);

        foreach (var game in data.Games)
        {
            if (!builtInsByTarget.TryGetValue(game.LaunchTarget, out var builtIn)) continue;
            if (string.IsNullOrWhiteSpace(builtIn.ThumbnailImagePath)) continue;
            if (!string.IsNullOrWhiteSpace(game.ThumbnailImagePath)) continue;

            game.ThumbnailImagePath = builtIn.ThumbnailImagePath;
            changed = true;
        }

        return changed;
    }

    // ---- Manners Garden (persists across sessions - it's meant to keep
    // growing, not reset every time the game is played) --------------------

    public async Task<List<string>> GetGardenItemsAsync(string profileId)
    {
        var data = await LoadAsync();
        return data.GardenItems.TryGetValue(profileId, out var items) ? items : new List<string>();
    }

    public async Task AddGardenItemAsync(string profileId, string item)
    {
        var data = await LoadAsync();
        if (!data.GardenItems.TryGetValue(profileId, out var items))
        {
            items = new List<string>();
            data.GardenItems[profileId] = items;
        }

        items.Add(item);
        await SaveAsync();
    }

    // ---- Personal bests (per profile, per game, per metric - each game
    // picks its own metricKey strings and whether lower or higher is
    // "better" for that metric; this service just stores/compares
    // whatever it's given) --------------------------------------------------

    public async Task<double?> GetBestAsync(string profileId, string gameId, string metricKey)
    {
        var data = await LoadAsync();
        if (data.GameBests.TryGetValue(profileId, out var byGame) &&
            byGame.TryGetValue(gameId, out var byMetric) &&
            byMetric.TryGetValue(metricKey, out var value))
        {
            return value;
        }
        return null;
    }

    // Persists `value` as the new best and returns true if it beats (or
    // there was no prior) recorded best for this profile/game/metric;
    // returns false and leaves the stored value untouched otherwise.
    public async Task<bool> TryRecordBestAsync(string profileId, string gameId, string metricKey, double value, bool lowerIsBetter)
    {
        var data = await LoadAsync();

        if (!data.GameBests.TryGetValue(profileId, out var byGame))
        {
            byGame = new();
            data.GameBests[profileId] = byGame;
        }
        if (!byGame.TryGetValue(gameId, out var byMetric))
        {
            byMetric = new();
            byGame[gameId] = byMetric;
        }

        var isNewBest = !byMetric.TryGetValue(metricKey, out var existing) ||
            (lowerIsBetter ? value < existing : value > existing);

        if (isNewBest)
        {
            byMetric[metricKey] = value;
            await SaveAsync();
        }

        return isNewBest;
    }

    // ---- Parent-locked game difficulty (Easy/Medium/Hard, per kid per
    // game) - lets a parent decide for games where a young kid picking
    // their own difficulty could set it too hard for themselves. ----------

    public async Task<string?> GetDifficultyOverrideAsync(string profileId, string gameId)
    {
        var data = await LoadAsync();
        return data.GameDifficultyOverrides.TryGetValue(profileId, out var byGame) && byGame.TryGetValue(gameId, out var value)
            ? value
            : null;
    }

    public async Task SetDifficultyOverrideAsync(string profileId, string gameId, string? difficulty)
    {
        var data = await LoadAsync();

        if (string.IsNullOrEmpty(difficulty))
        {
            if (data.GameDifficultyOverrides.TryGetValue(profileId, out var existing))
            {
                existing.Remove(gameId);
            }
        }
        else
        {
            if (!data.GameDifficultyOverrides.TryGetValue(profileId, out var byGame))
            {
                byGame = new Dictionary<string, string>();
                data.GameDifficultyOverrides[profileId] = byGame;
            }

            byGame[gameId] = difficulty;
        }

        await SaveAsync();
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
            ThumbnailImagePath = memoryMatchDef.ThumbnailImagePath,
            ThumbnailEmoji = memoryMatchDef.ThumbnailEmoji,
            LaunchTarget = memoryMatchDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = memoryMatchDef.MinAge,
            MaxAge = memoryMatchDef.MaxAge
        };

        var fishingDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.Fishing);
        var fishing = new Game
        {
            Title = fishingDef.Title,
            ThumbnailImagePath = fishingDef.ThumbnailImagePath,
            ThumbnailEmoji = fishingDef.ThumbnailEmoji,
            LaunchTarget = fishingDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = fishingDef.MinAge,
            MaxAge = fishingDef.MaxAge
        };

        var dressUpDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.DressUp);
        var dressUp = new Game
        {
            Title = dressUpDef.Title,
            ThumbnailImagePath = dressUpDef.ThumbnailImagePath,
            ThumbnailEmoji = dressUpDef.ThumbnailEmoji,
            LaunchTarget = dressUpDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = dressUpDef.MinAge,
            MaxAge = dressUpDef.MaxAge
        };

        var mannersGardenDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.MannersGarden);
        var mannersGarden = new Game
        {
            Title = mannersGardenDef.Title,
            ThumbnailImagePath = mannersGardenDef.ThumbnailImagePath,
            ThumbnailEmoji = mannersGardenDef.ThumbnailEmoji,
            LaunchTarget = mannersGardenDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = mannersGardenDef.MinAge,
            MaxAge = mannersGardenDef.MaxAge
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
            LaunchMode = GameLaunchMode.ExternalIframe,
            MinAge = 7,
            MaxAge = 13
        };

        // Turn-based artillery duel with tanks exploding after 5 hits -
        // same reasoning as Crown & Banner: seeded into the catalog, but
        // not auto-granted, since it's a different tone than the other
        // built-ins.
        var tankDuelDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.TankDuel);
        var tankDuel = new Game
        {
            Title = tankDuelDef.Title,
            ThumbnailImagePath = tankDuelDef.ThumbnailImagePath,
            ThumbnailEmoji = tankDuelDef.ThumbnailEmoji,
            LaunchTarget = tankDuelDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = tankDuelDef.MinAge,
            MaxAge = tankDuelDef.MaxAge
        };

        // Card and reflex-memory games - seeded into the catalog like Tank
        // Duel and Crown & Banner above, not auto-granted, so a parent
        // decides per-kid whether they're ready via the admin panel.
        var unoDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.Uno);
        var uno = new Game
        {
            Title = unoDef.Title,
            ThumbnailImagePath = unoDef.ThumbnailImagePath,
            ThumbnailEmoji = unoDef.ThumbnailEmoji,
            LaunchTarget = unoDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = unoDef.MinAge,
            MaxAge = unoDef.MaxAge
        };

        var simonSaysDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.SimonSays);
        var simonSays = new Game
        {
            Title = simonSaysDef.Title,
            ThumbnailImagePath = simonSaysDef.ThumbnailImagePath,
            ThumbnailEmoji = simonSaysDef.ThumbnailEmoji,
            LaunchTarget = simonSaysDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = simonSaysDef.MinAge,
            MaxAge = simonSaysDef.MaxAge
        };

        var slidingPuzzleDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.SlidingPuzzle);
        var slidingPuzzle = new Game
        {
            Title = slidingPuzzleDef.Title,
            ThumbnailImagePath = slidingPuzzleDef.ThumbnailImagePath,
            ThumbnailEmoji = slidingPuzzleDef.ThumbnailEmoji,
            LaunchTarget = slidingPuzzleDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = slidingPuzzleDef.MinAge,
            MaxAge = slidingPuzzleDef.MaxAge
        };

        var wordScrambleDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.WordScramble);
        var wordScramble = new Game
        {
            Title = wordScrambleDef.Title,
            ThumbnailImagePath = wordScrambleDef.ThumbnailImagePath,
            ThumbnailEmoji = wordScrambleDef.ThumbnailEmoji,
            LaunchTarget = wordScrambleDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = wordScrambleDef.MinAge,
            MaxAge = wordScrambleDef.MaxAge
        };

        var minesweeperDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.Minesweeper);
        var minesweeper = new Game
        {
            Title = minesweeperDef.Title,
            ThumbnailImagePath = minesweeperDef.ThumbnailImagePath,
            ThumbnailEmoji = minesweeperDef.ThumbnailEmoji,
            LaunchTarget = minesweeperDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = minesweeperDef.MinAge,
            MaxAge = minesweeperDef.MaxAge
        };

        var sudokuDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.Sudoku);
        var sudoku = new Game
        {
            Title = sudokuDef.Title,
            ThumbnailImagePath = sudokuDef.ThumbnailImagePath,
            ThumbnailEmoji = sudokuDef.ThumbnailEmoji,
            LaunchTarget = sudokuDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = sudokuDef.MinAge,
            MaxAge = sudokuDef.MaxAge
        };

        var whackAMoleDef = BuiltInGames.All.First(g => g.LaunchTarget == BuiltInGames.WhackAMole);
        var whackAMole = new Game
        {
            Title = whackAMoleDef.Title,
            ThumbnailImagePath = whackAMoleDef.ThumbnailImagePath,
            ThumbnailEmoji = whackAMoleDef.ThumbnailEmoji,
            LaunchTarget = whackAMoleDef.LaunchTarget,
            LaunchMode = GameLaunchMode.InternalRoute,
            MinAge = whackAMoleDef.MinAge,
            MaxAge = whackAMoleDef.MaxAge
        };

        var data = new AppData();
        data.Profiles.Add(admin);
        data.Profiles.Add(kid);
        data.Games.Add(memoryMatch);
        data.Games.Add(fishing);
        data.Games.Add(dressUp);
        data.Games.Add(mannersGarden);
        data.Games.Add(crownAndBanner);
        data.Games.Add(tankDuel);
        data.Games.Add(uno);
        data.Games.Add(simonSays);
        data.Games.Add(slidingPuzzle);
        data.Games.Add(wordScramble);
        data.Games.Add(minesweeper);
        data.Games.Add(sudoku);
        data.Games.Add(whackAMole);
        data.ProfileGameAccess[admin.Id] = new List<string>();
        data.ProfileGameAccess[kid.Id] = new List<string> { memoryMatch.Id, fishing.Id, dressUp.Id, mannersGarden.Id };

        return data;
    }
}
