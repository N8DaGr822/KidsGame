using KidsGameLauncher.Models;

namespace KidsGameLauncher.Services;

/// <summary>
/// Single source of truth for the games built into this app. GameHost
/// switches on a game's LaunchTarget to decide which component to render,
/// so the LaunchTarget keys here must stay in sync with GameHost.razor.
/// The admin "add game" picker lists these instead of asking a parent to
/// type an internal route key by hand.
/// </summary>
public static class BuiltInGames
{
    public const string MemoryMatch = "memory-match";
    public const string Fishing = "fishing-catch";
    public const string DressUp = "dress-up";
    public const string MannersGarden = "manners-garden";
    public const string TankDuel = "tank-duel";
    public const string Uno = "uno";
    public const string SimonSays = "simon-says";
    public const string SlidingPuzzle = "sliding-puzzle";
    public const string WordScramble = "word-scramble";
    public const string Minesweeper = "minesweeper";
    public const string Sudoku = "sudoku";
    public const string WhackAMole = "whack-a-mole";
    public const string CatchGame = "catch-the-falling-objects";
    public const string FruitSlice = "fruit-slice";
    public const string BubblePop = "bubble-pop";
    public const string ReactionTimer = "reaction-timer";
    public const string RedLightGreenLight = "red-light-green-light";
    public const string TicTacToe = "tic-tac-toe";
    public const string ConnectFour = "connect-four";
    public const string RockPaperScissors = "rock-paper-scissors";
    public const string HigherOrLower = "higher-or-lower";
    public const string PeekReveal = "peek-reveal";
    public const string SoundButtons = "sound-buttons";
    public const string PopAndSparkle = "pop-and-sparkle";
    public const string BabyPiano = "baby-piano";
    public const string MagicGarden = "magic-garden";
    public const string TowerDefense = "tower-defense";
    public const string OddOneOut = "odd-one-out";
    public const string PatternComplete = "pattern-complete";
    public const string NumberSequence = "number-sequence";

    public static readonly IReadOnlyList<BuiltInGame> All = new List<BuiltInGame>
    {
        new BuiltInGame("Memory Match", "🧠", MemoryMatch, "images/memory/animals/dog.png"),
        new BuiltInGame("Fishing Catch", "🎣", Fishing, "images/fishing/fish-blue.png"),
        new BuiltInGame("Dress Up", "👗", DressUp, "images/dressup/Girl.png"),
        new BuiltInGame("Manners Garden", "🌷", MannersGarden, "images/manners/benny-bear.png"),
        new BuiltInGame("Tank Duel", "🎯", TankDuel, "images/tanks/tank-body-green.png"),
        new BuiltInGame("UNO", "🃏", Uno),
        new BuiltInGame("Simon Says", "🔴", SimonSays),
        new BuiltInGame("Sliding Puzzle", "🧩", SlidingPuzzle),
        new BuiltInGame("Word Scramble", "🔤", WordScramble),
        new BuiltInGame("Minesweeper", "💣", Minesweeper),
        new BuiltInGame("Sudoku", "🔢", Sudoku),
        new BuiltInGame("Whack-a-Mole", "🐹", WhackAMole, "images/whack/mole.png"),
        new BuiltInGame("Catch the Falling Objects", "🧺", CatchGame),
        new BuiltInGame("Fruit Slice", "🍉", FruitSlice),
        new BuiltInGame("Bubble Pop", "🫧", BubblePop),
        new BuiltInGame("Reaction Timer", "⚡", ReactionTimer),
        new BuiltInGame("Red Light, Green Light", "🚦", RedLightGreenLight),
        new BuiltInGame("Tic-Tac-Toe", "⭕", TicTacToe),
        new BuiltInGame("Connect Four", "🔴", ConnectFour),
        new BuiltInGame("Rock Paper Scissors", "✊", RockPaperScissors),
        new BuiltInGame("Higher or Lower", "🎴", HigherOrLower),
        new BuiltInGame("Peek-a-Boo", "🎁", PeekReveal),
        new BuiltInGame("Sound Buttons", "🔔", SoundButtons),
        new BuiltInGame("Pop & Sparkle", "✨", PopAndSparkle),
        new BuiltInGame("Baby Piano", "🎹", BabyPiano),
        new BuiltInGame("Magic Garden", "🦋", MagicGarden),
        new BuiltInGame("Tower Defense", "🏯", TowerDefense, "images/tower-defense/tower-cannon.png"),
        new BuiltInGame("Odd One Out", "🔎", OddOneOut),
        new BuiltInGame("Pattern Complete", "🧩", PatternComplete),
        new BuiltInGame("Number Sequence", "🔢", NumberSequence),
    };
}
