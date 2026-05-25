using UnityEngine;

// NPC con el que el jugador puede interactuar.
// Implementando IInteractable para que el sistema de interacción lo detecte.

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("Menú que abre este NPC")]
    public GameObject menuNPC;

    [Header("Tipo de NPC")]
    // Mineral relacionado con este NPC
    // sirve para mostrar tienda/mejoras de ese mineral
    public MineralType mineralAsociado;
    public bool esNPCCentral = false;

    [Header("Texto de introducción")]
    [TextArea(3, 8)]
    // Texto que aparece en la pestaña Inicio
    public string textoIntro = "Hola, soy un NPC.";

    public bool CanInteract() => menuNPC != null && !menuNPC.activeSelf && !PauseController.IsGamePaused;
    // Puede interactuar si el menú existe y no está ya abierto, además de otros menus
    public void Interact()
    {
        if (!CanInteract()) return;
        AbrirMenu();
    }

    private void AbrirMenu()
    {
        // Activamos el menú en pantalla
        menuNPC.SetActive(true);
        // Pausamos el juego mientras el menú está abierto
        PauseController.SetPause(true);

        NPCMenuController menuCtrl = menuNPC.GetComponent<NPCMenuController>();

        if (menuCtrl != null)
        {
            // Inicializamos el menú con: Mineral asociado, Si es NPC central, false = no es acceso rápido
            menuCtrl.Inicializar(mineralAsociado, esNPCCentral, false);
            // Pasamos el texto de intro al menú
            menuCtrl.SetTextoIntro(textoIntro);
        }
    }

    // Llamado desde el boton de cerrar de la UI ocultando
    // el menú y quitando la pausa.
    public void CerrarMenu()
    {
        if (menuNPC != null)
            menuNPC.SetActive(false);
        PauseController.SetPause(false);
    }
}