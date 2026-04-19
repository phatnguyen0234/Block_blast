using UnityEngine;

public class PlacementValidator
{
    public static bool CanPlacePiece(BoardState boardState, PieceShapeData shapeData, int startX, int startY)
    {
        if (shapeData == null || boardState == null)
            return false;

        foreach (Vector2Int cell in shapeData.cells)
        {
            int x = startX + cell.x;
            int y = startY + cell.y;

            if (!boardState.IsInside(x, y))
                return false;

            if (boardState.IsOccupied(x, y))
                return false;
        }

        return true;
    }

    public static void PlacePiece(BoardState boardState, PieceShapeData shapeData, int startX, int startY)
    {
        foreach (Vector2Int cell in shapeData.cells)
        {
            int x = startX + cell.x;
            int y = startY + cell.y;
            boardState.SetCell(x, y, true);
        }
    }

    public static bool TryPlacePiece(BoardState boardState, PieceShapeData shapeData, int startX, int startY)
    {
        if (CanPlacePiece(boardState, shapeData, startX, startY))
        {
            PlacePiece(boardState, shapeData, startX, startY);
            return true;
        }
        return false;
    }

    public static bool HasAnyValidPlacement(BoardState boardState, PieceShapeData shapeData)
    {
        if (shapeData == null || boardState == null)
            return false;

        for (int y = 0; y < BoardState.Height; y++)
        {
            for (int x = 0; x < BoardState.Width; x++)
            {
                if (CanPlacePiece(boardState, shapeData, x, y))
                    return true;
            }
        }

        return false;
    }
}
