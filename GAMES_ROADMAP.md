# New Games Roadmap

A backlog of game ideas to add to the launcher, captured 2026-08-14. This is
separate from `ROADMAP.md`, which tracks UI/visual polish of the app shell —
this file tracks *new game content*. Check items off (or delete them) as they
land, and add new ones as they come up.

## How a new built-in game gets wired in

Every existing game (`Components/*.razor`) follows the same four-step
pattern; new games should follow it too rather than inventing a new shape:

1. **Component** — `Components/<GameName>.razor`. Built from the shared
   primitives in `Components/Shared/` (`GameSetupPanel`, `GameHud`,
   `StatPill`, `GameChoiceButton`, `GameOverlay`, `LockedChoiceNotice`).
   Exposes a `<GameName>Result` record and an `OnComplete` callback.
   Existing games run 300–400 lines; that's the right ballpark.
2. **Registry** — add a `LaunchTarget` const + `BuiltInGame` entry (title,
   emoji) in `Services/BuiltInGames.cs`.
3. **Host wiring** — add an `else if` branch in `Pages/GameHost.razor` to
   render the component, plus a `Handle<GameName>Complete` method that
   writes a `PlayHistoryEntry`.
4. **Difficulty overrides** — if the game has difficulty tiers, thread
   `ForcedDifficulty` through like `Sudoku`/`Minesweeper`/`SlidingPuzzle` do,
   so admin per-child overrides keep working for free.

Two ideas from the original list are already built and can be skipped:
**Word Scramble** and **Sliding Puzzle** both exist today
(`Components/WordScramble.razor`, `Components/SlidingPuzzle.razor`).

## Shared game systems

As the game inventory grows, avoid building every game as a fully isolated
application. New games should own their actual mechanics, but reuse shared
systems for the common shell around those mechanics. Keep future components as
condensed as practical by extracting repeated behavior only when more than one
game genuinely needs it.

Candidate shared systems:

- `GameSession` / `GameSessionService` — score, elapsed time, pause state,
  resume state, game-over handling, and completion logging.
- Scoring and high scores.
- Difficulty selection and parent-locked difficulty.
- Timers, countdowns, and round clocks.
- Pause/resume.
- Achievements.
- Sound/music playback and parent-controlled audio settings.
- Player profile context.
- Unlockables and progression.
- Per-game settings.
- Local multiplayer turn/state handling.
- Game statistics and play-history summaries.

Example: Tower Defense, Typing Defense, Dungeon Crawler, and Asteroids-style
Space Game should all be able to consume the same session service for score,
elapsed time, pause state, and game-over handling. Each game should then only
implement what makes it mechanically distinct.

## Fun pass checklist

Before implementing a new game, give it a quick game-design pass so it does
not land as a technically correct but visually flat mechanic. Every game
should have a clear fantasy, satisfying feedback, and at least one reason to
replay.

Ask these before coding:

- What is the fantasy? A garden, dungeon, space mission, toy box, mystery,
  tournament, workshop, aquarium, etc.
- What moves on screen? Static boards need animated pieces, reveals, effects,
  timers, progress, or character reactions.
- What sound/animation rewards the main action? Taps, matches, hits, solves,
  upgrades, and mistakes should all feel intentional.
- What can the player unlock? New levels, skins, characters, stickers,
  towers, songs, puzzles, creatures, decorations, badges, or tools.
- What makes a replay different? Randomized layouts, puzzle packs, upgrade
  drafts, AI personalities, daily challenges, optional goals, or player-made
  content.
- What is the smallest version that still feels complete? Prefer a polished
  vertical slice over a large unfinished system.

Age-specific expectations:

- Toddler games should feel like interactive toys: giant tap targets, slow
  movement, gentle audio, no failure state, and immediate cause/effect.
- Younger-kid games should use collectible rewards, friendly characters,
  bright motion, and forgiving difficulty.
- Older-kid games should add strategy, progression, mastery, records, optional
  challenge, and meaningful choices without manipulative grind.

## Priority

Build in roughly this order — cheapest, most-reusable, least-art-dependent
first, so early passes fund shared infrastructure that later passes lean on:

0. **Section 0** (toddler cause-and-effect games, ~1 year old) — build these
   as their own extremely simple interaction family: giant tap targets, slow
   motion, no failure state, no timers, no scoring pressure, and gentle audio.
   These are not "easy mode" versions of the older games; they are sensory,
   exploratory activities.
1. **Section 1** (zero-asset, procedurally generated) — fastest wins, no art
   blocking, and each one is close to a solved problem already in this repo
   (Sudoku/Minesweeper prove out the "generate a puzzle grid" pattern).
2. **Section 2** (classic 2-player/AI games) — self-contained logic, no art,
   good excuse to add a small shared "AI opponent" helper.
3. **Section 3** (timed tap/arcade) — worth building a shared
   `TimedTargetSpawner` primitive once, then reskinning it five times.
4. **Section 4** (word games) — reuses Word Scramble's word-list
   infrastructure.
5. **Section 5** (memory/sequence) — reuses Simon Says' sequence-and-replay
   loop.
6. **Section 6** (real-time game-loop arcade) — needs a canvas/`requestAnimationFrame`
   loop distinct from anything in the repo today (Tank Duel is the closest
   precedent). Higher effort per game; do after the loop exists once.
7. **Section 7** (creative/sandbox) — highest effort, most art-dependent;
   reuses Dress Up's drag/place infrastructure. Do last.
8. **Section 8** (extra puzzle/math) — pull from here opportunistically when
   a smaller puzzle fits an existing component pattern.
9. **Section 9** (older kids, ~10-15 years old) — deeper strategy, reading,
   planning, upgrades, puzzles, resource management, and replayability. Treat
   these as longer-session games with real progression and save state.

## Downloaded asset fit

Kenney packs found in Downloads on 2026-08-14 are worth using for these
roadmap games:

- `kenney_shooting-gallery.zip` - Bubble Pop, Reaction Timer, and other
  target-tap arcade variants.
- `kenney_food-kit.zip` - Catch the Falling Objects, Fruit Slice, food
  sorting, and Pet Care feeding interactions.
