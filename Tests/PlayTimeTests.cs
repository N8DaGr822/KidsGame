using KidsGameLauncher.Models;
using KidsGameLauncher.Services;
using Xunit;

namespace KidsGameLauncher.Tests;

public class PlayTimeTests
{
    [Fact]
    public async Task SlowTickCannotApplyPreviousChildsLockoutToNextChild()
    {
        var browser = new BrowserStorage();
        var data = new AppDataService(browser);
        var state = new AppState();
        var clock = new ManualClock();
        var kid = (await data.GetProfilesAsync()).Single(p => p.Id == "kid");
        kid.DailyTimeLimitMinutes = 1;
        var sibling = new Profile { Id = "sibling", Name = "Sibling", DailyTimeLimitMinutes = 5 };
        await data.AddProfileAsync(sibling);
        await data.AddUsageSecondsAsync(kid.Id, 45, clock.Now.Date);
        await using var tracker = new PlayTimeTracker(state, data, clock);
        state.SetCurrentProfile(kid);
        await tracker.FlushAsync();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        browser.WriteDelay = gate.Task;
        clock.Advance(20);
        var slowTick = tracker.FlushAsync();
        state.SetCurrentProfile(sibling);
        clock.Advance(5);
        var switched = tracker.FlushAsync();
        gate.SetResult();
        await Task.WhenAll(slowTick, switched);
        Assert.False(tracker.LimitReached);
        Assert.False(tracker.CheckingLimit);
        var saved = await data.LoadAsync();
        Assert.Equal(60, Usage(saved, "2026-10-06"));
        Assert.Equal(5, saved.DailyUsageSeconds["sibling"]["2026-10-06"]);
    }

    [Fact]
    public async Task ShortVisitsAreCountedAcrossProfileSwitches()
    {
        var data = new AppDataService(new BrowserStorage());
        var state = new AppState();
        var clock = new ManualClock();
        await using var tracker = new PlayTimeTracker(state, data, clock);
        var kid = (await data.GetProfilesAsync()).Single(p => p.Id == "kid");
        for (var i = 0; i < 3; i++)
        {
            state.SetCurrentProfile(kid);
            clock.Advance(4);
            state.ClearCurrentProfile();
        }
        await tracker.FlushAsync();
        Assert.Equal(12, Usage(await data.LoadAsync(), "2026-10-06"));
    }

    [Fact]
    public async Task SubsecondVisitsAccumulateInsteadOfRoundingEachVisit()
    {
        var data = new AppDataService(new BrowserStorage());
        var state = new AppState();
        var clock = new ManualClock();
        await using var tracker = new PlayTimeTracker(state, data, clock);
        var kid = (await data.GetProfilesAsync()).Single(p => p.Id == "kid");
        for (var i = 0; i < 3; i++)
        {
            state.SetCurrentProfile(kid);
            clock.Advance(0.4);
            state.ClearCurrentProfile();
        }
        await tracker.FlushAsync();
        Assert.Equal(1, Usage(await data.LoadAsync(), "2026-10-06"));
    }

    [Fact]
    public async Task ReturningToAnExhaustedProfileDoesNotGrantAnotherInterval()
    {
        var data = new AppDataService(new BrowserStorage());
        var state = new AppState();
        var clock = new ManualClock();
        var kid = (await data.GetProfilesAsync()).Single(p => p.Id == "kid");
        kid.DailyTimeLimitMinutes = 1;
        await data.AddUsageSecondsAsync("kid", 60, clock.Now.Date);
        await using var tracker = new PlayTimeTracker(state, data, clock);
        state.SetCurrentProfile(kid);
        await tracker.FlushAsync();
        Assert.True(tracker.LimitReached);
        state.ClearCurrentProfile();
        state.SetCurrentProfile(kid);
        await tracker.FlushAsync();
        Assert.True(tracker.LimitReached);
        Assert.False(tracker.CheckingLimit);
    }

    [Fact]
    public async Task DelayedTickCountsActualElapsedTimeAndSplitsMidnight()
    {
        var data = new AppDataService(new BrowserStorage());
        var state = new AppState();
        var clock = new ManualClock { Now = new(2026, 10, 6, 23, 59, 50, TimeSpan.Zero) };
        await using var tracker = new PlayTimeTracker(state, data, clock);
        state.SetCurrentProfile((await data.GetProfilesAsync()).Single(p => p.Id == "kid"));
        clock.Advance(70);
        await tracker.FlushAsync();
        var saved = await data.LoadAsync();
        Assert.Equal(10, Usage(saved, "2026-10-06"));
        Assert.Equal(60, Usage(saved, "2026-10-07"));
    }

    [Fact]
    public async Task DailyLockoutClearsAfterMidnightAndDoesNotAffectParent()
    {
        var data = new AppDataService(new BrowserStorage());
        var state = new AppState();
        var clock = new ManualClock { Now = new(2026, 10, 6, 23, 59, 59, TimeSpan.Zero) };
        var profiles = await data.GetProfilesAsync();
        var kid = profiles.Single(p => p.Id == "kid");
        kid.DailyTimeLimitMinutes = 1;
        await data.AddUsageSecondsAsync("kid", 60, clock.Now.Date);
        await using var tracker = new PlayTimeTracker(state, data, clock);
        state.SetCurrentProfile(kid);
        await tracker.FlushAsync();
        Assert.True(tracker.LimitReached);
        clock.Advance(2);
        await tracker.FlushAsync();
        Assert.False(tracker.LimitReached);
        state.SetCurrentProfile(profiles.Single(p => p.Type == ProfileType.Admin));
        await tracker.FlushAsync();
        Assert.False(tracker.LimitReached);
    }

    private static int Usage(Data.AppData data, string date) => data.DailyUsageSeconds["kid"][date];
}
