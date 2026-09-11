# Game evaluation — September 10, 2026

The best next investment is a reliability and teaching pass across the existing catalog. The collection already has substantial variety, forgiving activities for younger children, real strategy games for older children, reusable physics and AI services, and persistent personal bests in many games. More games would currently add less value than making the existing ones easier to understand, resume, and trust.

This is a catalog-wide source review with deeper checks of selected rules, input, asynchronous behavior, persistence, and accessibility. It covers all **109 registered built-in games and three bundled browser games**. Cat Defense shares TowerDefense's implementation. Existing uncommitted changes were included in the reviewed version and left untouched. This is not a claim that every game was played or every rule was exhaustively tested: no interactive browser playthrough was performed. Visual quality, touch feel, sound quality, and difficulty judgments still need device playtesting.

Validation: `dotnet build --no-restore` passed with zero warnings and zero errors. An independent constraint search over the current Nonogram data found a unique solution for each of its 11 boards. Recommendations below distinguish observed implementation problems from proposed enhancements. No application code was changed.

**Fix these first**

Priority: **P1** = correctness or a substantial usability failure; **P2** = valuable improvement; **P3** = optional expansion. Effort estimates describe relative scope, not delivery commitments.

| Priority | Finding and evidence | Smallest practical improvement | Tradeoff / validation |
|---|---|---|---|
| P1 | **Exiting does not reliably stop delayed gameplay.** Auto Battler's Exit buttons only change `phase`; `RunBattleLoop` and `Strike` continue without cancellation or phase checks. Animal Sound Guessing advances after an uncancelled delay. Chess's delayed CPU move also lacks a session check. Exiting and starting another match can let old work affect new state. [AutoBattler](Components/AutoBattler.razor), [AnimalSoundGuessingGame](Components/AnimalSoundGuessingGame.razor), [ChessGame](Components/ChessGame.razor). | Give each run a cancellation token and generation ID; invalidate on exit, restart, and disposal. Recheck the captured generation after awaits and make completion idempotent. Apply first to these components, then audit the same pattern elsewhere. | Medium scope across the catalog. A phase check alone is insufficient when the next session is already Playing. Test exit/restart during a CPU turn, reveal, and final animation; assert no stale mutation or duplicate result. |
| P1 | **Reduced-motion mode disables essential gameplay motion.** The global `* { animation: none !important; transition: none !important; }` rule stops Fruit Slice's toss and Bubble Pop's rise while their expiration logic continues. Follow the Cups loses its animated position transitions. [app.css](wwwroot/css/app.css), [FruitSlice CSS](Components/FruitSlice.razor.css), [BubblePop CSS](Components/BubblePop.razor.css), [FollowTheCups](Components/FollowTheCups.razor). | Separate decorative effects from motion that communicates game state. Provide slower/simpler gameplay or static alternatives for affected games instead of globally stopping all animations. | Small-to-medium scope. Merely overriding the preference everywhere defeats its purpose. Verify each affected game with reduced motion enabled. |
| P1 | **Bubble Pop penalizes ignoring distractors.** `ExpireItemAsync` resets `streak` for every unpopped bubble, including symbols the player was instructed not to pop. Targets can also switch while bubbles remain in flight. [BubblePop](Components/BubblePop.razor:275). | Reset the streak only for an eligible target that was missed. Define whether eligibility belongs to the target at spawn or at expiration; avoid abruptly reclassifying live targets without a cue. | Small scope. Test a distractor escaping during a correct streak and a target change with live bubbles. |
| P1 | **Word Ladder's instructions allow answers its validator rejects.** The UI asks for a real word differing by one letter, but `Submit` accepts only `chain[currentRungIndex]`. For the CAT → DOG ladder, CAT → COT → DOT → DOG is valid but DOT is rejected because the hidden chain requires COG. [WordLadder](Components/WordLadder.razor:259). | Smallest fix: make this explicitly a clue-guided, prescribed-chain puzzle and clue each required word. Better long-term: bundle a curated vocabulary graph and accept valid alternative routes to the destination. | Copy/clues are small; graph-based validation is medium. Do not merely accept arbitrary one-letter strings. Add the DOT alternative as a regression scenario if open routes are supported. |
| P1 | **Chess castling rights are inferred from occupancy.** A king or rook that moves away and returns to its starting square regains eligibility. This is a rules bug for full games. En passant and underpromotion are also explicitly omitted. [ChessRules](Services/ChessRules.cs:256). | Track castling rights in game position state and preserve them in AI search and undo snapshots. Describe the remaining simplified rules until they are implemented. | Medium scope: position history must propagate through the shared engine and training undo. Test king/rook return moves and rook captures. |
| P1 | **The host's back arrow bypasses built-in result logging.** `GoBack` immediately navigates; host disposal logs only iframe sessions. Many built-ins log only through their own Finish/Exit handlers, so leaving through the shared arrow or browser Back can omit a session. [GameHost](Pages/GameHost.razor), [BigSmall](Components/BigSmall.razor). | Add a narrowly scoped session-finalization contract used by the host and components, with a once-only result guard. Capture profile/game identity at session start. Distinguish completed and abandoned sessions. | Medium scope. Avoid double counting when a completed game is later unmounted. In-app navigation can be finalized reliably; browser/tab closure needs periodic checkpoints rather than relying on async disposal. |
| P1 | **Direct game routes do not enforce the profile's allowed game list.** GameSelect uses `GetGamesForProfileAsync`; GameHost loads from `GetAllGamesAsync` after checking only that a profile exists. A known disallowed `/play/{id}` route can therefore bypass the picker restriction. [GameSelect](Pages/GameSelect.razor), [GameHost](Pages/GameHost.razor:486). | Resolve the game against the current profile's permitted games at the route boundary. Use `OnParametersSetAsync` for changing `GameId`, since the same routed component can be reused. | Small scope. Preserve intended admin access. This enforces application behavior; a client-only local-storage app is not a tamper-resistant security boundary. |
| P2 | **Sound-themed games do not consistently deliver the named sound.** Sound Buttons plays `playPianoNote(index)` for every theme. Animal Sound Guessing speaks text such as animal noises through speech synthesis. Touch the Body Part plays a match sound but does not speak the requested/tapped part. [SoundButtons](Components/SoundButtons.razor:117), [AnimalSoundGuessingGame](Components/AnimalSoundGuessingGame.razor:234), [TouchTheBodyPart](Components/TouchTheBodyPart.razor:74). | Add a small local sound bank for animal/vehicle sounds; speak body-part prompts and responses; retain visible feedback when muted. | Medium content effort. Local assets support offline use but add download size. Evaluate actual recognizability with children. |
| P2 | **Pointer cancellation can fire a shot.** Archery and Basketball wire `pointercancel` to the release handler, which can resolve and launch the current aim. [ArcheryChallenge](Components/ArcheryChallenge.razor:65), [BasketballShot](Components/BasketballShot.razor:59). | Use a separate cancellation handler that clears aiming state without firing; require the matching active pointer ID for move/up. Audit the other drag-shot games for the same pattern. | Small scope per game. Test interrupted gestures and a second finger, not only normal mouse dragging. |
| P2 | **Some best-score records mix incomparable modes.** Tower Defense always writes under `BuiltInGames.TowerDefense`, including the Cat Defense theme, with a `best-level` key lacking difficulty. Word Ladder uses one `fewest-mistakes` key across difficulties and ladders. [TowerDefense](Components/TowerDefense.razor:1159), [WordLadder](Components/WordLadder.razor:303). | Key records by actual game, difficulty, and relevant ruleset; decide explicitly whether the two defense themes should share progress. | Small-to-medium scope. Existing records need migration or a clearly identified legacy bucket. Random puzzles also differ in intrinsic difficulty. |
| P2 | **Chess draws are recorded as losses.** The component has `Outcome.Draw`, but its result carries only `Won`; the host maps false to “Lost.” [ChessGame](Components/ChessGame.razor:379), [GameHost](Pages/GameHost.razor). | Return an outcome enum and persist Win/Loss/Draw/Abandoned separately. | Small scope for Chess, with a compatible history migration if generalized. |
| P2 | **Crown & Banner's save is shared across profiles.** The bundled app uses the fixed local-storage key `crownAndBanner.savedGame.v1`; no profile is passed in its iframe URL. | Namespace campaign saves by launcher profile using an explicit initialization contract. | Medium scope; preserve the existing save as an importable legacy campaign. The minified bundle is a poor long-term editing target—restore its authoring project first. |

