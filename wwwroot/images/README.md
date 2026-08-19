# Image Assets

Drop game and profile artwork here, then reference it by a path relative to
`wwwroot`.

Examples:

- `images/profiles/buddy.png`
- `images/game-thumbs/memory-match.png`
- `images/dressup/stickers/crown.png`
- `images/fishing/fish-blue.png`
- `images/manners/benny-bear.png`

Use PNG or WebP for raster art. SVG also works for icon-like assets. Keep the
existing emoji fallback fields populated while migrating so missing image paths
do not leave blank UI.

## Sources

- `fishing/*.png`, `manners/benny-bear.png`, `manners/poppy-penguin.png`,
  `manners/milo-monkey.png`, `memory/animals/*.png`,
  `whack/mole.png`, and `tanks/*.png` are all from
  [Kenney](https://kenney.nl) ("Fish Pack 2", "Animal Pack Remastered", and
  "Tanks"), CC0 - free for any use, no attribution required (credit given
  anyway because it's a great resource).
- `dressup/*.png` - see git history/commit messages for provenance if adding
  more from the same source.
- `racing/*.png` (added 2026-08-18) are from a second user-provided batch
  dropped in `Downloads/Assets/RaceCar/` (33 UUID-named PNGs across two
  drops that same day - no pack metadata, source/license unknown, same
  situation as the Checkers-Chess batch above). Each file was viewed
  individually before renaming/copying, same discipline as that batch.
  Used by `Components/RacingGame.razor` and (added 2026-08-18)
  `Components/TimeTrialRacer.razor`, both consuming the shared track/car
  layout in `Services/RaceTrack.cs` rather than duplicating file paths:
  - `car-*.png` (15 files: silver, navygold, yellow, blue, red, green,
    phoenix, shadow, patriot, sunset, sky, police, nightpatrol, rally,
    f1) were all catalogued, but only 14 are in the actual selectable
    roster (`RaceTrack.CarRoster`) - `car-f1.png` was found (after a
    user bug report) to be drawn in side profile rather than top-down
    like every other car, so it can never look right rotating around
    the track; it's excluded from the roster and only still used as
    Time Trial Racer's static launcher thumbnail. Raw pixel dimensions
    vary a lot between the rest (some near-square crops, some wide -
    likely different padding/perspective in how each was originally
    rendered), handled with `object-fit: contain` in a fixed-aspect box
    rather than assuming uniform source dimensions. One near-duplicate
    navy/gold car was found during cataloging and deliberately left
    uncopied (kept in Downloads only).
  - `tile-corner.png`, `tile-straight-h.png`, `tile-straight-v.png` are
    the road tiles every track is built from - one curve image reused
    at every corner via CSS rotation. Verified directly against the
    rendered pixels (an earlier version of this note guessed wrong): at
    `rotate(0deg)` the drivable asphalt touches the tile's **West and
    South** edges, not West/North. `Services/RaceTrack.cs` now ships
    three selectable `TrackDef` layouts built from these same three
    tiles - Grand Prix Loop (the original 4x6 rounded rectangle),
    Thunder Oval (the same shape at 6x4, resized), and Switchback
    Circuit (an L-shaped 6x6 layout with six corners instead of four) -
    see that file's own comments for how each corner's rotation is
    derived from its two actual neighbor tiles.
  - `tire-stack.png` and `grandstand.png` are infield decoration (purely
    cosmetic, `pointer-events: none`) - shown only on the two
    rectangular tracks, since Switchback Circuit's L-shaped infield has
    an empty notch the tuned decoration percentages weren't designed
    for. `finish-strip.png` is the start/finish banner and
    `checkered-flag.png` is used as the result overlay's title icon in
    place of an emoji.
  - Not used: a red/white traffic-cone-and-barrier set, a second curve-
    tile variant, a coin icon, and a stopwatch icon - none needed for
    this game's placement-race shape (no currency, no on-track
    obstacles, and the HUD shows lap/placement rather than a running
    timer).
