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
      instead of full chess.
- [ ] Checkers with AI — straightforward rules and strong difficulty-level
      potential.
- [ ] Reversi / Othello — simple rules, deep strategy, good AI-opponent
      candidate.
- [ ] Battleships — place ships, fire at coordinates, and play against AI or
      another local player.
- [ ] Mastermind — guess a hidden sequence of colors or symbols using feedback
      after each attempt.
- [ ] Logic Grid Puzzles — deduction clues such as "Alex has the red bike,
      Sam doesn't own the cat."
- [ ] Nonograms / Picross — number clues reveal a hidden pixel image; strong
      replayability from generated or data-driven puzzles.
- [ ] 2048-style Puzzle — slide matching tiles together and aim for larger
      values.
- [ ] Threes-style Number Puzzle — similar sliding-number category, with a
      distinct rule set.
- [ ] Hex / Territory Capture — players compete to control board sections by
      placing tiles.
- [ ] Dots and Boxes — simple local multiplayer with more strategy than it
      first appears.
- [ ] Ultimate Tic-Tac-Toe — nine Tic-Tac-Toe boards arranged inside one larger
      board.
- [ ] Sequence Puzzle — memorize increasingly long visual, audio, or
      directional sequences.
- [ ] Code Breaker — guess a numeric or symbol password using clues such as
      "2 digits correct, 1 in the right position."

### Mystery, escape, and code puzzles

- [ ] Escape Room — a small interactive room with clues, codes, switches,
      hidden objects, and puzzles.
- [ ] Detective Mystery — read clues, inspect suspects, identify
      contradictions, and solve a fictional case.
- [ ] Mystery Mansion — explore rooms, collect clues, unlock areas, and solve
      a central mystery.
- [ ] Treasure Map Puzzle — interpret riddles and map clues to locate hidden
      treasure.
- [ ] Cryptogram — decode substitution ciphers, symbols, or secret messages.
- [ ] Morse Code Challenge — decode or transmit simple messages under
      increasing difficulty.
- [ ] Programming Puzzle Game — issue commands like Move, Turn, Jump, Repeat
      to guide a robot through levels.
- [ ] Circuit Builder — connect batteries, switches, lights, motors, and logic
      gates to complete objectives.
- [ ] Factory Automation Puzzle — place conveyors, sorters, machines, and
      switches to route items correctly.

### Physics, sports, and skill challenges

- [ ] Bridge Builder — build a structure with limited pieces and test whether
      vehicles can cross it.
- [ ] Physics Puzzle — place ramps, blocks, springs, fans, or magnets to get
      an object to a target.
- [ ] Marble Run — build tracks and obstacles that guide a marble to the
      finish.
- [ ] Mini Golf — angle, power, and obstacles with strong level-design
      potential.
- [ ] Pool / Billiards Lite — physics, angles, and trick-shot challenges.
- [ ] Archery Challenge — changing distance, wind, moving targets, and scoring
      rings.
- [ ] Basketball Shot Game — angle and power control with moving hoops and
      harder shots.
- [ ] Penalty Shootout — choose direction and power while an AI goalkeeper
      reacts.
- [ ] Air Hockey — fast local multiplayer or player-vs-AI.
- [ ] Dodgeball Arena — move around an arena, dodge projectiles, and throw
      them back.

### Arcade, racing, and platforming

- [ ] Asteroids-style Space Game — fly a ship, shoot asteroids, collect
      upgrades, and survive waves.
- [ ] Space Trader — buy goods cheaply on one planet, sell elsewhere, upgrade
      the ship, and avoid hazards.
- [ ] Space Mining Game — mine resources, manage cargo, upgrade equipment, and
      explore deeper areas.
- [ ] Top-Down Racing — tracks, AI opponents, boosts, shortcuts, and lap
      times.
- [ ] Time Trial Racer — focus on personal records and ghost runs instead of
      AI opponents.
- [ ] Drift Challenge — score points by maintaining controlled slides around
      corners.
- [ ] Endless Survival Runner — upgrades, branching paths, hazards, missions,
      and unlockables.
- [ ] Platformer — handcrafted levels with jumping, collectibles, enemies,
      moving platforms, and checkpoints.
- [ ] Precision Platformer — short, difficult levels focused on mastering
      movement.
- [ ] Grappling Hook Game — swing between platforms using momentum.
- [ ] Ninja Wall Jump — climb vertically by bouncing between walls while
      avoiding hazards.
