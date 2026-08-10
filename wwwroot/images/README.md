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

- `fishing/*.png` and `manners/benny-bear.png`, `manners/poppy-penguin.png`,
  `manners/milo-monkey.png` are from [Kenney](https://kenney.nl) ("Fish Pack
  2" and "Animal Pack Remastered"), CC0 - free for any use, no attribution
  required (credit given anyway because it's a great resource).
- `dressup/*.png` - see git history/commit messages for provenance if adding
  more from the same source.

Lulu Lamb and Tilly Turtle don't have matching art in the Kenney animal pack
(no sheep/lamb or turtle in that set) and still render as emoji - drop in
`manners/lulu-lamb.png` / `manners/tilly-turtle.png` and set `ImagePath` on
their `CharacterDef` in `MannersGarden.razor` to finish the set.
