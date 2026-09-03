namespace KidsGameLauncher.Services;

/// <summary>
/// Maps a game's LaunchTarget to a curated background theme class for
/// GameHost's play surface (see .gh-theme-* in app.css) - a lightweight
/// stand-in for a bespoke look per game (85+ games makes that impractical)
/// while still giving each broad category of game its own "world" feel
/// instead of every game sitting on the same flat dark shell.
///
/// Themes stay dark-mode-compatible tinted gradients, not literal light
/// pastels - GameHud/StatPill/GameOverlay all use light text tuned for
/// the existing dark panel/control tokens, so a bright background would
/// break contrast for every migrated game. Dress Up keeps its own
/// per-scene background system and is deliberately left unmapped here
/// (falls through to the neutral default, which is visually inert since
/// DressUpGame renders full-bleed over it anyway).
/// </summary>
public static class GameThemes
{
    private static readonly Dictionary<string, string> ByLaunchTarget = new()
    {
        // ---- toddler: the ~1-3yo sensory/cause-and-effect set ----
        [BuiltInGames.PeekReveal] = "gh-theme-toddler",
        [BuiltInGames.SoundButtons] = "gh-theme-toddler",
        [BuiltInGames.PopAndSparkle] = "gh-theme-toddler",
        [BuiltInGames.BabyPiano] = "gh-theme-toddler",
        [BuiltInGames.MagicGarden] = "gh-theme-toddler",
        [BuiltInGames.FeedTheAnimal] = "gh-theme-toddler",
        [BuiltInGames.BigSmall] = "gh-theme-toddler",
        [BuiltInGames.ColorSplash] = "gh-theme-toddler",
        [BuiltInGames.FingerTrails] = "gh-theme-toddler",
        [BuiltInGames.SleepyAnimals] = "gh-theme-toddler",
        [BuiltInGames.GentleCreatures] = "gh-theme-toddler",
        [BuiltInGames.NightSky] = "gh-theme-toddler",
        [BuiltInGames.FireworksTouch] = "gh-theme-toddler",
        [BuiltInGames.LightsOnOff] = "gh-theme-toddler",
        [BuiltInGames.SpinTheWheel] = "gh-theme-toddler",
        [BuiltInGames.MakeItRain] = "gh-theme-toddler",
        [BuiltInGames.SnowDay] = "gh-theme-toddler",
        [BuiltInGames.PuddleSplash] = "gh-theme-toddler",
        [BuiltInGames.BathTime] = "gh-theme-toddler",
        [BuiltInGames.FunnyFaces] = "gh-theme-toddler",
        [BuiltInGames.TouchTheBodyPart] = "gh-theme-toddler",
        [BuiltInGames.StackTheBlocks] = "gh-theme-toddler",
        [BuiltInGames.KnockItDown] = "gh-theme-toddler",

        // ---- nature: animals, gardens, water ----
        [BuiltInGames.MemoryMatch] = "gh-theme-nature",
        [BuiltInGames.Fishing] = "gh-theme-nature",
        [BuiltInGames.MannersGarden] = "gh-theme-nature",
        [BuiltInGames.AnimalSoundGuessingGame] = "gh-theme-nature",
        [BuiltInGames.DuckShoot] = "gh-theme-nature",

        // ---- cardtable: card/board games played at a table ----
        [BuiltInGames.Uno] = "gh-theme-cardtable",
        [BuiltInGames.Solitaire] = "gh-theme-cardtable",
        [BuiltInGames.SlotMachine] = "gh-theme-cardtable",
        [BuiltInGames.HigherOrLower] = "gh-theme-cardtable",
        [BuiltInGames.Dominoes] = "gh-theme-cardtable",
        [BuiltInGames.Checkers] = "gh-theme-cardtable",
        [BuiltInGames.ChessPuzzles] = "gh-theme-cardtable",
        [BuiltInGames.ChessGame] = "gh-theme-cardtable",
        [BuiltInGames.TicTacToe] = "gh-theme-cardtable",
        [BuiltInGames.ConnectFour] = "gh-theme-cardtable",
        [BuiltInGames.Reversi] = "gh-theme-cardtable",
        [BuiltInGames.DotsAndBoxes] = "gh-theme-cardtable",
        [BuiltInGames.Battleships] = "gh-theme-cardtable",
        [BuiltInGames.RockPaperScissors] = "gh-theme-cardtable",
        [BuiltInGames.FollowTheCups] = "gh-theme-cardtable",
        [BuiltInGames.Mastermind] = "gh-theme-cardtable",
        [BuiltInGames.CodeBreaker] = "gh-theme-cardtable",

        // ---- puzzle: logic, word, and number puzzles ----
        [BuiltInGames.SlidingPuzzle] = "gh-theme-puzzle",
        [BuiltInGames.WordScramble] = "gh-theme-puzzle",
        [BuiltInGames.Minesweeper] = "gh-theme-puzzle",
        [BuiltInGames.Sudoku] = "gh-theme-puzzle",
        [BuiltInGames.OddOneOut] = "gh-theme-puzzle",
        [BuiltInGames.PatternComplete] = "gh-theme-puzzle",
        [BuiltInGames.NumberSequence] = "gh-theme-puzzle",
        [BuiltInGames.QuickMath] = "gh-theme-puzzle",
        [BuiltInGames.MathTarget] = "gh-theme-puzzle",
        [BuiltInGames.JigsawPuzzle] = "gh-theme-puzzle",
        [BuiltInGames.SpotTheDifference] = "gh-theme-puzzle",
        [BuiltInGames.ColorMatch] = "gh-theme-puzzle",
        [BuiltInGames.ShapeSorter] = "gh-theme-puzzle",
        [BuiltInGames.ShadowMatch] = "gh-theme-puzzle",
        [BuiltInGames.GuessTheWord] = "gh-theme-puzzle",
        [BuiltInGames.WordSearch] = "gh-theme-puzzle",
        [BuiltInGames.Cryptogram] = "gh-theme-puzzle",
        [BuiltInGames.MorseCodeChallenge] = "gh-theme-puzzle",
        [BuiltInGames.RobotCommands] = "gh-theme-puzzle",
        [BuiltInGames.WordLadder] = "gh-theme-puzzle",
        [BuiltInGames.TriviaBattle] = "gh-theme-puzzle",
        [BuiltInGames.FlagGuessingGame] = "gh-theme-puzzle",
        [BuiltInGames.Puzzle2048] = "gh-theme-puzzle",
        [BuiltInGames.CopyThePattern] = "gh-theme-puzzle",
        [BuiltInGames.MemorySequenceAdventure] = "gh-theme-puzzle",

        // ---- action: reflex, sports, and arcade-style targeting ----
        [BuiltInGames.WhackAMole] = "gh-theme-action",
        [BuiltInGames.CatchGame] = "gh-theme-action",
        [BuiltInGames.FruitSlice] = "gh-theme-action",
        [BuiltInGames.BubblePop] = "gh-theme-action",
        [BuiltInGames.ReactionTimer] = "gh-theme-action",
        [BuiltInGames.RedLightGreenLight] = "gh-theme-action",
        [BuiltInGames.ArcheryChallenge] = "gh-theme-action",
        [BuiltInGames.BasketballShot] = "gh-theme-action",
        [BuiltInGames.PenaltyShootout] = "gh-theme-action",
        [BuiltInGames.LaserMaze] = "gh-theme-action",
        [BuiltInGames.RhythmGame] = "gh-theme-action",
        [BuiltInGames.AirHockey] = "gh-theme-action",
        [BuiltInGames.PoolBilliards] = "gh-theme-action",
        [BuiltInGames.MiniGolf] = "gh-theme-action",
        [BuiltInGames.HotAndCold] = "gh-theme-action",
        [BuiltInGames.TreasureHunt] = "gh-theme-action",
        [BuiltInGames.RacingGame] = "gh-theme-action",
        [BuiltInGames.TimeTrialRacer] = "gh-theme-action",

        // ---- adventure: battle, dungeon, and sci-fi games ----
        [BuiltInGames.TankDuel] = "gh-theme-adventure",
        [BuiltInGames.TowerDefense] = "gh-theme-adventure",
        [BuiltInGames.DungeonCrawler] = "gh-theme-adventure",
        [BuiltInGames.RpgBattle] = "gh-theme-adventure",
        [BuiltInGames.CatDefense] = "gh-theme-adventure",
        [BuiltInGames.SpaceGame] = "gh-theme-adventure",
        [BuiltInGames.RoguelikeArena] = "gh-theme-adventure",
        [BuiltInGames.UltimateTicTacToe] = "gh-theme-puzzle",
        [BuiltInGames.Hex] = "gh-theme-puzzle",
        [BuiltInGames.SequencePuzzle] = "gh-theme-puzzle",
        [BuiltInGames.LogicGridPuzzle] = "gh-theme-puzzle",
        [BuiltInGames.Nonogram] = "gh-theme-puzzle",
        [BuiltInGames.ThreesPuzzle] = "gh-theme-puzzle",
    };

    public static string For(string launchTarget) =>
        ByLaunchTarget.GetValueOrDefault(launchTarget, "");
}
