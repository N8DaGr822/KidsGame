# Kids Game Launcher

A Blazor WebAssembly PWA game launcher for kids: a profile picker
(parent/admin + kid accounts), a per-kid game carousel, five built-in
games, and an admin panel for managing profiles, which games each kid
can access, and daily screen time. Works fully offline — no backend, no
server, no account — everything is stored locally in the browser.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Run it locally

```bash
dotnet restore
dotnet run
```

Then open the URL it prints (typically `http://localhost:5000`).

On first launch it seeds two profiles — an Admin ("Parent") and a Kid
("Buddy") — with four of the five built-in games already unlocked for
Buddy (Tank Duel isn't auto-granted, see below), so there's something to
click on right away.

## Install it on a tablet

Every push to `main` auto-deploys to GitHub Pages via
[.github/workflows/deploy.yml](.github/workflows/deploy.yml) (enable it
once under repo Settings → Pages → Source → "GitHub Actions"). The site
ends up at `https://<user>.github.io/KidsGame/`.

On the tablet, open that URL in the browser and use **"Add to Home
Screen"** (Chrome on Android, or Safari's Share menu — must be Safari —
on iPad). It installs with its own icon and launches full-screen,
offline-capable. Launch it once while online so the service worker can
cache everything before going offline.

Service workers (and therefore offline support) only register over
HTTPS, which is why this needs a real static host rather than
`python -m http.server` on the home network — that works for a quick
look but won't install as a true offline app.

To publish and host it somewhere else instead (Netlify, Cloudflare
Pages, Azure Static Web Apps, etc. all work — this is a pure static-file
PWA):
```bash
dotnet publish -c Release -o publish
```
Host the contents of `publish/wwwroot`. Note that `<base href="/" />`
in `wwwroot/index.html` and the app's internal navigation assume the
site is served from the host's root; the GitHub Actions workflow patches
`<base href>` to `/KidsGame/` at publish time to account for GitHub
Pages' project-subpath URLs; a host that serves from its own root
domain (Netlify, Cloudflare Pages) needs no such patch.

All the built-in games use touch-friendly tap targets and, where dragging
is involved (Dress Up, Manners Garden, Tank Duel's aiming), [Pointer Events](https://developer.mozilla.org/en-US/docs/Web/API/Pointer_events)
rather than the HTML5 drag-and-drop API, since HTML5 DnD doesn't work on
touch targets in Safari on iOS and is inconsistent elsewhere — Pointer
Events give one code path that works the same for mouse, touch, and pen.

Sound effects (match/mismatch chimes, catch/splash, tank fire/impact/
explosion) are procedural — a handful of Web Audio oscillator and noise
nodes in `wwwroot/js/interop.js`, not audio files. They only ever play in
direct response to a user-caused action (a tap, a shot the player fired),
which is exactly what's needed to unlock `AudioContext` playback in every
browser, so there's no separate "enable sound" step.

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
  web games can be added by URL — Crown & Banner, under
  `wwwroot/games/`, is a worked example), toggle which games each kid
  sees, and (`/admin/history/{id}`) view a kid's play history and set or
  reset their daily time limit.

### The five built-in games

- **Memory Match** — flip cards to find matching pairs. Choose Animals,
  ABC (uppercase↔lowercase), Numbers, or **Math** (an equation card like
  "5 + 7" matches its result card "12" — reuses the exact same matching
  engine as the other themes, just a different `GeneratePairs` function),
  and a difficulty. A chime plays on a match, a soft buzz on a mismatch.
- **Fishing Catch** — a basket on the dock shows a target letter, number,
  or (in **Colors** mode) a color name; catch 3 matching fish to advance.
  Colors mode matches by the fish's actual on-screen color, not a label -
  each fish gets the sprite for its own color, so "catch the blue one" is
  literally true. A splash plays on a miss, a catch chime on a hit, and
  clearing each target adds a chip to an in-round collection strip.
  Optional "3 Strikes" difficulty ends the round after 3 wrong catches.
