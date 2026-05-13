using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainMenuUI — Controlador de la pantalla de inicio.
///
/// Botones:
///   • Jugar ? carga Level1
///   • Salir ? cierra la aplicación
///
/// SETUP en Unity:
///   1. Crea una escena llamada "MainMenu".
///   2. Crea un Canvas con dos botones: "BtnPlay" y "BtnQuit".
///   3. Añade este script a un GameObject vacío.
///   4. En el onClick de cada botón, arrastra el GameObject y asigna
///      el método correspondiente (OnPlayClicked / OnQuitClicked).
///
/// IMPORTANTE: La escena "MainMenu" debe estar en el Build Settings
/// antes de las escenas de nivel.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Tooltip("Nombre de la primera escena de nivel")]
    public string firstLevelScene = "Level1";

    /// <summary>Botón "Jugar".</summary>
    public void OnPlayClicked()
    {
        // Si el GameManager existe, reiniciar estado antes de jugar
        if (GameManager.Instance != null)
            GameManager.Instance.ResetForNewGame();

        SceneManager.LoadScene(firstLevelScene);
    }

    /// <summary>Botón "Salir".</summary>
    public void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