**Improvements that benefit the whole collection**

- **Pause and resume:** add a shared pause signal for active play, hidden tabs, and the daily-limit overlay. The overlay currently covers the game rather than stopping its logic. Keep gameplay elapsed time separate from the parent's chosen daily-usage policy. Start save/resume with Chess, Solitaire, Tower Defense, Card Battle, and the RPGs.
- **Make learning visible:** use a short demonstration before the first round, progressive hints, and specific feedback after a mistake. For young children, a spoken prompt plus a picture is more useful than a setup paragraph. Allow a practice mode without speed pressure.
- **Use one dependable input vocabulary:** maintain tap-select/tap-place for boards and pull-back-to-shoot for aiming games. Add keyboard activation to pointer-only buttons, accessible names to unlabeled controls, and non-color identifiers where color is incidental. Baby Piano currently has empty, pointer-only key buttons.
- **Make 112 games browsable:** the picker already has Recently Played; add favorites and a few visual categories such as Create, Match, Think, and Action. Offer parents age/skill filters without making children configure a search form.
- **Improve history before adding more rewards:** `PlayHistoryEntry.Moves` currently represents moves, scores, placements, and score margins depending on the game, while `Difficulty` sometimes contains a result description. Add explicit outcome and metric fields incrementally. Show the relevant units in reports. Avoid adding scores to exploratory toddler toys.
- **Correct timing where it matters:** many quizzes and arcade games increment/decrement counters on timer callbacks. Use a monotonic elapsed-time source and explicit pause accounting for reaction, rhythm, racing, and timed records. For rhythm, synchronize judgment with the presented beat; for reaction tests, measure from presentation rather than before the render.
- **Profile performance before restructuring rendering:** target repeatable frame-time checks on the intended tablet. Dense Blazor particle scenes and high-frequency pointer events are candidates for batching or a small JS/canvas rendering layer only if measurements justify it. Keep turn-based boards idiomatic Blazor.
- **Retain narrow shared services:** InteropService, Physics2D, RaceTrack, GameAi, and ChessRules already provide useful reuse. Add a small session-lifetime abstraction; avoid forcing every game into a large inheritance hierarchy. Extract pure rule engines first when regression tests would meaningfully protect them.

