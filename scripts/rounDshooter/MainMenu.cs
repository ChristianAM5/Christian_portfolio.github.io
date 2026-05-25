using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private string firstLevelName = "Level01"; // Nombre exacto de tu primera escena

    [Header("Panel de Controles")]
    [SerializeField] private GameObject controlsPanel;

    private bool controlsOpen = false;

    void Update()
    {
        // Si el panel está abierto y se pulsa cualquier tecla o click
        if (controlsOpen)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame ||
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                HideControls();
            }
        }
    }

    /// <summary>
    /// Se llama al pulsar el bot�n de Jugar.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene(firstLevelName);
    }

    /// <summary>
    /// Se llama al pulsar el bot�n de Salir.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    /// <summary>
    /// Muestra el panel de controles.
    /// </summary>
    public void ShowControls()
    {
        controlsPanel.SetActive(true);
        controlsOpen = true;
    }

    /// <summary>
    /// Oculta el panel de controles.
    /// </summary>
    private void HideControls()
    {
        controlsPanel.SetActive(false);
        controlsOpen = false;
    }
}
