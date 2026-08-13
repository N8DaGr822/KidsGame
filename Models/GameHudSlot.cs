using Microsoft.AspNetCore.Components;

namespace KidsGameLauncher.Models;

// Lets a hosted game (rendered inside GameHost's surface) contribute extra
// controls into GameHost's own title bar, instead of drawing a second bar
// of its own. GameHost owns the slot and re-renders when it changes; the
// game component owns the RenderFragment's event handlers (Razor captures
// "this" as the declaring component at compile time), so clicks inside the
// hoisted content still update the game's own state correctly.
public sealed class GameHudSlot
{
    public RenderFragment? Content { get; private set; }
    public event Action? Changed;

    public void Set(RenderFragment? content)
    {
        Content = content;
        Changed?.Invoke();
    }
}