**Game-by-game recommendations**

The descriptions below summarize the present implementation. Each row proposes the next worthwhile change; it does not imply every other aspect of that game has been certified. P1 entries refer to the concrete findings above. Shared fixes apply even where a row focuses on a different improvement.

**Early exploration and creative play**

| Game | Current foundation | Recommended next improvement |
|---|---|---|
| Peek-a-Boo | Several reveal presentations with forgiving repeat taps. | P2: Give Curtain and Present Box genuinely different opening animations; preview each theme visually. |
| Sound Buttons | Large themed buttons with immediate musical feedback. | P2: Match audio to the pictured animal/vehicle instead of using piano notes for all themes. |
| Pop & Sparkle | Simple drifting targets and immediate respawn. | P2: Choose separated spawn positions so one target cannot obscure another; offer a slower drift setting. |
| Baby Piano | Eight colorful keys and Piano/Drums sound choices. | P2: Give Drums a drum-pad layout; add key labels, keyboard activation, and independent held-key feedback for simultaneous taps. |
| Magic Garden | Every plot can grow a flower and release butterflies. | P3: Add a seasonal garden choice and optional persistent picture of the child's garden. Keep it free of maintenance chores. |
| Feed the Animal | Two-tap food pairing with friendly reactions even to mismatches. | P2: Make the selected food unmistakable and speak animal/food names; demonstrate the two-step action once. |
| Big & Small | Clear reversible size change with new shapes. | P2: Speak “big” and “small” with the transformation; keep the target easy to hit at its smallest size. |
| Color Splash | Tap-positioned blobs and droplets. | P3: Offer a simple palette and a keep-my-picture mode alongside fading splashes. |
| Finger Trails | Several trail styles driven by pointer movement. | P2: Track active pointer IDs and cap/batch particles; offer a persistent drawing mode. Measure touch responsiveness before changing rendering technology. |
| Sleepy Animals | Distinct Wake Up and Bedtime loops; bedtime has a gentle endpoint. | P2: Strengthen bedtime's final quiet visual/audio cue and provide an obvious replay action after everyone sleeps. |
| Gentle Creatures | Fish/butterfly themes with forgiving tap reactions. | P2: Prevent excessive overlap and let parents select creature speed/count for easier tracking. |
| Night Sky | Persistent stars with repeatable twinkling and a count cap. | P3: Let children connect stars into simple constellations; provide visible feedback when the star cap is reached. |
| Fireworks Touch | Spatial cause-and-effect bursts. | P2: Add a calm burst style with lower flash intensity and fewer particles; make it the reduced-motion alternative. |
| Lights On, Lights Off | Reversible light toggles with no prescribed answer. | P3: Add a simple room scene where switches visibly control particular lamps; retain free exploration. |
| Spin the Wheel | Repeat taps add spin; there is no reward wager. | P2: Add an obvious slowdown/stop interaction and a calm presentation for reduced-motion users. |
| Make It Rain | Rain, sun, splashes, and a discoverable rainbow combination. | P3: Reinforce discoveries with short spoken weather words and clearer cause-to-effect animation. |
| Snow Day | Snowfall, footprints, and revealable objects. | P2: Make snow mounds stand out as interactive; bound footprint accumulation and make clearing playful. |
| Puddle Splash | The frog travels to the tapped puddle. | P3: Different puddle sizes could make distinct splash sounds, giving repeat taps more variety. |
| Bath Time | Several independent bath interactions. | P2: Add a brief visual demonstration of the cup, duck, and bubbles; use a different recognizable sound for each. |
| Funny Faces | Individual parts react independently. | P3: Add a small face/voice choice and optional spoken part names while preserving rapid repeat taps. |
| Touch the Body Part | A visible prompt changes after any tap; all taps are accepted. | P2: Speak the prompt and the part actually touched. Preserve exploration, but do not imply the requested answer was selected when it was not. |
| Stack the Blocks | Tap placement and reversible removal eliminate precision requirements. | P2: Clearly indicate the removable top block and make reaching the stack limit a small celebration rather than a dead tap. |
| Knock It Down | A tap collapses a tower, which rebuilds automatically. | P3: Vary the tower's silhouette and allow tapping to rebuild immediately after the collapse. |
| Roll the Ball | One-tap bowling sequence automatically resets. | P3: Offer a few ball/pin arrangements and distinct collision sounds without adding precision aiming to this toddler toy. |
| Dress Up | Rich sticker manipulation, drawing, undo, scenes, and a saved-look gallery. | P2: Add optional clothing snap guides and a simpler starter tray for younger children. Preserve free placement and the existing creative tools. |
| Build-a-Monster | Modular body-part slots make every combination valid. | P2: Add randomize, naming, and a per-profile saved-monster gallery. Reuse the gallery experience where practical. |
| Manners Garden | Spoken scenarios, forgiving retries, persistent garden rewards, and an outlined Achoo target. | P2: Offer tap-to-complete alongside elbow dragging and make the gesture cancellation-safe. Expand scenario variety after device testing confirms this interaction is clear. |

