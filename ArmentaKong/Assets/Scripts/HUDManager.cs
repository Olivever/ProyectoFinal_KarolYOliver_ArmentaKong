using UnityEngine;
using UnityEngine.UI;

// HUDManager — Muestra puntuacion, vidas y nivel en pantalla.
// Usa UI Text estandar (no requiere TextMeshPro).
//
// SETUP en Unity:
//   1. Crea un Canvas (Screen Space - Overlay).
//   2. Dentro del Canvas crea un GameObject vacio llamado "HUD".
//   3. Agrega 3 componentes Text (UI > Legacy > Text) al Canvas:
//      ScoreText, LivesText, LevelText
//   4. Agrega este script al GameObject "HUD".
//   5. Arrastra los tres Text al inspector de este script.

public class HUDManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Text scoreText;
    public Text livesText;
    public Text levelText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            UpdateScore(GameManager.Instance.currentScore);
            UpdateLives(GameManager.Instance.currentLives);
            UpdateLevel(GameManager.Instance.currentLevel);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score.ToString("D6");
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = "LIVES: " + lives.ToString();
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null)
            levelText.text = "LEVEL: " + level.ToString();
    }
}