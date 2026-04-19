using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private BoardView boardView;
    [SerializeField] private TrayController trayController;
    [SerializeField] private PieceDragHandler pieceDragHandler;
    [SerializeField] private SpawnDirector spawnDirector;

    private BoardState boardState;
    private GameHud gameHud;
    private GameAudio gameAudio;
    private int score;
    private int combo;
    private int turnIndex;
    private bool isGameOver;
    private float difficulty;
    private BoardAnalysisResult lastAnalysis;
    private bool wasGameOver;

    private void Start()
    {
        Debug.Log("[GameBootstrap] Starting game setup");

        if (pieceDragHandler == null)
            pieceDragHandler = GetComponent<PieceDragHandler>();
        if (pieceDragHandler == null)
            pieceDragHandler = gameObject.AddComponent<PieceDragHandler>();
        if (spawnDirector == null)
            spawnDirector = GetComponent<SpawnDirector>();
        if (spawnDirector == null)
            spawnDirector = gameObject.AddComponent<SpawnDirector>();
        
        if (boardView == null)
            Debug.LogError("[GameBootstrap] BoardView is NULL!");
        if (trayController == null)
            Debug.LogError("[GameBootstrap] TrayController is NULL!");
            
        // Initialize board state
        boardState = new BoardState();
        Debug.Log("[GameBootstrap] BoardState initialized");

        // Build board visuals
        boardView.BuildBoard();
        boardView.RefreshBoard(boardState);
        Debug.Log("[GameBootstrap] Board built");
        
        // Setup drag handler if it exists
        if (pieceDragHandler != null)
        {
            pieceDragHandler.Initialize(boardView, boardState);
            pieceDragHandler.PiecePlaced += HandlePiecePlaced;
            pieceDragHandler.PiecePlacementFailed += HandleInvalidPlacement;
            Debug.Log("[GameBootstrap] Drag handler initialized");
        }

        gameHud = GetComponent<GameHud>();
        if (gameHud == null)
            gameHud = gameObject.AddComponent<GameHud>();
        gameHud.NewGameRequested += ResetGame;

        gameAudio = GetComponent<GameAudio>();
        if (gameAudio == null)
            gameAudio = gameObject.AddComponent<GameAudio>();

        if (!TryRestoreGame())
        {
            SpawnNextTray();
            UpdateHud("Place a block");
        }

        EvaluateGameOver();
        SaveGame();
        Debug.Log("[GameBootstrap] Initial game state ready");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            ResetGame();
    }

    private void OnDestroy()
    {
        if (pieceDragHandler != null)
            pieceDragHandler.PiecePlaced -= HandlePiecePlaced;
        if (pieceDragHandler != null)
            pieceDragHandler.PiecePlacementFailed -= HandleInvalidPlacement;
        if (gameHud != null)
            gameHud.NewGameRequested -= ResetGame;
    }

    private void HandlePiecePlaced(PieceView piece)
    {
        if (isGameOver || piece == null)
            return;

        int placedBlocks = piece.ShapeData != null ? piece.ShapeData.cells.Count : 0;
        score += placedBlocks;
        turnIndex++;
        gameAudio?.PlayPlace();

        ResolveCompletedLines();
        boardView.RefreshBoard(boardState);

        trayController.RemovePiece(piece);

        if (trayController.AreAllSlotsEmpty())
            SpawnNextTray();

        EvaluateGameOver();
        SaveGame();
    }

    private void ResolveCompletedLines()
    {
        List<int> completedRows = boardState.GetCompletedRows();
        List<int> completedColumns = boardState.GetCompletedColumns();
        int linesCleared = completedRows.Count + completedColumns.Count;

        foreach (int row in completedRows)
            boardState.ClearRow(row);

        foreach (int column in completedColumns)
            boardState.ClearColumn(column);

        if (linesCleared > 0)
        {
            combo++;
            int clearScore = GetLineClearScore(linesCleared);
            int comboBonus = GetComboBonus(combo);
            score += clearScore + comboBonus;
            boardView.PlayClearEffect(completedRows, completedColumns);
            boardView.TriggerBoardPulse();
            gameHud?.ShowFloatingText($"+{clearScore + comboBonus}", new Color(1f, 0.95f, 0.45f));
            gameAudio?.PlayClear(linesCleared);
            if (combo > 1)
            {
                gameHud?.ShowFloatingText($"Combo x{combo}", new Color(1f, 0.65f, 0.2f));
                gameAudio?.PlayCombo(combo);
            }
            UpdateHud($"+{clearScore + comboBonus} clear");
        }
        else
        {
            combo = 0;
            UpdateHud("Placed block");
        }
    }

    private int GetLineClearScore(int linesCleared)
    {
        switch (linesCleared)
        {
            case 1:
                return 100;
            case 2:
                return 250;
            case 3:
                return 450;
            default:
                return 700 + Mathf.Max(0, linesCleared - 4) * 300;
        }
    }

    private int GetComboBonus(int comboCount)
    {
        switch (comboCount)
        {
            case 1:
                return 0;
            case 2:
                return 50;
            case 3:
                return 125;
            default:
                return 200 + Mathf.Max(0, comboCount - 4) * 75;
        }
    }

    private void EvaluateGameOver()
    {
        List<PieceView> activePieces = trayController.GetActivePieces();
        lastAnalysis = BoardAnalyzer.Analyze(boardState, activePieces.Select(piece => piece.ShapeData).ToList());
        difficulty = spawnDirector != null ? spawnDirector.GetDifficulty(score, turnIndex, boardState) : 0f;
        bool hasValidMove = false;

        foreach (PieceView piece in activePieces)
        {
            if (piece != null && PlacementValidator.HasAnyValidPlacement(boardState, piece.ShapeData))
            {
                hasValidMove = true;
                break;
            }
        }

        isGameOver = activePieces.Count > 0 && !hasValidMove;
        if (pieceDragHandler != null)
            pieceDragHandler.SetInputEnabled(!isGameOver);

        if (isGameOver && !wasGameOver)
            gameAudio?.PlayGameOver();
        wasGameOver = isGameOver;

        if (isGameOver)
            UpdateHud("Game Over");
        else if (activePieces.Count > 0)
            UpdateHud("Your turn");
    }

    private void UpdateHud(string status)
    {
        if (gameHud != null)
            gameHud.SetState(score, combo, turnIndex, difficulty, isGameOver, BuildStatus(status));
    }

    private void SpawnNextTray()
    {
        if (spawnDirector == null)
        {
            trayController.SpawnInitialPieces();
            return;
        }

        List<PieceShapeData> shapes = spawnDirector.BuildTray(boardState, score, turnIndex, 3);
        trayController.SpawnPieces(shapes);
    }

    private bool TryRestoreGame()
    {
        if (!SaveLoadService.TryLoad(out GameSaveData saveData) || saveData == null)
            return false;

        boardState.ImportCells(saveData.boardCells);
        score = saveData.score;
        combo = saveData.combo;
        turnIndex = saveData.turnIndex;
        isGameOver = saveData.isGameOver;

        trayController.ClearTray();

        List<PieceShapeData> shapes = new List<PieceShapeData>();
        PieceFactory pieceFactory = trayController.GetComponentInChildren<PieceFactory>();
        if (pieceFactory == null)
            pieceFactory = FindObjectOfType<PieceFactory>();

        if (saveData.trayPieceIds != null && pieceFactory != null)
        {
            foreach (string shapeId in saveData.trayPieceIds)
            {
                PieceShapeData shape = pieceFactory.GetShapeById(shapeId);
                if (shape != null)
                    shapes.Add(shape);
            }
        }

        if (shapes.Count == 0)
            SpawnNextTray();
        else
            trayController.SpawnPieces(shapes);

        boardView.RefreshBoard(boardState);
        UpdateHud("Loaded");
        return true;
    }

    private void SaveGame()
    {
        GameSaveData saveData = new GameSaveData
        {
            score = score,
            combo = combo,
            turnIndex = turnIndex,
            isGameOver = isGameOver,
            boardCells = boardState.ExportCells(),
            trayPieceIds = trayController.GetActivePieceIds().ToArray()
        };

        SaveLoadService.Save(saveData);
    }

    private void ResetGame()
    {
        SaveLoadService.Clear();

        score = 0;
        combo = 0;
        turnIndex = 0;
        isGameOver = false;
        difficulty = 0f;
        lastAnalysis = null;

        boardState.Clear();
        trayController.ClearTray();
        boardView.ClearPlacementPreview();
        boardView.RefreshBoard(boardState);

        SpawnNextTray();
        EvaluateGameOver();
        SaveGame();
        UpdateHud("New game");
        gameHud?.ShowFloatingText("Fresh start", new Color(0.65f, 1f, 0.8f));
        gameAudio?.PlayNewGame();
    }

    private void HandleInvalidPlacement()
    {
        gameHud?.ShowFloatingText("Invalid", new Color(1f, 0.55f, 0.55f));
        gameAudio?.PlayInvalid();
    }

    private string BuildStatus(string status)
    {
        int mobility = lastAnalysis != null ? lastAnalysis.maxPlacementsForAnyShape : 0;
        return $"{status} | M:{mobility}";
    }
}
