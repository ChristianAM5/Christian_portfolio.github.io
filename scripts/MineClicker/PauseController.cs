using UnityEngine;

public class PauseController : MonoBehaviour
{
    // Propiedad estática que indica si el juego está pausado.
    // 'static' permite acceder a ella como PauseController.IsGamePaused.
    // 'private set' significa que solo este script puede cambiar el valor, pero cualquiera puede leerlo.
    public static bool IsGamePaused { get; private set; } = false;

    /// <summary>
    /// Cambia el estado de pausa del juego.
    /// </summary>
    /// <param name="pause">True para pausar, False para reanudar.</param>
    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;
    }
}