- `space/*.png` (added 2026-08-18) are `ship_H.png`, `meteor_detailedLarge.png`,
  `meteor_detailedSmall.png`, `effect_yellow.png`, `star_tiny.png`, and
  `star_small.png` from [Kenney](https://kenney.nl)'s "Simple Space" pack
  (`kenney_simple-space.zip`), CC0. Used by `Components/SpaceGame.razor` -
  the ship is oriented purely via CSS `rotate()` (verified it points "up"
  by default before wiring the rotation formula), no animation pipeline
  needed. Renamed for clarity: `ship.png`, `meteor-large.png`,
  `meteor-small.png`, `thrust.png`, `star-tiny.png`, `star-small.png`.
- `tower-defense/*.png` (towers + coin only) are from
  [Kenney](https://kenney.nl) ("Tower Defense (top-down) Pack"), CC0. Pulled
  from `kenney_tower-defense-top-down.zip`'s 300 generically-numbered tiles
  (`towerDefense_tile203.png`, etc. - no XML atlas in that pack, so tiles
  were identified visually via a generated contact sheet):
  `tower-gatling.png` = tile203, `tower-cannon.png` = tile250,
  `tower-rocket.png` = tile205, `coin.png` = tile272. Terrain (grass/path) is
  plain CSS color rather than the pack's tiles - the terrain tiles are
  autotile edge/corner pieces meant to blend with neighbors, not flat
  seamless squares, so flat CSS reads cleaner for a grid this simple.
  Enemies switched from this pack's tanks (`enemy-tank.png`/
  `enemy-tank-tough.png`, since removed) to `memory/animals/*.png` - reads
  friendlier for a kids' game and reuses art already in the project instead
  of adding more.
- `effects/fusion-boom/frame-*.png`, `effects/goal-flash/frame-*.png`, and
  `effects/pocket-pop/frame-*.png` (10 frames each, renamed from
  `Explosion1.png`.."10.png" and equivalents for numeric-frame ordering)
  are three of the eleven 256x256 animation sets in
  [CraftPix](https://craftpix.net)'s free "11 Pixel Art Explosion Sprites"
  pack (`craftpix-net-270676-11-free-pixel-art-explosion-sprites.zip`),
  used under CraftPix's free-file license (see
  `https://craftpix.net/file-licenses/` bundled in the zip) - free to use
  in a project like this, not to be redistributed as standalone source
  assets. `fusion-boom` (the plain orange "Explosion" set) plays on Tower
  Defense's Mega-tower merge; `goal-flash` (`Explosion_blue_circle`, an icy
  blue burst matching the rink) plays on an Air Hockey goal; `pocket-pop`
  (`Circle_explosion`, a tighter orange radial burst) plays when Pool sinks
  a ball. All three swap frames via CSS `@keyframes` background-image
  changes rather than `steps()` + a spritesheet, since the source pack
  ships one PNG per frame instead of a laid-out atlas.
