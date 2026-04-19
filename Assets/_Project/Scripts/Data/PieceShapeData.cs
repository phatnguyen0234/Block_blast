using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PieceShapeData", menuName = "BlockPuzzle/Piece Shape")]
public class PieceShapeData : ScriptableObject
{
    public string id;
    public List<Vector2Int> cells = new List<Vector2Int>();

    public string StableId => string.IsNullOrWhiteSpace(id) ? name.ToLowerInvariant() : id;
}
