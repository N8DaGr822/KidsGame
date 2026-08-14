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
