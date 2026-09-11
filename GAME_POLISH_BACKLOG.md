# Game Polish Backlog

Active improvement work for games that already exist. New games belong in
`GAMES_ROADMAP.md`; completed games belong in `COMPLETED_GAMES.md`.

Section "P1" below folds in every finding from the catalog-wide
`GAME_EVALUATION.md` review (2026-09-10, no application code changed by that
review). Its per-game tables are reconciled into the sections that follow;
items it found already resolved are noted under "Backlog corrections" rather
than re-listed as open work. `GAME_EVALUATION.md` itself stays in the repo as
the fuller source document (design rationale, tradeoffs, validation notes) if
more context is needed than the terse bullets here.

## P1 - Correctness & Reliability

Suggested order: this group first (trust across many games), then the rules
items below it, then Systemic/Cross-Cutting, then per-game polish.

- [ ] Stale delayed gameplay survives exit/restart. `AutoBattler`'s
  `RunBattleLoop`/`Strike`, `AnimalSoundGuessingGame`'s round-advance delay,
  and `ChessGame`'s delayed CPU move all keep running after Exit or a
  restart because they check `phase` (if anything) rather than being
  cancelled. Give each run a cancellation token + generation ID; invalidate
  on exit, restart, and disposal; recheck the captured generation after
  every await; make completion idempotent. `MemoryMatchGame`'s mismatch
  resolution has the identical shape (delayed callback dereferences
  `firstPick`/`secondPick`, which `StartGame` resets) - fix it alongside
  these three, then audit the rest of the catalog for the same pattern.
  Test: begin a delayed action, exit/restart, and assert no stale mutation
  or duplicate result.
- [ ] Reduced-motion mode disables essential gameplay motion, not just
  decoration. The global `* { animation: none !important; transition: none
  !important; }` rule in `wwwroot/css/app.css` stops Fruit Slice's toss and
  Bubble Pop's rise while their expiration logic keeps running underneath,
  and removes Follow the Cups' trackable shuffle transitions. Separate
  decorative effects from state-communicating motion; give
  `FruitSlice.razor.css`, `BubblePop.razor.css`, and `FollowTheCups.razor` a
  slower/simpler or static alternative instead of a blanket override.
  Verify each affected game with reduced motion enabled, not just that the
  override compiles.
- [ ] Bubble Pop penalizes ignoring distractors. `ExpireItemAsync` resets
  `streak` for every unpopped bubble, including symbols the player was
  told not to pop, and the target can switch while bubbles are still in
  flight (`Components/BubblePop.razor:275`). Reset streak only for a missed
  *eligible* target; decide explicitly whether eligibility is fixed at
  spawn or re-evaluated at expiration, and give a cue before reclassifying
  a live target.
- [ ] Word Ladder's instructions promise more than the validator accepts.
  The UI asks for any real word one letter away; `Submit` only accepts
  `chain[currentRungIndex]` (`Components/WordLadder.razor:259`). For the
  CAT -> DOG ladder, CAT -> COT -> DOT -> DOG is a valid real-word route,
  but DOT is rejected because the hidden chain requires COG. Smallest fix:
  make it explicitly clue-guided and clue each required word. Better:
  bundle a curated word graph and accept any valid route to the
  destination - do not just accept arbitrary one-letter-off strings.
- [ ] Chess castling rights are inferred from occupancy instead of tracked.
  A king or rook that moves away and returns to its starting square
  regains castling eligibility (`Services/ChessRules.cs:256`). En passant
  and underpromotion are also unimplemented. Track castling rights in
  position state and thread them through AI search and undo snapshots;
  test king/rook return moves and rook captures.
