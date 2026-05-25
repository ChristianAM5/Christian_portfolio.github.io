using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Controlador de la página "Zonas" del NPC
// Permite ver qué zonas están desbloqueadas y desbloquear nuevas zonas

public class NPCZonasController : MonoBehaviour
{
    // CLASE INTERNA: UNA ENTRADA DE ZONA EN LA UI
    [System.Serializable]
    public class EntradaZona
    {
        public string nombreZona;           // Nombre de la zona
        public TextMeshProUGUI textoNombre; // Nombre visible en UI
        public TextMeshProUGUI textoCoste;  // Texto con coste de desbloqueo
        public TextMeshProUGUI textoEstado; // Bloqueada / Desbloqueada
        public Button botonDesbloquear;     // Botón para intentar desbloquear
    }

    [Header("Entradas de zona")]
    public List<EntradaZona> entradasZona; // Lista de todas las zonas que muestra el NPC

    [Header("Feedback global de la página")]
    public TextMeshProUGUI textoFeedback;
    public float duracionFeedback = 3f;

    private InventoryController inventoryController;
    private ZoneUnlockManager   zoneUnlockManager;   // Controla zonas desbloqueadas
    private Coroutine corrutinaFeedback;

    private void Awake()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
        zoneUnlockManager   = FindFirstObjectByType<ZoneUnlockManager>();
    }

    public void Inicializar()
    {
        // Conectamos botones de cada zona
        foreach (EntradaZona entrada in entradasZona)
        {
            string zona = entrada.nombreZona;
            entrada.botonDesbloquear?.onClick.AddListener(
                () => IntentarDesbloquear(zona));
        }
        Refrescar();  // Mostramos estado actual
    }

    // REFRESCAR TODAS LAS FILAS
    public void Refrescar()
    {
        if (zoneUnlockManager == null)
        {
            Debug.LogWarning("[NPCZonas] ZoneUnlockManager no encontrado.");
            return;
        }

        foreach (EntradaZona entrada in entradasZona)
        {
            bool desbloqueada = zoneUnlockManager.EstaDesbloqueada(entrada.nombreZona);

            // Nombre visible
            if (entrada.textoNombre != null)
                entrada.textoNombre.text = entrada.nombreZona;

            // Estado: Bloqueada / Desbloqueada
            if (entrada.textoEstado != null)
            {
                entrada.textoEstado.text  = desbloqueada ? "Desbloqueada" : "Bloqueada";
                entrada.textoEstado.color = desbloqueada
                    ? new Color(0.1f, 0.6f, 0.1f)  // VERDE
                    : new Color(0.8f, 0.1f, 0.1f); // ROJO
            }

            // Coste de desbloqueo
            if (entrada.textoCoste != null)
                entrada.textoCoste.text = desbloqueada
                    ? ""
                    : GetCostesTexto(entrada.nombreZona);

            // Configuración del botón
            if (entrada.botonDesbloquear != null)
            {
                // Activamos el botón solo si está bloqueada
                // Y si tiene todos los materiales
                bool puedeDesbloquear = !desbloqueada &&
                    string.IsNullOrEmpty(
                        zoneUnlockManager.GetMaterialesFaltantes(entrada.nombreZona));

                entrada.botonDesbloquear.interactable = !desbloqueada;

                // Color visual del botón
                ColorBlock cb = entrada.botonDesbloquear.colors;
                cb.normalColor = puedeDesbloquear
                    ? new Color(0.7f, 1f, 0.7f)
                    : new Color(1f, 1f, 1f);
                entrada.botonDesbloquear.colors = cb;
            }
        }
    }

    private void IntentarDesbloquear(string nombreZona)
    {
        bool exito = zoneUnlockManager.IntentarDesbloquear(
            nombreZona, out string error);

        if (exito)
	{
	    SoundEffectManager.Play("ZoneUnlock");
            MostrarFeedback($"¡Zona {nombreZona} desbloqueada!", true);
	}
        else
            MostrarFeedback(error, false);

        Refrescar();
    }

    private void MostrarFeedback(string mensaje, bool exito)
    {
        if (textoFeedback == null) return;
        if (corrutinaFeedback != null) StopCoroutine(corrutinaFeedback);
        corrutinaFeedback = StartCoroutine(FeedbackTemporal(mensaje, exito));
    }

    private IEnumerator FeedbackTemporal(string mensaje, bool exito)
    {
        textoFeedback.color = exito
            ? new Color(0.1f, 0.6f, 0.1f)
            : new Color(0.8f, 0.1f, 0.1f);
        textoFeedback.text = mensaje;
        textoFeedback.gameObject.SetActive(true);
        yield return new WaitForSeconds(duracionFeedback);
        textoFeedback.gameObject.SetActive(false);
    }
    
    // RETORNAR TEXTO DE COSTE SEGÚN ZONA
    private string GetCostesTexto(string nombreZona)
    {
        ZoneManager zm = ZoneManager.Instance;
        if (nombreZona == zm.zonaIzquierda)
            return "Coste: 50 Cuarzo";
        if (nombreZona == zm.zonaDerecha)
            return "Coste: 100 Cuarzo + 50 Carbón";
        if (nombreZona == zm.zonaArriba)
            return "Coste: 150 Cuarzo + 100 Carbón + 50 Bauxita";
        if (nombreZona == zm.zonaAbajo)
            return "Coste: 200 Cuarzo + 150 Carbón + 100 Bauxita + 50 Halita";
        return "";
    }
}