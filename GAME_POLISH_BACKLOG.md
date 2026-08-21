# Game Polish Backlog

Active improvement work for games that already exist. New games belong in
`GAMES_ROADMAP.md`; completed games belong in `COMPLETED_GAMES.md`.

## Systemic / Cross-Cutting

- [ ] Fix the latent JS interop disposal race shared by `PlaySoundAsync`-style
  helpers. Pattern: a component imports `_module`, navigation disposes it while
  a call is still in flight, and a fast transition can surface an
  `ObjectDisposedException`. This likely wants a shared safe-invoke helper or a
  consistent disposal guard across components.
- [ ] Extract timer/sound/format boilerplate from timed arcade games:
  `WhackAMole`, `CatchGame`, `FruitSlice`, `BubblePop`, `ReactionTimer`, and
  `RedLightGreenLight`.
- [ ] Consider a narrow shared spawner only for games that actually match:
  `FruitSlice` and `BubblePop`. Do not force `WhackAMole`, `CatchGame`,
  `ReactionTimer`, or `RedLightGreenLight` into that shape.

## Missing Progression / Rewards

Personal bests are wired into many games, but these still have natural metrics
available:

- [ ] SlidingPuzzle - fewest moves.
- [ ] WordScramble - fastest solve.
- [ ] WordSearch - fastest solve.
- [ ] OddOneOut - high score or best streak.
- [ ] PatternComplete - high score or best streak.
- [ ] NumberSequence - high score or best streak.
- [ ] ColorMatch - high score or best streak.
- [ ] ShapeSorter - high score or best streak.
- [ ] ShadowMatch - high score or best streak.
- [ ] CatchGame - high score.
- [ ] FruitSlice - high score.
- [ ] BubblePop - high score.
- [ ] RedLightGreenLight - high score / best streak.
- [ ] TicTacToe - consecutive win streak.
- [ ] ConnectFour - consecutive win streak.
- [ ] RockPaperScissors - consecutive win streak.
- [ ] HigherOrLower - consecutive win streak.

## Per-Game Polish

- [ ] FruitSlice, BubblePop, PopAndSparkle - random spawn positions can overlap;
  add collision avoidance or lane/slot selection.
- [ ] WordSearch - avoid repeating the same category across the three grids in
  a session; relax the word-length filter so words can fit exactly along an
  edge.
- [ ] TowerDefense - tapping a cell with an unaffordable tower gives no
  feedback; touch users also need a way to preview tower range before spending
  gold.
- [ ] FishingGame - Letters/Numbers mode always runs the full fixed-order
  26/20-item sequence; add shorter tiers.
- [ ] MannersGarden - the Achoo lesson's drop target is invisible; add a visual
  outline/cue.
- [ ] PeekReveal - Curtain and Present Box use the same `SingleReveal`
  animation; distinguish them visually.
- [ ] BabyPiano - Piano and Drums themes only change sound; give Drums a
  distinct visual layout.
- [ ] MagicGarden - add at least one theme/reskin choice for replay variety.
- [ ] ShapeSorter / ShadowMatch - dedupe the shared CSS `clip-path` shape
  definitions.
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
  search is automatically "medium" difficulty for a human opponent.

## Stable / No Current Action

SoundButtons, NumberSequence, and ShadowMatch had no findings beyond shared
items. WordSearch drag-select math and Minesweeper/Sudoku puzzle generation
were reviewed and looked correct. TicTacToe/ConnectFour's `GameAi` usage is
correct, and RedLightGreenLight/CatchGame have solid difficulty curves.