- **Dress Up** — pick Girl (Princess/Witch/Unicorn) or Boy (Prince/
  Knight/Dragon), then drag sticker outfit pieces onto the character.
  Free play, no win state — tap "Done!" to log the session. **Undo**
  reverses whichever you did last (place/remove a sticker, or a drawing
  stroke) via one shared history; **Save Picture** composites the
  character, stickers, and drawing into a single PNG and downloads it.
- **Manners Garden** — practice social scenarios (sharing, saying
  please/thank you) for ages 3–5; correct choices grow the garden. The
  garden **persists across every session** (via `AppDataService`, keyed
  per kid profile) rather than resetting each play — "View My Garden" is
  reachable right from the setup screen. The setup screen also offers
  **practicing a single skill** (3 rounds of just "Saying Please", etc.)
  instead of always drawing 3 random lessons.
- **Tank Duel** — turn-based artillery duel vs. one CPU tank. Drag
  anywhere on the battlefield to aim; a dotted trajectory preview shows
  exactly where the shot will land before you release to fire (same
  physics used for the preview and the actual shot). Five hits destroys
  a tank, with a fire/impact/explosion sound cue at each stage. A crate
  wall forces a real arc rather than a flat shot; each win makes the next
  battle harder along two axes at once - the CPU's own aim tightens
  (±30% error at level 1, shrinking to a ±6% floor), and the wall gets
  taller, then shifts off-center, then (level 7+) splits into two walls
  to clear in one shot. Seeded into the catalog but not auto-granted to
  the default kid profile, same reasoning as Crown & Banner - grant it
  per-kid from the admin panel.

## Screen time tracking and limits

`Services/PlayTimeTracker.cs` ticks every 15 seconds while a Kid profile
is active — regardless of which screen or game is showing, including
iframe-hosted games the app has no visibility into otherwise — and adds
that time to the profile's usage total for the current local day
(`AppData.DailyUsageSeconds`). `Layout/MainLayout.razor` wraps every
page, so once a profile's daily limit (set from `/admin/history/{id}`)
is reached, a full-screen lockout appears no matter what's on screen,
and stays until the kid switches back to the profile picker.

This total is separate from `AppData.PlayHistory`, which logs
individual play sessions (game, difficulty, moves where applicable, and
duration) for the "recent activity" list on that same admin page.
Built-in games log their own entry on completion; iframe games have no
completion signal at all, so `GameHost.razor` measures their session
length itself and logs it on exit instead.

Known limitation: the timer keeps running if the tablet is put to sleep
or the tab is backgrounded (there's no Page Visibility API hook yet), so
a long stretch with the screen off while the tab is open would still
count against the limit.

## Data storage

All data (profiles, game catalog, per-kid access lists, play history,
and daily usage totals) lives in browser `localStorage` as a single
JSON blob, via
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

## Adding images

Game thumbnails and profile avatars now support local image paths with
emoji fallbacks. Put artwork under `wwwroot/images/` and enter the path
relative to `wwwroot` in the admin UI.

Examples:

- Profile avatar: `images/profiles/buddy.png`
- Game thumbnail: `images/game-thumbs/memory-match.png`
- Dress Up sticker art: `images/dressup/stickers/crown.png`
- Fishing art: `images/fishing/fish-blue.png`
- Manners Garden art: `images/manners/benny-bear.png`

The reusable renderer is `Components/AssetGlyph.razor`: it shows the
image when a path is present and falls back to the existing emoji when
the path is blank. That lets you migrate one asset at a time without
breaking old localStorage data.

## Known gaps / next steps

- PWA icons in `wwwroot/icons/` are simple placeholders — swap in real
  art before shipping.
- No file upload UI yet — images need to be copied into `wwwroot/images/`
  and referenced by path from the admin screens.
- The admin PIN is a light deterrent (SHA-256 hash stored client-side),
  not real security — fine for a kid's tablet, not for anything that
  needs to resist a determined adult.
- Dress Up's sticker tray (`AllStickers` in `DressUpGame.razor`) is
  shared across every character rather than curated per-character; more
  can be added to that one list without touching the drag-and-drop logic.