- [ ] The host's back arrow (and browser Back) can skip result logging.
  `GoBack` in `Pages/GameHost.razor` navigates immediately; disposal only
  logs iframe sessions, so leaving through the shared arrow rather than a
  game's own Finish/Exit handler can drop a `PlayHistoryEntry`. Add a
  once-only session-finalization contract shared by the host and
  components, capturing profile/game identity at session start and
  distinguishing completed from abandoned sessions. In-app navigation can
  finalize reliably; browser/tab close needs periodic checkpoints rather
  than relying on async disposal.
- [ ] Direct `/play/{id}` routes bypass the profile's allowed-game list.
  `GameSelect` filters through `GetGamesForProfileAsync`, but `GameHost`
  only checks that a profile exists before loading from
  `GetAllGamesAsync` (`Pages/GameHost.razor:486`). Resolve the game against
  the current profile's permitted list at the route boundary (use
  `OnParametersSetAsync`, since the routed component is reused across
  `GameId` changes); preserve admin access. This is application-level
  enforcement, not a tamper-resistant boundary for a client-only
  local-storage app - Lucky Spin Slots (18+ in the catalog) is the game
  this most matters for today.

## Systemic / Cross-Cutting

- [x] ~~Fix the latent JS interop disposal race shared by `PlaySoundAsync`-style
  helpers~~ - already resolved by the shared `InteropService` (confirmed in
  the 2026-09-10 review; see Backlog corrections below). Left checked off
  here instead of deleted so it doesn't get "rediscovered."
- [ ] Extract timer/sound/format boilerplate from timed arcade games:
  `WhackAMole`, `CatchGame`, `FruitSlice`, `BubblePop`, `ReactionTimer`, and
  `RedLightGreenLight`.
- [ ] Consider a narrow shared spawner only for games that actually match:
  `FruitSlice` and `BubblePop`. Do not force `WhackAMole`, `CatchGame`,
  `ReactionTimer`, or `RedLightGreenLight` into that shape.
- [ ] Shared pause/resume signal for active play, hidden tabs, and the
  daily-limit overlay - the overlay currently covers the game without
  stopping its logic underneath. Keep gameplay elapsed time separate from
  the parent's daily-usage policy. Start save/resume with Chess, Solitaire,
  Tower Defense, Card Battle, and the RPGs.
- [ ] Make learning visible catalog-wide: a short demonstration before the
  first round, progressive hints, specific feedback after a mistake, and a
  practice mode without speed pressure. For young children a spoken prompt
  plus a picture beats a setup paragraph.
- [ ] One dependable input vocabulary: tap-select/tap-place for boards,
  pull-back-to-shoot for aiming games, keyboard activation on pointer-only
  buttons (Baby Piano's keys are pointer-only today with no accessible
  name), accessible names on other unlabeled controls, and non-color
  identifiers wherever color is currently the only cue.
- [ ] Pointer-cancellation-fires-a-shot bug: Archery and Basketball wire
  `pointercancel` straight to the release handler, which can resolve and
  launch the current aim (`Components/ArcheryChallenge.razor:65`,
  `Components/BasketballShot.razor:59`). Use a separate cancel handler that
  clears aiming state without firing, and require the matching active
  pointer ID for move/up. Audit other drag-shot games for the same
  pattern. Test interrupted gestures and a second finger, not just mouse
  dragging.
- [ ] Best-score records mix incomparable modes: Tower Defense always
  writes under the shared `BuiltInGames.TowerDefense` `best-level` key,
  including the Cat Defense theme, with no difficulty in the key
  (`Components/TowerDefense.razor:1159`); Word Ladder uses one
  `fewest-mistakes` key across difficulties and ladders
  (`Components/WordLadder.razor:303`). Key records by game, difficulty, and
  ruleset; decide explicitly whether the two defense themes should share
  progress; plan a migration or a clearly labeled legacy bucket for
  existing records.
- [ ] Play-history data model is overloaded: `PlayHistoryEntry.Moves`
  represents moves, scores, placements, or score margins depending on the
  game, and `Difficulty` sometimes holds a result description instead of a
  difficulty. Add explicit outcome/metric fields incrementally; show the
  right units in reports; don't add scores to exploratory toddler toys.
