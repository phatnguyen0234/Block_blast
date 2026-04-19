using UnityEngine;
using System;
using System.Collections.Generic;

public class GameHud : MonoBehaviour
{
    public event Action NewGameRequested;

    private int score;
    private int combo;
    private int turn;
    private float difficulty;
    private bool isGameOver;
    private string status = "Ready";
    private readonly Queue<FloatingMessage> floatingQueue = new Queue<FloatingMessage>();
    private string floatingText = string.Empty;
    private float floatingTimer;
    private Color floatingColor = Color.white;

    private GUIStyle titleStyle;
    private GUIStyle valueStyle;
    private GUIStyle statusStyle;
    private GUIStyle buttonStyle;
    private GUIStyle floatingStyle;

    public void SetState(int newScore, int newCombo, int newTurn, float newDifficulty, bool gameOver, string newStatus)
    {
        score = newScore;
        combo = newCombo;
        turn = newTurn;
        difficulty = newDifficulty;
        isGameOver = gameOver;
        status = newStatus;
    }

    public void ShowFloatingText(string text, Color color)
    {
        floatingQueue.Enqueue(new FloatingMessage(text, color));

        if (floatingTimer <= 0f)
            ShowNextFloatingMessage();
    }

    private void Update()
    {
        if (floatingTimer > 0f)
            floatingTimer -= Time.deltaTime;
        else if (floatingQueue.Count > 0)
            ShowNextFloatingMessage();
    }

    private void OnGUI()
    {
        EnsureStyles();

        GUILayout.BeginArea(new Rect(16f, 16f, 170f, 160f), GUI.skin.box);
        GUILayout.Label($"Score  {score}", titleStyle);
        GUILayout.Label($"Combo  x{combo}", valueStyle);
        GUILayout.Label($"Turn  {turn}  D:{Mathf.RoundToInt(difficulty * 100f)}", valueStyle);
        GUILayout.Label(status, statusStyle);

        if (isGameOver)
            GUILayout.Label("No moves", statusStyle);

        GUILayout.Space(8f);
        if (GUILayout.Button("New Game", buttonStyle, GUILayout.Height(24f)))
            NewGameRequested?.Invoke();

        GUILayout.EndArea();

        if (floatingTimer > 0f && !string.IsNullOrWhiteSpace(floatingText))
        {
            float normalized = 1f - Mathf.Clamp01(floatingTimer / 1.1f);
            float yOffset = Mathf.Lerp(0f, -24f, normalized);
            Color previous = GUI.color;
            GUI.color = new Color(floatingColor.r, floatingColor.g, floatingColor.b, Mathf.Lerp(1f, 0f, normalized));
            GUI.Label(new Rect(24f, 184f + yOffset, 220f, 28f), floatingText, floatingStyle);
            GUI.color = previous;
        }
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            normal = { textColor = Color.white }
        };

        statusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = new Color(1f, 0.93f, 0.45f) }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        floatingStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
    }

    private void ShowNextFloatingMessage()
    {
        if (floatingQueue.Count == 0)
            return;

        FloatingMessage message = floatingQueue.Dequeue();
        floatingText = message.text;
        floatingColor = message.color;
        floatingTimer = 1.1f;
    }

    private readonly struct FloatingMessage
    {
        public readonly string text;
        public readonly Color color;

        public FloatingMessage(string text, Color color)
        {
            this.text = text;
            this.color = color;
        }
    }
}