**Matching, memory, and early reasoning**

| Game | Current foundation | Recommended next improvement |
|---|---|---|
| Memory Match | Multiple themes, including math-pair generation, with persistent records. | P1: Cancel mismatch resolution across restarts: its delayed callback dereferences `firstPick`/`secondPick`, which StartGame resets. P2: add an optional short preview and spoken pair labels. |
| Fishing Catch | Letter, number, and color matching; three catches advance each target. | P2: Add short, shuffled target sets. The full alphabet requires 78 correct catches and the number sequence 60, which makes completion a large commitment. |
| Simon Says | Increasing sequences and difficulty-dependent mistake tolerance. | P2: Add optional slower playback and a visible round-progress cue; offer a natural stopping milestone on endless Easy play. |
| Copy the Pattern | Spatial recall of a simultaneously displayed set of cells. | P2: Add a forgiving Easy retry and adjustable viewing time; one wrong tap currently ends even the easiest run. |
| Memory Sequence Adventure | Simon's sequence mechanic presented through doors and a traveler. | P3: Give milestones different destinations or scenes so progress feels like an adventure beyond the reskin. |
| Follow the Cups | Six rounds with cup-count and shuffle-speed scaling. | P1: Preserve trackable swaps under reduced motion. P2: offer one slow demonstration shuffle before scored rounds. |
| Odd One Out | Eight rounds of visual discrimination without reading. | P2: Add controlled differences in size, orientation, and shape; avoid making difficulty depend only on more tiles. |
| Pattern Complete | Repeating cycles with increasing symbol counts. | P2: Add patterns such as AAB and ABB, then briefly highlight the repeating unit after an answer. |
| Number Sequence | Step size and counting direction provide real difficulty changes. | P2: Explain the step visually after each answer, with an optional number line on Easy. |
| Color Match | Exact hue matching with increasingly similar distractors. | P2: Tune palettes on the intended screens and add a color-name learning mode. Keep any symbol-assisted mode separate because symbols change the skill being assessed. |
| Shape Sorter | Touch-friendly select/place with forgiving wrong placements. | P2: Strengthen the selected-shape cue; add a short demonstration and spoken shape names. |
| Shadow Match | Silhouette matching with varying choice counts. | P3: Add familiar object/animal outlines after the basic shapes; vary rotation only as an explicit harder tier. |
| Animal Sound Guessing | Replayable prompts and difficulty-dependent choice counts. | P1: Cancel old round advances on exit/restart. P2: use recognizable animal recordings instead of synthesizing their written noises. |
| Hot and Cold | Temperature information remains on visited cells. | P2: Add a clear temperature legend and distance explanation; pair color with symbols/text. |
| Treasure Hunt | Movement-constrained search distinguishes it from Hot and Cold. | P2: Show available adjacent moves and explain distance feedback visually; add small themed maps after the basic search is understood. |