- `tanks/tank-body-desert.png` and `tanks/tank-body-navy.png` (added
  2026-08-18) are `tanks_tankDesert_body1.png`/`tanks_tankNavy_body1.png`
  from the same [Kenney](https://kenney.nl) "Tanks" pack as the existing
  green/grey bodies (`kenney_tanks.zip`) - confirmed identical 83x49
  dimensions to the existing bodies before wiring them into
  `Components/TankDuel.razor`'s new tank-color picker. Both pair with the
  existing `tank-barrel-grey.png` rather than the pack's third barrel
  color (`tanks_barrelRed.png`, not pulled in) - checked visually first,
  red clashed with both new hull colors.
- `fishing/seaweed-{1,2}.png`, `fishing/stone-{1,2}.png`, and
  `fishing/bubble-{1,2,3}.png` (added 2026-08-18) are `Seaweed_1.png`/
  `Seaweed_2.png`/`Stone_1.png`/`Stone_4.png`/`Bubble_1.png`/`Bubble_2.png`/
  `Bubble_3.png` from [CraftPix](https://craftpix.net)'s free "Underwater
  World 2D Game Objects" pack (`craftpix-901245-free-underwater-world-2d-
  game-objects.zip`), used under CraftPix's free-file license. Purely
  decorative background dressing in `Components/FishingGame.razor`'s
  `.fg-water` scene - the fish sprites themselves (`fishing/fish-*.png`)
  were deliberately left untouched since their color is the actual
  gameplay signal in Colors mode, not just decoration.
- `battleships/hit-fire.png` (added 2026-08-18) is `Fire4.png` from the
  same CraftPix "11 Pixel Art Explosion Sprites" pack already used for
  `effects/fusion-boom`/`goal-flash`/`pocket-pop` above
  (`craftpix-net-270676-11-free-pixel-art-explosion-sprites.zip`) - the
  pack's `Fire/` set (6 frames) hadn't been used yet. Only one frame was
  pulled in (not the full animated set like the other three effects)
  since a Battleships hit is permanent board state for the rest of the
  game, not a transient burst - a static "ship on fire" icon replacing
  the plain 🔥 emoji, rather than a looping animation that would keep
  playing on every hit cell simultaneously as they accumulate.
- `buildamonster/*.png` (added 2026-08-19) is the full `PNG/Default` set (178
  files) from [Kenney](https://kenney.nl)'s "Monster Builder Pack"
  (`kenney_monster-builder-pack.zip`), CC0 - real modular parts (arm/body/
  leg/eye/eyebrow/nose/mouth/detail/snot), not pre-assembled characters.
  `PNG/Double` (a two-tone stylistic variant) was left uncopied. Used by
  `Components/BuildAMonster.razor`, which hardcodes the file list per
  category rather than listing the directory at runtime. One correction
  made after viewing the actual art: `eye_blue.png` (the obvious-sounding
  default) renders as an X-marked "dead" eye, not a friendly round one -
  the component defaults to `eye_cute_dark.png` instead.
- `duckshoot/*.png` (added 2026-08-19) is a lean subset (8 of ~50 files)
  from [Kenney](https://kenney.nl)'s "Shooting Gallery" pack
  (`kenney_shooting-gallery.zip`), CC0: `duck_{yellow,white,brown}.png` and
  matching `duck_target_*.png` (used as hit feedback, not literally the
  pack's own intended meaning - repurposed since no distinct "hit/falling"
  duck sprite exists in the pack), plus `target_back.png` and
  `crosshair_red_small.png`. Used by `Components/DuckShoot.razor`. The
  pack's HUD digit sprites/curtain dressing were not pulled in - plain HTML
  text and a CSS gradient pond stand in for those.
- `dominoes/*.png` (added 2026-08-19) is the `Light` themed set (29 files:
  `tile_0_0.png` through `tile_6_6.png` plus `tile_empty.png`) from
  [Kenney](https://kenney.nl)'s "Domino Pack" (`kenney_domino-pack.zip`),
  CC0 - a complete standard double-six set. The other four themes (`Dark`,
  `Hearts`, `Stars`, `Gingerbread`) weren't copied, a cosmetic swap for
  later if wanted. Used by `Components/Dominoes.razor`. Art is portrait
  (verified by viewing `tile_3_4.png`: 3's pips on top, 4's on bottom) -
  the component rotates each placed tile 90deg to read left-to-right in a
  horizontal chain, direction computed per-tile from which value needs to
  end up on which side (see `Dominoes.razor`'s `ChainTileStyle`), not a
  fixed rotation.
- `dungeoncrawler/hero.png`, `dungeoncrawler/enemy-goblin.png`, and
  `dungeoncrawler/enemy-ogre.png` (added 2026-08-19) are single 32x32
  frames cropped from [CraftPix](https://craftpix.net)'s free "Top-Down
  Roguelike Game Kit" (`craftpix-net-436971-free-top-down-roguelike-game-
  kit-pixel-art.zip`), used under CraftPix's free-file license - the first
  frame of each character's `D_Idle.png` (a 128x32 4-frame sheet;
  `hero.png` from `1 Characters/1/`, the enemies from `3 Dungeon
  Enemies/1/` and `/2/`). Only a static pose is used, not the pack's full
  walk/attack/hurt/death animation sets or its Tiled dungeon map/tileset -
  floor and walls render as plain CSS in `Components/DungeonCrawler.razor`
  instead, the same "autotile art doesn't suit a grid this simple" call
  `Components/TowerDefense.razor` already made for its own terrain.
- `checkers/*.png` and `chess/*.png` are from a user-provided batch dropped
  in `Downloads/Assets/Checkers-Chess` on 2026-08-18 (33 UUID-named PNGs,
  no pack metadata - source/license unknown, unlike the Kenney/CraftPix
  packs above). Each file was viewed individually (not batched - a batched
  read previously shuffled sticker art against its filenames during Dress
  Up work, see git history) before being renamed and copied, so the
  mapping below is verified by eye, not by the original filename:
  - `checkers/black-piece.png`/`black-king.png` and `red-piece.png`/
    `red-king.png` are the plain/king pieces for a classic black-vs-red
    checkers set (flame emblem = plain, crown emblem = king, per color).
    `checkers/tile-dark.png`/`tile-light.png` are plain wood/cream board
    squares.
  - `chess/*-pawn-{1,2,3}.png` (3 face variants per color, for visual
    variety across 8 pawns), `*-rook.png`, `*-knight.png`,
    `*-queen-{1,2}.png` (2 face variants per color), and `*-king.png`
    are a full cute cartoon set for white and black - **except no bishop
    art exists in this batch for either color**.
    `Components/ChessPuzzles.razor` renders the bishop as a styled Unicode
    glyph (♗/♝) sized to match the other piece art rather than leaving it
    blank or mismatching the art style further - swap in real bishop PNGs
    here (`chess/white-bishop.png`/`chess/black-bishop.png`) if matching
    art turns up later. `chess/tile-dark.png`/`tile-light.png` are a more
    ornate carved-wood/marble board pair (kept visually distinct from
    checkers' plainer tiles). `chess/selected-glow.png` is a teal magic
    rune ring used as the selected-square/legal-move highlight.
  - Not used, left in `Downloads/Assets/Checkers-Chess`: three fantasy
    miniature figures (elf archer, goblin warrior, griffin-mounted knight -
    only one of each, can't fill both color slots of a piece type), a red
    "X" icon, a third "wood-brown" king color, and two plain side-view
    duplicate checkers pieces (black/red) that render the same piece from
    a different angle.
- `medals/best.png` is `PNG/shaded_medal6.png` from
  [Kenney](https://kenney.nl) ("Medals"), CC0 - the gold-and-blue ribbon
  read best as a generic "personal best" icon among the pack's 9 color
  variants. Used by `Components/Shared/PersonalBestBadge.razor`, the
  shared per-profile/per-game record badge (`AppDataService.GetBestAsync`/
  `TryRecordBestAsync`).

## Downloaded Kenney pack triage

The standard Downloads folder was reviewed on 2026-08-14. Keep using these
selectively rather than bulk-importing whole ZIPs:

- `kenney_animal-pack-remastered.zip` - best immediate fit. Used for Memory
  Match animal cards and Whack-a-Mole's temporary pop-up critter art. Also
  good for future Animal Sound Guessing and other child-friendly card games.
- `kenney_shooting-gallery.zip` - good fit for future Bubble Pop/Reaction
  Timer/target-tap variants. Not used in Whack-a-Mole because the target
  boards read more like carnival shooting than a soft kid game.
- `kenney_food-kit.zip` - strong fit for Catch the Falling Objects, Fruit
  Slice, food sorting, and Pet Care feeding.
- `kenney_cube-pets_1.0.zip` - strong fit for Pet Care Game.
- `kenney_monster-builder-pack.zip` - strong fit for Build-a-Monster.
- `kenney_space-shooter-remastered.zip`, `kenney_space-shooter-extension.zip`,
  and `kenney_simple-space.zip` - good fit for future real-time arcade games
  such as Endless Runner, Snake variants, or a kid-safe space dodger.
- `kenney_shape-characters.zip`, `kenney_scribble-platformer.zip`,
  `kenney_tiny-town.zip`, and `kenney_tiny-dungeon.zip` - useful later, but
  they need more game-specific implementation before importing assets.
- `kenney_medals.zip` - useful for shared reward/result overlays, not core
  gameplay art.
- `kenney_tower-defense-top-down.zip` - used for the Tower Defense game (see
  `tower-defense/*.png` above). `kenney_tower-defense.zip` (2015 isometric-ish
  cube style) and `kenney_tower-defense-kit.zip` (3D FBX models) were not
  used - both need a different rendering approach than this project's flat
  2D/CSS style.

Lulu Lamb and Tilly Turtle don't have matching art in the Kenney animal pack
(no sheep/lamb or turtle in that set) and still render as emoji - drop in
`manners/lulu-lamb.png` / `manners/tilly-turtle.png` and set `ImagePath` on
their `CharacterDef` in `MannersGarden.razor` to finish the set.