- [ ] Timing correctness: many quiz/arcade games increment or decrement
  counters directly off timer callbacks instead of a monotonic elapsed-time
  source with explicit pause accounting (reaction, rhythm, racing, timed
  records). Rhythm should judge against the presented beat, not wall-clock
  drift. Reaction Timer should measure from presentation (not before
  render) and lock the input phase before the first awaited sound call so
  one cue can't accept repeated taps.
- [ ] Profile actual frame time on the intended tablet before restructuring
  rendering. Dense particle scenes and high-frequency pointer events are
  candidates for batching or a small canvas layer only if measurements
  justify it; keep turn-based boards idiomatic Blazor.
- [ ] Extend shared services narrowly rather than forcing every game into
  one big inheritance hierarchy: a small session-lifetime abstraction ties
  into the pause/resume and host-logging P1 items above. Extract pure rule
  engines (e.g. `ChessRules`) first where regression tests would
  meaningfully protect them.
- [ ] Sound-themed games don't deliver the named sound: Sound Buttons plays
  `playPianoNote(index)` for every theme (`Components/SoundButtons.razor:117`);
  Animal Sound Guessing speaks animal-noise *text* through speech synthesis
  instead of a real sound (`Components/AnimalSoundGuessingGame.razor:234`);
  Touch the Body Part plays a match sound but never speaks the
  requested/tapped part (`Components/TouchTheBodyPart.razor:74`), and
  should keep its exploratory feel without implying the requested part was
  the one actually tapped when it wasn't. Add a small local animal/vehicle
  sound bank; speak body-part prompts and responses; retain visible
  feedback when muted. Evaluate actual recognizability with children -
  local assets support offline use but add download size.
- [ ] Bundled browser games (Crown & Banner, Highway Hauler, Truck Repair
  Bay): add an explicit, origin-validated launcher message contract for
  profile initialization, pause, and results - the host currently only
  captures their mounted duration, not game-specific outcomes. The two
  truck games also load a Google font remotely; bundle it locally if their
  offline visual presentation should be dependable.

## Missing Progression / Rewards

Personal bests are wired into many games. The 2026-09-10 review confirmed
these are already present, so they're removed from the open list below:
SlidingPuzzle, WordScramble, WordSearch, CatchGame, ColorMatch, TicTacToe.

Still missing a natural metric:

- [ ] OddOneOut - high score or best streak.
- [ ] PatternComplete - high score or best streak.
- [ ] NumberSequence - high score or best streak.
- [ ] ColorMatch - color-name learning mode should track separately from
  hue-matching if/when it's added (see per-game item below).
- [ ] ShapeSorter - high score or best streak.
- [ ] ShadowMatch - high score or best streak.
- [ ] FruitSlice - high score.
- [ ] BubblePop - high score.
- [ ] RedLightGreenLight - high score / best streak.
- [ ] ConnectFour - consecutive win streak.
- [ ] RockPaperScissors - consecutive win streak.
- [ ] HigherOrLower - consecutive win streak.

## Per-Game Polish

- [ ] FruitSlice, BubblePop, PopAndSparkle - random spawn positions can
  overlap; add collision avoidance or lane/slot selection. Pop & Sparkle
  additionally wants a slower drift setting; Bubble Pop should clearly
  announce target changes (see its P1 above for the streak-penalty bug).
- [ ] WordSearch - avoid repeating the same category across the three grids
  in a session; relax the word-length filter so words can fit exactly
  along an edge (currently `<= size - 1`, should allow `<= size`); add
  tap-start/tap-end selection as a drag alternative.
- [ ] TowerDefense - tapping a cell with an unaffordable tower gives no
  feedback; touch users also need a way to preview tower range before
  spending gold. Longer term: checkpoint saves, and decide whether Cat
  Defense keeps separate records (see Systemic best-score-keying item).
