using UnityEngine;
using System.Collections.Generic;

public class PieceFactory : MonoBehaviour
{
    [SerializeField] private PieceView piecePrefab;
    [SerializeField] private PieceShapeData[] availableShapes;

    public IReadOnlyList<PieceShapeData> AvailableShapes => availableShapes;

    public PieceView CreateRandomPiece(Transform parent)
    {
        int index = Random.Range(0, availableShapes.Length);
        PieceShapeData shape = availableShapes[index];
        return CreatePiece(shape, parent);
    }

    public PieceView CreatePiece(PieceShapeData shape, Transform parent)
    {
        if (shape == null || piecePrefab == null || parent == null)
            return null;

        PieceView piece = Instantiate(piecePrefab, parent);
        piece.transform.localPosition = Vector3.zero;
        piece.transform.localRotation = Quaternion.identity;

        piece.Setup(shape);
        return piece;
    }

    public PieceShapeData GetShapeById(string shapeId)
    {
        if (string.IsNullOrWhiteSpace(shapeId) || availableShapes == null)
            return null;

        foreach (PieceShapeData shape in availableShapes)
        {
            if (shape != null && shape.StableId == shapeId)
                return shape;
        }

        return null;
    }
}
