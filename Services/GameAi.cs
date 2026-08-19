namespace KidsGameLauncher.Services;

/// <summary>
/// Move-picking helpers for grid-based AI opponents (Tic-Tac-Toe, Connect
/// Four). Difficulty controls how close to optimal the AI plays rather
/// than always picking the best move, so a kid can actually win on Easy.
/// </summary>
public static class GameAi
{
    public enum Difficulty { Easy, Medium, Hard }

    private static readonly Random Rng = new();

    // ---- Tic-Tac-Toe --------------------------------------------------

    private static readonly int[][] TicTacToeLines =
    {
        new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 },
        new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 },
        new[] { 0, 4, 8 }, new[] { 2, 4, 6 },
    };

    /// <summary>Returns 'X' or 'O' if a line is complete, else '\0'. Cells are '\0' when empty.</summary>
    public static char TicTacToeWinner(char[] board)
    {
        foreach (var line in TicTacToeLines)
        {
            var a = board[line[0]];
            if (a != '\0' && a == board[line[1]] && a == board[line[2]]) return a;
        }
        return '\0';
    }

    /// <summary>Returns the three cell indices of the completed line, or null if none. Lets callers highlight the winning line.</summary>
    public static int[]? TicTacToeWinningLine(char[] board)
    {
        foreach (var line in TicTacToeLines)
        {
            var a = board[line[0]];
            if (a != '\0' && a == board[line[1]] && a == board[line[2]]) return line;
        }
        return null;
    }

    public static int TicTacToeMove(char[] board, char ai, char human, Difficulty difficulty)
    {
        var empty = new List<int>();
        for (var i = 0; i < 9; i++) if (board[i] == '\0') empty.Add(i);
        if (empty.Count == 0) return -1;

        // Easy is fully random; Medium plays optimally only half the time
        // so it's beatable but not a pushover; Hard never misses.
        if (difficulty == Difficulty.Easy || (difficulty == Difficulty.Medium && Rng.NextDouble() < 0.5))
        {
            return empty[Rng.Next(empty.Count)];
        }

        var bestScore = int.MinValue;
        var bestMoves = new List<int>();

        foreach (var i in empty)
        {
            board[i] = ai;
            var score = TicTacToeMinimax(board, 1, false, ai, human);
            board[i] = '\0';

            if (score > bestScore)
            {
                bestScore = score;
                bestMoves.Clear();
                bestMoves.Add(i);
            }
            else if (score == bestScore)
            {
                bestMoves.Add(i);
            }
        }

        return bestMoves[Rng.Next(bestMoves.Count)];
    }

    private static int TicTacToeMinimax(char[] board, int depth, bool maximizing, char ai, char human)
    {
        var winner = TicTacToeWinner(board);
        if (winner == ai) return 10 - depth;
        if (winner == human) return depth - 10;

        var hasEmpty = false;
        var best = maximizing ? int.MinValue : int.MaxValue;

        for (var i = 0; i < 9; i++)
        {
            if (board[i] != '\0') continue;
            hasEmpty = true;

            board[i] = maximizing ? ai : human;
            var score = TicTacToeMinimax(board, depth + 1, !maximizing, ai, human);
            board[i] = '\0';

            best = maximizing ? Math.Max(best, score) : Math.Min(best, score);
        }

        return hasEmpty ? best : 0;
    }

    // ---- Connect Four ---------------------------------------------------

    public const int ConnectFourCols = 7;
    public const int ConnectFourRows = 6;

    /// <summary>The row a disc would land in for the given column, or null if the column is full. Row 0 is the bottom.</summary>
    public static int? DropRow(int[,] board, int col)
    {
        for (var r = 0; r < ConnectFourRows; r++)
        {
            if (board[col, r] == 0) return r;
        }
        return null;
    }

    /// <summary>True if the disc just placed at (col, row) completes 4 in a row in any direction.</summary>
    public static bool ConnectFourCheckWin(int[,] board, int col, int row, int player)
    {
        return CountDirection(board, col, row, player, 1, 0) + CountDirection(board, col, row, player, -1, 0) >= 3
            || CountDirection(board, col, row, player, 0, 1) + CountDirection(board, col, row, player, 0, -1) >= 3
            || CountDirection(board, col, row, player, 1, 1) + CountDirection(board, col, row, player, -1, -1) >= 3
            || CountDirection(board, col, row, player, 1, -1) + CountDirection(board, col, row, player, -1, 1) >= 3;
    }

    private static int CountDirection(int[,] board, int col, int row, int player, int dc, int dr)
    {
        var count = 0;
        var c = col + dc;
        var r = row + dr;
        while (c >= 0 && c < ConnectFourCols && r >= 0 && r < ConnectFourRows && board[c, r] == player)
        {
            count++;
            c += dc;
            r += dr;
        }
        return count;
    }

    public static int ConnectFourMove(int[,] board, int ai, int human, Difficulty difficulty)
    {
        var validCols = new List<int>();
        for (var c = 0; c < ConnectFourCols; c++) if (DropRow(board, c) is not null) validCols.Add(c);
        if (validCols.Count == 0) return -1;

        if (difficulty == Difficulty.Easy)
        {
            return validCols[Rng.Next(validCols.Count)];
        }

        var depth = difficulty == Difficulty.Medium ? 3 : 5;
        var bestScore = int.MinValue;
        var bestCols = new List<int>();

        // Search center-out so equal-scoring ties favor the stronger center columns.
        foreach (var c in validCols.OrderBy(c => Math.Abs(c - ConnectFourCols / 2)))
        {
            var row = DropRow(board, c)!.Value;
            board[c, row] = ai;
            var win = ConnectFourCheckWin(board, c, row, ai);
            var score = win ? 100000 : ConnectFourMinimax(board, depth - 1, false, ai, human, int.MinValue, int.MaxValue);
            board[c, row] = 0;

            if (score > bestScore)
            {
                bestScore = score;
                bestCols.Clear();
                bestCols.Add(c);
            }
            else if (score == bestScore)
            {
                bestCols.Add(c);
            }
        }

        return bestCols[Rng.Next(bestCols.Count)];
    }

    private static int ConnectFourMinimax(int[,] board, int depth, bool maximizing, int ai, int human, int alpha, int beta)
    {
        var validCols = new List<int>();
        for (var c = 0; c < ConnectFourCols; c++) if (DropRow(board, c) is not null) validCols.Add(c);

        if (depth == 0 || validCols.Count == 0)
        {
            return EvaluateBoard(board, ai, human);
        }

        var player = maximizing ? ai : human;
        var best = maximizing ? int.MinValue : int.MaxValue;

        foreach (var c in validCols)
        {
            var row = DropRow(board, c)!.Value;
            board[c, row] = player;
            var win = ConnectFourCheckWin(board, c, row, player);
            var score = win
                ? (maximizing ? 100000 : -100000)
                : ConnectFourMinimax(board, depth - 1, !maximizing, ai, human, alpha, beta);
            board[c, row] = 0;

            if (maximizing)
            {
                best = Math.Max(best, score);
                alpha = Math.Max(alpha, best);
            }
            else
            {
                best = Math.Min(best, score);
                beta = Math.Min(beta, best);
            }

            if (beta <= alpha) break;
        }

        return best;
    }

    private static int EvaluateBoard(int[,] board, int ai, int human)
    {
        var score = 0;

        for (var c = 0; c < ConnectFourCols; c++)
        {
            for (var r = 0; r < ConnectFourRows; r++)
            {
                if (c <= ConnectFourCols - 4) score += EvaluateWindow(board, c, r, 1, 0, ai, human);
                if (r <= ConnectFourRows - 4) score += EvaluateWindow(board, c, r, 0, 1, ai, human);
                if (c <= ConnectFourCols - 4 && r <= ConnectFourRows - 4) score += EvaluateWindow(board, c, r, 1, 1, ai, human);
                if (c <= ConnectFourCols - 4 && r >= 3) score += EvaluateWindow(board, c, r, 1, -1, ai, human);
            }
        }

        // Center column control tends to open more winning lines.
        var center = ConnectFourCols / 2;
        for (var r = 0; r < ConnectFourRows; r++)
        {
            if (board[center, r] == ai) score += 3;
            else if (board[center, r] == human) score -= 3;
        }

        return score;
    }

    private static int EvaluateWindow(int[,] board, int col, int row, int dc, int dr, int ai, int human)
    {
        var aiCount = 0;
        var humanCount = 0;
        var emptyCount = 0;

        for (var i = 0; i < 4; i++)
        {
            var cell = board[col + dc * i, row + dr * i];
            if (cell == ai) aiCount++;
            else if (cell == human) humanCount++;
            else emptyCount++;
        }

        if (aiCount > 0 && humanCount > 0) return 0;
        if (aiCount == 3 && emptyCount == 1) return 50;
        if (aiCount == 2 && emptyCount == 2) return 10;
        if (humanCount == 3 && emptyCount == 1) return -80;
        if (humanCount == 2 && emptyCount == 2) return -10;
        return 0;
    }

    // ---- Reversi / Othello -----------------------------------------------

    public const int ReversiSize = 8;

    private static readonly (int DR, int DC)[] ReversiDirections =
    {
        (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1),
    };

    // Corners are the most valuable cells (can never be flipped back); the
    // cells diagonally and orthogonally adjacent to a corner are the worst
    // (playing there commonly hands the opponent that corner) - classic
    // Othello positional strategy, not just raw disc count.
    private static readonly int[,] ReversiWeights =
    {
        { 100, -20, 10, 5, 5, 10, -20, 100 },
        { -20, -50, -2, -2, -2, -2, -50, -20 },
        { 10, -2, -1, -1, -1, -1, -2, 10 },
        { 5, -2, -1, -1, -1, -1, -2, 5 },
        { 5, -2, -1, -1, -1, -1, -2, 5 },
        { 10, -2, -1, -1, -1, -1, -2, 10 },
        { -20, -50, -2, -2, -2, -2, -50, -20 },
        { 100, -20, 10, 5, 5, 10, -20, 100 },
    };

    private static bool ReversiInBounds(int r, int c) => r >= 0 && r < ReversiSize && c >= 0 && c < ReversiSize;

    private static List<(int Row, int Col)> ReversiFlipsFor(int[,] board, int row, int col, int player, int opponent)
    {
        var flips = new List<(int, int)>();
        foreach (var (dr, dc) in ReversiDirections)
        {
            var line = new List<(int, int)>();
            var r = row + dr;
            var c = col + dc;
            while (ReversiInBounds(r, c) && board[r, c] == opponent)
            {
                line.Add((r, c));
                r += dr;
                c += dc;
            }
            if (line.Count > 0 && ReversiInBounds(r, c) && board[r, c] == player)
            {
                flips.AddRange(line);
            }
        }
        return flips;
    }

    public static List<(int Row, int Col)> ReversiLegalMoves(int[,] board, int player)
    {
        var opponent = player == 1 ? 2 : 1;
        var moves = new List<(int, int)>();
        for (var r = 0; r < ReversiSize; r++)
        {
            for (var c = 0; c < ReversiSize; c++)
            {
                if (board[r, c] != 0) continue;
                if (ReversiFlipsFor(board, r, c, player, opponent).Count > 0) moves.Add((r, c));
            }
        }
        return moves;
    }

    /// <summary>Places player's disc at (row, col) and flips every disc it
    /// captures. Returns false (no board change) if the move isn't legal.</summary>
    public static bool ReversiApplyMove(int[,] board, int row, int col, int player)
    {
        var opponent = player == 1 ? 2 : 1;
        var flips = ReversiFlipsFor(board, row, col, player, opponent);
        if (flips.Count == 0) return false;

        board[row, col] = player;
        foreach (var (r, c) in flips) board[r, c] = player;
        return true;
    }

    public static (int Row, int Col)? ReversiMove(int[,] board, int ai, int human, Difficulty difficulty)
    {
        var moves = ReversiLegalMoves(board, ai);
        if (moves.Count == 0) return null;

        if (difficulty == Difficulty.Easy)
        {
            return moves[Rng.Next(moves.Count)];
        }

        // Depth capped lower than Connect Four's - Reversi's branching
        // factor (up to ~20 legal moves in the midgame, vs. at most 7
        // columns) makes a naive equal depth much slower on a single WASM
        // UI thread, even with alpha-beta pruning.
        var depth = difficulty == Difficulty.Medium ? 2 : 4;
        var bestScore = int.MinValue;
        var bestMoves = new List<(int, int)>();

        // Exploring likely-strong moves (corners/edges) first tightens
        // alpha-beta pruning a lot - Reversi's branching factor (up to
        // ~20) is high enough that move order matters more than it does
        // for Connect Four's much narrower search.
        foreach (var (row, col) in moves.OrderByDescending(m => ReversiWeights[m.Item1, m.Item2]))
        {
            var copy = (int[,])board.Clone();
            ReversiApplyMove(copy, row, col, ai);
            var score = ReversiMinimax(copy, depth - 1, false, ai, human, int.MinValue, int.MaxValue);

            if (score > bestScore)
            {
                bestScore = score;
                bestMoves.Clear();
                bestMoves.Add((row, col));
            }
            else if (score == bestScore)
            {
                bestMoves.Add((row, col));
            }
        }

        return bestMoves[Rng.Next(bestMoves.Count)];
    }

    private static int ReversiMinimax(int[,] board, int depth, bool maximizing, int ai, int human, int alpha, int beta)
    {
        var player = maximizing ? ai : human;
        var moves = ReversiLegalMoves(board, player);

        if (depth == 0)
        {
            return ReversiEvaluate(board, ai, human);
        }

        if (moves.Count == 0)
        {
            // A real pass, not a dead end - if the OTHER player also has
            // no moves the game is over; otherwise play continues with
            // the same maximizing/minimizing side flipped once more.
            var otherMoves = ReversiLegalMoves(board, maximizing ? human : ai);
            if (otherMoves.Count == 0) return ReversiEvaluate(board, ai, human);
            return ReversiMinimax(board, depth - 1, !maximizing, ai, human, alpha, beta);
        }

        var best = maximizing ? int.MinValue : int.MaxValue;

        foreach (var (row, col) in moves.OrderByDescending(m => ReversiWeights[m.Item1, m.Item2]))
        {
            var copy = (int[,])board.Clone();
            ReversiApplyMove(copy, row, col, player);
            var score = ReversiMinimax(copy, depth - 1, !maximizing, ai, human, alpha, beta);

            if (maximizing)
            {
                best = Math.Max(best, score);
                alpha = Math.Max(alpha, best);
            }
            else
            {
                best = Math.Min(best, score);
                beta = Math.Min(beta, best);
            }

            if (beta <= alpha) break;
        }

        return best;
    }

    private static int ReversiEvaluate(int[,] board, int ai, int human)
    {
        var positional = 0;
        for (var r = 0; r < ReversiSize; r++)
        {
            for (var c = 0; c < ReversiSize; c++)
            {
                if (board[r, c] == ai) positional += ReversiWeights[r, c];
                else if (board[r, c] == human) positional -= ReversiWeights[r, c];
            }
        }

        // Mobility (how many replies each side has) matters as much as
        // position in real Othello strategy - being forced to pass, or
        // into a bad cell, is often worse than a slightly worse position.
        var mobility = ReversiLegalMoves(board, ai).Count - ReversiLegalMoves(board, human).Count;

        return positional + mobility * 5;
    }

    // ---- Checkers ---------------------------------------------------------
    //
    // Board is an 8x8 int[,] like Reversi's, but pieces only ever sit on
    // "dark" squares ((row+col) % 2 == 1). A "turn" is the unit the AI
    // searches over, not a single hop: a turn is either one simple move,
    // or a full mandatory multi-jump chain for one piece (English/American
    // draughts forces a capture whenever one is available anywhere on the
    // board, and a capturing piece must keep jumping with the same piece
    // until no further jump is available - stopping immediately if it
    // promotes to king mid-chain, which is also a real rule, not a bug).
    // CheckersLegalTurns enumerates every complete legal turn so the AI's
    // minimax treats "my whole forced chain" as a single ply, matching how
    // a human actually experiences a turn.

    public const int CheckersSize = 8;
    public const int CheckersEmpty = 0;
    public const int CheckersRedMan = 1;
    public const int CheckersRedKing = 2;
    public const int CheckersBlackMan = 3;
    public const int CheckersBlackKing = 4;

    public record struct CheckersHop(int FromRow, int FromCol, int ToRow, int ToCol, int? CapRow, int? CapCol);

    public static bool CheckersIsDarkSquare(int row, int col) => (row + col) % 2 == 1;

    private static bool CheckersInBounds(int r, int c) => r >= 0 && r < CheckersSize && c >= 0 && c < CheckersSize;

    private static bool CheckersIsKingPiece(int cell) => cell == CheckersRedKing || cell == CheckersBlackKing;

    /// <summary>1 (red) or 2 (black) for an occupied cell, 0 for empty.</summary>
    public static int CheckersOwner(int cell) => cell switch
    {
        CheckersRedMan or CheckersRedKing => 1,
        CheckersBlackMan or CheckersBlackKing => 2,
        _ => 0,
    };

    public static bool CheckersIsKing(int[,] board, int row, int col) => CheckersIsKingPiece(board[row, col]);

    private static int CheckersOpponent(int player) => player == 1 ? 2 : 1;

    private static IEnumerable<(int Dr, int Dc)> CheckersMoveDirs(int cell)
    {
        if (CheckersIsKingPiece(cell)) return new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) };
        if (cell == CheckersRedMan) return new[] { (-1, -1), (-1, 1) };
        if (cell == CheckersBlackMan) return new[] { (1, -1), (1, 1) };
        return Array.Empty<(int, int)>();
    }

    public static List<(int ToRow, int ToCol)> CheckersSimpleMovesFor(int[,] board, int row, int col)
    {
        var result = new List<(int, int)>();
        var cell = board[row, col];
        if (cell == CheckersEmpty) return result;

        foreach (var (dr, dc) in CheckersMoveDirs(cell))
        {
            var toR = row + dr;
            var toC = col + dc;
            if (CheckersInBounds(toR, toC) && board[toR, toC] == CheckersEmpty) result.Add((toR, toC));
        }
        return result;
    }

    public static List<(int ToRow, int ToCol, int CapRow, int CapCol)> CheckersCaptureHopsFor(int[,] board, int row, int col)
    {
        var result = new List<(int, int, int, int)>();
        var cell = board[row, col];
        if (cell == CheckersEmpty) return result;
        var opponent = CheckersOpponent(CheckersOwner(cell));

        foreach (var (dr, dc) in CheckersMoveDirs(cell))
        {
            var midR = row + dr;
            var midC = col + dc;
            var toR = row + 2 * dr;
            var toC = col + 2 * dc;
            if (!CheckersInBounds(toR, toC) || !CheckersInBounds(midR, midC)) continue;

            var midCell = board[midR, midC];
            if (midCell != CheckersEmpty && CheckersOwner(midCell) == opponent && board[toR, toC] == CheckersEmpty)
            {
                result.Add((toR, toC, midR, midC));
            }
        }
        return result;
    }

    public static bool CheckersHasAnyCapture(int[,] board, int player)
    {
        for (var r = 0; r < CheckersSize; r++)
        {
            for (var c = 0; c < CheckersSize; c++)
            {
                if (CheckersOwner(board[r, c]) == player && CheckersCaptureHopsFor(board, r, c).Count > 0) return true;
            }
        }
        return false;
    }

    /// <summary>Pieces the given player may legally start a turn with. Under
    /// tournament rules (mandatoryCapture: true, the default and the real
    /// official rule), only capturing pieces count when any capture is
    /// available anywhere on the board; under free play, capturing is
    /// optional, so any piece with either kind of move counts.</summary>
    public static List<(int Row, int Col)> CheckersLegalOrigins(int[,] board, int player, bool mandatoryCapture = true)
    {
        var mustCapture = mandatoryCapture && CheckersHasAnyCapture(board, player);
        var result = new List<(int, int)>();

        for (var r = 0; r < CheckersSize; r++)
        {
            for (var c = 0; c < CheckersSize; c++)
            {
                if (CheckersOwner(board[r, c]) != player) continue;
                var hasMove = mustCapture
                    ? CheckersCaptureHopsFor(board, r, c).Count > 0
                    : CheckersSimpleMovesFor(board, r, c).Count > 0 || CheckersCaptureHopsFor(board, r, c).Count > 0;
                if (hasMove) result.Add((r, c));
            }
        }
        return result;
    }

    /// <summary>Applies one hop (simple move or single jump) to the board -
    /// moves the piece, removes a captured piece if this was a jump, and
    /// promotes to king on reaching the far row. Returns whether it was a
    /// capture and whether the piece was just promoted - a promotion ends
    /// the turn immediately even if another jump would otherwise be
    /// available, a real draughts rule and not a bug.</summary>
    public static (bool WasCapture, bool JustPromoted) CheckersApplyHop(int[,] board, int fromRow, int fromCol, int toRow, int toCol, int? capRow, int? capCol)
    {
        var cell = board[fromRow, fromCol];
        var wasKingBefore = CheckersIsKingPiece(cell);
        board[fromRow, fromCol] = CheckersEmpty;

        var wasCapture = capRow.HasValue;
        if (wasCapture) board[capRow!.Value, capCol!.Value] = CheckersEmpty;

        if (cell == CheckersRedMan && toRow == 0) cell = CheckersRedKing;
        else if (cell == CheckersBlackMan && toRow == CheckersSize - 1) cell = CheckersBlackKing;

        board[toRow, toCol] = cell;
        return (wasCapture, !wasKingBefore && CheckersIsKingPiece(cell));
    }

    private static void CheckersApplyTurn(int[,] board, List<CheckersHop> turn)
    {
        foreach (var hop in turn) CheckersApplyHop(board, hop.FromRow, hop.FromCol, hop.ToRow, hop.ToCol, hop.CapRow, hop.CapCol);
    }

    /// <summary>Every complete legal turn for a player. Under tournament
    /// rules (mandatoryCapture: true) that's one hop for a simple move, or
    /// a full maximal forced-capture chain (branching whenever a mid-chain
    /// piece has more than one further jump) for a capturing piece - the
    /// unit the AI searches over, see the header note above for why a
    /// multi-jump chain counts as a single ply. Under free play, every hop
    /// (simple or capture) is its own complete one-hop turn instead - a
    /// capture is offered but never forced, and never forces a
    /// continuation chain either, the simplest "may capture, never forced
    /// to" casual ruleset.</summary>
    public static List<List<CheckersHop>> CheckersLegalTurns(int[,] board, int player, bool mandatoryCapture = true)
    {
        var turns = new List<List<CheckersHop>>();
        var mustCapture = mandatoryCapture && CheckersHasAnyCapture(board, player);
        var origins = CheckersLegalOrigins(board, player, mandatoryCapture);

        foreach (var (row, col) in origins)
        {
            if (!mustCapture)
            {
                foreach (var (toR, toC) in CheckersSimpleMovesFor(board, row, col))
                {
                    turns.Add(new List<CheckersHop> { new(row, col, toR, toC, null, null) });
                }

                if (!mandatoryCapture)
                {
                    foreach (var (toR, toC, capR, capC) in CheckersCaptureHopsFor(board, row, col))
                    {
                        turns.Add(new List<CheckersHop> { new(row, col, toR, toC, capR, capC) });
                    }
                }
                continue;
            }

            var path = new List<CheckersHop>();
            void Dfs(int[,] b, int curR, int curC)
            {
                var hops = CheckersCaptureHopsFor(b, curR, curC);
                if (hops.Count == 0)
                {
                    if (path.Count > 0) turns.Add(new List<CheckersHop>(path));
                    return;
                }

                foreach (var (toR, toC, capR, capC) in hops)
                {
                    var copy = (int[,])b.Clone();
                    var (_, justPromoted) = CheckersApplyHop(copy, curR, curC, toR, toC, capR, capC);
                    path.Add(new CheckersHop(curR, curC, toR, toC, capR, capC));

                    if (justPromoted) turns.Add(new List<CheckersHop>(path));
                    else Dfs(copy, toR, toC);

                    path.RemoveAt(path.Count - 1);
                }
            }
            Dfs(board, row, col);
        }

        return turns;
    }

    public static List<CheckersHop>? CheckersMove(int[,] board, int ai, int human, Difficulty difficulty, bool mandatoryCapture = true)
    {
        var turns = CheckersLegalTurns(board, ai, mandatoryCapture);
        if (turns.Count == 0) return null;

        if (difficulty == Difficulty.Easy)
        {
            return turns[Rng.Next(turns.Count)];
        }

        var depth = difficulty == Difficulty.Medium ? 3 : 5;
        var bestScore = int.MinValue;
        var bestTurns = new List<List<CheckersHop>>();

        foreach (var turn in turns)
        {
            var copy = (int[,])board.Clone();
            CheckersApplyTurn(copy, turn);
            var score = CheckersMinimax(copy, depth - 1, false, ai, human, int.MinValue, int.MaxValue, mandatoryCapture);

            if (score > bestScore)
            {
                bestScore = score;
                bestTurns.Clear();
                bestTurns.Add(turn);
            }
            else if (score == bestScore)
            {
                bestTurns.Add(turn);
            }
        }

        return bestTurns[Rng.Next(bestTurns.Count)];
    }

    private static int CheckersMinimax(int[,] board, int depth, bool maximizing, int ai, int human, int alpha, int beta, bool mandatoryCapture)
    {
        var player = maximizing ? ai : human;
        var turns = CheckersLegalTurns(board, player, mandatoryCapture);

        // No legal turn = that player has lost right now, regardless of
        // remaining depth - checkers has no "pass," being stuck is a loss.
        if (turns.Count == 0) return maximizing ? -100000 : 100000;
        if (depth == 0) return CheckersEvaluate(board, ai, human);

        var best = maximizing ? int.MinValue : int.MaxValue;
        foreach (var turn in turns)
        {
            var copy = (int[,])board.Clone();
            CheckersApplyTurn(copy, turn);
            var score = CheckersMinimax(copy, depth - 1, !maximizing, ai, human, alpha, beta, mandatoryCapture);

            if (maximizing)
            {
                best = Math.Max(best, score);
                alpha = Math.Max(alpha, best);
            }
            else
            {
                best = Math.Min(best, score);
                beta = Math.Min(beta, best);
            }

            if (beta <= alpha) break;
        }
        return best;
    }

    // Kings outweigh men, and a man's value creeps up as it nears
    // promotion (encourages pushing forward instead of camping the back
    // row) - the same "positional weight beyond raw piece count" idea
    // Reversi's evaluator uses, just simpler since checkers pieces don't
    // have Othello's corner/edge asymmetry.
    private static int CheckersEvaluate(int[,] board, int ai, int human)
    {
        var score = 0;
        for (var r = 0; r < CheckersSize; r++)
        {
            for (var c = 0; c < CheckersSize; c++)
            {
                var cell = board[r, c];
                var owner = CheckersOwner(cell);
                if (owner == 0) continue;

                var isKing = CheckersIsKingPiece(cell);
                var value = isKing ? 50 : 30;
                if (!isKing)
                {
                    var advancement = owner == 1 ? (CheckersSize - 1 - r) : r;
                    value += advancement;
                }

                score += owner == ai ? value : -value;
            }
        }
        return score;
    }

    // ---- Chess ---------------------------------------------------------
    //
    // Reuses Services/ChessRules.cs for move generation/legality/check
    // detection rather than re-deriving chess rules here - this only picks
    // a move. Chess's branching factor (~30-40 legal moves per position)
    // makes even a shallow full-width search noticeably slower than
    // Reversi/Checkers on a single WASM UI thread, so depth is capped
    // lower; Easy plays a legal move at random but still grabs a free
    // capture most of the time so it doesn't feel oblivious to a kid.

    private static readonly Dictionary<int, int> ChessPieceValues = new()
    {
        [ChessRules.Pawn] = 100,
        [ChessRules.Knight] = 320,
        [ChessRules.Bishop] = 330,
        [ChessRules.Rook] = 500,
        [ChessRules.Queen] = 900,
        [ChessRules.King] = 20000,
    };

    public static ChessRules.Move? ChessMove(int[,] board, bool aiIsWhite, Difficulty difficulty)
    {
        var moves = ChessRules.LegalMovesForColor(board, aiIsWhite);
        if (moves.Count == 0) return null;

        if (difficulty == Difficulty.Easy)
        {
            var captures = moves.Where(m => board[m.ToRow, m.ToCol] != ChessRules.Empty).ToList();
            if (captures.Count > 0 && Rng.NextDouble() < 0.6) return captures[Rng.Next(captures.Count)];
            return moves[Rng.Next(moves.Count)];
        }

        var depth = difficulty == Difficulty.Medium ? 2 : 3;
        var bestScore = int.MinValue;
        var bestMoves = new List<ChessRules.Move>();

        // Trying captures first tightens alpha-beta pruning a lot, same
        // idea as Reversi ordering by corner/edge weight before searching.
        foreach (var move in moves.OrderByDescending(m => CapturedValue(board, m)))
        {
            var copy = (int[,])board.Clone();
            ChessRules.ApplyMove(copy, move);
            var score = ChessMinimax(copy, depth - 1, false, aiIsWhite, int.MinValue, int.MaxValue);

            if (score > bestScore)
            {
                bestScore = score;
                bestMoves.Clear();
                bestMoves.Add(move);
            }
            else if (score == bestScore)
            {
                bestMoves.Add(move);
            }
        }

        return bestMoves[Rng.Next(bestMoves.Count)];
    }

    private static int CapturedValue(int[,] board, ChessRules.Move move)
    {
        var target = board[move.ToRow, move.ToCol];
        return target == ChessRules.Empty ? 0 : ChessPieceValues[ChessRules.PieceType(target)];
    }

    private static int ChessMinimax(int[,] board, int depth, bool maximizing, bool aiIsWhite, int alpha, int beta)
    {
        var sideToMove = maximizing ? aiIsWhite : !aiIsWhite;
        var moves = ChessRules.LegalMovesForColor(board, sideToMove);

        // No legal move: checkmate (score it, from the AI's perspective,
        // as extreme - slightly preferring a faster mate/slower loss via
        // the depth term) or stalemate (neutral), regardless of search
        // depth remaining - a terminal position is terminal.
        if (moves.Count == 0)
        {
            if (!ChessRules.IsInCheck(board, sideToMove)) return 0;
            return maximizing ? -100000 - depth : 100000 + depth;
        }

        if (depth == 0) return ChessEvaluate(board, aiIsWhite);

        var best = maximizing ? int.MinValue : int.MaxValue;
        foreach (var move in moves.OrderByDescending(m => CapturedValue(board, m)))
        {
            var copy = (int[,])board.Clone();
            ChessRules.ApplyMove(copy, move);
            var score = ChessMinimax(copy, depth - 1, !maximizing, aiIsWhite, alpha, beta);

            if (maximizing)
            {
                best = Math.Max(best, score);
                alpha = Math.Max(alpha, best);
            }
            else
            {
                best = Math.Min(best, score);
                beta = Math.Min(beta, best);
            }

            if (beta <= alpha) break;
        }

        return best;
    }

    private static int ChessEvaluate(int[,] board, bool aiIsWhite)
    {
        var material = 0;
        for (var r = 0; r < ChessRules.Size; r++)
        {
            for (var c = 0; c < ChessRules.Size; c++)
            {
                var piece = board[r, c];
                if (piece == ChessRules.Empty) continue;
                var value = ChessPieceValues[ChessRules.PieceType(piece)];
                material += ChessRules.IsWhite(piece) == aiIsWhite ? value : -value;
            }
        }

        // Mobility nudges the AI toward active positions rather than
        // shuffling - a much smaller weight than material since a chess
        // move's tactical value swings far more than its mobility count.
        var mobility = ChessRules.LegalMovesForColor(board, aiIsWhite).Count
                     - ChessRules.LegalMovesForColor(board, !aiIsWhite).Count;

        return material + mobility * 2;
    }
}