- [ ] FishingGame - Letters/Numbers mode always runs the full fixed-order
  26/20-item sequence (78/60 correct catches to finish); add shorter,
  shuffled tiers.
- [x] ~~MannersGarden - the Achoo lesson's drop target is invisible; add a
  visual outline/cue~~ - done (dashed outline + pulsing background).
  Next: offer tap-to-complete alongside elbow dragging, and make the
  dragging gesture itself cancellation-safe against the same
  exit/restart pattern as the P1 items above. Expand scenario variety
  only after device testing confirms the interaction is clear.
- [ ] PeekReveal - Curtain and Present Box use the same `SingleReveal`
  animation; distinguish them visually.
- [ ] BabyPiano - Piano and Drums themes only change sound, not layout;
  give Drums a distinct drum-pad layout, add key labels and keyboard
  activation, and give simultaneous taps independent held-key feedback.
- [ ] MagicGarden - add at least one theme/reskin choice for replay
  variety; consider a seasonal garden option and an optional persistent
  picture of the child's garden. Keep it free of maintenance chores.
- [ ] ShapeSorter / ShadowMatch - dedupe the shared CSS `clip-path` shape
  definitions. Shape Sorter also wants a stronger selected-shape cue, a
  short demonstration, and spoken shape names; Shadow Match wants familiar
  object/animal outlines after the basic shapes, with rotation only as an
  explicit harder tier.
- [ ] ChessGame - user report (2026-08-21): Medium felt unbeatable across 3
  games as a self-described non-novice; hasn't tried Hard yet, still
  testing. Likely explanation, not yet confirmed as the fix: `Services/GameAi.cs`'s
  `ChessMove` runs real alpha-beta minimax at depth 2 for Medium (3 for
  Hard, `Difficulty.Easy` skips search entirely) with no blunder injection -
  Easy is the only tier that mixes in random moves
  (`difficulty == Difficulty.Easy || (Medium && 50% roll)` inside the other
  per-piece-type search helpers). A consistent, non-blundering 2-ply search
  can already punish an intermediate human's tactical mistakes, so "Medium"
  may be playing closer to "Hard" in practice. Revisit once more games
  (including a Hard comparison) confirm whether Medium needs an eval
  handicap or occasional deliberate mistakes, rather than assuming depth-2
  search is automatically "medium" difficulty for a human opponent. Also
  see the castling-rights P1 above; once that's fixed, preserve draws as
  their own outcome (currently mapped to "Lost" - `ChessGame.Outcome.Draw`
  exists but the result only carries `Won`), correct undo's check-status
  side, and add save/resume.

## Per-Game Polish - additional findings (GAME_EVALUATION.md, 2026-09-10)

Everything below is new from the catalog review and not yet represented
above. Grouped the same way the review grouped it; priority tags (P2/P3)
carried over from that document (P1s are all in the section at the top).

**Early exploration and creative play**

- [ ] Feed the Animal (P2) - make the selected food unmistakable and speak
  animal/food names; demonstrate the two-step pairing action once.
- [ ] Big & Small (P2) - speak "big" and "small" with the transformation;
  keep the target easy to hit at its smallest size.
- [ ] Color Splash (P3) - offer a simple palette and a keep-my-picture mode
  alongside the fading splashes.
- [ ] Finger Trails (P2) - track active pointer IDs and cap/batch
  particles; offer a persistent drawing mode. Measure touch responsiveness
  before changing rendering technology.
- [ ] Sleepy Animals (P2) - strengthen Bedtime's final quiet visual/audio
  cue and provide an obvious replay action after everyone sleeps.
- [ ] Gentle Creatures (P2) - prevent excessive creature overlap; let
  parents select creature speed/count for easier tracking.
- [ ] Night Sky (P3) - let children connect stars into simple
  constellations; show visible feedback when the star cap is reached.
