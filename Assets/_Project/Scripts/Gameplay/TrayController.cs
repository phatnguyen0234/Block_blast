using UnityEngine;
using System.Collections.Generic;

public class TrayController : MonoBehaviour
{
    [SerializeField] private PieceFactory pieceFactory;
    [SerializeField] private Transform[] slots;

    private void Start()
    {
        FindSlots();
    }

    private void FindSlots()
    {
        if (slots == null || slots.Length == 0)
        {
            List<Transform> foundSlots = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                foundSlots.Add(transform.GetChild(i));
            }
            slots = foundSlots.ToArray();
            
            Debug.Log($"[TrayController] Found {slots.Length} slots");
        }
    }

    public void SpawnInitialPieces()
    {
        SpawnPiecesInEmptySlots();
    }

    public void SpawnPiecesInEmptySlots()
    {
        FindSlots();

        if (slots != null && slots.Length > 0 && pieceFactory != null)
        {
            foreach (Transform slot in slots)
            {
                if (slot.childCount > 0)
                    continue;

                pieceFactory.CreateRandomPiece(slot);
            }
        }
        else
        {
            Debug.LogWarning($"[TrayController] Failed to spawn pieces - Slots: {slots?.Length}, Factory: {(pieceFactory != null)}");
        }
    }

    public void SpawnPieces(List<PieceShapeData> shapes)
    {
        FindSlots();

        if (shapes == null || pieceFactory == null || slots == null)
            return;

        int count = Mathf.Min(shapes.Count, slots.Length);
        for (int i = 0; i < count; i++)
        {
            if (slots[i].childCount > 0)
                continue;

            pieceFactory.CreatePiece(shapes[i], slots[i]);
        }
    }

    public void RemovePiece(PieceView piece)
    {
        if (piece == null)
            return;

        piece.transform.SetParent(null);
        Destroy(piece.gameObject);
    }

    public bool AreAllSlotsEmpty()
    {
        FindSlots();

        foreach (Transform slot in slots)
        {
            if (slot.childCount > 0)
                return false;
        }

        return true;
    }

    public List<PieceView> GetActivePieces()
    {
        FindSlots();

        List<PieceView> pieces = new List<PieceView>();
        foreach (Transform slot in slots)
        {
            PieceView piece = slot.GetComponentInChildren<PieceView>();
            if (piece != null)
                pieces.Add(piece);
        }

        return pieces;
    }

    public List<string> GetActivePieceIds()
    {
        List<string> ids = new List<string>();
        foreach (PieceView piece in GetActivePieces())
        {
            if (piece != null && piece.ShapeData != null)
                ids.Add(piece.ShapeData.StableId);
        }

        return ids;
    }

    public void ClearTray()
    {
        FindSlots();

        foreach (Transform slot in slots)
        {
            for (int i = slot.childCount - 1; i >= 0; i--)
                Destroy(slot.GetChild(i).gameObject);
        }
    }
} 