**Word, knowledge, and math games**

| Game | Current foundation | Recommended next improvement |
|---|---|---|
| Word Scramble | Five-word sessions with picture clues and forgiving retries. | P2: Add spoken words, a first-letter hint, and minimal correction that preserves useful work instead of clearing every letter. |
| Guess the Word | A rocket/fuel presentation avoids punitive hangman imagery. | P2: Add category/pronunciation help and adaptive hints before fuel is exhausted; show the completed word long enough to learn it. |
| Word Search | Three procedural grids with difficulty-dependent placement directions. | P2: Avoid repeating categories in a session and permit words of exactly the grid size (`<= size`, currently `<= size - 1`). Add tap-start/tap-end selection as a drag alternative. |
| Word Ladder | Curated chains ensure intended routes exist and hints prevent permanent stalls. | P1: Align instructions and validation or accept alternate real-word routes. P2: separate records by difficulty and puzzle. |
| Cryptogram | Whole-cipher-letter substitution supports real deduction. | P2: Add a frequency view, a substitution reference, and undo; offer optional word-level hints without turning each letter into an answer checker. |
| Morse Code Challenge | Reference charts on Easy/Medium and five-word sessions. | P2: Play actual dot/dash timing on demand and let a wrong word be edited without erasing all progress. |
| Trivia Battle | Categories, shuffled choices, streaks, and a lifeline. | P2: Add a one-sentence explanation after answers and track recently seen questions per profile to reduce repetition. |
| Flag Guessing Game | Difficulty-dependent flag pools and multiple-choice answers. | P2: Use bundled flag images rather than relying entirely on regional-indicator emoji rendering. Add an optional map/region reveal after answering. |
| Quick Math | Ten rounds with operation and range changes. | P2: Offer practice without speed emphasis and use number-line/grouping explanations for mistakes; let parents select operations independently. |
| Math Target | Guaranteed construction of target sums, with rejection of overshoots and a Clear action. | P2: Display the running equation and allow undoing the last tile rather than clearing everything. Offer a partial-sum hint when stuck. |

**Puzzles and deduction**

| Game | Current foundation | Recommended next improvement |
|---|---|---|
| Sliding Puzzle | Legal-move shuffling guarantees solvability; fewest-move records and a reference-picture toggle already exist. | P2: Add one-move undo and a practice hint. Calibrate shuffle difficulty without counting immediate backtracks as meaningful scrambling. |
| Jigsaw Puzzle | Any-two-piece swaps always permit a solution. | P2: Offer optional correct-piece locking and a ghost reference. Label the swap interaction clearly; irregular jigsaw pieces are a larger optional expansion. |
| Spot the Difference | Procedural marker differences avoid repeating a single fixed pair. | P2: Make changes belong to the scene—missing objects, altered size, orientation—rather than relying mostly on floating icon differences. Keep hit areas generous. |
| Minesweeper | First-click neighborhood safety and a touch flag toggle are already implemented. | P2: Add a guided first puzzle and chord-reveal; consider a separately labeled no-guess puzzle mode. First-click safety does not guarantee the whole board is deduction-solvable. |
| Sudoku | 4x4/6x6/9x9 grids and a uniqueness-preserving generator. | P2: Add candidate notes, undo, and a conflict-only practice mode; distinguish rule conflicts from disagreement with the stored solution. |
| Sequence Puzzle | Clues are generated until they determine a unique ordering. | P2: Allow marking clues as used and rearranging placed items directly; provide a hint that explains one relationship rather than revealing the entire order. |
| Logic Grid Puzzle | Unique matching clues and automatic row/column elimination. | P2: Add undo for a mistaken affirmative mark, since it changes several cells; support clue highlighting and contradiction explanations. |
| Nonogram | Three sizes; all 11 current boards independently verified unique. | P2: Add explicit Fill/Mark tools and drag painting so larger boards require fewer taps; highlight completed clue runs and retain uniqueness checks for new art. |
| Mastermind | Easy adds concrete per-peg feedback while harder modes retain aggregate deduction. | P2: Give colors optional symbols and include a worked repeated-color example; make guess history easy to compare. |
| Code Breaker | Nonrepeating digits provide a distinct deduction problem. | P2: Add digit-elimination notes and an example separating exact-position matches from misplaced digits. |
| 2048 | Swipe and arrow-button controls, merge rules, and target milestones. | P2: Allow continuing after reaching the target and save unfinished boards; consider one-step undo in an explicitly assisted mode. |
| Threes | 1+2/equal-large-tile merges and a visible next tile create a different strategy from 2048. | P2: Demonstrate valid merges. The implementation compresses whole rows and spawns in any empty cell; either describe those house rules clearly or separately validate and implement the intended original movement rules. |
| Robot Commands | Generated reachable mazes, forgiving execution, and a highlighted active instruction. | P2: Add step-through execution and instruction insertion/reordering, then a small repeat block as a later teaching extension. |
| Laser Maze | Path-first generation guarantees a solution; blockers affect wrong routes. | P2: Add undo and a clearer source/target legend. Let children inspect the beam one segment at a time to understand reflection. |