- [ ] Fireworks Touch (P2) - add a calm burst style with lower flash
  intensity and fewer particles; make it the reduced-motion alternative
  (see the reduced-motion P1 above).
- [ ] Lights On, Lights Off (P3) - add a simple room scene where switches
  visibly control particular lamps; retain free exploration.
- [ ] Spin the Wheel (P2) - add an obvious slowdown/stop interaction and a
  calm presentation for reduced-motion users.
- [ ] Make It Rain (P3) - reinforce discoveries with short spoken weather
  words and clearer cause-to-effect animation.
- [ ] Snow Day (P2) - make snow mounds stand out as interactive; bound
  footprint accumulation and make clearing playful.
- [ ] Puddle Splash (P3) - give different puddle sizes distinct splash
  sounds for more variety on repeat taps.
- [ ] Bath Time (P2) - add a brief visual demonstration of the cup, duck,
  and bubbles; use a different recognizable sound for each.
- [ ] Funny Faces (P3) - add a small face/voice choice and optional spoken
  part names while preserving rapid repeat taps.
- [ ] Touch the Body Part (P2) - preserve exploration, but don't imply the
  requested answer was selected when it wasn't (see the sound-themed-games
  Systemic item above for the speech part of this).
- [ ] Stack the Blocks (P2) - clearly indicate the removable top block and
  make reaching the stack limit a small celebration rather than a dead
  tap.
- [ ] Knock It Down (P3) - vary the tower's silhouette and allow tapping to
  rebuild immediately after the collapse.
- [ ] Roll the Ball (P3) - offer a few ball/pin arrangements and distinct
  collision sounds; keep this a toddler toy, not a precision-aiming game.
- [ ] Dress Up (P2) - add optional clothing snap guides and a simpler
  starter tray for younger children; preserve free placement and the
  existing creative tools.
- [ ] Build-a-Monster (P2) - add randomize, naming, and a per-profile
  saved-monster gallery; reuse Dress Up's gallery experience where
  practical.

**Matching, memory, and early reasoning**

- [ ] Memory Match (P2) - add an optional short preview and spoken pair
  labels (the restart-cancellation bug for this game is in the P1 section
  above).
- [ ] Simon Says (P2) - add optional slower playback and a visible
  round-progress cue; offer a natural stopping milestone on endless Easy
  play.
- [ ] Copy the Pattern (P2) - add a forgiving Easy retry and adjustable
  viewing time; one wrong tap currently ends even the easiest run.
- [ ] Memory Sequence Adventure (P3) - give milestones different
  destinations/scenes so progress feels like an adventure beyond the
  reskin of Simon's sequence mechanic.
- [ ] Follow the Cups (P2) - offer one slow demonstration shuffle before
  scored rounds (its reduced-motion trackable-swaps bug is in the P1
  section above).
- [ ] Odd One Out (P2) - add controlled differences in size, orientation,
  and shape; avoid making difficulty depend only on more tiles.
- [ ] Pattern Complete (P2) - add patterns such as AAB and ABB, then
  briefly highlight the repeating unit after an answer.
- [ ] Number Sequence (P2) - explain the step visually after each answer,
  with an optional number line on Easy.
- [ ] Color Match (P2) - tune palettes on the intended screens and add a
  color-name learning mode; keep any symbol-assisted mode separate since
  symbols change the skill being assessed.
- [ ] Hot and Cold (P2) - add a clear temperature legend and distance
  explanation; pair color with symbols/text.
- [ ] Treasure Hunt (P2) - show available adjacent moves and explain
  distance feedback visually; add small themed maps once the basic search
  is understood.

**Word, knowledge, and math games**

- [ ] Word Scramble (P2) - add spoken words, a first-letter hint, and
  minimal correction that preserves useful work instead of clearing every
  letter.
- [ ] Guess the Word (P2) - add category/pronunciation help and adaptive
  hints before fuel is exhausted; show the completed word long enough to
  learn it.
