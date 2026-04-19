using UnityEngine;
using System.Collections.Generic;

public class BoardView : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Color emptyColor = new Color(0.31f, 0.86f, 0.89f);
    [SerializeField] private Color filledColor = new Color(0.99f, 0.90f, 0.29f);
    [SerializeField] private Color validPreviewColor = new Color(0.58f, 1f, 0.66f);
    [SerializeField] private Color invalidPreviewColor = new Color(1f, 0.49f, 0.49f);
    [SerializeField] private Color clearFlashColor = new Color(1f, 1f, 1f);

    private Transform cellContainer;
    private GameObject[,] cells;
    private SpriteRenderer[,] cellRenderers;
    private BoardState currentBoardState;
    private readonly List<Vector2Int> previewCells = new List<Vector2Int>();
    private readonly List<Vector2Int> clearCells = new List<Vector2Int>();
    private bool previewValid;
    private float clearFlashTimer;
    private float boardPulseTimer;
    private Vector3 baseScale = Vector3.one;

    public float CellSize => cellSize;
    public Vector3 BoardOrigin => transform.position;

    public void BuildBoard()
    {
        if (cellContainer != null)
            Destroy(cellContainer.gameObject);

        cellContainer = new GameObject("Cells").transform;
        cellContainer.SetParent(transform, false);

        cells = new GameObject[BoardState.Width, BoardState.Height];
        cellRenderers = new SpriteRenderer[BoardState.Width, BoardState.Height];

        for (int y = 0; y < BoardState.Height; y++)
        {
            for (int x = 0; x < BoardState.Width; x++)
            {
                GameObject cell = Instantiate(cellPrefab, cellContainer);
                cell.transform.localPosition = new Vector3(x * cellSize, y * cellSize, 0f);
                cell.transform.localRotation = Quaternion.identity;
                cell.name = $"Cell_{x}_{y}";
                cells[x, y] = cell;
                cellRenderers[x, y] = cell.GetComponent<SpriteRenderer>();
            }
        }

        ClearBoardVisuals();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (clearFlashTimer > 0f)
        {
            clearFlashTimer -= Time.deltaTime;
            if (clearFlashTimer <= 0f)
            {
                clearCells.Clear();
                ApplyVisualState();
            }
        }

        if (boardPulseTimer > 0f)
        {
            boardPulseTimer -= Time.deltaTime;
            float pulse = 1f + Mathf.Sin((1f - boardPulseTimer / 0.18f) * Mathf.PI) * 0.05f;
            transform.localScale = baseScale * pulse;
        }
        else
        {
            transform.localScale = baseScale;
        }
    }

    public bool WorldToGridPosition(Vector3 worldPos, out int gridX, out int gridY)
    {
        Vector2Int gridPos = WorldToGridCoordinates(worldPos);
        gridX = gridPos.x;
        gridY = gridPos.y;

        return gridX >= 0 && gridX < BoardState.Width && gridY >= 0 && gridY < BoardState.Height;
    }

    public Vector2Int WorldToGridCoordinates(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - BoardOrigin;
        int gridX = Mathf.RoundToInt(localPos.x / cellSize);
        int gridY = Mathf.RoundToInt(localPos.y / cellSize);
        return new Vector2Int(gridX, gridY);
    }

    public Vector3 GridToWorldPosition(int gridX, int gridY)
    {
        return BoardOrigin + new Vector3(gridX * cellSize, gridY * cellSize, 0f);
    }

    public GameObject GetCell(int x, int y)
    {
        if (x >= 0 && x < BoardState.Width && y >= 0 && y < BoardState.Height)
            return cells[x, y];
        return null;
    }

    public void RefreshBoard(BoardState boardState)
    {
        if (boardState == null || cellRenderers == null)
            return;

        currentBoardState = boardState;
        ApplyVisualState();
    }

    public void ClearBoardVisuals()
    {
        if (cellRenderers == null)
            return;

        for (int y = 0; y < BoardState.Height; y++)
        {
            for (int x = 0; x < BoardState.Width; x++)
            {
                SpriteRenderer renderer = cellRenderers[x, y];
                if (renderer != null)
                    renderer.color = emptyColor;
            }
        }
    }

    public void ShowPlacementPreview(PieceShapeData shapeData, int startX, int startY, bool isValid)
    {
        previewCells.Clear();
        previewValid = isValid;

        if (shapeData == null)
        {
            ApplyVisualState();
            return;
        }

        foreach (Vector2Int cell in shapeData.cells)
        {
            int x = startX + cell.x;
            int y = startY + cell.y;
            if (x >= 0 && x < BoardState.Width && y >= 0 && y < BoardState.Height)
                previewCells.Add(new Vector2Int(x, y));
        }

        ApplyVisualState();
    }

    public void ClearPlacementPreview()
    {
        if (previewCells.Count == 0)
            return;

        previewCells.Clear();
        ApplyVisualState();
    }

    public void PlayClearEffect(List<int> clearedRows, List<int> clearedColumns)
    {
        clearCells.Clear();

        if (clearedRows != null)
        {
            foreach (int row in clearedRows)
            {
                for (int x = 0; x < BoardState.Width; x++)
                    clearCells.Add(new Vector2Int(x, row));
            }
        }

        if (clearedColumns != null)
        {
            foreach (int column in clearedColumns)
            {
                for (int y = 0; y < BoardState.Height; y++)
                {
                    Vector2Int cell = new Vector2Int(column, y);
                    if (!clearCells.Contains(cell))
                        clearCells.Add(cell);
                }
            }
        }

        clearFlashTimer = clearCells.Count > 0 ? 0.16f : 0f;
        ApplyVisualState();
    }

    public void TriggerBoardPulse()
    {
        boardPulseTimer = 0.18f;
    }

    private void ApplyVisualState()
    {
        if (cellRenderers == null)
            return;

        for (int y = 0; y < BoardState.Height; y++)
        {
            for (int x = 0; x < BoardState.Width; x++)
            {
                SpriteRenderer renderer = cellRenderers[x, y];
                if (renderer == null)
                    continue;

                Color color = emptyColor;
                if (currentBoardState != null && currentBoardState.IsOccupied(x, y))
                    color = filledColor;

                Vector2Int cell = new Vector2Int(x, y);
                if (previewCells.Contains(cell))
                    color = previewValid ? validPreviewColor : invalidPreviewColor;

                if (clearCells.Contains(cell))
                    color = clearFlashColor;

                renderer.color = color;
            }
        }
    }
}
