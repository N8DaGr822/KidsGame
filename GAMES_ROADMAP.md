# New Games Roadmap

Active backlog for games that are not built yet. Completed games live in
`COMPLETED_GAMES.md`, asset notes live in `ASSET_TRIAGE.md`, and polish work
for shipped games lives in `GAME_POLISH_BACKLOG.md`.

## How a new built-in game gets wired in

1. **Component** - `Components/<GameName>.razor`, plus a matching isolated CSS
   file when needed. Keep the component focused on the game's actual mechanic.
2. **Registry** - add a `LaunchTarget` const and `BuiltInGame` entry in
   `Services/BuiltInGames.cs`.
3. **Host wiring** - add a branch in `Pages/GameHost.razor`, plus a
   `Handle<GameName>Complete` method that writes a `PlayHistoryEntry`.
4. **Admin support** - if the game has difficulty tiers, add it to the
   parent-lockable difficulty list.
5. **Game polish** - run the fun-pass checklist below before calling it done.

## Shared game systems

Avoid building every game as a fully isolated application. New games should own
their mechanics, but reuse shared systems for the common shell around them.

Candidate shared systems:

- `GameSession` / `GameSessionService` - score, elapsed time, pause state,
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
elapsed time, pause state, and game-over handling. Each game should only
implement what makes it mechanically distinct.

## Fun pass checklist

Before implementing a new game, give it a quick design pass so it does not land
as a technically correct but visually flat mechanic.

- What is the fantasy? A garden, dungeon, space mission, toy box, mystery,
  tournament, workshop, aquarium, etc.
- What moves on screen? Static boards need animated pieces, reveals, effects,
  timers, progress, or character reactions.
- What sound/animation rewards the main action? Taps, matches, hits, solves,
  upgrades, and mistakes should all feel intentional.
- What can the player unlock? New levels, skins, characters, stickers, towers,
  songs, puzzles, creatures, decorations, badges, or tools.
- What makes a replay different? Randomized layouts, puzzle packs, upgrade
  drafts, AI personalities, daily challenges, optional goals, or player-made
  content.
- What is the smallest version that still feels complete? Prefer a polished
  vertical slice over a large unfinished system.

Age-specific expectations:

- Toddler games should feel like interactive toys: giant tap targets, slow
  movement, gentle audio, no failure state, and immediate cause/effect.
- Younger-kid games should use collectible rewards, friendly characters, bright
  motion, and forgiving difficulty.
- Older-kid games should add strategy, progression, mastery, records, optional
  challenge, and meaningful choices without manipulative grind.

## Priority

Current recommendation:

1. **Shared systems first** - timer/sound/session helpers are now the biggest
   code-health multiplier.
2. **Toddler physical/sensory games** - small, visual, and quick to ship.
3. **Remaining zero/low-asset puzzles** - Jigsaw, Quick Math, Math Target.
4. **RPG/strategy vertical slice** - Dungeon Crawler and Mini RPG Battle
   both shipped; Roguelike Arena next if this category continues, now
   that usable CraftPix RPG assets exist.
5. **Creation tools and simulations** - defer until save/editing
   infrastructure exists.

## Toddler Cause-And-Effect Games (~1 Year Old)

Design for exploration, not correctness. Every interaction should be valid,
obvious, forgiving, and pleasant. Use huge targets, minimal text, slow
movement, low visual clutter, soft sounds, and short animations. Avoid fail
states, countdowns, score pressure, reading requirements, precision dragging,
and multi-step rules.

### Peek/reveal

