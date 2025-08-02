using UnityEngine;

public class ReversiEvaluator
{
    // マスの重み付けテーブル
    private static readonly int[,] WeightTable = new int[8, 8]
    {
        { 30, -12,  0, -1, -1,  0, -12, 30},
        {-12, -15, -3, -3, -3, -3, -15,-12},
        {  0,  -3,  0, -1, -1,  0,  -3,  0},
        { -1,  -3, -1, -1, -1, -1,  -3, -1},
        { -1,  -3, -1, -1, -1, -1,  -3, -1},
        {  0,  -3,  0, -1, -1,  0,  -3,  0},
        {-12, -15, -3, -3, -3, -3, -15,-12},
        { 30, -12,  0, -1, -1,  0, -12, 30}
    };

    // ゲーム進行度に応じた重み付け係数
    private const float EARLY_WEIGHT_MULTIPLIER = 1.5f;
    private const float MID_WEIGHT_MULTIPLIER = 1.0f;
    private const float LATE_WEIGHT_MULTIPLIER = 0.5f;

    // 石数差の重み付け係数
    private const float EARLY_STONE_MULTIPLIER = 0.5f;
    private const float MID_STONE_MULTIPLIER = 2.0f;
    private const float LATE_STONE_MULTIPLIER = 4.0f;

    // 着手可能手数の重み付け係数
    private const float MOBILITY_WEIGHT = 1.0f;

    public float EvaluatePosition(GameController.MassState[,] board, int moveCount, GameController.MassState currentColor)
    {
        float score = 0;

        // 基本点（マスの重み）の計算
        score += CalculateWeightScore(board, currentColor);

        // 石数の差の計算
        var stoneDiff = CalculateStoneDiff(board, currentColor);

        // 着手可能手数の差の計算
        var mobilityDiff = CalculateMobilityDiff(board, currentColor);

        // ゲーム進行度による重み付け
        if (moveCount <= 20) // 序盤
        {
            score *= EARLY_WEIGHT_MULTIPLIER;
            score += stoneDiff * EARLY_STONE_MULTIPLIER;
            score += mobilityDiff * MOBILITY_WEIGHT;
        }
        else if (moveCount <= 40) // 中盤
        {
            score *= MID_WEIGHT_MULTIPLIER;
            score += stoneDiff * MID_STONE_MULTIPLIER;
            score += mobilityDiff * MOBILITY_WEIGHT;
        }
        else // 終盤
        {
            score *= LATE_WEIGHT_MULTIPLIER;
            score += stoneDiff * LATE_STONE_MULTIPLIER;
        }

        return score;
    }

    private float CalculateWeightScore(GameController.MassState[,] board, GameController.MassState currentColor)
    {
        float score = 0;
        var opponentColor = currentColor == GameController.MassState.BLACK ? 
                           GameController.MassState.WHITE : 
                           GameController.MassState.BLACK;

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (board[y, x] == currentColor)
                    score += WeightTable[y, x];
                else if (board[y, x] == opponentColor)
                    score -= WeightTable[y, x];
            }
        }

        return score;
    }

    private int CalculateStoneDiff(GameController.MassState[,] board, GameController.MassState currentColor)
    {
        int currentCount = 0;
        int opponentCount = 0;
        var opponentColor = currentColor == GameController.MassState.BLACK ? 
                           GameController.MassState.WHITE : 
                           GameController.MassState.BLACK;

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (board[y, x] == currentColor)
                    currentCount++;
                else if (board[y, x] == opponentColor)
                    opponentCount++;
            }
        }

        return currentCount - opponentCount;
    }

    private int CalculateMobilityDiff(GameController.MassState[,] board, GameController.MassState currentColor)
    {
        // この関数は後でGameControllerのCanReverseメソッドを使用して実装
        // 現時点ではダミーの値を返す
        return 0;
    }
}
