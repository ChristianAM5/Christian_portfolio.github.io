using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

// Controlador de la página "Vender" del NPC
// Muestra los minerales que el jugador tiene, sus precios, y permite venderlos

public class NPCVenderController : MonoBehaviour
{
    // UNA FILA DE MINERAL EN LA UI
    [System.Serializable]
    public class FilaMineral
    {
        public MineralType tipo;               // Tipo de mineral
        public TextMeshProUGUI textoCantidad;  // "Cuarzo: 10 uds"
        public TextMeshProUGUI textoPrecio;    // "10€/ud"
        public TextMeshProUGUI textoTotal;     // "→ 100€"
        public Button botonVender;             // Botón de vender
    }

    [Header("Filas de minerales")]
    public FilaMineral filaCuarzo;
    public FilaMineral filaCarbon;
    public FilaMineral filaBauxita;
    public FilaMineral filaHalita;
    public FilaMineral filaCobre;

    [Header("Feedback global de la página")]
    public TextMeshProUGUI textoFeedback;
    public float duracionFeedback = 3f;

    private InventoryController inventoryController;
    private FilaMineral[] todasLasFilas;            // Array con todas las filas
    private Coroutine corrutinaFeedback;            // Para mensajes temporales

    private void Awake()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();

        // Creamos array con todas las filas
        todasLasFilas = new FilaMineral[]
        { filaCuarzo, filaCarbon, filaBauxita, filaHalita, filaCobre };

        // Conectamos cada botón a su función de venta
        foreach (FilaMineral fila in todasLasFilas)
        {
            if (fila == null) continue;
            MineralType tipo = fila.tipo;
            fila.botonVender?.onClick.AddListener(() => VenderMineral(tipo));
        }
    }

    // INICIALIZAR PÁGINA
    public void Inicializar() => Refrescar();

    // REFRESCAR TODAS LAS FILAS   
    public void Refrescar()
    {
        if (MineralUpgradeManager.Instance == null) return;
        foreach (FilaMineral fila in todasLasFilas)
            if (fila != null) RefrescarFila(fila);
    }

    // REFRESCAR UNA FILA INDIVIDUAL
    private void RefrescarFila(FilaMineral fila)
    {
        int   cantidad = inventoryController.GetCantidadMineral(fila.tipo);
        float precio   = MineralUpgradeManager.Instance.GetPrecioVenta(fila.tipo);
        float total    = cantidad * precio;

        if (fila.textoCantidad != null)
            fila.textoCantidad.text = $"{fila.tipo}: {cantidad} uds";
        if (fila.textoPrecio != null)
            fila.textoPrecio.text = $"{precio}€/ud";
        if (fila.textoTotal != null)
            fila.textoTotal.text = $"→ {Mathf.RoundToInt(total)}€";
        // Solo habilitamos botón si hay al menos 1 unidad
        if (fila.botonVender != null)
            fila.botonVender.interactable = cantidad > 0;
    }

    private void VenderMineral(MineralType tipo)
    {
        int   cantidad = inventoryController.GetCantidadMineral(tipo);
        float precio   = MineralUpgradeManager.Instance.GetPrecioVenta(tipo);
        int   total    = Mathf.RoundToInt(cantidad * precio);

        // Validar cantidad
        if (cantidad <= 0)
        {
            MostrarFeedback($"No tienes {tipo} que vender.", false);
            return;
        }

        // Descontamos del inventario y añadimos euros
        inventoryController.GastarMineral(tipo, cantidad);
        EuroManager.Instance.AnadirEuros(total);

        // Reproducir efecto de sonido
	    SoundEffectManager.Play("Sell");

        MostrarFeedback($"¡Vendiste {cantidad} de {tipo} por {total}€!", true);
        Refrescar();
    }

    private void MostrarFeedback(string mensaje, bool exito)
    {
        if (textoFeedback == null) return;

        // Si hay mensaje activo, lo paramos
        if (corrutinaFeedback != null) StopCoroutine(corrutinaFeedback);
        corrutinaFeedback = StartCoroutine(FeedbackTemporal(mensaje, exito));
    }

    private IEnumerator FeedbackTemporal(string mensaje, bool exito)
    {
        // Verde si éxito, rojo si error
        textoFeedback.color = exito
            ? new Color(0.1f, 0.6f, 0.1f)   // verde
            : new Color(0.8f, 0.1f, 0.1f);   // rojo
        textoFeedback.text = mensaje;
        textoFeedback.gameObject.SetActive(true);
        yield return new WaitForSeconds(duracionFeedback);
        textoFeedback.gameObject.SetActive(false);
    }
}