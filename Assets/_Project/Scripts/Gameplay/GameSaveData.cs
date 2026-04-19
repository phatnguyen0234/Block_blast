using System;

[Serializable]
public class GameSaveData
{
    public int score;
    public int combo;
    public int turnIndex;
    public bool isGameOver;
    public bool[] boardCells;
    public string[] trayPieceIds;
}
