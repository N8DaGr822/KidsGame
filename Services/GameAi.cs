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
}
