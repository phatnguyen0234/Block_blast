using System;

[Serializable]
public class BoardAnalysisResult
{
    public int occupiedCells;
    public int emptyCells;
    public int completableRows;
    public int completableColumns;
    public int tightRows;
    public int tightColumns;
    public int maxPlacementsForAnyShape;
    public float fillRatio;
}