- [ ] Cryptogram (P2) - add a frequency view, a substitution reference, and
  undo; offer optional word-level hints without turning each letter into
  an answer checker.
- [ ] Morse Code Challenge (P2) - play actual dot/dash timing on demand and
  let a wrong word be edited without erasing all progress.
- [ ] Trivia Battle (P2) - add a one-sentence explanation after answers and
  track recently seen questions per profile to reduce repetition.
- [ ] Flag Guessing Game (P2) - use bundled flag images rather than relying
  entirely on regional-indicator emoji rendering; add an optional
  map/region reveal after answering.
- [ ] Quick Math (P2) - offer practice without speed emphasis and use
  number-line/grouping explanations for mistakes; let parents select
  operations independently.
- [ ] Math Target (P2) - display the running equation and allow undoing
  the last tile rather than clearing everything; offer a partial-sum hint
  when stuck.

**Puzzles and deduction**

- [ ] Sliding Puzzle (P2) - add one-move undo and a practice hint;
  calibrate shuffle difficulty without counting immediate backtracks as
  meaningful scrambling.
- [ ] Jigsaw Puzzle (P2) - offer optional correct-piece locking and a ghost
  reference; label the swap interaction clearly (irregular jigsaw pieces
  are a larger optional expansion, not this).
- [ ] Spot the Difference (P2) - make changes belong to the scene (missing
  objects, altered size/orientation) rather than relying mostly on
  floating icon differences; keep hit areas generous.
