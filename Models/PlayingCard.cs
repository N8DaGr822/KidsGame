namespace KidsGameLauncher.Models;

/// <summary>
/// A standard 52-card suit/rank model shared by any card game (Solitaire
/// today, potentially others later) that needs more than the inline
/// suit-emoji-plus-text approach HigherOrLower/UNO use for a single card.
/// </summary>
public enum CardSuit { Spades, Hearts, Diamonds, Clubs }

public class PlayingCard
{
    public required CardSuit Suit { get; init; }

    /// <summary>1 = Ace, 11 = Jack, 12 = Queen, 13 = King.</summary>
    public required int Rank { get; init; }

    public bool FaceUp { get; set; }

    public bool IsRed => Suit is CardSuit.Hearts or CardSuit.Diamonds;

    public string RankLabel => Rank switch
    {
        1 => "A",
        11 => "J",
        12 => "Q",
        13 => "K",
        _ => Rank.ToString(),
    };

    public string SuitGlyph => Suit switch
    {
        CardSuit.Spades => "♠",
        CardSuit.Hearts => "♥",
        CardSuit.Diamonds => "♦",
        _ => "♣",
    };
}
