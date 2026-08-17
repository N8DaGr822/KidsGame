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
    public const string ColorMatch = "color-match";
    public const string ShapeSorter = "shape-sorter";
    public const string ShadowMatch = "shadow-match";
    public const string GuessTheWord = "guess-the-word";
    public const string WordSearch = "word-search";
    public const string Mastermind = "mastermind";

    public static readonly IReadOnlyList<BuiltInGame> All = new List<BuiltInGame>
    {
        new BuiltInGame("Memory Match", "🧠", MemoryMatch, "images/memory/animals/dog.png", MinAge: 3, MaxAge: 8),
        new BuiltInGame("Fishing Catch", "🎣", Fishing, "images/fishing/fish-blue.png", MinAge: 3, MaxAge: 8),
        new BuiltInGame("Dress Up", "👗", DressUp, "images/dressup/Girl.png", MinAge: 3, MaxAge: 9),
        new BuiltInGame("Manners Garden", "🌷", MannersGarden, "images/manners/benny-bear.png", MinAge: 3, MaxAge: 7),
        new BuiltInGame("Tank Duel", "🎯", TankDuel, "images/tanks/tank-body-green.png", MinAge: 6, MaxAge: 12),
        new BuiltInGame("UNO", "🃏", Uno, MinAge: 6, MaxAge: 12),
        new BuiltInGame("Simon Says", "🔴", SimonSays, MinAge: 4, MaxAge: 9),
        new BuiltInGame("Sliding Puzzle", "🧩", SlidingPuzzle, MinAge: 5, MaxAge: 10),
        new BuiltInGame("Word Scramble", "🔤", WordScramble, MinAge: 5, MaxAge: 10),
        new BuiltInGame("Minesweeper", "💣", Minesweeper, MinAge: 6, MaxAge: 12),
        new BuiltInGame("Sudoku", "🔢", Sudoku, MinAge: 7, MaxAge: 13),
        new BuiltInGame("Whack-a-Mole", "🐹", WhackAMole, "images/whack/mole.png", MinAge: 3, MaxAge: 8),
        new BuiltInGame("Catch the Falling Objects", "🧺", CatchGame, MinAge: 4, MaxAge: 9),
        new BuiltInGame("Fruit Slice", "🍉", FruitSlice, MinAge: 4, MaxAge: 9),
        new BuiltInGame("Bubble Pop", "🫧", BubblePop, MinAge: 3, MaxAge: 8),
        new BuiltInGame("Reaction Timer", "⚡", ReactionTimer, MinAge: 5, MaxAge: 12),
        new BuiltInGame("Red Light, Green Light", "🚦", RedLightGreenLight, MinAge: 4, MaxAge: 9),
        new BuiltInGame("Tic-Tac-Toe", "⭕", TicTacToe, MinAge: 4, MaxAge: 10),
        new BuiltInGame("Connect Four", "🔴", ConnectFour, MinAge: 5, MaxAge: 11),
        new BuiltInGame("Rock Paper Scissors", "✊", RockPaperScissors, MinAge: 4, MaxAge: 10),
        new BuiltInGame("Higher or Lower", "🎴", HigherOrLower, MinAge: 5, MaxAge: 11),
        new BuiltInGame("Peek-a-Boo", "🎁", PeekReveal, MinAge: 1, MaxAge: 3),
        new BuiltInGame("Sound Buttons", "🔔", SoundButtons, MinAge: 1, MaxAge: 3),
        new BuiltInGame("Pop & Sparkle", "✨", PopAndSparkle, MinAge: 1, MaxAge: 3),
        new BuiltInGame("Baby Piano", "🎹", BabyPiano, MinAge: 1, MaxAge: 3),
        new BuiltInGame("Magic Garden", "🦋", MagicGarden, MinAge: 1, MaxAge: 3),
        new BuiltInGame("Tower Defense", "🏯", TowerDefense, "images/tower-defense/tower-cannon.png", MinAge: 8, MaxAge: 14),
        new BuiltInGame("Odd One Out", "🔎", OddOneOut, MinAge: 3, MaxAge: 7),
        new BuiltInGame("Pattern Complete", "🧩", PatternComplete, MinAge: 4, MaxAge: 8),
        new BuiltInGame("Number Sequence", "🔢", NumberSequence, MinAge: 4, MaxAge: 9),
        new BuiltInGame("Color Match", "🎨", ColorMatch, MinAge: 3, MaxAge: 7),
        new BuiltInGame("Shape Sorter", "🔷", ShapeSorter, MinAge: 3, MaxAge: 7),
        new BuiltInGame("Shadow Match", "🌑", ShadowMatch, MinAge: 4, MaxAge: 8),
        new BuiltInGame("Guess the Word", "🚀", GuessTheWord, MinAge: 5, MaxAge: 10),
        new BuiltInGame("Word Search", "🔍", WordSearch, MinAge: 6, MaxAge: 12),
        new BuiltInGame("Mastermind", "🎯", Mastermind, MinAge: 8, MaxAge: 14),
    };
}
