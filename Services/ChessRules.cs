namespace KidsGameLauncher.Services;

/// <summary>
/// A real (if intentionally scoped) chess rules engine backing
/// Components/ChessPuzzles.razor: full legal move generation per piece
/// type with proper check/pin filtering (a move that leaves your own
/// king in check is illegal), plus check/checkmate/stalemate detection.
/// Deliberately omits castling and en passant - the curated mate-in-1/
/// mate-in-2 puzzle bank never needs either, and every puzzle position is
/// hand-placed rather than reached by playing from the start, so there
/// are no castling rights to track in the first place. Pawn promotion
/// always promotes to a queen (no underpromotion UI) since no puzzle in
/// the bank needs anything else.
///
/// Board encoding: int[8,8], row 0 = rank 8 (black's back rank, top of
/// the board as normally drawn), row 7 = rank 1 (white's back rank,
/// bottom). Positive = white, negative = black, magnitude = piece type
/// (1=Pawn, 2=Knight, 3=Bishop, 4=Rook, 5=Queen, 6=King). 0 = empty.
/// </summary>
public static class ChessRules
{
    public const int Size = 8;
    public const int Empty = 0;
    public const int Pawn = 1;
    public const int Knight = 2;
    public const int Bishop = 3;
    public const int Rook = 4;
    public const int Queen = 5;
    public const int King = 6;

    public record struct Move(int FromRow, int FromCol, int ToRow, int ToCol);

    public static bool InBounds(int r, int c) => r >= 0 && r < Size && c >= 0 && c < Size;

    public static bool IsWhite(int piece) => piece > 0;
    public static bool IsBlack(int piece) => piece < 0;
    public static int PieceType(int piece) => Math.Abs(piece);

    /// <summary>Parses 8 rows of 8 characters each (rank 8 first) into a
    /// board: PNBRQK for white, pnbrqk for black, '.' for empty. Throws if
    /// a row isn't exactly 8 characters - every puzzle position is
    /// hand-authored, so a malformed row is a bug worth failing loudly on
    /// rather than silently misplacing pieces.</summary>
    public static int[,] ParseBoard(string[] rows)
    {
        if (rows.Length != Size) throw new ArgumentException($"Board must have {Size} rows, got {rows.Length}");
        var board = new int[Size, Size];
        for (var r = 0; r < Size; r++)
        {
            if (rows[r].Length != Size) throw new ArgumentException($"Row {r} must have {Size} chars, got '{rows[r]}'");
            for (var c = 0; c < Size; c++)
            {
                board[r, c] = rows[r][c] switch
                {
                    'P' => Pawn, 'N' => Knight, 'B' => Bishop, 'R' => Rook, 'Q' => Queen, 'K' => King,
                    'p' => -Pawn, 'n' => -Knight, 'b' => -Bishop, 'r' => -Rook, 'q' => -Queen, 'k' => -King,
                    '.' => Empty,
                    var ch => throw new ArgumentException($"Unknown board char '{ch}' at row {r} col {c}"),
                };
            }
        }
        return board;
    }

    /// <summary>Algebraic square name ("e4") for display/hints.</summary>
    public static string SquareName(int row, int col) => $"{(char)('a' + col)}{Size - row}";

    private static readonly (int Dr, int Dc)[] KnightOffsets =
    {
        (-2, -1), (-2, 1), (-1, -2), (-1, 2), (1, -2), (1, 2), (2, -1), (2, 1),
    };

    private static readonly (int Dr, int Dc)[] BishopDirs = { (-1, -1), (-1, 1), (1, -1), (1, 1) };
    private static readonly (int Dr, int Dc)[] RookDirs = { (-1, 0), (1, 0), (0, -1), (0, 1) };
    private static readonly (int Dr, int Dc)[] KingOffsets =
    {
        (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1),
    };

    /// <summary>True if any piece of the given color attacks (row, col) -
    /// raw attack pattern (e.g. a pawn's diagonal), not a full legal move
    /// (destination doesn't need to be empty for this check). Used for
    /// check detection and for filtering king moves into attacked squares.</summary>
    public static bool IsSquareAttacked(int[,] board, int row, int col, bool byWhite)
    {
        var dir = byWhite ? -1 : 1; // white pawns attack "upward" (toward row 0)
        foreach (var dc in new[] { -1, 1 })
        {
            var r = row - dir; // the attacking pawn sits one row behind the attacked square, from the pawn's own advancing direction
            var c = col + dc;
            if (InBounds(r, c) && board[r, c] == (byWhite ? Pawn : -Pawn)) return true;
        }

        foreach (var (dr, dc) in KnightOffsets)
        {
            var r = row + dr; var c = col + dc;
            if (InBounds(r, c) && board[r, c] == (byWhite ? Knight : -Knight)) return true;
        }

        foreach (var (dr, dc) in KingOffsets)
        {
            var r = row + dr; var c = col + dc;
            if (InBounds(r, c) && board[r, c] == (byWhite ? King : -King)) return true;
        }

        foreach (var (dr, dc) in BishopDirs)
        {
            var r = row + dr; var c = col + dc;
            while (InBounds(r, c))
            {
                var cell = board[r, c];
                if (cell != Empty)
                {
                    if (cell == (byWhite ? Bishop : -Bishop) || cell == (byWhite ? Queen : -Queen)) return true;
                    break;
                }
                r += dr; c += dc;
            }
        }

        foreach (var (dr, dc) in RookDirs)
        {
            var r = row + dr; var c = col + dc;
            while (InBounds(r, c))
            {
                var cell = board[r, c];
                if (cell != Empty)
                {
                    if (cell == (byWhite ? Rook : -Rook) || cell == (byWhite ? Queen : -Queen)) return true;
                    break;
                }
                r += dr; c += dc;
            }
        }

        return false;
    }

