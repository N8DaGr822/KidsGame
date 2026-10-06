using System.Reflection;
using System.Text.Json;
using KidsGameLauncher.Data;
using KidsGameLauncher.Models;
using KidsGameLauncher.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace KidsGameLauncher.Tests;

internal sealed class BrowserStorage : IJSRuntime, IJSObjectReference
{
    public Dictionary<string, string> Values { get; } = new();
    public bool FailReads { get; set; }
    public bool FailWrites { get; set; }
    public Task? ReadDelay { get; set; }
    public Task? WriteDelay { get; set; }
    public int Writes { get; private set; }
    public int Reads { get; private set; }

    public BrowserStorage()
    {
        Values["kgl_appdata"] = JsonSerializer.Serialize(new AppData
        {
            Profiles = [new() { Id = "parent", Name = "Parent", Type = ProfileType.Admin }, new() { Id = "kid", Name = "Buddy" }],
            Games = [new() { Id = "memory", Title = "Memory", LaunchTarget = BuiltInGames.MemoryMatch }, new() { Id = "chess", Title = "Chess", LaunchTarget = BuiltInGames.ChessGame }],
            ProfileGameAccess = new() { ["kid"] = ["memory"] }
        });
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, default, args);

    public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        object? value = null;
        if (identifier == "localStorage.getItem")
        {
            if (ReadDelay is not null) await ReadDelay;
            Reads++;
            if (FailReads) throw new JSException("Storage unavailable");
            value = Values.GetValueOrDefault((string)args![0]!);
        }
        else if (identifier == "localStorage.setItem")
        {
            if (WriteDelay is not null) await WriteDelay;
            if (FailWrites) throw new JSException("Quota exceeded");
            Writes++;
            Values[(string)args![0]!] = (string)args[1]!;
        }
        else if (identifier == "import") value = this;
        return value is null ? default! : (TValue)value;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

internal sealed class ManualClock : TimeProvider
{
    public DateTimeOffset Now { get; set; } = new(2026, 10, 6, 12, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Now;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    public void Advance(double seconds) => Now = Now.AddSeconds(seconds);
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) => new ManualTimer();
    private sealed class ManualTimer : ITimer
    {
        public bool Change(TimeSpan dueTime, TimeSpan period) => true;
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

internal sealed class TestNavigation : NavigationManager
{
    public TestNavigation() => Initialize("http://localhost/", "http://localhost/");
    public string? Destination { get; private set; }
    protected override void NavigateToCore(string uri, bool forceLoad) => Destination = uri;
    protected override void SetNavigationLockState(bool value) { }
}

internal sealed class ComponentRenderer : Renderer
{
    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
    public List<Exception> Errors { get; } = new();
    public ComponentRenderer(IServiceProvider services) : base(services, services.GetRequiredService<ILoggerFactory>()) { }
    protected override Task UpdateDisplayAsync(in RenderBatch batch) => Task.CompletedTask;
    protected override void HandleException(Exception exception) => Errors.Add(exception);
    public async Task<(T Component, int Id)> Mount<T>(Dictionary<string, object?>? parameters = null) where T : IComponent
    {
        T component = default!;
        int id = 0;
        await Dispatcher.InvokeAsync(async () =>
        {
            component = (T)InstantiateComponent(typeof(T));
            id = AssignRootComponentId(component);
            await RenderRootComponentAsync(id, ParameterView.FromDictionary(parameters ?? new()));
        });
        return (component, id);
    }
    public Task Render(int id, Dictionary<string, object?> parameters) => Dispatcher.InvokeAsync(() => RenderRootComponentAsync(id, ParameterView.FromDictionary(parameters)));
    public Task Call(object component, string method, params object?[] args) => Dispatcher.InvokeAsync(() => Invoke(component, method, args));
    public static Task Invoke(object component, string method, params object?[] args)
        => component.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.Invoke(component, args) as Task ?? Task.CompletedTask;
    public static T Field<T>(object component, string name)
        => (T)component.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(component)!;
}
