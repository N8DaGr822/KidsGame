# Asset Triage

Notes about downloaded asset packs and how they fit the game roadmap. Keep
this file focused on reusable decisions, not every historical listing command.

Source folder: `C:\Users\nathen.lentz\Downloads\Assets` (zips are left
untouched there - only the specific sprites a landed game actually needs get
copied into `wwwroot/images/`).

## Rules

- Do not bulk-import asset packs.
- Copy only the sprites actually used by a landed game.
- Document copied subsets in `wwwroot/images/README.md`.
- Ignore vendor project scaffolding such as Unity `.meta`, `.prefab`, `.cs`,
  source PSD/EPS/AI files, and 3D-only formats unless the app adds a real 3D
  rendering pipeline.

## Currently Useful Packs

Already used by a landed game - see `wwwroot/images/README.md` for exactly
which files were copied.

- `kenney_animal-pack-remastered.zip` - 2D animal sprites. Used for Memory
  Match animal cards, Feed the Animal, and Whack-a-Mole's temporary critter
  sprite. Still useful for toddler animal games.
- `kenney_medals.zip` - 2D medal/ribbon icons. Used by the personal-best
  system.
- `kenney_tanks.zip` - 2D tanks. Used by Tank Duel; extra colors are useful
  only for visual variety.
- `kenney_pirate-pack.zip` - 2D ship art. Used by Battleships.
- `kenney_simple-space.zip` - 2D space sprites. Used by Space Survival.
- `kenney_tower-defense.zip` and `kenney_tower-defense-top-down.zip` - 2D
  tower/tile art. Tower Defense already has art from these; see "Tower
  Defense Variety" below for more enemy/tileset options.
- `kenney_toy-brick-pack.zip` - good fit for Stack the Blocks / Knock It Down.
- `kenney_pixel-vehicle-pack.zip` - good fit for Vehicle Sounds or future
  vehicle/racing variants.
- `kenney_new-platformer-pack-1.1.zip` and `kenney_scribble-platformer.zip` -
  useful once a platformer engine exists.
- `kenney_ui-pack-adventure.zip` - possible fantasy/RPG UI chrome (see also
  `craftpix-852098-free-tds-modern-gui-pixel-art.zip` below, a fuller
  alternative).
- `kenney_monster-builder-pack.zip` - real modular monster parts. Used by
  Build-a-Monster (full `PNG/Default` set, 178 files).
- `kenney_shooting-gallery.zip` - a complete carnival duck-shoot kit. Used
  by Duck Shoot (a lean 8-file subset).
- `kenney_domino-pack.zip` - a complete double-six domino set in 5 cosmetic
  themes. Used by Dominoes (`Light` theme, 29 files) - the other 4 themes
  are still available for a cosmetic variant later.
- The `Checkers-Chess` and `RaceCar` folders (loose UUID-named drops, not
  zips) are fully catalogued already - see `wwwroot/images/README.md`. Both
  are shipped (Checkers/Chess/Chess Puzzles, Top-Down Racing/Time Trial
  Racer).

## RPG / Dungeon Crawler Kit

The strongest current fit for roadmap priority #4 (Dungeon Crawler, Mini RPG
Battle Game, Roguelike Arena, Auto Battler, Card Battle Game). Verified by
viewing sample sprites from each pack, not guessed from filenames alone.

**Heroes** (playable-character sprite sheets, all confirmed genuine top-down
or 3/4-view pixel art, not modular parts):
- `craftpix-891165-assassin-mage-viking-free-pixel-art-game-heroes.zip` -
  Knight/Mage/Rogue, three classes. Verified: clean top-down pixel hero,
  good default for a starting hero roster.
- `craftpix-062999-2d-fantasy-knight-free-sprite-sheets.zip` - knight,
  separate sprite-sheet PNGs per pose/animation (also ships an `_SCML` rig
  variant, ignore that - the flat PNGs are simpler to use).
- `craftpix-392011-2d-fantasy-elf-free-sprite-sheets.zip` - elf archer, same
  shape as the knight pack.

**Enemies / monsters:**
- `craftpix-561178-free-rpg-monster-sprites-pixel-art.zip` (demon, dragon,
  jinn, lizard, medusa, small_dragon) - verified high quality, but these are
  **side-view battle portraits**, not top-down movement sprites. Best fit
  for a turn-based battle screen (Mini RPG Battle Game, Card Battle Game),
  not for walking around a dungeon.
- `craftpix-986711-2d-fantasy-trolls-free-sprite-sheets.zip` - troll enemy,
  same sprite-sheet shape as the hero packs (top-down/3/4-view, not
  battle-portrait style).
- `craftpix-341189-free-2d-monster-sprites.zip` and
  `craftpix-437811-free-monster-enemy-game-sprites.zip` - 10 distinct
  monsters each (folders `1`-`10`), general enemy variety pool.