    private static (int Row, int Col) FindKing(int[,] board, bool white)
    {
        var target = white ? King : -King;
        for (var r = 0; r < Size; r++)
            for (var c = 0; c < Size; c++)
                if (board[r, c] == target) return (r, c);
        throw new InvalidOperationException($"No {(white ? "white" : "black")} king on the board - every puzzle position must include both kings.");
    }

    public static bool IsInCheck(int[,] board, bool white)
    {
        var (kr, kc) = FindKing(board, white);
        return IsSquareAttacked(board, kr, kc, !white);
    }

    /// <summary>Pseudo-legal moves for the piece at (row, col) - obeys
    /// piece movement/capture rules but does not yet filter out moves that
    /// leave the mover's own king in check. See LegalMovesFor for that.</summary>
    private static List<Move> PseudoMovesFor(int[,] board, int row, int col)
    {
        var piece = board[row, col];
        var moves = new List<Move>();
        if (piece == Empty) return moves;
        var white = IsWhite(piece);
        var type = PieceType(piece);

        bool FriendlyAt(int r, int c) => InBounds(r, c) && board[r, c] != Empty && (IsWhite(board[r, c]) == white);
        bool EnemyAt(int r, int c) => InBounds(r, c) && board[r, c] != Empty && (IsWhite(board[r, c]) != white);

        switch (type)
        {
            case Pawn:
                var dir = white ? -1 : 1;
                var startRow = white ? Size - 2 : 1;
                var oneStep = row + dir;
                if (InBounds(oneStep, col) && board[oneStep, col] == Empty)
                {
                    moves.Add(new Move(row, col, oneStep, col));
                    var twoStep = row + 2 * dir;
                    if (row == startRow && board[twoStep, col] == Empty)
                    {
                        moves.Add(new Move(row, col, twoStep, col));
                    }
                }
                foreach (var dc in new[] { -1, 1 })
                {
                    var tr = row + dir; var tc = col + dc;
                    if (EnemyAt(tr, tc)) moves.Add(new Move(row, col, tr, tc));
                }
                break;

            case Knight:
                foreach (var (dr, dc) in KnightOffsets)
                {
                    var r = row + dr; var c = col + dc;
                    if (InBounds(r, c) && !FriendlyAt(r, c)) moves.Add(new Move(row, col, r, c));
                }
                break;

            case King:
                foreach (var (dr, dc) in KingOffsets)
                {
                    var r = row + dr; var c = col + dc;
                    if (InBounds(r, c) && !FriendlyAt(r, c)) moves.Add(new Move(row, col, r, c));
                }
                break;

            case Bishop:
            case Rook:
            case Queen:
                var dirs = type == Bishop ? BishopDirs : type == Rook ? RookDirs : BishopDirs.Concat(RookDirs);
                foreach (var (dr, dc) in dirs)
                {
                    var r = row + dr; var c = col + dc;
                    while (InBounds(r, c) && !FriendlyAt(r, c))
                    {
                        moves.Add(new Move(row, col, r, c));
                        if (board[r, c] != Empty) break; // captured a piece, ray stops here
                        r += dr; c += dc;
                    }
                }
                break;
        }

        return moves;
    }

    /// <summary>Applies a move to the board (mutating it), including
    /// auto-queen promotion. Does not validate legality - callers must
    /// only pass moves from LegalMovesFor/LegalMovesForColor.</summary>
    public static void ApplyMove(int[,] board, Move move)
    {
        var piece = board[move.FromRow, move.FromCol];
        board[move.FromRow, move.FromCol] = Empty;

        if (PieceType(piece) == Pawn && (move.ToRow == 0 || move.ToRow == Size - 1))
        {
            piece = IsWhite(piece) ? Queen : -Queen;
        }

        board[move.ToRow, move.ToCol] = piece;
    }

    /// <summary>Legal moves for the piece at (row, col): pseudo-legal moves
    /// filtered to exclude any that would leave the mover's own king in
    /// check (covers pins and "must block/capture/move king out of check"
    /// automatically, since it's the same underlying check).</summary>
    public static List<Move> LegalMovesFor(int[,] board, int row, int col)
    {
        var piece = board[row, col];
        if (piece == Empty) return new List<Move>();
        var white = IsWhite(piece);
        var legal = new List<Move>();

        foreach (var move in PseudoMovesFor(board, row, col))
        {
            var copy = (int[,])board.Clone();
            ApplyMove(copy, move);
            if (!IsInCheck(copy, white)) legal.Add(move);
        }

        return legal;
    }

    public static List<Move> LegalMovesForColor(int[,] board, bool white)
    {
        var all = new List<Move>();
        for (var r = 0; r < Size; r++)
        {
            for (var c = 0; c < Size; c++)
            {
                var piece = board[r, c];
                if (piece != Empty && IsWhite(piece) == white) all.AddRange(LegalMovesFor(board, r, c));
            }
        }
        return all;
    }

    public static bool IsCheckmate(int[,] board, bool white) =>
        IsInCheck(board, white) && LegalMovesForColor(board, white).Count == 0;

    public static bool IsStalemate(int[,] board, bool white) =>
        !IsInCheck(board, white) && LegalMovesForColor(board, white).Count == 0;
}