- `kenney_cube-pets_1.0.zip` - Pet Care Game.
- `kenney_monster-builder-pack.zip` - Build-a-Monster.
- `kenney_space-shooter-remastered.zip`, `kenney_space-shooter-extension.zip`,
  and `kenney_simple-space.zip` - real-time arcade games after the shared game
  loop exists.
- `kenney_shape-characters.zip` - earmarked for Shape Sorter/Pattern Complete
  dressing, but both landed CSS-only (`clip-path` shapes, zero-asset by
  design - see Section 1) and don't need it now. Still fine for future
  procedural-puzzle dressing.
- `kenney_tiny-town.zip` and `kenney_tiny-dungeon.zip` - later Maze Escape,
  Treasure Hunt, or Story Builder scenes.

More packs found in Downloads/Assets on 2026-08-14 (second pass, same session
as the improvement-backlog audit below). Most of this second batch is 3D FBX
models - **not directly usable** by this app, which is entirely 2D
CSS/PNG/canvas art with no 3D rendering pipeline (no Three.js/Babylon or
similar). Flagging that explicitly so nobody burns time on a 3D import path
this stack doesn't support:

- `kenney_medals.zip` **(2D PNG, usable)** - flat award-ribbon/medal icons.
  Directly fits the single most-repeated gap from the improvement-backlog
  audit below: almost no game persists a personal-best/record, so there's
  nothing to show for a strong run. This pack is the art half of that fix -
  see "Missing progression & rewards" below.
- `kenney_animal-pack-remastered.zip` **(2D PNG, usable)** - "Round" style
  flat animal sprites (bear, buffalo, chick, chicken, cow, crocodile, etc).
  More variety than the current Memory Match/animal art draws from; also a
  candidate for a Word Search "Animals" category thumbnail or a new toddler
  animal-sound theme.
- `kenney_scribble-platformer.zip` **(2D PNG, usable)** - hand-drawn
  "scribble" style character + parallax background art. Fits Section 9's
  Platformer/Precision Platformer entries once the real-time game loop
  exists (Section 6).
- `kenney_tanks.zip` **(2D PNG, usable)** - tank bodies, turrets, bullets,
  arrows in more colors/styles than Tank Duel's current single green/grey
  pair (`wwwroot/images/tanks/`). A variety upgrade for an already-shipped
  game, not a new-game unlock - low priority, but cheap if picked up.
- `kenney_tower-defense.zip` and `kenney_tower-defense-top-down.zip`
  **(2D PNG/spritesheet, usable)** - landscape tiles + towers in brown/grey/
  red variants. Same story as the tanks pack: Tower Defense already has its
  own art (`wwwroot/images/tower-defense/`), this would only be a visual
  variety pass, not a blocker for anything on this roadmap.
- `kenney_blocky-characters_20.zip`, `kenney_car-kit.zip`,
  `kenney_mini-characters.zip`, `kenney_mini-forest_1.0.zip`,
  `kenney_tower-defense-kit.zip` - **3D FBX only, not usable as-is.** Leave
  these untouched unless the app ever adds a 3D rendering path; re-triage
  then rather than guessing now.

Do not bulk-import these packs. Pull only the sprites used by a landed game
or shared UI reward, then document the copied subset in `wwwroot/images/README.md`.

## 0. Toddler cause-and-effect games (~1 year old)

Target audience: very young kids around 1 year old. Design for exploration,
not correctness. Every interaction should be valid, obvious, forgiving, and
pleasant. Use huge targets, minimal text, slow movement, low visual clutter,
soft sounds, and short animations. Avoid fail states, countdowns, score
pressure, reading requirements, precision dragging, and multi-step rules.

Implementation notes:

- These can share a `CauseAndEffectScene` primitive rather than the more
  competitive `TimedTargetSpawner` shape used by older arcade games.
- Completion logging should record play sessions/time, not win/loss. A child
  may never "finish" the activity.
- Prefer tap-first interactions. Drag can exist only when it is extremely
  forgiving and snaps automatically.
- Audio needs parent controls and should avoid sudden loud effects.

Landed as 5 components, each covering several bullets below as reskinned
themes rather than one component per bullet (per the shared-primitive note
above) — `PeekReveal.razor` (Curtain/Present Box/Doors/Egg themes),
`SoundButtons.razor` (Animals/Vehicles/Fun Sounds themes), `PopAndSparkle.razor`
(Stars/Balloons/Bubbles themes), `BabyPiano.razor` (Piano/Drums themes), and
`MagicGarden.razor` (single theme). None of these have a natural "finish", so
each logs its play session (time + tap count) from an in-scene "All done"
button instead of a win/lose overlay - see the doc comment at the top of each
component. Bullets left unchecked below are either a genuinely different
mechanic (not yet built) or a close cousin absorbed into one of the five
above without being its own theme.

### Peek/reveal games

- [x] Peekaboo Animals — tap a curtain, bush, box, or door and an animal pops
      out with a sound.
- [x] Open the Doors — colorful doors reveal an animal, toy, vehicle, or silly
      character.
- [ ] Where Did It Go? — an object slowly hides behind something; tap the
      hiding place to reveal it.
- [x] What's in the Box? — tap a present or box to open it and reveal a
      surprise.
- [x] Egg Surprise — tap a large egg several times; it cracks and reveals a
      baby animal.

### Big tap targets and sensory feedback

- [x] Tap the Star — one large star appears somewhere on screen; tap it and it
      sparkles, makes a sound, then moves.
- [x] Pop the Balloons — huge slowly floating balloons burst with a gentle
      animation and sound when tapped.
- [ ] Lights On / Lights Off — tap lamps, switches, stars, or windows to
      toggle them on and off.
- [ ] Big / Small — tap a tiny object and it becomes huge; tap again and it
      shrinks.
- [ ] Color Splash — every screen tap produces a large blob of color with a
      soft sound or animation.
- [ ] Finger Trails — drag a finger and leave stars, bubbles, snowflakes,
      paint, or sparkles behind.
- [ ] Fireworks Touch — tap anywhere for a soft colorful burst; keep it gentle,
      not realistic or loud.