**Board and card strategy**

| Game | Current foundation | Recommended next improvement |
|---|---|---|
| Tic-Tac-Toe | Difficulty-scaled AI, tally, and persistent streak records. | P2: Alternate the starting player and offer a two-player mode. On Hard, celebrate a draw as successful defense. |
| Ultimate Tic-Tac-Toe | Correctly distinguishes local and meta-board objectives. | P2: Animate which sub-board the last move sends the opponent to, and highlight the forced destination before accepting the next move. |
| Connect Four | Shared minimax AI and visible round outcomes. | P2: Add alternating starts and an optional “show the threat” hint before a losing move; a two-player mode is a natural extension. |
| Hex | Six-neighbor connection play and difficulty-scaled board size. | P2: Make each side's goal edges unmistakable and show the winning chain; explain connection with a tiny sample board. |
| Rock Paper Scissors | CPU behavior responds to previous choices rather than reading the current input. | P2: Explain the result and offer a short pattern-awareness tip; keep difficulty descriptions honest about how predictable the opponent is. |
| Higher or Lower | Ties are safe and the implemented ranges now make Easy more forgiving. | P2: Add an optional visual number range and likelihood explanation. Give endless streak-chasing a friendly stopping milestone. |
| Battleships | Manual/random fleet placement and progressively smarter search AI. | P2: Preview a ship's footprint before placing it and explain invalid placement. Make sunk ships and the remaining fleet easy to read on a small screen. |
| Dots and Boxes | Extra turns for completed boxes and a scaled CPU heuristic. | P2: Enlarge invisible edge hit areas without thickening the drawing; show why a player gets another turn. Add chain teaching only after this is clear. |
| Reversi | Legal-move guides, pass handling, and positional AI. | P2: Preview which discs a move would flip and explicitly announce passes; add training undo separate from competitive records. |
| Checkers | Tournament and Free Play rules, capture chains, and whole-turn AI search. | P2: Explain forced captures and promotion-ending-turn behavior in context; offer practice undo and clearer skill tiers after playtesting. |
| Chess Puzzles | Real legality and mate checks, with hints after misses. | P2: Generalize mate-in-two validation: it currently requires exactly one Black reply rather than proving mate against every legal reply. Retain the current bank as a simpler forced-reply lesson set. |
| Chess | Legal-move guidance, AI tiers, and training undo. | P1: Fix castling-history state. P2: preserve draws in history, correct undo's check-status side, add save/resume, then calibrate Medium with actual players. |
| UNO | Three-player simplified rules avoid some punitive penalties. | P2: Animate turn direction and skipped/draw-penalty turns; explain playable cards and provide an optional classic-rules mode only after the simple mode is clear. |
| Dominoes | Draw/pass behavior and selectable chain ends support touch play. | P2: Highlight matching pips and valid ends together; keep a long chain readable using controlled scrolling or wrapping. |
| Solitaire | Undo and Auto Finish already exist; draw/redeal rules vary by tier. | P2: Add save/resume, a legal-move hint, and a useful stuck-state explanation. Label draw/redeal rules independently of perceived deal difficulty. |
| Card Battle | Persistent collection, deckbuilding, energy, creature combat, and keywords. | P2: Add a short starter battle that teaches energy, summoning delay, Guard, and spells one at a time. Show deck composition and affordable actions; harden delayed-turn cancellation before expanding the card pool. |
| Auto Battler | Three-unit drafting and automatic combat offer a small strategy entry point. | P1: Stop combat on exit/restart. P2: make formation order selectable—the current team is created in roster order—and add one or two differentiated abilities before increasing roster size. |
| Lucky Spin Slots | Play-money slots, paylines, and a cash-out session boundary; catalog metadata sets age 18+. | P2: Keep it explicitly separated from the children's selection and enforce profile access at the route. Make selected paylines, stake, and payout explanations clear; prioritize the children's games ahead of expanding this one. |

