using System.Collections.Generic;
using UnityEngine;

public class MinimaxAI
{
    private readonly ReversiEvaluator _evaluator = new ReversiEvaluator();
    private int _moveCount;
    private const int MAX_DEPTH_EARLY = 3;
    
    // デバッグ情報
    public string DebugInfo { get; private set; }
    private int _evaluatedPositions;
    private int _searchDepth;
    private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
    private const int MAX_DEPTH_MID = 4;
    private const int MAX_DEPTH_LATE = 5;

    // 探索済みの盤面をキャッシュ
    private Dictionary<string, float> _transpositionTable = new Dictionary<string, float>();

    public Vector2Int FindBestMove(GameController.MassState[,] board, int moveCount)
    {
        _moveCount = moveCount;
        _transpositionTable.Clear();
        _evaluatedPositions = 0;
        _stopwatch.Restart();
        
        var bestScore = float.NegativeInfinity;
        var bestMove = new Vector2Int(-1, -1);
        var depth = GetSearchDepth();
        var alpha = float.NegativeInfinity;
        var beta = float.PositiveInfinity;

        // 合法手をすべて生成
        var moves = GenerateMoves(board, GameController.MassState.BLACK);

        foreach (var move in moves)
        {
            var newBoard = SimulateMove(board, move, GameController.MassState.BLACK);
            var score = Minimax(newBoard, depth - 1, false, alpha, beta);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
            alpha = Mathf.Max(alpha, bestScore);
        }

        _stopwatch.Stop();
        UpdateDebugInfo(bestScore, bestMove, depth);

        return bestMove;
    }

    private float Minimax(GameController.MassState[,] board, int depth, bool isMaximizing, float alpha, float beta)
    {
        var boardHash = GetBoardHash(board);
        if (_transpositionTable.ContainsKey(boardHash))
        {
            return _transpositionTable[boardHash];
        }

        if (depth == 0)
        {
            var score = _evaluator.EvaluatePosition(board, _moveCount, 
                isMaximizing ? GameController.MassState.BLACK : GameController.MassState.WHITE);
            _transpositionTable[boardHash] = score;
            return score;
        }

        var currentColor = isMaximizing ? GameController.MassState.BLACK : GameController.MassState.WHITE;
        var moves = GenerateMoves(board, currentColor);

        // 合法手がない場合
        if (moves.Count == 0)
        {
            // パスして相手の手番
            return Minimax(board, depth - 1, !isMaximizing, alpha, beta);
        }

        float bestScore = isMaximizing ? float.NegativeInfinity : float.PositiveInfinity;

        foreach (var move in moves)
        {
            var newBoard = SimulateMove(board, move, currentColor);
            var score = Minimax(newBoard, depth - 1, !isMaximizing, alpha, beta);

            if (isMaximizing)
            {
                bestScore = Mathf.Max(bestScore, score);
                alpha = Mathf.Max(alpha, bestScore);
            }
            else
            {
                bestScore = Mathf.Min(bestScore, score);
                beta = Mathf.Min(beta, bestScore);
            }

            if (beta <= alpha)
                break;
        }

        _transpositionTable[boardHash] = bestScore;
        return bestScore;
    }

    private int GetSearchDepth()
    {
        if (_moveCount <= 20) return MAX_DEPTH_EARLY;
        if (_moveCount <= 40) return MAX_DEPTH_MID;
        return MAX_DEPTH_LATE;
    }

    private List<Vector2Int> GenerateMoves(GameController.MassState[,] board, GameController.MassState color)
    {
        var moves = new List<Vector2Int>();
        
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (board[y, x] == GameController.MassState.DEFAULT &&
                    CanPutStone(new Vector2Int(x, y), board, color))
                {
                    moves.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return moves;
    }

    private bool CanPutStone(Vector2Int position, GameController.MassState[,] board, GameController.MassState color)
    {
        // 8方向をチェック
        return CheckDirection(position, board, color, new Vector2Int(0, -1)) ||
               CheckDirection(position, board, color, new Vector2Int(0, 1)) ||
               CheckDirection(position, board, color, new Vector2Int(-1, 0)) ||
               CheckDirection(position, board, color, new Vector2Int(1, 0)) ||
               CheckDirection(position, board, color, new Vector2Int(1, -1)) ||
               CheckDirection(position, board, color, new Vector2Int(1, 1)) ||
               CheckDirection(position, board, color, new Vector2Int(-1, -1)) ||
               CheckDirection(position, board, color, new Vector2Int(-1, 1));
    }

    private bool CheckDirection(Vector2Int position, GameController.MassState[,] board, 
                              GameController.MassState color, Vector2Int direction)
    {
        int count = 0;
        while (true)
        {
            var target = position + direction * (count + 1);
            if (!IsInBoard(target) || board[target.y, target.x] == GameController.MassState.DEFAULT)
            {
                return false;
            }
            if (board[target.y, target.x] == color)
            {
                return count > 0;
            }
            count++;
        }
    }

    private bool IsInBoard(Vector2Int position)
    {
        return position.x >= 0 && position.y >= 0 && position.x < 8 && position.y < 8;
    }

    private GameController.MassState[,] SimulateMove(GameController.MassState[,] board, 
                                                    Vector2Int move, 
                                                    GameController.MassState color)
    {
        var newBoard = board.Clone() as GameController.MassState[,];
        newBoard[move.y, move.x] = color;

        // 8方向の石をひっくり返す
        SimulateReverse(move, newBoard, color, new Vector2Int(0, -1));
        SimulateReverse(move, newBoard, color, new Vector2Int(0, 1));
        SimulateReverse(move, newBoard, color, new Vector2Int(-1, 0));
        SimulateReverse(move, newBoard, color, new Vector2Int(1, 0));
        SimulateReverse(move, newBoard, color, new Vector2Int(1, -1));
        SimulateReverse(move, newBoard, color, new Vector2Int(1, 1));
        SimulateReverse(move, newBoard, color, new Vector2Int(-1, -1));
        SimulateReverse(move, newBoard, color, new Vector2Int(-1, 1));

        return newBoard;
    }

    private void SimulateReverse(Vector2Int position, GameController.MassState[,] board,
                                GameController.MassState color, Vector2Int direction)
    {
        var target = Vector2Int.zero;
        int count = 0;
        while (true)
        {
            target = position + direction * (count + 1);
            if (!IsInBoard(target) || board[target.y, target.x] == GameController.MassState.DEFAULT)
            {
                count = -1;
                break;
            }
            if (board[target.y, target.x] == color)
            {
                break;
            }
            count++;
        }

        for (int i = 0; i < count; i++)
        {
            target = position + direction * (i + 1);
            board[target.y, target.x] = color;
        }
    }

    private string GetBoardHash(GameController.MassState[,] board)
    {
        var hash = "";
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                hash += (int)board[y, x];
        return hash;
    }

    private void UpdateDebugInfo(float bestScore, Vector2Int bestMove, int depth)
    {
        var phase = _moveCount <= 20 ? "序盤" : _moveCount <= 40 ? "中盤" : "終盤";
        DebugInfo = $"思考時間: {_stopwatch.ElapsedMilliseconds}ms\n" +
                   $"探索深さ: {depth}\n" +
                   $"評価局面数: {_evaluatedPositions}\n" +
                   $"最善手: ({bestMove.x}, {bestMove.y})\n" +
                   $"評価値: {bestScore:F2}\n" +
                   $"現在のフェーズ: {phase}({_moveCount}手目)";
        _evaluatedPositions++;
    }
}