- [ ] Stealth Game — avoid guards and cameras, distract enemies, grab an
      objective, and escape.

### Spatial and rhythm puzzles

- [ ] Laser Maze — rotate mirrors to direct a laser beam toward a target.
- [ ] Portal Puzzle — place linked portals or teleporters to move objects
      through a level.
- [ ] Gravity Switch — flip gravity to navigate puzzles and avoid obstacles.
- [ ] Light and Shadow Puzzle — move lights or objects to create specific
      shadow shapes.
- [ ] Rhythm Game — hit keys or taps in time with music; difficulty and combo
      systems fit this age range well.

### Educational challenge games

- [ ] Typing Racer — type words accurately to move a car or character forward.
- [ ] Typing Defense — enemies approach carrying words; type the word to
      destroy them.
- [ ] Trivia Battle — categories, streaks, lifelines, difficulty levels, and
      multiplayer.
- [ ] Geography Challenge — identify countries, capitals, flags, landmarks, or
      locations on a map.
- [ ] Flag Guessing Game — expandable with many countries and regions.
- [ ] Periodic Table Challenge — timed matching with symbols, atomic numbers,
      and element categories.
- [ ] Vocabulary Duel — definitions, synonyms, antonyms, spelling, and
      word-building challenges.
- [ ] Word Ladder — change one letter at a time to transform one word into
      another.
- [ ] Boggle-style Word Hunt — find connected words in a letter grid before
      time runs out.
- [ ] Anagram Battle — players race to create words from the same letters.
- [ ] Trivia Survival — wrong answers cost health; see how far the player can
      progress.
- [ ] Quiz RPG — correct answers power attacks against monsters.
- [ ] Fake News Detective — use fictional posts/articles to identify
      suspicious clues, weak sources, and manipulative headlines.

### Simulations, management, and collection

- [ ] Stock Market Simulator — fictional companies and generated events; an
      introduction to risk without real money.
- [ ] City Builder Lite — roads, houses, shops, parks, power, and a few
      resources without becoming Spreadsheet Simulator 2026.
- [ ] Colony Survival — manage food, shelter, population, and resources while
      random events happen.
- [ ] Restaurant Manager — take orders, cook items, manage upgrades, and keep
      customers happy.
- [ ] Shop Simulator — buy inventory, set prices, serve customers, and expand
      the store.
- [ ] Theme Park Builder Lite — rides, food stalls, paths, decorations, money,
      and visitor happiness.
- [ ] Fishing Game — different fish, equipment, rarity, timing mechanics, and
      upgrades.
- [ ] Creature Collector — find creatures with stats/rarities and use them in
      simple battles.
- [ ] Monster Fusion — combine creatures to create new ones with inherited
      traits.
- [ ] Character Builder — equipment, stats, classes, abilities, and cosmetic
      customization.

### Creation tools

- [ ] Pixel Art Challenge — prompts or templates with a limited palette.
- [ ] Level Creator — create a maze, platformer, or puzzle level and play it.
- [ ] Music Sequencer — place beats, drums, melodies, and effects on a
      timeline.
- [ ] Animation Studio — place characters, move them frame-by-frame, and
      create short animations.

## 10. Existing-game improvement backlog

Captured 2026-08-14 via a full read-through of every `Components/*.razor`
game (+ matching `.razor.css`) after Sections 0-4 landed — a retrospective
pass, not a new-game pass. This is a documentation-only audit: nothing below
has been fixed yet, it's a prioritized punch list. Re-triage as items land;
delete a bullet once it's actually fixed rather than checking it off, since
most of these aren't a single-line change.

### Bugs to fix first

These are real, reproducible defects, not polish:

- [ ] **PatternComplete — the "pattern" never has to be understood.**
      `correctSymbol = activeSet[(displayLen - 1) % cycleLength]` reduces to
      `activeSet[0]` on every round at every difficulty (`displayLen =
      cycleLength * 2 + 1`, so `(displayLen-1) % cycleLength` is always 0).
      The right answer is always identical to the first tile shown - a kid
      can score 100% by copying the leftmost tile and ignoring the sequence
      entirely.
- [ ] **DressUpGame — "Dress Another" can crash the tab.** The button sets
      `celebrate = false; selectedCharacter = null;` directly instead of
      going through `ChangeCharacter()`/`PushHud()`, leaving `HudSlot`
      holding a stale fragment that dereferences `selectedCharacter!.Name`.
      The next unrelated re-render while back on character-select throws a
      `NullReferenceException`.