**Action, sports, and timing**

| Game | Current foundation | Recommended next improvement |
|---|---|---|
| Whack-a-Mole | Short rounds and streaks without point subtraction. | P2: Add untimed practice and a slower introductory tier; distinguish a miss from an intentional tap on an empty hole during exploration. |
| Catch the Falling Objects | Lane-based control, hazards, and a meaningful difficulty curve. | P2: Add hold/drag lane movement and arrow-key control where appropriate; visually separate hazards from good items beyond color. |
| Fruit Slice | Real swipe intersection and visible blade trails. | P1: Preserve playability under reduced motion. P2: avoid overlapping fruit/bomb spawns that make a safe swipe unreadable; add a brief swipe demonstration. |
| Bubble Pop | Target matching and rising targets combine attention with motor control. | P1: Fix distractor-expiration penalties and reduced-motion behavior. P2: avoid target overlap and clearly announce target changes. |
| Reaction Timer | Random waits, forgiving false starts, and multiple measurements. | P2: Lock the input phase before the first awaited sound call so one cue cannot accept repeated taps; use a monotonic, presentation-aware timestamp. |
| Red Light, Green Light | Short progress race with forgiving setbacks. | P2: Pair the light color with a clear symbol/word/audio cue and add hold-to-move as an optional alternative to rapid repeated tapping. |
| Duck Shoot | Lanes, ammo, streaks, and bounded rounds. | P2: Add a practice tier with generous ammo and a result breakdown of accuracy versus escaped targets. |
| Tank Duel | Pull-back aiming and a trajectory preview lower the learning barrier. | P2: Add a short aiming tutorial and practice targets; introduce wind or terrain only as an optional later tier, keeping preview and actual physics consistent. |
| Archery Challenge | Aim/power, wind scaling, and target scoring. | P2: Cancel interrupted gestures without firing and show impact offsets to teach adjustments on the next shot. |
| Basketball Shot | Aiming arcs and difficulty-dependent hoop tolerance/position. | P2: Separate pointer cancellation from release, then make rim/backboard feedback explain near misses. |
| Penalty Shootout | Five-shot directional prediction game with history-based keeper behavior. | P2: Present it as choosing a direction to beat the keeper; a separately labeled timing mode could add a new skill later. |
| Air Hockey | Shared collision physics and CPU speed/prediction tiers. | P2: Offer a short first-to-three match, an adjustable paddle offset so fingers do not obscure the puck, and pause/resume. |
| Pool / Billiards | Solo table-clearing with aiming, ball collisions, pockets, and scratches. | P2: Add a shot-path guide and clear scratch recovery feedback. A two-player ruleset is a larger optional expansion, not the first polish task. |
| Mini Golf | Short generated courses, sand friction, and a stroke cap prevent endless stalls. | P2: Make capped holes explicit in results so a forced advance is not confused with holing out; add a few authored teaching holes before more procedural obstacles. |
| Rhythm Game | Timestamp-based beat positions and fixed hit windows across tempos. | P2: Add audio/visual latency calibration and richer short patterns after timing is measured on the intended device; retain an unscored practice option. |
| Space Survival | Continuous physics, drag steering, auto-fire, and bounded simulation substeps. | P2: Add a pause control, a forgiving opening wave, and an optional offset control scheme to reduce finger occlusion. |
| Top-Down Racing | CPU racers use shared movement physics; grass slows rather than blocks. | P2: Add clear next-checkpoint guidance and wrong-way feedback, then vary tracks. Measure whether drag-to-steer remains readable at top speed. |
| Time Trial Racer | Shared racing physics with solo timed laps. | P2: Add best-lap splits and a ghost replay. Separate lap count from driving difficulty so children can choose a short session without losing challenge. |
| Snake | Discrete movement, speed ramp, keyboard, and swipe controls. | P2: Add pause and an optional wraparound practice board; keep scoring separate for wall and wrap modes. |
| Knight's March | Three-lane planning, junction choices, and limited squires create real priorities. | P2: Make the committed next lane, available squires, and work completion times easy to see together. Start with a short authored tutorial and a safe scouting/pause phase. |