- [ ] Minesweeper (P2) - add a guided first puzzle and chord-reveal;
  consider a separately labeled no-guess mode (first-click safety alone
  doesn't guarantee the whole board is deduction-solvable).
- [ ] Sudoku (P2) - add candidate notes, undo, and a conflict-only practice
  mode; distinguish rule conflicts from disagreement with the stored
  solution.
- [ ] Sequence Puzzle (P2) - allow marking clues as used and rearranging
  placed items directly; provide a hint that explains one relationship
  rather than revealing the entire order.
- [ ] Logic Grid Puzzle (P2) - add undo for a mistaken affirmative mark,
  since it changes several cells; support clue highlighting and
  contradiction explanations.
- [ ] Nonogram (P2) - add explicit Fill/Mark tools and drag painting so
  larger boards need fewer taps; highlight completed clue runs and retain
  uniqueness checks for new art.
- [ ] Mastermind (P2) - give colors optional symbols and include a worked
  repeated-color example; make guess history easy to compare.
- [ ] Code Breaker (P2) - add digit-elimination notes and an example
  separating exact-position matches from misplaced digits.
- [ ] 2048 (P2) - allow continuing after reaching the target and save
  unfinished boards; consider one-step undo in an explicitly assisted
  mode.
- [ ] Threes (P2) - demonstrate valid merges; the implementation compresses
  whole rows and spawns in any empty cell, which differs from the original
  rules - either document those as intentional house rules or separately
  validate/implement the original movement rules.
- [ ] Robot Commands (P2) - add step-through execution and instruction
  insertion/reordering; a small repeat block is a later teaching
  extension.
- [ ] Laser Maze (P2) - add undo and a clearer source/target legend; let
  children inspect the beam one segment at a time to understand
  reflection.

**Board and card strategy**

- [ ] Tic-Tac-Toe (P2) - alternate the starting player and offer a
  two-player mode; on Hard, celebrate a draw as successful defense.
- [ ] Ultimate Tic-Tac-Toe (P2) - animate which sub-board the last move
  sends the opponent to, and highlight the forced destination before the
  next move.
- [ ] Connect Four (P2) - add alternating starts and an optional
  "show the threat" hint before a losing move; a two-player mode is a
  natural extension.
- [ ] Hex (P2) - make each side's goal edges unmistakable and show the
  winning chain; explain connection with a tiny sample board.
- [ ] Rock Paper Scissors (P2) - explain the result and offer a short
  pattern-awareness tip; keep difficulty descriptions honest about how
  predictable the opponent is.
- [ ] Higher or Lower (P2) - add an optional visual number range and
  likelihood explanation; give endless streak-chasing a friendly stopping
  milestone.
- [ ] Battleships (P2) - preview a ship's footprint before placing it and
  explain invalid placement; make sunk ships and the remaining fleet easy
  to read on a small screen.
- [ ] Dots and Boxes (P2) - enlarge invisible edge hit areas without
  thickening the drawing; show why a player gets another turn before
  adding chain teaching.
- [ ] Reversi (P2) - preview which discs a move would flip and explicitly
  announce passes; add training undo separate from competitive records.
- [ ] Checkers (P2) - explain forced captures and promotion-ending-turn
  behavior in context; offer practice undo and clearer skill tiers after
  playtesting.
- [ ] Chess Puzzles (P2) - generalize mate-in-two validation (it currently
  requires exactly one Black reply rather than proving mate against every
  legal reply); keep the current bank as a simpler forced-reply lesson
  set.
- [ ] UNO (P2) - animate turn direction and skipped/draw-penalty turns;
  explain playable cards and provide an optional classic-rules mode only
  after the simple mode is clear.
- [ ] Dominoes (P2) - highlight matching pips and valid ends together;
  keep a long chain readable with controlled scrolling or wrapping.
- [ ] Solitaire (P2) - add save/resume, a legal-move hint, and a useful
  stuck-state explanation; label draw/redeal rules independently of
  perceived deal difficulty.
- [ ] Card Battle (P2) - add a short starter battle that teaches energy,
  summoning delay, Guard, and spells one at a time; show deck composition
  and affordable actions; harden delayed-turn cancellation before
  expanding the card pool (same stale-delay family as the P1 items above).
- [ ] Auto Battler (P2) - make formation order selectable (the team is
  currently created in roster order) and add one or two differentiated
  abilities before increasing roster size (its exit/restart combat bug is
  in the P1 section above).
- [ ] Lucky Spin Slots (P2) - make selected paylines, stake, and payout
  explanations clear; prioritize the children's games ahead of expanding
  this one (its profile-access enforcement is the route-bypass P1 above).

**Action, sports, and timing**

- [ ] Whack-a-Mole (P2) - add untimed practice and a slower introductory
  tier; distinguish a miss from an intentional tap on an empty hole during
  exploration.
- [ ] Catch the Falling Objects (P2) - add hold/drag lane movement and
  arrow-key control where appropriate; visually separate hazards from good
  items beyond color.
- [ ] Fruit Slice (P2) - add a brief swipe demonstration (spawn overlap is
  covered above alongside Bubble Pop and Pop & Sparkle).
- [ ] Archery Challenge (P2) - once pointer-cancel-fires-a-shot is fixed
  (Systemic item above), show impact offsets to teach adjustments on the
  next shot.
- [ ] Basketball Shot (P2) - once pointer-cancel is separated from release
  (same Systemic item), make rim/backboard feedback explain near misses.
- [ ] Penalty Shootout (P2) - present it as choosing a direction to beat
  the keeper; a separately labeled timing mode could add a new skill
  later.
- [ ] Air Hockey (P2) - offer a short first-to-three match, an adjustable
  paddle offset so fingers don't obscure the puck, and pause/resume.
- [ ] Pool / Billiards (P2) - add a shot-path guide and clear scratch
  recovery feedback (a two-player ruleset is a larger optional expansion,
  not the first polish task).
- [ ] Mini Golf (P2) - make capped holes explicit in results so a forced
  advance isn't confused with holing out; add a few authored teaching
  holes before more procedural obstacles.
- [ ] Rhythm Game (P2) - add audio/visual latency calibration and richer
  short patterns after timing is measured on the intended device; retain
  an unscored practice option.
- [ ] Space Survival (P2) - add a pause control, a forgiving opening wave,
  and an optional offset control scheme to reduce finger occlusion.
- [ ] Top-Down Racing (P2) - add clear next-checkpoint guidance and
  wrong-way feedback, then vary tracks; measure whether drag-to-steer
  remains readable at top speed.
- [ ] Time Trial Racer (P2) - add best-lap splits and a ghost replay;
  separate lap count from driving difficulty.
- [ ] Snake (P2) - add pause and an optional wraparound practice board;
  keep scoring separate for wall and wrap modes.
- [ ] Knight's March (P2) - make the committed next lane, available
  squires, and work completion times easy to see together; start with a
  short authored tutorial and a safe scouting/pause phase.

**Longer campaigns and bundled browser games**

- [ ] Cat Defense (P2) - apply the same placement/save improvements as
  Tower Defense above and decide whether it owns separate records; keep
  theme data separate from the shared rules implementation.
- [ ] Dungeon Crawler (P2) - preview enemy threat ranges and likely
  retaliation; offer room-entry checkpoints and optional undo in a
  teaching mode.
- [ ] Mini RPG Battle (P2) - save at sequence boundaries and explain enemy
  intentions so Guard/Items become informed choices; show upcoming
  healing/recovery clearly.
- [ ] Roguelike Arena (P2) - add suspend/resume that preserves the current
  run without undoing death, plus a death recap identifying the decisive
  choices.
- [ ] Crown & Banner (P2) - isolate saves by profile (it currently uses the
  fixed local-storage key `crownAndBanner.savedGame.v1` with no profile in
  its iframe URL) and add a short guided first campaign. Restore the
  authoring source/build workflow before editing features in the minified
  bundle; preserve the existing save as an importable legacy campaign.
- [ ] Highway Hauler (P2) - add pause, clear held keys on blur, and teach
  gears on a short practice route; show progress toward the next shop and
  keep personal records per profile.
- [ ] Truck Repair Bay (P2) - add a guided first day and pause; replace
  clickable divs with keyboard-operable controls; update existing
  queue/bay nodes instead of rebuilding them every 200ms, which can
  disrupt focus and clicks.

## Backlog Corrections (2026-09-10)

The 2026-09-10 `GAME_EVALUATION.md` review found this file was partly stale
and confirmed the following are already done - not re-listed as open work
above:

- The JS interop disposal race (see Systemic item, checked off).
- Personal-best calls for SlidingPuzzle, WordScramble, WordSearch,
  CatchGame, ColorMatch, and TicTacToe (removed from Missing
  Progression / Rewards).
- Manners Garden's Achoo target has a dashed outline and pulsing
  background (see the per-game item, checked off).
- Minesweeper first-click protection, unique Sudoku generation, Chess
  training undo, Solitaire undo/Auto Finish, and a Dress Up saved-look
  gallery all already exist.

Fixing an interop call site being safe does not by itself cancel obsolete
gameplay tasks - keep that distinction in mind when working the P1 stale-
delayed-gameplay item above.

## Stable / No Current Action

SoundButtons, NumberSequence, and ShadowMatch had no findings beyond shared
items. WordSearch drag-select math and Minesweeper/Sudoku puzzle generation
were reviewed and looked correct (reconfirmed 2026-09-10). TicTacToe/ConnectFour's
`GameAi` usage is correct, and RedLightGreenLight/CatchGame have solid
difficulty curves.

## Acceptance checklist for this backlog

From the 2026-09-10 review: exercise the smallest supported phone and the
intended tablet with mouse, touch, keyboard, muted audio, reduced motion,
and background/resume. Concentrate on: begin a delayed action and
exit/restart; use the host arrow and browser Back; cancel an aiming
gesture; finish a round while a result save is in flight. A passing build
establishes compilation, not these interaction guarantees.
