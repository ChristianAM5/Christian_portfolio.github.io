using UnityEngine;

// Controla que canvas de acertijo se abre en cada cartel,
// además de permitir rellenar las respuestas validas para el mismo
// y recoge si ha sido completado con un booleano

public class RiddleSign : MonoBehaviour, IInteractable
{
    [Header("Canvas de este acertijo")]
    public GameObject riddleCanvas;

    [Header("Respuestas válidas (separadas, sin importar mayúsculas)")]
    public string[] respuestasValidas;

    [Header("¿Ya está resuelto?")]
    public bool resuelto = false;

    public bool CanInteract() => !resuelto && !PauseController.IsGamePaused;
    // se puede interactuar si no esta resuelto y no esta pausado el juego

    public void Interact()
    {
        if (!CanInteract()) return;

        riddleCanvas.SetActive(true);
        PauseController.SetPause(true);

        // Le pasamos este cartel al controlador del canvas
        RiddleUI ctrl = riddleCanvas.GetComponent<RiddleUI>();
        if (ctrl != null)
            ctrl.Inicializar(this);
    }

    // Marca el acertijo como completado y destruye el objeto cartel
    public void MarcarResuelto()
    {
        resuelto = true;
        Destroy(gameObject);
    }
}
