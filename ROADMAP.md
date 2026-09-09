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

All done (2026-09-01). Live in `Components/Shared/`:

- [x] `GameSetupPanel` - the "choose options, then Start Game" card shell
- [x] `GameHud` - the top bar shown during play (title, exit, stats)
- [x] `StatPill` - the small label+value chip
- [x] `GameChoiceButton` - theme/difficulty/mode picker buttons
- [x] `GameOverlay` - the win/lose/game-over modal shell (deliberately
      untyped - also reused for non-result modals like Uno's color picker
      and Dress Up's gallery/lightbox; takes an optional `OnOverlayClick`
      for click-outside-to-close and a `Class` for cases needing a
      different stacking z-index)
- [x] `PrimaryActionButton` / `SecondaryActionButton` - the accent-filled vs.
      outlined button pattern every game reimplements per-component
- [x] `LockedChoiceNotice` / `PersonalBestBadge` - two more primitives that
      emerged during the migration (parent-locked difficulty notice,
      record/new-best badge)

All 8 games that had drift (`FishingGame`, `MemoryMatchGame`,
`MannersGarden`, `TankDuel`, `SimonSays`, `UnoGame`, `DressUpGame`, and
`CatchGame` - not in the original list but in the same unmigrated state)
are now on these components.

Gotcha hit during the migration, worth remembering for any future shared
component: Blazor CSS isolation only stamps a component's scope attribute
onto elements declared directly in *that* component's own `.razor`
markup - not onto elements rendered by a child component it passes a
`Class`/`PanelClass` parameter to. A game's per-game override CSS
targeting one of these shared components' classes needs `::deep` (e.g.
`::deep .du-done-btn { ... }`) or it silently never applies. Every
touched `*.razor.css` file now documents this inline where it matters.

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

- [x] Real game thumbnails for every entry in `Services/BuiltInGames.cs`.
      Memory Match, Fishing Catch, Dress Up, Manners Garden, Tank Duel, and
      Whack-a-Mole already had image-backed defaults (raster sprites from
      asset packs); UNO, Simon Says, Sliding Puzzle, Word Scramble,
      Minesweeper, and Sudoku had none of that kind of art available, so
      each got a small hand-authored SVG icon instead (`wwwroot/images/
      game-thumbs/*.svg`) depicting an actual scene from the game (a
      3-card fan for UNO, the Simon pad wheel, a scrambled numbered tile
      grid, tilted Scrabble-style letters spelling PLAY, a mine-flagged
      grid, and a shaded Sudoku grid) rather than generic clip art -
      verified rendered at the real 104px card-thumb size, not just at
      native resolution.
- [ ] Finish Lulu Lamb and Tilly Turtle art in `MannersGarden` - noted
      already in `wwwroot/images/README.md` (no sheep/lamb or turtle in the
      Kenney animal pack this project otherwise draws from) **[art needed]**
- [ ] Replace Manners Garden props/rewards with images (currently emoji)
      **[art needed]**
- [x] Replace Memory Match animal emoji with image cards
- [x] Simon Says stays CSS (no art migration planned) - but push on making
      the CSS itself feel like a polished toy (pad materials/shadows/press
      feedback), not a placeholder. Done: circular bezel console (dark
      radial-gradient plate with an inset rim shadow), domed radial-gradient
      pad materials instead of flat fills, a center hub cap like the real
      toy, and inset/outer shadow layering for press and lit-glow feedback.

## 4. Asset naming cleanup

- [x] Normalize `wwwroot/images/dressup/stickers/` to lowercase-kebab
      filenames, no spaces/mixed-casing/duplicate-suffix leftovers
      (`Dress4 (2).png`, `Crown.jpg`, `FairyWings3.png`, etc.). Checked
      2026-09-09: the whole 200-file folder is already lowercase-kebab (a
      `grep` for uppercase/space/paren characters across every filename
      turns up nothing), and every `StickerArt + "..."` reference in
      `DressUpGame.razor` matches an on-disk file 1:1 in both directions -
      nothing orphaned, nothing dangling. Whatever pass did this rename
      (see git history around 2026-08-14 through 2026-09-01, e.g. the 22
      gowns added 2026-08-14 that came in UUID-named and were renamed while
      wiring them in) already finished the job this section was tracking;
      it just never got checked off here. Going forward, new art dropped
      into that folder should keep following the convention rather than
      needing another cleanup pass later.

## 5. Immersive game screens

The global dark shell is right for admin/launcher, but gameplay should feel
more like a small world once it starts:

- [ ] Minimize top chrome during active play - the outer `.game-host-bar`
      is already a thin 44px strip with just a compact icon button for most
      games (see next item), so there isn't much air left to cut without a
      real per-game "focus mode" (e.g. auto-fade after a few seconds of
      inactivity). Not attempted yet - touches `GameHost.razor`, the single
      shared host for all 90+ games, so needs its own careful pass.
- [ ] Let the game scene be the dominant surface (less HUD chrome fighting
      it for attention) - investigated 2026-09-09: the outer host bar's
      circular exit icon and a game's own inner "Exit" button (from
      `GameHud`) can both show on the same screen (e.g. Simon Says), which
      reads like duplicate chrome at a glance. Confirmed this is
      *intentional*, not drift, before touching it - they're not redundant:
      the outer icon is a no-save "just leave" escape hatch
      (`GameHost.GoBack`, a plain nav-away), the inner button records the
      in-progress score/best and logs play history before exiting
      (e.g. `SimonSays.ExitToSetup`). Left as-is.
- [x] Themed background per game - `Services/GameThemes.cs` (landed
      2026-09-03 alongside unrelated feature work, never checked off here)
      maps every game to one of 6 curated category tints (toddler, nature,
      cardtable, puzzle, action, adventure) rather than one bespoke gradient
      each, a deliberate scope call given 90+ games - see that file's own
      header comment. Coverage gap found and closed 2026-09-09: Simon Says,
      Snake, and Knight's March had shipped after the map was last updated
      and were falling through to the flat untinted default; Build-a-Monster
      had no fitting bucket, so it seeded a new `gh-theme-creative` tint
      (warm amber) that the upcoming creative/sandbox game tier can reuse.
      Dress Up stays deliberately unmapped (owns its own full-bleed scene).
- [x] Move "Exit game" into a compact overlay button instead of a full HUD
      bar item - already true (`.game-host-exit-btn`: a 34px icon-only
      circle, not a labeled bar item), just never checked off here.

## 6. Responsive layout

- [x] `.carousel-wrap` uses a fixed `width: 71rem` - should be
      `width: min(71rem, 100%)` so it can't force horizontal overflow on
      narrower viewports. (Small, isolated, safe to land any time.)
