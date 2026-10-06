using KidsGameLauncher.Models;
using KidsGameLauncher.Services;
using Xunit;

namespace KidsGameLauncher.Tests;

public class PlaySessionTests
{
    [Fact]
    public async Task CheckpointsAndRepeatedExitProduceOneAbandonedEntry()
    {
        var data = new AppDataService(new BrowserStorage());
        var clock = new ManualClock();
        var session = new PlaySession(data, "kid", "memory", false, clock);
        clock.Advance(15);
        await session.CheckpointAsync();
        Assert.Equal(PlaySessionStatus.InProgress, Assert.Single(await data.GetPlayHistoryAsync("kid")).Status);
        clock.Advance(3);
        await Task.WhenAll(session.EndAsync(), session.EndAsync(), session.CheckpointAsync());
        var entry = Assert.Single(await data.GetPlayHistoryAsync("kid"));
        Assert.Equal(18, entry.ElapsedSeconds);
        Assert.Equal(PlaySessionStatus.Abandoned, entry.Status);
    }

    [Fact]
    public async Task ResultReplacesCheckpointWithoutAnExtraExitEntry()
    {
        var data = new AppDataService(new BrowserStorage());
        var clock = new ManualClock();
        var session = new PlaySession(data, "kid", "memory", false, clock);
        clock.Advance(15);
        await session.CheckpointAsync();
        await session.RecordResultAsync(new() { ProfileId = "wrong", GameId = "wrong", Moves = 8, ElapsedSeconds = 14 });
        await session.EndAsync();
        var entry = Assert.Single(await data.GetPlayHistoryAsync("kid"));
        Assert.Equal("memory", entry.GameId);
        Assert.Equal(8, entry.Moves);
        Assert.Equal(PlaySessionStatus.ResultRecorded, entry.Status);
    }

    [Fact]
    public async Task SubsequentReportedRoundsAreRetainedButLateCallbacksAreIgnored()
    {
        var data = new AppDataService(new BrowserStorage());
        var session = new PlaySession(data, "kid", "memory", false);
        await session.RecordResultAsync(new() { Moves = 8 });
        await session.RecordResultAsync(new() { Moves = 6 });
        await session.EndAsync();
        await session.RecordResultAsync(new() { Moves = 99 });
        Assert.Equal(new[] { 8, 6 }, (await data.GetPlayHistoryAsync("kid")).Select(h => h.Moves));
    }

    [Fact]
    public async Task EmbeddedGameExitIsRecordedAsEnded()
    {
        var data = new AppDataService(new BrowserStorage());
        var clock = new ManualClock();
        var session = new PlaySession(data, "kid", "external", true, clock);
        clock.Advance(3);
        await session.EndAsync();
        Assert.Equal(PlaySessionStatus.Ended, Assert.Single(await data.GetPlayHistoryAsync("kid")).Status);
    }
}