- [x] Bubble Machine — tap a machine and bubbles pour out; tap bubbles to pop
      them.
- [ ] Spin the Wheel — swipe or tap a large wheel, fan, pinwheel, or carousel
      and watch it spin.

### Animals, body, and sound

- [x] Animal Sounds — large animal buttons; tap a cow to hear "moo", tap a
      duck to hear "quack", etc.
- [x] Vehicle Sounds — tap a car, train, airplane, tractor, etc.; it moves
      briefly and makes its sound.
- [ ] Wake Up the Animals — sleeping animals wake, stretch, make a sound, then
      eventually fall asleep again.
- [ ] Funny Faces — tap parts of a face; nose honks, ears wiggle, eyes blink,
      tongue pops out.
- [ ] Touch the Body Part — a friendly character highlights "nose", "hand",
      "foot", etc.; exploratory rather than a correct-answer quiz.
- [x] Sound Buttons — large buttons for giggles, claps, sneezes, bells,
      boings, animal sounds, etc.
- [ ] Bedtime Animals — tap animals to tuck them into bed, turn off the lamp,
      and hear a tiny sleepy sound.

### Gentle worlds

- [ ] Touch the Fish — fish swim slowly around an aquarium; touching one makes
      it wiggle, bubble, or swim away.
- [ ] Make It Rain — tap clouds for rain, then sunshine, rainbows, puddles,
      etc.
- [x] Magic Garden — tap empty spots and flowers grow; tap flowers and
      butterflies appear.
- [ ] Baby Aquarium — mostly passive fish, bubbles, and plants; touching
      anything causes a small response.
- [ ] Night Sky — tap the dark sky to add stars; tap stars to make them
      twinkle.
- [ ] Snow Day — tap to make snow fall, create footprints, or reveal objects
      beneath snow.
- [ ] Puddle Splash — tap puddles and watch a character jump into them.
- [ ] Follow the Butterfly — a butterfly moves slowly around; touching it
      makes it flutter somewhere else.

### Food, music, and physical play

- [ ] Feed the Animal — tap or drag food to an animal; banana to monkey,
      carrot to bunny, etc.
- [x] Baby Piano — 4-8 enormous colorful keys with friendly sounds; no wrong
      notes and no objectives.
- [x] Drum Pad — big drums, bells, rattles, and clapping sounds; controlled
      noise production, the ancient toddler art form.
- [ ] Bath Time — tap bubbles, splash water, squeak a rubber duck, or pour
      water from a cup.
- [ ] Stack the Blocks — extremely forgiving drag-and-drop blocks that snap
      together automatically.
- [ ] Knock It Down — start with a block tower; tap it and everything tumbles,
      then automatically rebuilds.
- [ ] Roll the Ball — swipe or tap a large ball and watch it roll, bounce, or
      knock over objects.

## 1. Zero-asset procedural puzzles

Great for younger kids, cheapest to build, no art needed — same shape as
Sudoku/Minesweeper's "generate a grid, validate a solution" loop.

- [x] Odd One Out — spot the tile that differs from the rest; grid size
      (4/6/9 tiles) scales with difficulty.
- [x] Pattern Complete — repeating emoji cycle with the last symbol
      hidden; cycle length (2/3/4 symbols) scales with difficulty.
- [x] Number Sequence — counting sequence (up or down, variable step)
      with the next number hidden.
- [x] Color Match — tap the swatch matching a target color, no reading
      required; palette size and hue closeness scale with difficulty.
- [x] Shape Sorter — tap a shape then tap its matching outline, rather
      than true pointer drag (native HTML5 drag-and-drop doesn't fire on
      touch/tablet browsers, which is how this app is actually played).
      CSS `clip-path` shapes, no art assets.
- [x] Shadow Match — match a dark CSS-silhouette shape to its colored
      counterpart among several options; used the CSS-mask escape hatch
      instead of real art, same shape set as Shape Sorter.

All six follow the `HigherOrLower.razor` template shape (Setup/Playing/
Finished phase enum, `ForcedDifficulty` threading, fixed-length session,
`<GameName>Result` record) and reuse the shared `game-setup-panel` /
`game-hud` / `game-option-grid` / `game-result-*` CSS classes from
`app.css` rather than hand-rolling setup/HUD/overlay chrome. Each is a
fixed 8-round (or 4-set, for Shape Sorter) session with score/streak
tracking, ending in a result overlay — no fail state, matches the
scoring feel of `HigherOrLower`/`TicTacToe`. New games only appear for a
kid profile once a parent adds them via Manage games → "+ Add game to
catalog" — that's the per-profile catalog opt-in, not a bug to chase if
a freshly-added game isn't visible right away.

## 2. Classic strategy / AI opponent games

- [x] Tic-Tac-Toe (easy/medium/hard AI — minimax for hard)
- [x] Connect Four (shares board/AI shape with Tic-Tac-Toe; do second)
- [x] Rock Paper Scissors (win streaks, unlockable themes)
- [x] Higher or Lower (card/number guessing, streak scoring)

A small `Services/GameAi` (or similar) minimax helper, written once for
Tic-Tac-Toe, should directly cover Connect Four's harder difficulty tiers.

## 3. Timed tap / arcade reflex games

All variations on "things appear, tap them before they vanish, timer +
scoring." Worth one shared `TimedTargetSpawner` component/primitive before
building the first of these, since all five reskin it.

- [x] Whack-a-Mole
- [x] Catch the Falling Objects
- [x] Fruit Slice
- [x] Bubble Pop (can double as educational — colors/numbers/letters)
- [x] Reaction Timer
- [x] Red Light, Green Light

## 4. Word games

- [x] Hangman / Guess the Word — landed as `Components/GuessTheWord.razor`,
      the non-hangman fail state suggested here: wrong guesses burn rocket
      fuel instead of drawing a gallows, a solved word launches the rocket.
- [x] Word Search — landed as `Components/WordSearch.razor`, procedurally
      generated grid from themed word lists (Animals/Space/Food/Ocean).
      Drag-select tracks the pointer at the grid-container level rather
      than per-cell, the same touch-reliability reasoning as Dress Up's
      drag (see `ROADMAP.md` asset-cleanup notes and Shape Sorter above
      for the same lesson applied elsewhere in this repo).

