using Microsoft.JSInterop;

namespace KidsGameLauncher.Services;

// Owns the JS interop module for the whole app session instead of each
// component importing (and disposing) its own copy. Registered scoped,
// which in Blazor WASM means one instance for the app's entire lifetime -
// the module is never disposed mid-navigation, which is what actually
// removes the disposal race components used to hit when a fast navigation
// tore down a component while a sound/interop call was still in flight.
// The try/catch below is defense in depth for real app-unload, not the
// primary fix.
public sealed class InteropService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private Task<IJSObjectReference>? _moduleTask;

    public InteropService(IJSRuntime js) => _js = js;

    private Task<IJSObjectReference> ModuleAsync() =>
        _moduleTask ??= _js.InvokeAsync<IJSObjectReference>("import", "./js/interop.js").AsTask();

    public async Task InvokeVoidAsync(string identifier, params object?[] args)
    {
        try
        {
            var module = await ModuleAsync();
            await module.InvokeVoidAsync(identifier, args);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    // default! on disconnect is intentional: the component that issued the
    // call is being torn down, so nothing observes the result anyway.
    public async Task<T> InvokeAsync<T>(string identifier, params object?[] args)
    {
        try
        {
            var module = await ModuleAsync();
            return await module.InvokeAsync<T>(identifier, args);
        }
        catch (JSDisconnectedException) { return default!; }
        catch (ObjectDisposedException) { return default!; }
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask is not null)
        {
            try { await (await _moduleTask).DisposeAsync(); }
            catch (JSDisconnectedException) { }
        }
    }
}