- [x] Where Did It Go? - an object slowly hides behind something; tap the
  hiding place to reveal it. (Covered by `PeekReveal.razor`'s "Egg
  Surprise" theme - already shipped, just not checked off here.)

### Big tap targets and sensory feedback

- [x] Lights On / Lights Off - tap lamps, switches, stars, or windows to toggle
  them on and off.
- [x] Big / Small - tap a tiny object and it becomes huge; tap again and it
  shrinks.
- [x] Color Splash - every screen tap produces a large blob of color with a
  soft sound or animation.
- [x] Finger Trails - drag a finger and leave stars, bubbles, snowflakes, paint,
  or sparkles behind.
- [x] Fireworks Touch - tap anywhere for a soft colorful burst; keep it gentle,
  not realistic or loud.
- [ ] Spin the Wheel - swipe or tap a large wheel, fan, pinwheel, or carousel
  and watch it spin.

### Animals, body, and sound

- [x] Wake Up the Animals - sleeping animals wake, stretch, make a sound, then
  eventually fall asleep again. (`SleepyAnimals.razor`'s "Wake Up" mode.)
- [ ] Funny Faces - tap parts of a face; nose honks, ears wiggle, eyes blink,
  tongue pops out.
- [ ] Touch the Body Part - a friendly character highlights "nose", "hand",
  "foot", etc.; exploratory rather than a correct-answer quiz.
- [x] Bedtime Animals - tap animals to tuck them into bed, turn off the lamp,
  and hear a tiny sleepy sound. (`SleepyAnimals.razor`'s "Bedtime" mode.)

### Gentle worlds

- [x] Touch the Fish - fish swim slowly around an aquarium; touching one makes
  it wiggle, bubble, or swim away. (`GentleCreatures.razor`'s "Fish Tank" theme.)
- [ ] Make It Rain - tap clouds for rain, then sunshine, rainbows, puddles,
  etc.
- [x] Baby Aquarium - mostly passive fish, bubbles, and plants; touching
  anything causes a small response. (Same "Fish Tank" theme as Touch the
  Fish - identical roadmap idea, one component.)
- [x] Night Sky - tap the dark sky to add stars; tap stars to make them
  twinkle.
- [ ] Snow Day - tap to make snow fall, create footprints, or reveal objects
  beneath snow.
- [ ] Puddle Splash - tap puddles and watch a character jump into them.
- [x] Follow the Butterfly - a butterfly moves slowly around; touching it makes
  it flutter somewhere else. (`GentleCreatures.razor`'s "Follow the
  Butterfly" theme.)

### Food, music, and physical play

- [ ] Bath Time - tap bubbles, splash water, squeak a rubber duck, or pour
  water from a cup.
- [ ] Stack the Blocks - extremely forgiving drag-and-drop blocks that snap
  together automatically.
- [ ] Knock It Down - start with a block tower; tap it and everything tumbles,
  then automatically rebuilds.
- [ ] Roll the Ball - swipe or tap a large ball and watch it roll, bounce, or
  knock over objects.

## Real-Time Game-Loop Arcade

These need a continuous loop, collision model, and responsive controls.

- [ ] Snake
- [ ] Breakout
- [ ] Pong
- [ ] Maze Escape
- [ ] Frogger-style Crossing Game
- [ ] Endless Runner

## Creative / Sandbox Games

These need reusable editing/save infrastructure before they are worth building
one by one.

- [ ] Pet Care Game
- [ ] Build-a-Monster
- [ ] Coloring Book
- [ ] Pixel Art Creator
- [ ] Story Builder

## Puzzle / Math

- [ ] Jigsaw Puzzle
- [ ] Quick Math
- [ ] Math Target
- [ ] Spot the Difference

## Older-Kid Strategy / RPG Systems (~10-15)

These assume longer sessions, more reading, planning, upgrades, and save state.
Start with small vertical slices.

- [ ] Roguelike Arena
- [ ] Card Battle Game
- [ ] Auto Battler

## Older-Kid Board, Logic, And Deduction

- [ ] Logic Grid Puzzles
- [ ] Nonograms / Picross
- [ ] Threes-style Number Puzzle
- [ ] Hex / Territory Capture
- [ ] Ultimate Tic-Tac-Toe
- [ ] Sequence Puzzle

## Mystery, Escape, And Code Puzzles

- [ ] Escape Room
- [ ] Detective Mystery
- [ ] Mystery Mansion
- [ ] Treasure Map Puzzle
- [ ] Circuit Builder
- [ ] Factory Automation Puzzle

## Physics, Sports, And Skill Challenges

- [ ] Bridge Builder
- [ ] Physics Puzzle
- [ ] Marble Run
- [ ] Dodgeball Arena

## Arcade, Racing, And Platforming

- [ ] Platformer / Precision Platformer / Ninja Wall Jump
- [ ] Space Trader
- [ ] Space Mining Game
- [ ] Endless Survival Runner
- [ ] Grappling Hook Game
- [ ] Stealth Game

## Spatial And Rhythm Puzzles

- [ ] Portal Puzzle
- [ ] Gravity Switch
- [ ] Light and Shadow Puzzle

## Educational Challenge Games

- [ ] Typing Racer
- [ ] Typing Defense
- [ ] Geography Challenge
- [ ] Periodic Table Challenge
- [ ] Vocabulary Duel
- [ ] Boggle-style Word Hunt
- [ ] Anagram Battle
- [ ] Trivia Survival
- [ ] Quiz RPG
- [ ] Fake News Detective

## Simulation, Management, And Collection

These need persistent economy/progression infrastructure.

- [ ] Stock Market Simulator
- [ ] City Builder Lite
- [ ] Colony Survival
- [ ] Restaurant Manager
- [ ] Shop Simulator
- [ ] Theme Park Builder Lite
- [ ] Fishing Game with equipment/rarity/upgrades
- [ ] Creature Collector
- [ ] Monster Fusion
- [ ] Character Builder

## Creation Tools

These are editor products, not simple round-based games. Each needs save/load,
editing UI, and replay/export decisions.

- [ ] Pixel Art Challenge
- [ ] Level Creator
- [ ] Music Sequencer
- [ ] Animation Studio
