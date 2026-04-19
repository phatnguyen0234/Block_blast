using UnityEngine;
using System;

public class PieceDragHandler : MonoBehaviour
{
    public event Action<PieceView> PiecePlaced;
    public event Action PiecePlacementFailed;

    private BoardView boardView;
    private BoardState boardState;

    private PieceView draggedPiece;
    private Vector3 dragOffset;
    private Vector3 originalPosition;
    private Vector2Int grabbedCellOffset;
    private bool inputEnabled = true;
    private Vector3 dragTargetPosition;
    private PieceView ghostPiece;

    public void Initialize(BoardView bv, BoardState bs)
    {
        boardView = bv;
        boardState = bs;
        Debug.Log("[PieceDragHandler] Initialized with BoardView and BoardState");
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    private void Update()
    {
        if (boardView == null || boardState == null || !inputEnabled)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }
        else if (Input.GetMouseButton(0) && draggedPiece != null)
        {
            HandleMouseDrag();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }
    }

    private void HandleMouseDown()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePoint = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePoint, Vector2.zero);

        if (hit.collider != null)
        {
            PieceView piece = hit.collider.GetComponent<PieceView>();
            if (piece != null && piece.DragEnabled)
            {
                draggedPiece = piece;
                originalPosition = draggedPiece.transform.position;
                dragOffset = draggedPiece.transform.position - new Vector3(mousePoint.x, mousePoint.y, 0f);
                grabbedCellOffset = GetClosestCellOffset(draggedPiece, mousePoint);
                dragTargetPosition = originalPosition;
                CreateGhostPiece();
                
                Debug.Log($"[PieceDragHandler] Started dragging piece: {draggedPiece.name}");
            }
        }
    }

    private void HandleMouseDrag()
    {
        Vector3 mouseWorldPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseWorldPos);

        dragTargetPosition = mouseWorldPos + dragOffset;
        draggedPiece.transform.position = Vector3.Lerp(
            draggedPiece.transform.position,
            dragTargetPosition,
            20f * Time.deltaTime);
        draggedPiece.SyncBasePosition();

        Vector2Int gridPos = GetClampedGridPosition(mouseWorldPos);
        bool canPlace = PlacementValidator.CanPlacePiece(boardState, draggedPiece.ShapeData, gridPos.x, gridPos.y);
        boardView.ShowPlacementPreview(draggedPiece.ShapeData, gridPos.x, gridPos.y, canPlace);
        UpdateGhostPiece(gridPos, canPlace);
    }

    private void HandleMouseUp()
    {
        if (draggedPiece == null)
            return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = GetClampedGridPosition(mouseWorldPos);
        int gridX = gridPos.x;
        int gridY = gridPos.y;
        boardView.ClearPlacementPreview();
        DestroyGhostPiece();

        if (PlacementValidator.CanPlacePiece(boardState, draggedPiece.ShapeData, gridX, gridY))
        {
            PlacementValidator.PlacePiece(boardState, draggedPiece.ShapeData, gridX, gridY);
            Vector3 snappedPos = boardView.GridToWorldPosition(gridX, gridY);
            draggedPiece.transform.position = snappedPos;
            draggedPiece.SyncBasePosition();
            draggedPiece.SetDragEnabled(false);
            PiecePlaced?.Invoke(draggedPiece);
            
            Debug.Log($"[PieceDragHandler] Piece placed at ({gridX}, {gridY})");
        }
        else
        {
            draggedPiece.transform.position = originalPosition;
            draggedPiece.SyncBasePosition();
            draggedPiece.PlayShake();
            PiecePlacementFailed?.Invoke();
            Debug.Log($"[PieceDragHandler] Placement invalid, returned to original position");
        }

        draggedPiece = null;
    }

    private Vector2Int GetClampedGridPosition(Vector3 mouseWorldPos)
    {
        Vector2Int hoveredGridPos = boardView.WorldToGridCoordinates(mouseWorldPos);
        Vector2Int proposedGridPos = hoveredGridPos - grabbedCellOffset;
        GetShapeBounds(draggedPiece.ShapeData, out int minCellX, out int minCellY, out int maxCellX, out int maxCellY);

        int minGridX = -minCellX;
        int maxGridX = BoardState.Width - 1 - maxCellX;
        int minGridY = -minCellY;
        int maxGridY = BoardState.Height - 1 - maxCellY;

        int gridX = Mathf.Clamp(proposedGridPos.x, minGridX, maxGridX);
        int gridY = Mathf.Clamp(proposedGridPos.y, minGridY, maxGridY);
        return new Vector2Int(gridX, gridY);
    }

    private Vector2Int GetClosestCellOffset(PieceView piece, Vector2 mouseWorldPos)
    {
        Vector3 localHitPos = piece.transform.InverseTransformPoint(mouseWorldPos);
        Vector2Int closestCell = Vector2Int.zero;
        float closestDistance = float.MaxValue;

        foreach (Vector2Int cell in piece.ShapeData.cells)
        {
            Vector3 cellCenter = new Vector3(
                cell.x * boardView.CellSize,
                cell.y * boardView.CellSize,
                0f);

            float sqrDistance = (cellCenter - localHitPos).sqrMagnitude;
            if (sqrDistance < closestDistance)
            {
                closestDistance = sqrDistance;
                closestCell = cell;
            }
        }

        return closestCell;
    }

    private void GetShapeBounds(PieceShapeData shapeData, out int minX, out int minY, out int maxX, out int maxY)
    {
        minX = int.MaxValue;
        minY = int.MaxValue;
        maxX = int.MinValue;
        maxY = int.MinValue;

        foreach (Vector2Int cell in shapeData.cells)
        {
            minX = Mathf.Min(minX, cell.x);
            minY = Mathf.Min(minY, cell.y);
            maxX = Mathf.Max(maxX, cell.x);
            maxY = Mathf.Max(maxY, cell.y);
        }
    }

    private void CreateGhostPiece()
    {
        DestroyGhostPiece();

        if (draggedPiece == null)
            return;

        ghostPiece = Instantiate(draggedPiece.gameObject).GetComponent<PieceView>();
        if (ghostPiece == null)
            return;

        ghostPiece.name = $"{draggedPiece.name}_Ghost";
        ghostPiece.SetDragEnabled(false);
        Collider2D ghostCollider = ghostPiece.GetComponent<Collider2D>();
        if (ghostCollider != null)
            ghostCollider.enabled = false;

        ghostPiece.SetVisualColor(new Color(1f, 1f, 1f, 1f));
        ghostPiece.SetVisualAlpha(0.35f);
    }

    private void UpdateGhostPiece(Vector2Int gridPos, bool canPlace)
    {
        if (ghostPiece == null)
            return;

        ghostPiece.transform.position = boardView.GridToWorldPosition(gridPos.x, gridPos.y);
        ghostPiece.SyncBasePosition();
        ghostPiece.SetVisualColor(canPlace ? new Color(0.7f, 1f, 0.78f, 1f) : new Color(1f, 0.55f, 0.55f, 1f));
        ghostPiece.SetVisualAlpha(canPlace ? 0.32f : 0.42f);
    }

    private void DestroyGhostPiece()
    {
        if (ghostPiece != null)
            Destroy(ghostPiece.gameObject);

        ghostPiece = null;
    }
}