- [ ] **RedLightGreenLight — penalty can stack from one rapid tap burst.**
      `Move()` has no debounce while `caughtFlash` is true, so mashing
      during/right after getting caught applies multiple `strikes++` and
      progress penalties for what reads to the kid as one mistake.
- [ ] **WhackAMole — hitting a mole can immediately count as a miss.**
      During the 320ms post-hit "hop out" animation the hole is still
      `Up` with `Bonked = true`; a second tap in that window (very likely
      from an excited kid) falls through to the miss branch and resets the
      streak right after a successful hit.
- [ ] **MannersGarden — drag can get stuck "on."** `StartDragToy`/
      `StartDragElbow` await two `getBoundingRect` calls + `setPointerCapture`
      *before* setting the dragging flag, so a fast tap-release during that
      window can leave the flag set with no matching pointer-up to clear it.
      `DressUpGame` guards this exact race elsewhere in the same codebase
      (`if (pinchingSticker is not null) return;` after every await) - this
      file doesn't have the equivalent check.
- [ ] **TankDuel — same stuck-drag race as MannersGarden**, in
      `OnFieldPointerDown`: a very fast tap-release can leave `isDragging`
      stuck true with the aim-preview trajectory active until the next real
      pointerdown overwrites it.
- [ ] **TowerDefense — `Tick`'s timer callback has no re-entrancy guard.**
      If a tick's cumulative `await PlaySoundAsync(...)` calls (one per tower
      firing that tick) run past the 100ms `TickMs` interval, the next timer
      callback can start while the previous one is still suspended, both
      mutating the same `enemies`/`towers`/`gold` state. Latent, more likely
      under a full board of towers firing simultaneously.
- [ ] **MemoryMatchGame — `FlipCard` is `async void`**, not `async Task`
      like every other async handler in the codebase. An exception inside it
      (e.g. a JS interop failure) bypasses Blazor's normal event-handler
      error handling instead of failing gracefully.

### Systemic, cross-cutting

One pattern, many files - worth fixing once with a shared approach rather
than N one-off patches:

- [ ] **Abandoning a game mid-session never logs to play history.** Every
      single game's "← Exit"/"Change Game" button resets straight to the
      setup screen without invoking `OnComplete`. Confirmed present in
      literally every game audited: OddOneOut, PatternComplete,
      NumberSequence, ColorMatch, ShapeSorter, ShadowMatch, SimonSays,
      TicTacToe, ConnectFour, RockPaperScissors, HigherOrLower, UnoGame,
      WhackAMole, CatchGame, FruitSlice, BubblePop, ReactionTimer,
      RedLightGreenLight, WordScramble, GuessTheWord, WordSearch,
      SlidingPuzzle, Minesweeper, Sudoku, MemoryMatchGame, FishingGame,
      DressUpGame, MannersGarden, TankDuel, TowerDefense. A parent looking
      at play history has no idea how much a kid actually played beyond
      completed sessions. Worth a shared convention (e.g. an
      `OnComplete`-with-an-`Abandoned`-flag call, or a lightweight
      `GameSessionService` per the "Shared game systems" list above) rather
      than editing 29 files by hand.
- [ ] **`.game-choice-locked`/`.game-stat` get reimplemented per-game
      instead of reused.** `app.css` already has these classes, but
      WhackAMole, CatchGame, FruitSlice, BubblePop, ReactionTimer,
      RedLightGreenLight, WordScramble, SlidingPuzzle, Minesweeper, Sudoku,
      PeekReveal, SoundButtons, PopAndSparkle, BabyPiano, and MagicGarden
      all define byte-for-byte (or near-identical) duplicates in their own
      `.razor.css` (`.wa-difficulty-locked`, `.cg-difficulty-locked`, etc.)
      even though their markup could just apply the shared class directly.
      Cheap, low-risk cleanup, no behavior change.
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
- [ ] **No audio at all in Sudoku, Minesweeper, SlidingPuzzle, or
      WordScramble** - no `IJSRuntime` even injected in any of the four.
      Minesweeper is the starkest miss: the mine explosion and win state
      both have zero sound. WordScramble already has dedicated shake/solve
      animation states a sound would pair naturally with.
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

The single most-repeated "fun pass" gap across the whole audit: almost
nothing persists a record of a strong run.

