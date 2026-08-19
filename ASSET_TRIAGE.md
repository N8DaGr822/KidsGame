# Asset Triage

Notes about downloaded asset packs and how they fit the game roadmap. Keep
this file focused on reusable decisions, not every historical listing command.

## Rules

- Do not bulk-import asset packs.
- Copy only the sprites actually used by a landed game.
- Document copied subsets in `wwwroot/images/README.md`.
- Ignore vendor project scaffolding such as Unity `.meta`, `.prefab`, `.cs`,
  source PSD/EPS/AI files, and 3D-only formats unless the app adds a real 3D
  rendering pipeline.

## Currently Useful Packs

- `kenney_animal-pack-remastered.zip` - 2D animal sprites. Already used for
  Memory Match animal cards, Feed the Animal, and Whack-a-Mole's temporary
  critter sprite. Still useful for toddler animal games.
- `kenney_medals.zip` - 2D medal/ribbon icons. Used by the personal-best
  system.
- `kenney_tanks.zip` - 2D tanks. Used by Tank Duel; extra colors are useful
  only for visual variety.
- `kenney_pirate-pack.zip` - 2D ship art. Used by Battleships.
- `kenney_simple-space.zip` - 2D space sprites. Used by Space Survival; still
  useful for Space Trader / Space Mining if those get built.
- `kenney_tower-defense.zip` and `kenney_tower-defense-top-down.zip` - 2D
  tower/tile art. Tower Defense already has art; these are visual-variety
  candidates, not blockers.
- `kenney_toy-brick-pack.zip` - good fit for Stack the Blocks / Knock It Down.
- `kenney_pixel-vehicle-pack.zip` - good fit for Vehicle Sounds or future
  vehicle/racing variants.
- `kenney_new-platformer-pack-1.1.zip` and `kenney_scribble-platformer.zip` -
  useful once a platformer engine exists.
- `kenney_ui-pack-adventure.zip` - possible fantasy/RPG UI chrome.

## CraftPix RPG Kit

The CraftPix fantasy/top-down batches are the strongest current fit for:

- Dungeon Crawler
- Mini RPG Battle Game
- Roguelike Arena
- Auto Battler

Useful groups:

- Hero/class sprites: knight, elf, assassin/mage/viking.
- Enemy sprites: goblin, trolls, demons, dragon/jinn monsters, bosses.
- Top-down roguelike kit: closest camera match for Dungeon Crawler /
  Roguelike Arena.
- Environment packs: forest objects, ruins, rocks, bushes, castle/city
  tilesets.
- Effects/UI: pixel explosions and RPG-style HUD/inventory chrome.
- Loot/icons: fruit/vegetable RPG icon packs for item drops or shops.

Pull only PNG subfolders. Do not import Unity project files or source art.

## Low-Priority Or Not Useful Right Now

- `kenney_food-kit.zip` - re-verified as 3D/texture-focused, not usable 2D
  food sprites for this app.
- `kenney_blocky-characters_20.zip`, `kenney_car-kit.zip`,
  `kenney_mini-characters.zip`, `kenney_mini-forest_1.0.zip`,
  `kenney_tower-defense-kit.zip` - 3D FBX-oriented, not usable as-is.
- `craftpix-889156-free-racing-game-kit.zip` - rejected for Top-Down Racing;
  cars are pre-rendered at only a few fixed angles.
- Military boat and pirate character packs - no current game needs them.
- Generic scene-dressing packs - re-triage when a specific game needs them.