## 5. Memory / sequence games

Adjacent to Simon Says (`Components/SimonSays.razor`) — same
show-a-sequence, replay-it-back mechanic with different presentation.

- [ ] Copy the Pattern (grid version, spatial rather than sequential)
- [ ] Memory Sequence Adventure (Simon Says mechanic, character-travels-through-doors presentation)
- [ ] Follow the Cups (shell game)
- [ ] Hot and Cold
- [ ] Treasure Hunt (map/room navigation via clues)
- [ ] Animal Sound Guessing Game

## 6. Real-time game-loop arcade

Needs a genuine game loop (canvas or `requestAnimationFrame`-driven), closer
to Tank Duel than anything else in the repo. Build the loop once, reuse for
all of these.

- [ ] Snake
- [ ] Breakout
- [ ] Pong
- [ ] Maze Escape (random maze generation + keyboard/swipe navigation)
- [ ] Frogger-style Crossing Game
- [ ] Endless Runner

## 7. Creative / sandbox games

Highest effort, most art-dependent. Reuse Dress Up's drag/place/asset
infrastructure rather than building new.

- [ ] Pet Care Game **[art needed]** — reuses Dress Up UI/asset infra
- [ ] Build-a-Monster **[art needed]** — same infra as Dress Up, sillier
      asset set
- [ ] Coloring Book **[art needed]** — fill-region mechanic; free
      drawing/stickers/save are later extensions, not v1
- [ ] Pixel Art Creator (grid painter; symmetry mode + templates are
      extensions, not v1)
- [ ] Story Builder (card-based, not generative-AI — characters/locations/
      objects/actions as reusable cards)

## 8. Extra puzzle/math (not yet bucketed to a section above)

- [ ] Jigsaw Puzzle (start rectangular pieces; reuse Sliding Puzzle's grid
      math as a starting point before attempting true jigsaw geometry)
- [ ] Quick Math (timed arithmetic, combo streaks)
- [ ] Math Target (reach a target number using given numbers + operators —
      older kids)
- [ ] Spot the Difference (two similar scenes, 3–10 differences) **[art
      needed]** — more asset-heavy than most of this list; low priority
      until art pipeline supports it

## 9. Older-kid games (~10-15 years old)

Target audience: older kids and teens. These can assume more reading,
planning, delayed rewards, multi-step rules, failure/retry loops, and longer
sessions than the toddler and young-kid sections. They should still stay
age-appropriate: no gambling, real-money mechanics, dark themes, or
manipulative engagement loops. Prefer transparent progression, optional
difficulty, clear saves, and parent-visible play history.

Implementation notes:

- Many of these need real save state: campaign progress, unlocked levels,
  upgrades, custom creations, records, or inventories.
- Start with small vertical slices. A 10-level Tower Defense or 8-room
  Dungeon Crawler is better than a giant unfinished system.
- Shared infrastructure candidates: grid/board engine, `GameAi`, simple
  pathfinding, deterministic level data, physics helpers, local leaderboards,
  inventory/upgrades, and save slots.
- Educational games should be good games first; the learning content should
  ride on strong feedback loops instead of feeling like homework with sprites.

**Status, 2026-08-17:** all nine subsections below have been worked through.
Landed: 6 board/logic/deduction games, 3 mystery/code puzzles, 3
physics/sports games (analytic trajectory-solve pattern), 2 spatial/rhythm
puzzles, and 4 educational challenge games - 18 new games total this pass.
Deferred with reasoning documented inline per subsection: arcade/racing/
platforming (needs a continuous-physics/collision engine this app doesn't
have), most of physics/sports and spatial puzzles beyond what shipped (same
reasoning), several educational items as redundant with an existing game or
with Trivia Battle's format, and the entire simulations/management/
collection and creation-tools subsections (persistent-progression economy
and content-authoring/editor tooling are both new product categories, not
same-day additions). Chess Puzzles, Checkers with AI, and a few others from
the strategy subsection remain open too - see that subsection below.

### Strategy, tactics, and RPG systems

