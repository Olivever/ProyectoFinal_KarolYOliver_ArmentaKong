using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // SINGLETON
    public static GameManager Instance { get; private set; }

    // CONFIGURACION
    [Header("Configuracion de Juego")]
    public int startingLives = 3;
    public int pointsPerBarrel = 300;
    public int pointsForWin = 1000;
    public string mainMenuScene = "MainMenu";
    public string levelScenePrefix = "Level";
    public int totalLevels = 2;

    // ESTADO
    [HideInInspector] public int currentLives;
    [HideInInspector] public int currentScore;
    [HideInInspector] public int currentLevel = 1;

    // UNITY LIFECYCLE
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetForNewGame();
    }

    // METODOS PUBLICOS
    public void ResetForNewGame()
    {
        currentLives = startingLives;
        currentScore = 0;
        currentLevel = 1;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        HUDManager hud = FindFirstObjectByType<HUDManager>();
        if (hud != null) hud.UpdateScore(currentScore);
    }

    public void PlayerDied()
    {
        currentLives--;

        HUDManager hud = FindFirstObjectByType<HUDManager>();
        if (hud != null) hud.UpdateLives(currentLives);

        if (currentLives <= 0)
            Invoke(nameof(LoadGameOver), 1.5f);
        else
            Invoke(nameof(ReloadCurrentLevel), 1.5f);
    }

    public void LevelWon()
    {
        AddScore(pointsForWin);
        currentLevel++;
        if (currentLevel > totalLevels)
            currentLevel = 1;
        Invoke(nameof(LoadNextLevel), 2f);
    }

    // CARGA DE ESCENAS
    private void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }

    private void ReloadCurrentLevel()
    {
        SceneManager.LoadScene(levelScenePrefix + currentLevel);
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(levelScenePrefix + currentLevel);
    }

    public void RestartGame()
    {
        ResetForNewGame();
        SceneManager.LoadScene(levelScenePrefix + "1");
    }

    public void GoToMainMenu()
    {
        ResetForNewGame();
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}