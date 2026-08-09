# Kids Game Launcher

A Blazor WebAssembly PWA game launcher for kids: a profile picker
(parent/admin + kid accounts), a per-kid game carousel, three built-in
games, and an admin panel for managing profiles and which games each kid
can access. Works fully offline — no backend, no server, no account —
everything is stored locally in the browser.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Run it locally

```bash
dotnet restore
dotnet run
```

Then open the URL it prints (typically `http://localhost:5000`).

On first launch it seeds two profiles — an Admin ("Parent") and a Kid
("Buddy") — with all three built-in games already unlocked for Buddy, so
there's something to click on right away.

## Install it on a tablet

1. Publish a release build:
   ```bash
   dotnet publish -c Release -o publish
   ```
2. Host the contents of `publish/wwwroot` somewhere reachable from the
   tablet — a static file host (GitHub Pages, Azure Static Web Apps,
   Netlify, or a local `python -m http.server` on your home network for
   testing) all work, since this is a pure static-file PWA.
3. On the tablet, open that URL in the browser and use **"Add to Home
   Screen"** (Chrome on Android, or Safari's Share menu on iPad). It
   installs with its own icon and launches full-screen, offline-capable.

All three games use touch-friendly tap targets and, where dragging is
involved (Dress Up), [Pointer Events](https://developer.mozilla.org/en-US/docs/Web/API/Pointer_events)
rather than the HTML5 drag-and-drop API, since HTML5 DnD doesn't work on
touch targets in Safari on iOS and is inconsistent elsewhere — Pointer
Events give one code path that works the same for mouse, touch, and pen.

## What's here

- **Profile picker** (`/`) — carousel of profile cards. Kid profiles go
  straight to their game screen; the Admin profile is PIN-gated (set a
  PIN from the admin panel — it's open by default until you set one).
- **Game select** (`/games`) — carousel of the active kid's allowed
  games, with a "See all" link once there are more than fit nicely.
- **Full game list** (`/games/all`) — grid of all of that kid's games.
- **Game host** (`/play/{id}`) — launches a game, either as a built-in
  component or via iframe for externally-hosted games.
- **Admin panel** (`/admin`) — add/edit/remove kid profiles, add games
  to the catalog (built-in games are one click via a picker; external
  web games can be added by URL), and toggle which games each kid sees.

### The three built-in games

- **Memory Match** — flip cards to find matching pairs. Choose Animals,
  ABC (uppercase↔lowercase), or Numbers, and a difficulty.
- **Fishing Catch** — a basket on the dock shows a target letter or
  number; catch 3 matching fish to advance to the next one, all the way
  through the alphabet or 1–20. Optional "3 Strikes" difficulty ends the
  round after 3 wrong catches.
- **Dress Up** — pick Girl (Princess/Witch/Unicorn) or Boy (Prince/
  Knight/Dragon), then drag sticker outfit pieces onto the character.
  Free play, no win state — tap "Done!" to log the session.

## Data storage

All data (profiles, game catalog, per-kid access lists, and play
history) lives in browser `localStorage` as a single JSON blob, via
`Services/AppDataService.cs`. This is intentional for v1: single
device, fully offline, no backend to run.

If you outgrow localStorage (multi-device sync, more complex data),
`AppDataService` is the only place that knows about storage — swap its
internals for a real database or API without touching any page.

## Adding a game

- **Built-in** — implement it as a Razor component (see
  `Components/MemoryMatchGame.razor`, `FishingGame.razor`, or
  `DressUpGame.razor` for the pattern: an `OnComplete` callback carrying
  a result record, wired up in `Pages/GameHost.razor`), register it in
  `Services/BuiltInGames.cs` with a unique `LaunchTarget` key, and add a
  branch in `GameHost.razor`'s `LaunchTarget` switch. It'll then show up
  in the admin "add game" picker automatically.
- **External (iframe)** — host the game elsewhere and add it from the
  admin panel with its full URL. `GameHost` loads it directly in an
  iframe, no code changes needed.

## Known gaps / next steps

- PWA icons in `wwwroot/icons/` are simple placeholders — swap in real
  art before shipping.
- No image/thumbnail upload — games and profiles use emoji as
  placeholder art for now.
- The admin PIN is a light deterrent (SHA-256 hash stored client-side),
  not real security — fine for a kid's tablet, not for anything that
  needs to resist a determined adult.
- Dress Up ships with 2 stickers per character; more can be added to
  each `CharacterOption` in `DressUpGame.razor` without touching the
  drag-and-drop logic.