- [x] Tower Defense — place different towers along a path to stop waves of
      enemies; good for upgrades, strategy, balancing, and progression.
      Landed as one vertical slice (`Components/TowerDefense.razor`) and
      since expanded: levels are procedurally generated (unlimited
      distinct paths, no fixed set to pick from) and auto-cycle - clear a
      level's 8 waves and the next one generates automatically, towers
      refunded so the player can rebuild for the new layout. Waves
      auto-advance a few seconds after each clears (manual "Start Now"
      skip available). 3 towers with a working Upgrade/Sell economy plus
      range previews and visible shots; towers now rotate to face their
      current (or last) target instead of standing static. 6 animal enemy
      types (reusing Memory Match's art) with weighted, ever-escalating
      wave composition. Easy/Medium/Hard are a completable 3-level
      campaign; Endless (same starting bar as Hard) removes that cap
      entirely. Two max-level towers of the same kind can Merge into a
      Mega tower (splash damage, a real "boom" moment with explosion
      sound/visual at fusion) instead of just upgrading further - an
      in-game legend (❓ button, setup screen and HUD) explains all three
      combinations. Still no persistent save state (best-run records,
      unlocks) - see the implementation notes above for why that's
      intentional for now.
- [ ] Dungeon Crawler — small randomly generated rooms with enemies, treasure,
      keys, traps, and upgrades.
- [ ] Roguelike Arena — survive increasingly difficult waves, choose upgrades
      after each round, and build a different character each run.
- [ ] Mini RPG Battle Game — turn-based combat with attack, defend, magic,
      items, elemental weaknesses, and simple character progression.
- [ ] Card Battle Game — build a small deck and fight AI opponents; simplified
      strategy cards, not a massive collectible-card system.
- [ ] Auto Battler — buy or place units, then watch them automatically fight;
      strategy comes from team composition and positioning.

### Board, logic, and deduction games

- [ ] Chess Puzzles — "mate in 1", "mate in 2", and tactical challenges
      instead of full chess. Deferred - needs either a curated puzzle bank
      or real chess-move validation, more upfront content/logic cost than
      the rest of this subsection.
- [ ] Checkers with AI — straightforward rules and strong difficulty-level
      potential. Deferred - jump-chain rules and king promotion are a
      bigger rules surface than this batch's games.
- [x] Reversi / Othello — landed as `Components/Reversi.razor`. Standard
      8x8 board vs a CPU opponent; AI extends `Services/GameAi.cs`
      (`ReversiLegalMoves`/`ReversiApplyMove`/`ReversiMove`) with the same
      alpha-beta minimax shape as Tic-Tac-Toe/Connect Four, evaluating a
      positional weight table (corners great, cells next to a corner bad)
      plus a mobility term, with move ordering for pruning efficiency
      given Reversi's higher branching factor.
- [x] Battleships — landed as `Components/Battleships.razor`. Vs a CPU,
      both fleets auto-placed (manual placement deferred - the strategy
      here is firing/hunting). CPU difficulty is a real targeting
      algorithm: Easy random, Medium hunts after a hit, Hard adds
      checkerboard-parity search while hunting blind.
- [x] Mastermind — landed as `Components/Mastermind.razor`. Colored-peg
      deduction, colors can repeat, black/white feedback pegs, guess
      budget scales with difficulty.
- [ ] Logic Grid Puzzles — deduction clues such as "Alex has the red bike,
      Sam doesn't own the cat."
- [ ] Nonograms / Picross — number clues reveal a hidden pixel image; strong
      replayability from generated or data-driven puzzles.
- [x] 2048-style Puzzle — landed as `Components/Puzzle2048.razor`. Swipe
      (net pointerdown/pointerup displacement, no per-cell tracking needed)
      or on-screen arrows; difficulty scales board size and target tile.
- [ ] Threes-style Number Puzzle — similar sliding-number category, with a
      distinct rule set. Deferred - close enough to 2048 above that
      shipping both back-to-back risked feeling redundant; revisit if the
      distinct "next tile preview" rule earns its own slot later.
- [ ] Hex / Territory Capture — players compete to control board sections by
      placing tiles.
- [x] Dots and Boxes — landed as `Components/DotsAndBoxes.razor`. Real
      "complete a box, go again" rule intact; renders on the standard
      doubled-coordinate grid technique. CPU difficulty ladder: Easy fully
      random once nothing's free, Medium avoids handing over free boxes,
      Hard also minimizes what it gives away when forced to open a chain.
- [ ] Ultimate Tic-Tac-Toe — nine Tic-Tac-Toe boards arranged inside one larger
      board.
- [ ] Sequence Puzzle — memorize increasingly long visual, audio, or
      directional sequences. Deferred to Section 5 (memory/sequence) where
      it fits better alongside Simon Says' sequence-and-replay loop.
- [x] Code Breaker — landed as `Components/CodeBreaker.razor`. Numeric
      safe-cracking with no-repeat digits (a genuinely different
      constraint from Mastermind's repeatable colors), spot/digit
      feedback shown as counts instead of peg dots.

All six landed games follow the same pattern as every other built-in:
`GameSetupPanel`-shaped setup screen reusing `app.css`'s shared classes,
`ForcedDifficulty` threading for admin per-kid locks, abandon-logs-to-
history and personal-best wiring built in from the start (not retrofitted
the way earlier sections needed), and a `<GameName>Result` record. All six
also wired up `Components/Shared/PersonalBestBadge.razor`: fewest guesses
(Mastermind, Code Breaker), fewest shots (Battleships), highest tile
(2048), biggest win margin (Reversi, Dots and Boxes).

### Mystery, escape, and code puzzles

- [ ] Escape Room — a small interactive room with clues, codes, switches,
      hidden objects, and puzzles. Deferred - needs real authored
      multi-puzzle room content (not just one generated mechanic), a
      bigger content-design lift than this batch's games.
- [ ] Detective Mystery — read clues, inspect suspects, identify
      contradictions, and solve a fictional case. Deferred - needs either
      hand-authored cases or a template-based case generator; picking that
      approach deserves its own pass rather than a rushed cut here.
- [ ] Mystery Mansion — explore rooms, collect clues, unlock areas, and solve
      a central mystery. Deferred, same reasoning as Escape Room.
- [ ] Treasure Map Puzzle — interpret riddles and map clues to locate hidden
      treasure. Deferred - needs an authored riddle bank to feel real
      rather than arbitrary.
- [x] Cryptogram — landed as `Components/Cryptogram.razor`. Substitution-
      cipher phrase decoding; tap a coded letter, tap your guess, every
      occurrence updates at once. No per-letter feedback, same as a real
      cryptogram - only a fully-correct phrase counts as solved.
- [x] Morse Code Challenge — landed as `Components/MorseCodeChallenge.razor`.
      Decode a word shown only as Morse; Easy/Medium keep a reference
      chart visible, Hard hides it.
- [x] Programming Puzzle Game — landed as `Components/RobotCommands.razor`
      (Move/Turn Left/Turn Right through a BFS-verified-solvable
      procedural maze). Repeat/loop blocks intentionally deferred -
      difficulty scales grid size, wall density, and max program length
      instead.
- [ ] Circuit Builder — connect batteries, switches, lights, motors, and logic
      gates to complete objectives. Deferred - node-graph connection UI +
      circuit simulation is a genuinely new interaction model, not a
      reskin of anything already built here.
- [ ] Factory Automation Puzzle — place conveyors, sorters, machines, and
      switches to route items correctly. Deferred, same reasoning as
      Circuit Builder - a real flow-simulation engine, not a quick slice.

### Physics, sports, and skill challenges

- [x] Archery Challenge — drag-to-aim slingshot mechanic reusing TankDuel's
      analytic trajectory solve (`SolveImpactTime`, exact quadratic solve
      rather than a stepped simulation). Adds per-shot random wind that
      shifts the arc, and a 5-ring scoring system (10/8/6/4/2/miss) based on
      distance from ring center at the moment the arrow crosses the target
      plane.
- [x] Basketball Shot Game — same drag-to-aim/trajectory-solve approach as
      Archery, but binary make/miss through a hoop tolerance window instead
      of graded rings, and the hoop itself moves per shot on Hard. Personal
      best tracks longest make streak rather than points, specifically so it
      reads differently from Archery's ring-total best.
- [x] Penalty Shootout — simultaneous zone-choice game (tap Left/Center/Right
      while the CPU keeper independently picks a dive zone) rather than a
      power/timing minigame — no physics engine needed. Keeper AI reuses the
      recency-weighted prediction approach fixed for Rock Paper Scissors
      earlier in this pass, reading a rolling window of recent shots instead
      of full match history, so Hard can't be baited by an old pattern.
- [x] Air Hockey — the "needs a real continuous-collision physics engine"
      reasoning that deferred this whole group turned out to be worth
      solving properly rather than working around: added
      `Services/Physics2D.cs`, a small reusable circle/circle +
      circle/wall impulse-collision layer (position, velocity, mass,
      restitution, time-based drag), stepped at a fixed 1/120s timestep
      with capped substeps per rendered frame so a fast puck can't tunnel
      through a paddle or wall on a slow frame. The player's paddle
      derives its velocity each substep from how far the pointer target
      moved, not just its position, so a fast flick hits harder than a
      slow nudge. CPU paddle AI predicts the puck's future X (scaled by
      difficulty) rather than just chasing its current position. No
      spin/angular velocity yet - deliberately scoped to "get the
      fundamentals feeling right" first. Personal best tracks biggest
      winning margin.
- [ ] Pool / Billiards Lite — next up, building on the same Physics2D
      layer now that it exists. Needs more than Air Hockey used: many
      balls instead of one puck (broad-phase collision checks between
      all pairs), pockets (a ball leaving play rather than bouncing),
      cue-stick aim/power input, and eventually spin (angular velocity +
      tangential impulse at the contact point, and sliding-vs-rolling
      friction) for realistic cue-ball control - explicitly the reason
      Physics2D's circle collision doesn't model spin yet, per its own
      header comment.
- [ ] Bridge Builder — still deferred. A structural/rigid-body physics
      engine (load-bearing joints, stress, collapse) is a different
      problem from point-mass circle collision; Physics2D doesn't cover
      it.
- [ ] Physics Puzzle — still deferred, same reasoning as Bridge Builder:
      ramps/springs/fans/magnets acting on a moving object need more than
      circle/circle and circle/wall collision.
- [ ] Marble Run — still deferred, same reasoning.
- [ ] Mini Golf — reconsider now that Physics2D exists: it's much closer
      to feasible than before (circle/wall bounce off course edges is
      exactly what the new engine already does), mainly needing sloped
      terrain/friction-per-surface and a hole-capture check added on top.
      Not built yet, but no longer blocked on "no physics engine."
- [ ] Dodgeball Arena — still deferred. Needs real-time multi-projectile
      collision *and* arena movement (the player's own avatar dodging),
      a bigger scope than a single controlled paddle/cue.

### Arcade, racing, and platforming

Deferred as a whole subsection. Every entry here needs continuous real-time
movement and collision against hand-built levels/tracks (a platformer's
jump arcs and moving platforms, a racer's track boundaries, a runner's
obstacle timing) - a fundamentally different engine from the turn-based
and analytic-solve patterns (drag-to-aim trajectory solve, grid/phase-based
puzzles) used everywhere else in this app. Most also assume dedicated
character/vehicle/level art rather than the emoji-and-CSS-shape approach
that's kept every other game in this roadmap asset-free. Building even one
of these properly (a real physics/collision step loop, camera, level
format) is a new engine investment, not an evening's reskin - worth
revisiting as a dedicated project, not folded into this pass.

- [ ] Asteroids-style Space Game
- [ ] Space Trader
- [ ] Space Mining Game
- [ ] Top-Down Racing
- [ ] Time Trial Racer
- [ ] Drift Challenge
- [ ] Endless Survival Runner
- [ ] Platformer
- [ ] Precision Platformer
- [ ] Grappling Hook Game
- [ ] Ninja Wall Jump
- [ ] Stealth Game

### Spatial and rhythm puzzles

- [x] Laser Maze — procedurally generated by building a guaranteed-solvable
      path first (random walk with 90-degree turns, each turn assigned the
      one mirror orientation that produces it) and scrambling every mirror
      away from that solution, so no separate solver/BFS-verify pass is
      needed. Personal best tracks fewest mirror flips to solve.
- [x] Rhythm Game — a pad pulses on a fixed tempo; tap it as close to the
      pulse as you can. Timing is judged against wall-clock time the beat
      fired (same approach as ReactionTimer) rather than animation frames.
      Difficulty only changes tempo, not the hit windows, so Hard stays
      strictly harder instead of accidentally becoming more forgiving (the
      inversion-bug class fixed for HigherOrLower/ReactionTimer earlier).
- [ ] Portal Puzzle — deferred. Needs an object-movement/level-traversal
      model (something walks or slides through the level and needs its
      path re-simulated through linked portals) that's a step beyond a
      single reflected beam; worth a dedicated pass, not a quick addition.
- [ ] Gravity Switch — deferred. Needs continuous fall/movement physics
      to navigate as gravity flips, same "new engine" reasoning as the
      physics/sports subsection above.
- [ ] Light and Shadow Puzzle — deferred. Needs real shadow-casting geometry
      (light source + occluder + projected shape matching), a distinct
      rendering problem from anything else built here.

### Educational challenge games

- [x] Word Ladder — change one letter at a time to climb from a start word
      to a target word, every step a real word. Uses hand-authored ladder
      chains (like Cryptogram's phrase list) rather than validating
      against a dictionary. Three wrong guesses on a rung locks one letter
      in as a hint. Personal best tracks fewest mistakes.
- [x] Trivia Battle — 42-question hand-authored bank across 5 categories
      (Animals, Space, Science, Geography, History) plus Mixed, split by
      difficulty tier. Streak bonus grows score for consecutive correct
      answers; a 50/50 lifeline (2 uses Easy, 1 Medium, 0 Hard) removes two
      wrong options. Multiplayer explicitly out of scope, same as every
      other game in this app. Personal best tracks high score.
- [x] Flag Guessing Game — shows a country's flag as a plain Unicode flag
      emoji (a two-letter regional-indicator sequence) instead of an image
      asset, so it needed zero art. 45 countries across three difficulty
      tiers by flag recognizability. Personal best tracks high score.
- [ ] Typing Racer / Typing Defense — deferred. Genuinely easy to build
      (type-the-word-before-it-arrives is a simple timer + text-match
      loop) but redundant with the typing-under-pressure skill Trivia
      Battle and Word Ladder already exercise via tapping; revisit if a
      dedicated typing-speed game is wanted specifically.
- [ ] Geography Challenge (capitals/landmarks/map-based) — deferred.
      Distinct from Flag Guessing Game once it involves an actual map (a
      new interactive-map rendering problem this app doesn't have), though
      a capitals-only multiple-choice version would be a near-clone of
      Trivia Battle's Geography category and isn't worth duplicating.
- [ ] Periodic Table Challenge — deferred as niche: real content (118
      elements) is easy to author, but the audience for element-symbol
      drilling is a narrower slice of this app's age range than the games
      already shipped.
- [ ] Vocabulary Duel — deferred as redundant with Trivia Battle's format
      (multiple-choice knowledge quiz) - a synonym/antonym/definition
      quiz is the same interaction with a different content bank, not a
      new mechanic.
- [ ] Boggle-style Word Hunt — deferred as redundant with the existing
      Word Search game (grid-of-letters word finding is already covered).
- [ ] Anagram Battle — deferred as redundant with the existing Word
      Scramble game (rearrange-letters-into-a-word is already covered).
- [ ] Trivia Survival / Quiz RPG — deferred as variants of Trivia Battle
      (health-loss and monster-battle framing around the same
      multiple-choice-question core) rather than a new mechanic; revisit
      as a reskin later if there's appetite for a second trivia mode.
- [ ] Fake News Detective — deferred. The content itself needs careful,
      deliberate authoring (realistic-but-clearly-fictional examples,
      calibrated so the "tells" are fair without teaching kids to distrust
      real news) - that's an editorial task worth its own careful pass,
      not something to rush through in this batch.

### Simulations, management, and collection

Deferred as a whole subsection. Every entry here is a persistent-progression
game (inventory, currency, unlockable upgrades, stats that carry between
sessions and grow over many play sessions) rather than a single self-
contained round with a phase enum and an `OnComplete` callback - the shape
every other game in this app follows, including the existing lightweight
Fishing Catch minigame this list's "Fishing Game" entry would need to grow
well past. Building one properly needs a real save-data/economy layer (item
definitions, balance tuning, persistence beyond a personal-best number) -
a scoped feature project in its own right, not a same-day addition.

- [ ] Stock Market Simulator
- [ ] City Builder Lite
- [ ] Colony Survival
- [ ] Restaurant Manager
- [ ] Shop Simulator
- [ ] Theme Park Builder Lite
- [ ] Fishing Game (equipment/rarity/upgrades version)
- [ ] Creature Collector
- [ ] Monster Fusion
- [ ] Character Builder

### Creation tools

Deferred as a whole subsection. These are authoring/editor tools (draw
pixel art, build a playable level, sequence music, animate frames), a
different product category from "play a round of a game" - each needs its
own editing UI plus a way to save and revisit created content, neither of
which this app currently has anywhere. Worth its own design pass, not a
same-day slot-in next to the games above.

- [ ] Pixel Art Challenge
- [ ] Level Creator
- [ ] Music Sequencer
- [ ] Animation Studio

## 10. Existing-game improvement backlog

Captured 2026-08-14 via a full read-through of every `Components/*.razor`
game (+ matching `.razor.css`) after Sections 0-4 landed — a retrospective
pass, not a new-game pass. Re-triage as items land; delete a bullet once
it's actually fixed rather than checking it off, since most of these
aren't a single-line change.

**Update, same day:** everything under "Bugs to fix first," "Balance," and
most of "Systemic"/"Missing progression & rewards"/"Per-game polish" below
landed in a single follow-up pass (see commit history from this date) -
fixed bullets have been deleted per the convention above rather than left
checked off. What's left in each section below is genuinely still open.
Summary of what shipped:

- All 8 named bugs fixed, including two real defects (PatternComplete's
  answer was mathematically always the first tile; DressUpGame's "Dress
  Another" could throw a `NullReferenceException` via a stale HUD
  fragment) and three stuck-drag/re-entrancy races (MannersGarden,
  TankDuel, TowerDefense).
- The "abandon mid-session never logs" gap fixed across all 30 games with
  a session concept - each now invokes `OnComplete` with whatever was
  reached so far, guarded against double-firing on top of a natural finish.
- A new personal-best/record system (`AppDataService.GetBestAsync`/
  `TryRecordBestAsync`, `Components/Shared/PersonalBestBadge.razor`, medal
  art from `kenney_medals.zip`) wired into the 5 games named below -
  MemoryMatchGame (best time), FishingGame (fewest attempts), TankDuel
  and TowerDefense (best level - `TankDuelResult` also gained the missing
  `Level` field), GuessTheWord (best streak, all-time).
- All 9 named balance issues fixed (HigherOrLower's inverted difficulty,
  RockPaperScissors' baitable/peeking AI, ReactionTimer's inverted wait
  range, FishingGame's flat strike budget, TankDuel's plateauing CPU,
  Minesweeper's flat mine density, ColorMatch's hue collision, OddOneOut's
  untiered pair difficulty, SlidingPuzzle's always-visible numbers).
- `.game-choice-locked` deduped across the 10 games that had a true
  byte-identical copy (each game's `-stat` classes were left alone -
  turned out to be plain text styling, not the bordered-pill `.game-stat`
  they superficially resembled).
- Sound added to the 4 games that had none at all (Sudoku, Minesweeper,
  SlidingPuzzle, WordScramble) - Minesweeper's explosion/win sound was the
  starkest gap.
- WordSearch's mismatch feedback and PeekReveal's missing `.crack-3` CSS
  fixed.

### Systemic, cross-cutting

- [ ] **Timer/sound/format boilerplate duplicated across the 6 timed-arcade
      games** (WhackAMole, CatchGame, FruitSlice, BubblePop, ReactionTimer,
      RedLightGreenLight): the `System.Threading.Timer` tick-loop
      (`TickMs = 100`, start/stop/tick), `FormatTime`, the lazy-JS-module
      `PlaySoundAsync` pattern, and the identical streak-multiplier formula
      `1 + Math.Min(streak / 5, 3)` are copy-pasted across 4-5 of these
      files. Worth extracting as shared helpers. **Narrower than the
      original "shared `TimedTargetSpawner`" idea in Section 3 above** -
      only FruitSlice and BubblePop are actually close enough in shape
      (free random-XY spawn, `Task.Delay` expiry, CSS-animation-driven
      motion) to share a spawner; WhackAMole (fixed grid slots) and
      CatchGame (lane + fall-position) are geometry-specific and don't
      generalize cleanly. ReactionTimer/RedLightGreenLight don't spawn
      targets at all. Build the narrow spawner for FruitSlice/BubblePop
      only, and the timer/sound/format helpers for all six.
- [ ] **Only one game (UnoGame) actually uses the `Components/Shared/*`
      components** (`GameSetupPanel`, `GameHud`, `GameOverlay`, etc.) -
      every other game hand-rolls equivalent markup against the same CSS
      classes those components wrap. This is the same tension `ROADMAP.md`
      item 1 ("Shared UI primitives") already flags; worth resolving that
      open question (migrate everyone to the components, or accept
      hand-rolling as the real convention and consider removing the mostly
      unused shared components) rather than letting new games keep picking
      one pattern arbitrarily.

### Missing progression & rewards

The personal-best system now exists and covers the 5 games explicitly
flagged in the original audit. Not yet wired into any other game -
opportunistic candidates if a natural "best X" metric presents itself
(e.g. Minesweeper fastest-clear per difficulty, Sudoku fastest-solve).

### Per-game polish (smaller items)

- [ ] **FruitSlice** — tap targets are only as big as the rendered emoji
      glyph (~26-42px, under the ~44px touch guideline) on a moving arc;
      the most touch-unfriendly hitbox found in the audit.
- [ ] **FruitSlice, BubblePop, PopAndSparkle** — spawn position is fully
      random with no collision avoidance, so items can stack/overlap,
      worst at Fruit Slice's Hard tier (`MaxActive: 8`).
- [ ] **BubblePop** — a wrong-symbol pop and a correct pop look identical
      (both just fade/scale out); only the sound tells them apart, which
      fails silently for a kid playing with sound off.
- [ ] **CatchGame** — caught items just vanish instantly, no catch
      animation, unlike FruitSlice's slice-fade or BubblePop's pop-fade.
- [ ] **ConnectFour** — no drop-preview ghost disc before a column tap, and
      discs appear instantly with no drop/bounce animation despite the CSS
      transition already being wired up but never triggered.
- [ ] **TicTacToe** — no cell-placement animation and no winning-line
      highlight; the result overlay is the only payoff.
- [ ] **UnoGame** — the mechanically richest game here has the least
      reactive visuals: no card-flight animation, no opponent reaction to
      Skip/Reverse/Draw landing on them. Also the only card/board game in
      the app with no difficulty selector at all - may be intentional given
      its simplified greedy bots, but worth a deliberate decision either way.
- [ ] **WordSearch** — category isn't deduped across the 3 grids in a
      session, so the same theme can repeat back-to-back; the word-length
      filter (`w.Length <= size - 1`) is stricter than necessary and
      excludes words that would actually fit exactly along one edge.
- [ ] **Sudoku** — puzzle generation runs synchronously on the UI thread and
      genuinely freezes the tab; the "Generating…" message is static text
      with no spinner/pulse, which risks reading as broken on a slow tablet.
- [ ] **TowerDefense** — selecting an unaffordable tower gives no feedback
      when tapping a cell (silent no-op); range preview is explicitly
      mouse-only per its own header comment, so touch users can't preview
      range before spending gold, a real gap in an otherwise touch-first app.
- [ ] **DressUpGame** — the action menu has no click-outside-to-close;
      "Save Picture" downloads a PNG with no in-app gallery to browse past
      looks, despite being the most art-heavy game in the catalog.
- [ ] **FishingGame** — hand-rolls setup/HUD markup instead of using
      `Components/Shared/*`, and Letters/Numbers mode always runs the full
      fixed-order 26/20-item sequence with no shorter tier, unlike
      MemoryMatchGame's Easy/Medium/Hard pair-count options.
- [ ] **MannersGarden** — the Achoo lesson's drop target is an invisible
      `<div>` with no visual outline, so a kid dragging the elbow up has no
      on-screen cue where to drop it beyond trial-and-error snap-back.
- [ ] **PeekReveal** — Curtain and Present Box themes are the same
      `SingleReveal` mechanic with zero distinguishing animation, effectively
      3 mechanics wearing 4 labels.
- [ ] **BabyPiano** — "Piano" and "Drums" themes only swap which sound
      function fires; the key grid itself never changes, so "Drums" has no
      visual identity, audio-only theming.
- [ ] **MagicGarden** — the only Section 0 game with no theme/reskin choice,
      inconsistent with its four siblings (PeekReveal, SoundButtons,
      PopAndSparkle, BabyPiano all offer one) and a cheap missed
      replay-variety win.
- [ ] **ShapeSorter/ShadowMatch** — the six shape `clip-path` definitions
      are copy-pasted verbatim between the two files' `.razor.css`; any
      future outline tweak has to be made twice or the shapes will drift.

### What's already solid (no action needed)

Worth naming so it doesn't get re-litigated: SoundButtons, NumberSequence,
and ShadowMatch had no real findings beyond the shared items above. Word
Search's drag-select math (direction-locking, `dragRect` null-safety) and
Minesweeper/Sudoku's core puzzle-generation logic were all traced carefully
and are correct. TicTacToe/ConnectFour's use of `Services/GameAi` is
correct and the three difficulty tiers are genuinely distinct in strength.
RedLightGreenLight and CatchGame both have real, meaningfully-scaling
difficulty curves worth using as the model for other balance work.
