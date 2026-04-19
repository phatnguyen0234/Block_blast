using UnityEngine;
using System.Collections.Generic;

public class PieceView : MonoBehaviour
{
    [SerializeField] private GameObject blockUnitPrefab;
    [SerializeField] private float blockSize = 1f;

    private PieceShapeData shapeData;
    private bool dragEnabled = true;
    private BoxCollider2D boxCollider;
    private readonly List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    private Vector3 basePosition;
    private float shakeTimer;
    private float shakeStrength;

    public PieceShapeData ShapeData => shapeData;
    public bool DragEnabled => dragEnabled;

    private void OnEnable()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();
        
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
    }

    public void Setup(PieceShapeData data)
    {
        shapeData = data;
        BuildVisual();
        UpdateCollider();
        basePosition = transform.position;
    }

    private void Update()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            Vector2 offset = Random.insideUnitCircle * shakeStrength * Mathf.Clamp01(shakeTimer / 0.14f);
            transform.position = basePosition + new Vector3(offset.x, offset.y, 0f);
        }
        else if (transform.position != basePosition)
        {
            transform.position = basePosition;
        }
    }

    private void BuildVisual()
    {
        spriteRenderers.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        foreach (Vector2Int cell in shapeData.cells)
        {
            GameObject unit = Instantiate(blockUnitPrefab, transform);
            unit.transform.localPosition = new Vector3(cell.x * blockSize, cell.y * blockSize, 0f);
            unit.transform.localRotation = Quaternion.identity;
            unit.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 20;
                spriteRenderers.Add(sr);
            }
        }
    }

    private void UpdateCollider()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();

        if (shapeData.cells.Count > 0)
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (Vector2Int cell in shapeData.cells)
            {
                minX = Mathf.Min(minX, cell.x * blockSize);
                maxX = Mathf.Max(maxX, cell.x * blockSize);
                minY = Mathf.Min(minY, cell.y * blockSize);
                maxY = Mathf.Max(maxY, cell.y * blockSize);
            }

            Vector2 size = new Vector2(maxX - minX + blockSize, maxY - minY + blockSize);
            Vector2 center = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);

            boxCollider.size = size;
            boxCollider.offset = center;
        }
    }

    public void SetDragEnabled(bool enabled)
    {
        dragEnabled = enabled;
    }

    public void SetVisualColor(Color color)
    {
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer != null)
                renderer.color = color;
        }
    }

    public void SetVisualAlpha(float alpha)
    {
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer == null)
                continue;

            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    public void SyncBasePosition()
    {
        basePosition = transform.position;
    }

    public void PlayShake(float duration = 0.14f, float strength = 0.08f)
    {
        SyncBasePosition();
        shakeTimer = duration;
        shakeStrength = strength;
    }
}
