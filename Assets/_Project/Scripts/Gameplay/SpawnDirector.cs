using System.Collections.Generic;
using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    [SerializeField] private PieceFactory pieceFactory;

    public List<PieceShapeData> BuildTray(BoardState boardState, int score, int turnIndex, int traySize)
    {
        List<PieceShapeData> selection = new List<PieceShapeData>();
        if (pieceFactory == null)
            pieceFactory = GetComponent<PieceFactory>();
        if (pieceFactory == null)
            pieceFactory = FindObjectOfType<PieceFactory>();

        IReadOnlyList<PieceShapeData> allShapes = pieceFactory != null ? pieceFactory.AvailableShapes : null;
        if (allShapes == null || allShapes.Count == 0)
            return selection;

        float difficulty = GetDifficulty(score, turnIndex, boardState);
        int[] targetSizes = GetTargetSizes(difficulty, traySize);

        for (int i = 0; i < traySize; i++)
        {
            int targetSize = i < targetSizes.Length ? targetSizes[i] : 2;
            PieceShapeData nextShape = SelectShape(allShapes, boardState, difficulty, selection, targetSize);
            if (nextShape != null)
                selection.Add(nextShape);
        }

        return selection;
    }

    public float GetDifficulty(int score, int turnIndex, BoardState boardState)
    {
        float scoreFactor = Mathf.Clamp01(score / 1500f);
        float turnFactor = Mathf.Clamp01(turnIndex / 10f);
        float fillFactor = boardState != null ? Mathf.Clamp01(boardState.CountOccupiedCells() / 32f) : 0f;
        return Mathf.Clamp01((scoreFactor * 0.35f) + (turnFactor * 0.35f) + (fillFactor * 0.30f));
    }

    private int[] GetTargetSizes(float difficulty, int traySize)
    {
        if (traySize <= 0)
            return new int[0];

        if (difficulty < 0.33f)
            return new[] { 1, 2, 3 };

        if (difficulty < 0.66f)
            return new[] { 2, 3, 2 };

        return new[] { 3, 4, 2 };
    }

    private PieceShapeData SelectShape(IReadOnlyList<PieceShapeData> allShapes, BoardState boardState, float difficulty, List<PieceShapeData> currentSelection, int targetSize)
    {
        List<ShapeCandidate> candidates = new List<ShapeCandidate>();
        int mobility = EstimateTrayMobility(boardState, currentSelection);

        foreach (PieceShapeData shape in allShapes)
        {
            if (shape == null)
                continue;

            int validPlacements = BoardAnalyzer.CountValidPlacements(boardState, shape);
            int cellCount = shape.cells.Count;
            bool isDot = cellCount == 1;
            bool isEasyShape = cellCount <= 2;
            float duplicatePenalty = currentSelection.Contains(shape) ? 5f : 0f;
            float sizeDistancePenalty = Mathf.Abs(cellCount - targetSize) * 4f;
            float shapeDifficulty = Mathf.InverseLerp(1f, 4f, cellCount);
            float evaluation;

            if (difficulty < 0.5f)
            {
                evaluation = (validPlacements * 4.5f) - sizeDistancePenalty - duplicatePenalty - (shapeDifficulty * 1.5f);
            }
            else
            {
                float pressureWeight = Mathf.Lerp(0.8f, 2.2f, difficulty);
                evaluation = (validPlacements * pressureWeight) - sizeDistancePenalty - duplicatePenalty + (shapeDifficulty * 7f);

                if (isDot)
                    evaluation -= Mathf.Lerp(8f, 28f, difficulty);
                else if (isEasyShape)
                    evaluation -= Mathf.Lerp(2f, 10f, difficulty);

                if (validPlacements > 10)
                    evaluation -= Mathf.Lerp(0f, 12f, difficulty);
                if (validPlacements <= 3)
                    evaluation += Mathf.Lerp(0f, 10f, difficulty);

                if (mobility > 12 && cellCount >= 3)
                    evaluation += 4f;
            }

            if (validPlacements == 0)
                evaluation -= 100f;

            if (difficulty >= 0.75f && mobility > 6 && isDot)
                evaluation -= 50f;

            candidates.Add(new ShapeCandidate(shape, evaluation));
        }

        candidates.Sort((a, b) => b.score.CompareTo(a.score));

        int choiceCount = difficulty < 0.5f ? Mathf.Min(3, candidates.Count) : Mathf.Min(2, candidates.Count);
        if (choiceCount == 0)
            return null;

        int pickedIndex = Random.Range(0, choiceCount);
        return candidates[pickedIndex].shape ?? allShapes[Random.Range(0, allShapes.Count)];
    }

    private int EstimateTrayMobility(BoardState boardState, List<PieceShapeData> currentSelection)
    {
        int mobility = 0;
        if (boardState == null || currentSelection == null)
            return mobility;

        foreach (PieceShapeData shape in currentSelection)
        {
            if (shape != null)
                mobility += BoardAnalyzer.CountValidPlacements(boardState, shape);
        }

        return mobility;
    }

    private readonly struct ShapeCandidate
    {
        public readonly PieceShapeData shape;
        public readonly float score;

        public ShapeCandidate(PieceShapeData shape, float score)
        {
            this.shape = shape;
            this.score = score;
        }
    }
}