- [ ] No persisted best time/fewest moves per theme+difficulty in
      MemoryMatchGame, despite already tracking both stats every round.
- [ ] No persisted fastest-clear/fewest-attempts record in FishingGame.
- [ ] No persisted best-level-reached in TankDuel (`StartNewCampaign()`
      always resets to level 1) or TowerDefense (best-level/total-kills/
      unlocked starting bonus all absent, despite being the most
      mechanically complete game in the app).
- [ ] GuessTheWord tracks `streak`/`bestStreak` but never uses either for
      anything (badge, cosmetic, unlock) - the fun-pass checklist's "what's
      unlockable" box is effectively unaddressed despite this game being
      designed against that checklist.
- [ ] MannersGarden's persisted cross-session Garden
      (`AppDataService.GetGardenItemsAsync`/`AddGardenItemAsync`) is the one
      genuine progression/collection hook in the whole app today - the
      model other games above should copy, not reinvent differently each
      time.
- `kenney_medals.zip` (see "Downloaded asset fit" above) is the art half of
  a fix here - flat medal/ribbon icons ready to use as the visual reward
  once a shared "personal best" or achievement concept exists. Pairs
  naturally with the `GameSessionService`/achievements idea already listed
  under "Shared game systems".

### Balance

- [ ] **HigherOrLower's difficulty is backwards.** "Hard" narrows the range
      to 1-8, which makes ties (which never break a streak) *more* common,
      not less - the tightest range is the most forgiving one, opposite of
      what the label promises.
- [ ] **RockPaperScissors "Hard" AI is exploitable.** Both Medium and Hard
      counter the player's *all-time* most-frequent choice (gated at 50%
      vs. 100% probability), with no recency weighting. A kid who baits it
      by throwing one choice a few times then switching can farm wins
      against "Hard" once they notice the tell.
- [ ] **ReactionTimer's "Hard" has a longer max wait than "Easy."**
      Hard's delay range (700-3600ms) tops out higher than Easy's
      (1500-3000ms) - the tiers differ in unpredictability more than in
      being straightforwardly faster, which doesn't match the label.
- [ ] **FishingGame's Letters/Numbers mode is likely near-unwinnable.**
      `MaxStrikes = 3` for the whole session, but Letters/Numbers require
      clearing a fixed 26- or 20-item sequence (`WinCount = 3` per item) -
      3 total misses across 60-78 catches is a very tight budget compared
      to Colors mode's 3 misses across 18 catches.
- [ ] **TankDuel's CPU difficulty plateaus around level 9** (`spread =
      Math.Max(0.06, 0.3 - (level-1)*0.03)`, obstacle height caps at level
      7) - a skilled player stops seeing new challenge well before they'd
      stop wanting to replay.
- [ ] **Minesweeper's mine density barely changes across tiers** (~15.6% /
      16% / 16.7%) - difficulty comes almost entirely from grid size, not
      genuinely different risk/reasoning demands.
- [ ] **ColorMatch's Hard palette has a near-duplicate pair** - Violet
      (#6A0DAD) and Easy's Purple (#9D4EDD) sit in the same hue family and
      can read as "the same purple" to a kid going by hue rather than exact
      shade, undercutting the palette's own difficulty-via-similarity
      design.
- [ ] **OddOneOut's odd-tile visual distinctness isn't tied to
      difficulty** - only grid size scales; the base/odd pair is drawn fully
      at random, so a "Hard" round can draw an obvious pair while "Easy"
      draws a subtle one.
- [ ] **SlidingPuzzle's always-visible tile numbers flatten the curve** -
      combined with the picture-preview toggle, the puzzle is largely
      solvable by numeric bookkeeping alone; a Hard 5x5 is "more numbers,"
      not more spatially demanding.

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
- [ ] **WordSearch** — a released drag that matches nothing gets zero
      feedback (no sound/shake), unlike GuessTheWord's `playMismatchSound`
      on a wrong letter; category isn't deduped across the 3 grids in a
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
- [ ] **TankDuel** — `TankDuelResult` doesn't carry the level reached, so
      play history/any future high-score UI can't show campaign progress.
- [ ] **PeekReveal** — Curtain and Present Box themes are the same
      `SingleReveal` mechanic with zero distinguishing animation, effectively
      3 mechanics wearing 4 labels; the crack-animation CSS stops at
      `.crack-2` with no `.crack-3` even though `EggCracksNeeded = 3`.
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
difficulty curves worth using as the model for the balance fixes above.
