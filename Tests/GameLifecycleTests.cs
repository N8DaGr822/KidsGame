using System.Collections;
using KidsGameLauncher.Components;
using KidsGameLauncher.Pages;
using KidsGameLauncher.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Xunit;

namespace KidsGameLauncher.Tests;

public class GameLifecycleTests : IAsyncLifetime
{
    private readonly ServiceProvider services;
    private readonly ComponentRenderer renderer;
    public GameLifecycleTests()
    {
        services = new ServiceCollection().AddLogging()
            .AddSingleton<IJSRuntime>(new BrowserStorage()).AddSingleton<AppDataService>()
            .AddSingleton<AppState>().AddSingleton<InteropService>()
            .AddSingleton<NavigationManager, TestNavigation>().BuildServiceProvider();
        renderer = new ComponentRenderer(services);
    }
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync()
    {
        await renderer.DisposeAsync();
        await services.DisposeAsync();
        Assert.Empty(renderer.Errors);
    }

    [Fact]
    public async Task MemoryMismatchCannotTouchRestartedDeck()
    {
        var (game, _) = await renderer.Mount<MemoryMatchGame>();
        await renderer.Call(game, "StartGame");
        var cards = ComponentRenderer.Field<IList>(game, "cards");
        var first = cards[0]!;
        var key = first.GetType().GetProperty("MatchKey")!;
        var other = cards.Cast<object>().First(c => !Equals(key.GetValue(c), key.GetValue(first)));
        await renderer.Call(game, "FlipCard", first);
        Task pending = Task.CompletedTask;
        await renderer.Dispatcher.InvokeAsync(() => { pending = ComponentRenderer.Invoke(game, "FlipCard", other); });
        await renderer.Call(game, "ResetToSetup");
        await renderer.Call(game, "StartGame");
        await pending;
        Assert.Null(ComponentRenderer.Field<object?>(game, "firstPick"));
        Assert.False(ComponentRenderer.Field<bool>(game, "isResolvingMismatch"));
        Assert.Equal(0, ComponentRenderer.Field<int>(game, "moves"));
    }

    [Fact]
    public async Task AnimalRoundCannotAdvanceAfterRestart()
    {
        var (game, _) = await renderer.Mount<AnimalSoundGuessingGame>();
        await renderer.Call(game, "StartGame");
        var choices = ComponentRenderer.Field<IList>(game, "currentChoices");
        Task pending = Task.CompletedTask;
        await renderer.Dispatcher.InvokeAsync(() => { pending = ComponentRenderer.Invoke(game, "OnGuess", choices[0]); });
        await renderer.Call(game, "ExitToSetup");
        await renderer.Call(game, "StartGame");
        await pending;
        Assert.Equal(0, ComponentRenderer.Field<int>(game, "roundIndex"));
        Assert.Equal("Guessing", ComponentRenderer.Field<object>(game, "roundState").ToString());
    }

    [Fact]
    public async Task ChessCpuCannotMoveOnRestartedBoard()
    {
        var (game, _) = await renderer.Mount<ChessGame>();
        await renderer.Call(game, "StartGame");
        var original = (int[,])ComponentRenderer.Field<int[,]>(game, "board").Clone();
        Task pending = Task.CompletedTask;
        await renderer.Dispatcher.InvokeAsync(() => { pending = ComponentRenderer.Invoke(game, "ApplyPlayerMoveAsync", 6, 4, 4, 4); });
        await renderer.Call(game, "ExitToSetup");
        await renderer.Call(game, "StartGame");
        await pending;
        Assert.Equal(original.Cast<int>(), ComponentRenderer.Field<int[,]>(game, "board").Cast<int>());
        Assert.Equal("Player", ComponentRenderer.Field<object>(game, "turn").ToString());
    }

    [Fact]
    public async Task AutoBattleCannotContinueAfterExit()
    {
        var (game, _) = await renderer.Mount<AutoBattler>();
        await renderer.Call(game, "EnterDraft");
        var roster = (IEnumerable)typeof(AutoBattler).GetField("Roster", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.GetValue(null)!;
        foreach (var unit in roster.Cast<object>().Take(3))
            await renderer.Call(game, "ToggleDraft", unit.GetType().GetProperty("Id")!.GetValue(unit));
        Task pending = Task.CompletedTask;
        await renderer.Dispatcher.InvokeAsync(() => { pending = ComponentRenderer.Invoke(game, "StartBattle"); });
        await renderer.Call(game, "ExitToSetup");
        await renderer.Call(game, "EnterDraft");
        await pending;
        Assert.Equal("Draft", ComponentRenderer.Field<object>(game, "phase").ToString());
        Assert.Equal(0, ComponentRenderer.Field<int>(game, "roundsTaken"));
    }

    [Fact]
    public async Task ParentCanOpenEnabledGamesWithoutAnAssignment()
    {
        var data = services.GetRequiredService<AppDataService>();
        services.GetRequiredService<AppState>().SetCurrentProfile((await data.GetProfilesAsync()).Single(p => p.Id == "parent"));
        var (host, _) = await renderer.Mount<GameHost>(new() { ["GameId"] = "chess" });
        Assert.Equal("chess", ComponentRenderer.Field<Models.Game>(host, "_game").Id);
    }

    [Fact]
    public async Task GameHostRejectsUnassignedAndDisabledRoutesAndReloadsParameters()
    {
        var data = services.GetRequiredService<AppDataService>();
        var state = services.GetRequiredService<AppState>();
        state.SetCurrentProfile((await data.GetProfilesAsync()).Single(p => p.Id == "kid"));
        var (host, id) = await renderer.Mount<GameHost>(new() { ["GameId"] = "chess" });
        Assert.Equal("games", ((TestNavigation)services.GetRequiredService<NavigationManager>()).Destination);
        Assert.Null(ComponentRenderer.Field<object?>(host, "_game"));
        await renderer.Render(id, new() { ["GameId"] = "memory" });
        Assert.Equal("memory", ComponentRenderer.Field<Models.Game>(host, "_game").Id);
        var memory = (await data.GetAllGamesAsync()).Single(g => g.Id == "memory");
        memory.IsCatalogEnabled = false;
        await renderer.Render(id, new() { ["GameId"] = "memory" });
        Assert.Null(ComponentRenderer.Field<object?>(host, "_game"));
    }
}
