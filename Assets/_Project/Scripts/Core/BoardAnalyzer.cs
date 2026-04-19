using System.Collections.Generic;

public static class BoardAnalyzer
{
    public static BoardAnalysisResult Analyze(BoardState boardState, IReadOnlyList<PieceShapeData> shapes)
    {
        BoardAnalysisResult result = new BoardAnalysisResult();
        if (boardState == null)
            return result;

        result.occupiedCells = boardState.CountOccupiedCells();
        result.emptyCells = BoardState.Width * BoardState.Height - result.occupiedCells;
        result.fillRatio = (float)result.occupiedCells / (BoardState.Width * BoardState.Height);

        for (int y = 0; y < BoardState.Height; y++)
        {
            int occupiedInRow = 0;
            for (int x = 0; x < BoardState.Width; x++)
            {
                if (boardState.IsOccupied(x, y))
                    occupiedInRow++;
            }

            if (occupiedInRow >= BoardState.Width - 1)
                result.tightRows++;
            if (occupiedInRow == BoardState.Width)
                result.completableRows++;
        }

        for (int x = 0; x < BoardState.Width; x++)
        {
            int occupiedInColumn = 0;
            for (int y = 0; y < BoardState.Height; y++)
            {
                if (boardState.IsOccupied(x, y))
                    occupiedInColumn++;
            }

            if (occupiedInColumn >= BoardState.Height - 1)
                result.tightColumns++;
            if (occupiedInColumn == BoardState.Height)
                result.completableColumns++;
        }

        if (shapes != null)
        {
            foreach (PieceShapeData shape in shapes)
            {
                if (shape == null)
                    continue;

                int placements = CountValidPlacements(boardState, shape);
                if (placements > result.maxPlacementsForAnyShape)
                    result.maxPlacementsForAnyShape = placements;
            }
        }

        return result;
    }

    public static int CountValidPlacements(BoardState boardState, PieceShapeData shapeData)
    {
        if (boardState == null || shapeData == null)
            return 0;

        int count = 0;
        for (int y = 0; y < BoardState.Height; y++)
        {
            for (int x = 0; x < BoardState.Width; x++)
            {
                if (PlacementValidator.CanPlacePiece(boardState, shapeData, x, y))
                    count++;
            }
        }

        return count;
    }
}