- `craftpix-net-168163-free-monster-enemy-sprites-for-tower-defense.zip` -
  10 more monster folders, explicitly TD-oriented (see "Tower Defense
  Variety" below).

**Bosses - integration caveat:** `craftpix-net-176111-free-tribal-warrior-boss-characters-asset-pack.zip`
(Aztec/Maya/Nordic), `craftpix-net-907874-free-top-down-boss-character-4-direction-pack.zip`
(Caveman/Giant/Viking), and `craftpix-net-228980-free-top-down-goblin-character-sprite.zip`
(Chief/Female/Male) were all verified to ship as **modular body-part PNGs**
("Body - Front.png", "Right Arm - Front.png", etc.), not flat animated
sprite sheets - they need a compositing/rigging step (or Spine/DragonBones)
to actually pose and animate. Meaningfully higher integration cost than
every other pack here - defer until a boss fight actually needs one, don't
plan around them casually.

**Dungeon/roguelike kit:**
- `craftpix-net-436971-free-top-down-roguelike-game-kit-pixel-art.zip` -
  verified: genuine tiny top-down character walk-cycles plus a ready Tiled
  `.tmx` dungeon map and tileset. This is the actual closest match for
  building Dungeon Crawler / Roguelike Arena - start here.
- `kenney_tiny-dungeon.zip` - lightweight alternative/starter dungeon
  tileset + character portraits, also Tiled-ready. Good for a smaller-scope
  first pass before reaching for the CraftPix kit's larger footprint.
- `kenney_roguelike-characters.zip` - simple top-down character spritesheet
  (magenta-key and pre-cut transparent versions), a lighter-weight character
  option than the hero packs above.

**Environment props** (top-down, drop into any dungeon/overworld tile grid):
- `craftpix-net-505052-free-forest-objects-top-down-pixel-art.zip`
- `craftpix-net-675652-free-rocks-pixel-art-asset-pack.zip`
- `craftpix-net-699134-free-bush-assets-pixel-art-pack.zip`
- `craftpix-net-668008-free-bridges-top-down-pixel-art-asset-pack.zip`
- `craftpix-net-934618-free-top-down-ruins-pixel-art.zip`
- `craftpix-234566-free-castle-2d-game-assets.zip` - verified: this one is a
  full painted background scene (castle + mountains), not tile props - use
  as a backdrop, not a tileset.

**UI chrome:**
- `craftpix-852098-free-tds-modern-gui-pixel-art.zip` - a complete HUD kit
  (HUD, Inventory, Levels, Loading, Main menu, Minimap, Mission, Pause,
  Settings, Upgrade, Victory folders). Worth reaching for before hand-rolling
  more CSS chrome once an RPG-shaped game exists.
- `craftpix-net-280167-free-level-map-pixel-art-assets-pack.zip` - a
  level-select/world-map screen kit, useful for any game with a
  level-progression structure.

**Loot / crafting icons** (numbered icon sets, useful for shop/inventory UI
or item drops in an RPG, or Simulation/Management games like Shop
Simulator/Restaurant Manager):
- `craftpix-net-576335-free-alchemy-plants-game-icons.zip`
- `craftpix-net-688754-free-minerals-pixel-art-icons.zip` - verified: small
  clean gem/mineral icons.
- `craftpix-net-628761-free-mining-pixel-32x32-icons.zip`
- `craftpix-net-717437-free-vegetables-vector-icon-pack-for-rpg.zip`
- `craftpix-net-772742-free-fruit-vector-icon-pack-for-rpg.zip`

**Effects:**
- `craftpix-net-270676-11-free-pixel-art-explosion-sprites.zip` - verified:
  small transparent explosion/fire/lightning sprites, 11 variants. Same
  purpose as the existing `wwwroot/images/effects/` set (fusion-boom,
  goal-flash, pocket-pop) - good for adding impact-effect variety to Air
  Hockey/Pool/Tank Duel or a new RPG's combat feedback.

## Tower Defense Variety

Tower Defense already shipped with art from `kenney_tower-defense.zip` /
`kenney_tower-defense-top-down.zip`. These add enemy and tileset variety if
it gets a content refresh, or could seed a second, differently-themed TD
game:

- `craftpix-net-168163-free-monster-enemy-sprites-for-tower-defense.zip` -
  10 monster folders, purpose-built for this genre.
- `craftpix-net-397030-free-cartoon-cat-defense-game-asset-kit.zip` - a
  complete themed kit (cat defenders + enemies) - distinct enough in tone to
  be its own game rather than a reskin.
- `craftpix-net-305231-free-tower-defense-2d-vector-tileset.zip` - background
  and tileset variety.

## Toddler & Casual Game Fits

Packs that match specific items in `GAMES_ROADMAP.md`'s toddler and
simulation sections, or suggest a small addition not yet on the list.

- `kenney_shape-characters.zip` - verified: geometric shapes (square,
  triangle, diamond, circle) with cute faces. Strong fit for the toddler
  "Big / Small" or "Funny Faces" roadmap items, or as bonus characters
  anywhere a friendly mascot helps.
- `craftpix-901245-free-underwater-world-2d-game-objects.zip` - verified:
  bubbles and small decorative underwater objects. Good for Fishing Catch
  visual variety, or the roadmap's "Baby Aquarium" / "Touch the Fish"
  toddler items.
- `craftpix-net-789196-free-top-down-hunt-animals-pixel-sprite-pack.zip`
  (Black Grouse, Boar, Deer, Fox, Hare) - top-down forest-animal sprites,
  good for toddler animal games or a foraging mechanic in a future
  overworld game.
- `kenney_1-bit-pack.zip` - a very large single-style icon/tile spritesheet
  spanning fantasy, interior, platformer, and urban themes (weapons, UI
  icons, numbers/letters, arrows, skulls, and hundreds more). Broad utility
  as a fallback icon source for almost any future game, and a legitimate
  platformer tileset if Platformer/Ninja Wall Jump gets built.

## City / Simulation Building Blocks

For the roadmap's Simulation/Management section (City Builder Lite,
Restaurant Manager, Shop Simulator) once that infrastructure is worth
building:

- `kenney_tiny-farm.zip` - farm-themed tileset, fits a Restaurant
  Manager/Shop Simulator supply chain or a farm-themed toddler game.
- `kenney_tiny-town.zip` and `kenney_pico-8-city.zip` - small city
  tilesets, Tiled-ready, fit City Builder Lite.
- `kenney_tiny-battle.zip` - small tactics/wargame tileset, fits Auto
  Battler or a Hex/Territory Capture game.
- `craftpix-988114-free-tropical-medieval-city-2d-tileset.zip` - a much
  larger, more detailed city tileset (buildings/decor/land/road folders) -
  reach for this over the Kenney options if City Builder Lite ends up
  wanting real visual depth instead of a minimal placeholder look.

## Space / Shooter

- `kenney_space-shooter-extension.zip` and
  `kenney_space-shooter-remastered.zip` - could refresh/extend the already-
  shipped Space Survival, or seed a second twin-stick shooter.
- `craftpix-net-814823-free-roguelike-shoot-em-up-pixel-art-game-kit.zip` -
  fits a future Space Trader/Space Mining or dedicated shoot-em-up.

## Background / Scenery Packs

Broad seasonal and thematic variety, useful as backdrops across many future
games rather than tied to one:

- `craftpix-665532-free-fairy-tale-game-backgrounds.zip`
- `craftpix-891165-free-winter-holiday-game-backgrounds.zip`
- `craftpix-net-799827-free-mountain-backgrounds-pixel-art-unity.zip`
- `craftpix-net-381103-free-simple-summer-top-down-vector-tileset.zip`
- `craftpix-net-686291-free-pixel-art-fantasy-2d-battlegrounds-unity.zip` -
  each scene ships as 5-8 separate depth layers (`Layer_1.png`...`Layer_8.png`)
  for true parallax scrolling, not a flat image. The best-suited background
  pack here for the roadmap's Endless Runner, or a scrolling RPG battle
  screen.
- `kenney_background-elements-remastered.zip` and
  `kenney_foliage-sprites.zip` - lighter-weight decoration/scene-dressing
  pieces rather than full scenes.

## Physics / Climbing

- `craftpix-788112-free-stone-tower-game-assets.zip` - ~27 numbered PNGs of
  stone-tower climbing pieces. Fits the roadmap's Bridge Builder or Physics
  Puzzle items, or works as castle-theme scene dressing if a physics game
  doesn't happen.

## Low-Priority Or Not Useful Right Now

- `kenney_food-kit.zip`, `kenney_blocky-characters_20.zip`,
  `kenney_car-kit.zip`, `kenney_mini-characters.zip`,
  `kenney_mini-forest_1.0.zip`, `kenney_tower-defense-kit.zip`, and
  `kenney_cube-pets_1.0.zip` - all 3D FBX/GLB/OBJ model packs (confirmed via
  their `Models/` folder structure), not usable as-is in this CSS/HTML
  rendering app.
- `craftpix-889156-free-racing-game-kit.zip` - rejected for Top-Down Racing;
  cars are pre-rendered at only a few fixed angles. (The racing games shipped
  with a different, user-provided car batch instead - see
  `wwwroot/images/README.md`.)
- `craftpix-901180-free-2d-pirate-character-sprites.zip` - a pirate captain
  character (ships as an `_SCML` rig, same modular-parts caveat as the boss
  packs above). Battleships already has ship art from `kenney_pirate-pack`;
  this would only matter if a game ever wants a pirate character avatar, not
  just ships.
- `craftpix-net-578218-free-top-down-military-boats-pixel-art.zip` - no
  current game needs boats.
- Generic scene-dressing packs not listed above - re-triage when a specific
  game needs them.
