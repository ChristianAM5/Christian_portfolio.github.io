using UnityEngine;

// Este script permite abrir rápidamente el menú de venta
// presionando la tecla (N), pero solo cuando el acertijo correspondiente este completado.
public class CentralNPCQuickAccess : MonoBehaviour
{
    [Header("UI NPC VENDER")]
    public GameObject canvasNPCcentral;

    // Controla si el jugador puede usar el acceso rápido
    // Empieza desactivado
    private bool accesoActivo = false;

    // Esta función la llama desde RiddleRewardManager cuando se completa el acertijo.
    public void ActivarAcceso()
    {
        accesoActivo = true;
    }

    private void Update()
    {
        // Si el acceso rápido no está habilitado, no hacemos nada.
        if (!accesoActivo) return;

        if (Input.GetKeyDown(KeyCode.N))
        {
            canvasNPCcentral.SetActive(true);
            PauseController.SetPause(true);

            // Obtenemos el script que controla el menu
            NPCMenuController menu = canvasNPCcentral.GetComponent<NPCMenuController>();
            
            // Y lo inicializamos para que solo muestre la pagina de vender.
            if (menu != null)
            {
                menu.Inicializar(default, true, true);
            }
        }
    }
}
