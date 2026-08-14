# UI/Visual Polish Roadmap

A working backlog for making the app look and feel more intentional, captured
2026-08-11. Nothing here is committed to a specific order beyond what's noted
under "Priority" - check items off (or delete them) as they land, and add new
ones as they come up. Items marked **[art needed]** are blocked on real
artwork/assets rather than code - those are on the human side of this repo,
not something an implementation pass can resolve on its own.

## Priority

Polish order, front-to-back: **launcher first** (profile select → game select
→ game cards → admin screens), since it's the "front door" every session
starts at - a better game card design (real thumbnails, stronger spacing,
consistent image framing) reads as a whole-app improvement for relatively
contained effort. Shared UI primitives are the mechanism that makes the
launcher work (and everything after it) look intentional rather than
one-off, so the two are tightly linked in practice.

## 1. Shared UI primitives

Currently duplicated (with drift) across `FishingGame`, `MemoryMatchGame`,
`MannersGarden`, `TankDuel`, `SimonSays`, `UnoGame`, and `DressUpGame`:

- [ ] `GameSetupPanel` - the "choose options, then Start Game" card shell
- [ ] `GameHud` - the top bar shown during play (title, exit, stats)
- [ ] `StatPill` - the small label+value chip (`.mg-stat`, `.tk-hp-group`,
      etc. are all the same idea today)
- [ ] `GameChoiceButton` - theme/difficulty/mode picker buttons (`.mg-diff-btn`,
      `.simon-diff-btn`, character/theme select buttons, etc.)
- [ ] `GameResultOverlay` - the win/lose/game-over modal (`.mg-win-overlay`,
      `.uno-overlay` + `.uno-win-panel`, `.simon-overlay`, `.du-win-overlay`,
      `.tk-*` equivalent, etc.)
- [ ] `PrimaryActionButton` / `SecondaryActionButton` - the accent-filled vs.
      outlined button pattern every game reimplements per-component

Once these exist, migrate each of the 7 games to use them (can land
incrementally, one game per pass, without a flag day).

## 2. Launcher polish (do this first)

- [x] Profile select - bigger/richer avatar framing (radial-gradient ring +
      inset border + shadow), stronger card spacing
- [x] Game select - inherits the game-card work below
- [x] Game cards - stronger spacing, consistent image framing (same
      radial-gradient ring treatment as profile avatars, so the two feel
      like one design language). **Real thumbnail art is still
      art-needed** - the framing now exists to drop real images into, but
      every built-in game still renders its emoji fallback.
- [ ] Admin screens - light spacing pass only so far (`.admin-section`
      padding/margin bumped); no deeper redesign yet

Also fixed in this pass (found while screenshotting, not originally on the
list): Blazor's `FocusOnNavigate` focuses the page's `h1` on every route
change for screen readers, which was rendering the browser's *default*
outline (a stray box around every page heading) since our custom
`:focus-visible` styling didn't cover it. Suppressed for that specific
programmatic-focus case (`h1[tabindex="-1"]:focus`) since it's never
reachable by real Tab navigation anyway.

## 3. Image migration, by category

Current state is uneven:

| Game | State |
|---|---|
| Fishing Catch | mostly image-based |
| Tank Duel | image-based |
| Dress Up | partially image-based (ongoing - see asset cleanup sessions) |
| Manners Garden | mixed image/emoji |
| Memory Match | animal cards image-based; ABC/Numbers/Math intentionally text |
| UNO | mostly CSS/emoji/text |
| Simon Says | mostly CSS/emoji/text |

Fastest visual wins, in order:

- [ ] Real game thumbnails for every entry in `Services/BuiltInGames.cs`
      **[partially done]** - Memory Match, Fishing Catch, Dress Up, Manners
      Garden, Tank Duel, and Whack-a-Mole now have image-backed defaults;
      UNO, Simon Says, Sliding Puzzle, Word Scramble, Minesweeper, and Sudoku
      still need purpose-built thumbnails rather than unrelated filler art.
- [ ] Finish Lulu Lamb and Tilly Turtle art in `MannersGarden` - noted
      already in `wwwroot/images/README.md` (no sheep/lamb or turtle in the
      Kenney animal pack this project otherwise draws from) **[art needed]**
- [ ] Replace Manners Garden props/rewards with images (currently emoji)
      **[art needed]**
- [x] Replace Memory Match animal emoji with image cards
- [ ] Simon Says stays CSS (no art migration planned) - but push on making
      the CSS itself feel like a polished toy (pad materials/shadows/press
      feedback), not a placeholder

## 4. Asset naming cleanup

`wwwroot/images/dressup/stickers/` has accumulated mixed casing, spaces, and
duplicate-suffix filenames (`Dress4 (2).png`, `Crown.jpg`, `FairyWings3.png`).
Normalize to lowercase-kebab, e.g.:

- `Crown.jpg` → `crown.jpg`
- `Dress1.png` / `Dress2.png` / `Dress3.png` → `dress-pink.png` /
  `dress-green.png` / `dress-blue.png`
- `FairyWings2.png` / `FairyWings3.png` → `fairy-wings.png` /
  `fairy-wings-alt.png`
- `Volcano.jpg` → `volcano.jpg`
- (etc. for the rest of the sticker set)

Needs a coordinated pass: rename on disk + update every `StickerArt + "..."`
reference in `DressUpGame.razor` in the same change, so nothing 404s
mid-migration. Going forward, new art dropped into that folder should already
follow the convention rather than needing another cleanup pass later - done
this way for the 22 gowns added 2026-08-14 (were UUID-named, e.g.
`009c08a4-b018-4a11-87b9-d39af385a3f3.png`, renamed to
`periwinkle-vine-gown.png` etc. while wiring them in). The pre-existing
`Dress1.png`/`Crown.jpg`/mixed-casing files above are still unmigrated.

## 5. Immersive game screens

The global dark shell is right for admin/launcher, but gameplay should feel
more like a small world once it starts:

- [ ] Minimize top chrome during active play
- [ ] Let the game scene be the dominant surface (less HUD chrome fighting
      it for attention)
- [ ] Themed background per game (Dress Up's pastel sky gradient is the
      model to extend to the others - most currently sit on the flat app
      shell background)
- [ ] Move "Exit game" into a compact overlay button instead of a full HUD
      bar item

This depends on `GameHud` (item 1) existing first, so the chrome-minimizing
behavior can be built once and inherited rather than hand-tuned per game.

## 6. Responsive layout

- [x] `.carousel-wrap` uses a fixed `width: 71rem` - should be
      `width: min(71rem, 100%)` so it can't force horizontal overflow on
      narrower viewports. (Small, isolated, safe to land any time.)