**Longer campaigns and bundled browser games**

| Game | Current foundation | Recommended next improvement |
|---|---|---|
| Tower Defense | Procedural routes, tower upgrades, multiple enemy types, and campaign/endless modes. | P2: Explain unaffordable placement instead of silently returning; offer a touch-friendly range preview before spending, then add checkpoint saves and correctly scoped records. |
| Cat Defense | A themed variant of the shared defense engine. | P2: Apply the same placement/save improvements and decide whether it owns separate records. Keep theme data separate from the common rules implementation. |
| Dungeon Crawler | Finite room runs with adjacent movement and bump combat. | P2: Preview enemy threat ranges and likely retaliation; offer room-entry checkpoints and optional undo in a teaching mode. |
| Mini RPG Battle | Heroes, multiple actions, item choices, escalating encounters, and regroup checkpoints. | P2: Save at sequence boundaries and explain enemy intentions so Guard/Items become informed choices; show upcoming healing/recovery clearly. |
| Roguelike Arena | Hero selection, per-floor upgrades, endless progression, and run-ending defeat. | P2: Add suspend/resume that preserves the current run without undoing death, plus a death recap identifying the decisive choices. |
| Crown & Banner | Bundled strategy campaign with armies, cities, production, battle forecasts, a manual, and autosave. | P2: Isolate saves by profile and add a short guided first campaign. Restore the authoring source/build workflow before editing features in the minified bundle. Inspection was limited to the shipped bundle and styles. |
| Highway Hauler | Canvas driving/shifting, keyboard/touch controls, engine sound, shops, and upgrades. | P2: Add pause, clear held keys on blur, and teach gears on a short practice route. Show progress toward the next shop and retain personal records per profile. |
| Truck Repair Bay | Queue assignment, timed jobs, collection, and escalating customer pressure. | P2: Add a guided first day and pause; replace clickable divs with keyboard-operable controls. Update existing queue/bay nodes instead of rebuilding them every 200 ms, which can disrupt focus and clicks. |

The two truck games load a Google font remotely. Bundle that font locally if their visual presentation should be dependable offline. For all three iframe games, add an explicit, origin-validated launcher message contract for profile initialization, pause, and results. The host currently captures their mounted duration, not game-specific outcomes.

**Backlog corrections and implementation order**

`GAME_POLISH_BACKLOG.md` is partly stale. The shared InteropService already addresses the previously described component-owned module disposal race. Personal-best calls also already exist in examples listed as missing, including Sliding Puzzle, Word Scramble, Word Search, Catch the Falling Objects, Color Match, and Tic-Tac-Toe. Manners Garden's Achoo target now has a dashed outline and pulsing background. Keep this separate from the still-present stale-session problem: making JS calls safe does not cancel obsolete gameplay tasks. Existing code also already provides first-click protection in Minesweeper, unique Sudoku generation, Chess training undo, Solitaire undo/Auto Finish, and a Dress Up gallery.

Suggested sequence:

1. **Reliability:** session cancellation and once-only completion; host exit logging and route access; reduced-motion gameplay; Bubble Pop eligibility. These improve trust across many games.
2. **Rules and feedback:** Word Ladder's answer contract, Chess castling/outcomes, interrupted aim gestures, and clear sound/prompt behavior. Add focused regression cases around these rules and transitions.
3. **Learning and access:** short tutorials, hints, keyboard/control labels, readable touch targets, favorites/categories, and a consistent pause contract.
4. **Long-session value:** per-profile saves for strategy games, correctly scoped best scores, more authored lessons and content. Expand mechanics only after the current loops are understandable and reliable.

For browser acceptance, exercise the smallest supported phone and the intended tablet with mouse, touch, keyboard, muted audio, reduced motion, and background/resume. Concentrate first on the identified paths: begin a delayed action and exit/restart; use the host arrow and browser Back; cancel an aiming gesture; finish a round while a result save is in flight. A passing build establishes compilation, not these interaction guarantees.
