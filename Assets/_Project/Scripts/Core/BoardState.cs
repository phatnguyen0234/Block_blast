using System.Collections.Generic;

public class BoardState
{
    public const int Width = 8;
    public const int Height = 8;

    private readonly bool[,] cells = new bool[Width, Height];

    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public bool IsOccupied(int x, int y)
    {
        return cells[x, y];
    }

    public void SetCell(int x, int y, bool occupied)
    {
        cells[x, y] = occupied;
    }

    public void Clear()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                cells[x, y] = false;
            }
        }
    }

    public List<int> GetCompletedRows()
    {
        List<int> completedRows = new List<int>();

        for (int y = 0; y < Height; y++)
        {
            bool isComplete = true;
            for (int x = 0; x < Width; x++)
            {
                if (!cells[x, y])
                {
                    isComplete = false;
                    break;
                }
            }

            if (isComplete)
                completedRows.Add(y);
        }

        return completedRows;
    }

    public List<int> GetCompletedColumns()
    {
        List<int> completedColumns = new List<int>();

        for (int x = 0; x < Width; x++)
        {
            bool isComplete = true;
            for (int y = 0; y < Height; y++)
            {
                if (!cells[x, y])
                {
                    isComplete = false;
                    break;
                }
            }

            if (isComplete)
                completedColumns.Add(x);
        }

        return completedColumns;
    }

    public void ClearRow(int row)
    {
        for (int x = 0; x < Width; x++)
            cells[x, row] = false;
    }

    public void ClearColumn(int column)
    {
        for (int y = 0; y < Height; y++)
            cells[column, y] = false;
    }

    public bool[] ExportCells()
    {
        bool[] exported = new bool[Width * Height];
        int index = 0;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
                exported[index++] = cells[x, y];
        }

        return exported;
    }

    public void ImportCells(bool[] serializedCells)
    {
        Clear();

        if (serializedCells == null)
            return;

        int index = 0;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (index < serializedCells.Length)
                    cells[x, y] = serializedCells[index];

                index++;
            }
        }
    }

    public int CountOccupiedCells()
    {
        int occupied = 0;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (cells[x, y])
                    occupied++;
            }
        }

        return occupied;
    }
}
