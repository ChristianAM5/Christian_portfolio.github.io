using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

// Controla la página de mejoras del NPC:
// Muestra niveles, muestra costes, permite comprar mejoras y da feedback visual

public class NPCMejorasController : MonoBehaviour
{
    // Representa un bloque visual con:
    // Nivel | Descripción | Coste | Botón
    [System.Serializable]
    public class EntradaMejora
    {
        public TextMeshProUGUI textoNivel;
        public TextMeshProUGUI textoDescripcion;
        public TextMeshProUGUI textoCoste;
        public Button botonMejorar;
    }

    [Header("Mejoras")]
    public EntradaMejora mejoraCantidad;
    public EntradaMejora mejoraPrecio;
    public EntradaMejora mejoraCapacidad;
    public EntradaMejora mejoraAutoGen;

    [Header("Feedback global de la página")]
    public TextMeshProUGUI textoFeedback;
    public float duracionFeedback = 3f;

    private MineralType mineralActual;    // Mineral del NPC actual
    private Coroutine corrutinaFeedback;  // Para poder reiniciar el temporizador

    // conectar botones a funciones por codigo ?. evita error si algo no está asignado
    private void Awake()
    {
        mejoraCantidad?.botonMejorar.onClick.AddListener(MejorarCantidad);
        mejoraPrecio?.botonMejorar.onClick.AddListener(MejorarPrecio);
        mejoraCapacidad?.botonMejorar.onClick.AddListener(MejorarCapacidad);
        mejoraAutoGen?.botonMejorar.onClick.AddListener(MejorarAutoGen);
    }

    // INICIALIZAR PÁGINA CON UN MINERAL
    public void Inicializar(MineralType mineral)
    {
        mineralActual = mineral;
        Refrescar();
    }

    // ACTUALIZAR TODOS LOS DATOS EN PANTALLA
    public void Refrescar()
    {
        if (MineralUpgradeManager.Instance == null) return;

        MineralUpgradeManager mgr = MineralUpgradeManager.Instance;
        int max = MineralUpgradeManager.MAX_NIVEL;

        // Cada llamada actualiza una fila de mejora
        RefrescarEntrada(mejoraCantidad,
            mgr.GetNivelCantidad(mineralActual) + 1, max,
            $"Cantidad por click: {mgr.GetCantidadPorClick(mineralActual)}",
            mgr.GetCosteSiguienteCantidad(mineralActual));

        RefrescarEntrada(mejoraPrecio,
            mgr.GetNivelPrecio(mineralActual) + 1, max,
            $"Precio venta: {mgr.GetPrecioVenta(mineralActual)}€/ud",
            mgr.GetCosteSiguientePrecio(mineralActual));

        RefrescarEntrada(mejoraCapacidad,
            mgr.GetNivelCapacidad(mineralActual) + 1, max,
            $"Capacidad slot: {mgr.GetCapacidad(mineralActual)} uds",
            mgr.GetCosteSiguienteCapacidad(mineralActual));

        RefrescarEntrada(mejoraAutoGen,
            mgr.GetNivelAutoGen(mineralActual), max,
            $"Auto-generadores: {mgr.GetNivelAutoGen(mineralActual)}/{max}",
            mgr.GetCosteSiguienteAutoGen(mineralActual));
    }

    // ACTUALIZA UNA FILA DE MEJORA
    private void RefrescarEntrada(EntradaMejora e, int nivel, int max, string desc, int coste)
    {
        if (e == null) return;
        bool esMaximo = nivel >= max;                 // ¿ya está al tope?
        int euros = EuroManager.Instance?.Euros ?? 0; // dinero actual

        if (e.textoNivel != null)
            e.textoNivel.text = $"Nivel {nivel}/{max}";
        if (e.textoDescripcion != null)
            e.textoDescripcion.text = desc;
        if (e.textoCoste != null)
            e.textoCoste.text = esMaximo ? "MÁXIMO" : $"Coste: {coste}€";
        if (e.botonMejorar != null)
            // Solo se puede pulsar si:  No está al máximo y tienes suficiente dinero
            e.botonMejorar.interactable = !esMaximo && euros >= coste;
    }

    // BOTONES DE MEJORA
    private void MejorarCantidad()
    {
        int coste = MineralUpgradeManager.Instance.GetCosteSiguienteCantidad(mineralActual);
        if (coste < 0) { MostrarFeedback("¡Ya está al nivel máximo!", false); return; }
        if (!EuroManager.Instance.GastarEuros(coste))
        { MostrarFeedback($"Necesitas {coste}€", false); return; }
        MineralUpgradeManager.Instance.MejorarCantidad(mineralActual);
        MostrarFeedback($"¡Cantidad mejorada! Coste: {coste}€", true);
        Refrescar();
    }

    private void MejorarPrecio()
    {
        int coste = MineralUpgradeManager.Instance.GetCosteSiguientePrecio(mineralActual);
        if (coste < 0) { MostrarFeedback("¡Ya está al nivel máximo!", false); return; }
        if (!EuroManager.Instance.GastarEuros(coste))
        { MostrarFeedback($"Necesitas {coste}€", false); return; }
        MineralUpgradeManager.Instance.MejorarPrecio(mineralActual);
        MostrarFeedback($"¡Precio mejorado! Coste: {coste}€", true);
        Refrescar();
    }

    private void MejorarCapacidad()
    {
        int coste = MineralUpgradeManager.Instance.GetCosteSiguienteCapacidad(mineralActual);
        if (coste < 0) { MostrarFeedback("¡Ya está al nivel máximo!", false); return; }
        if (!EuroManager.Instance.GastarEuros(coste))
        { MostrarFeedback($"Necesitas {coste}€", false); return; }
        MineralUpgradeManager.Instance.MejorarCapacidad(mineralActual);
        MostrarFeedback($"¡Capacidad mejorada! Coste: {coste}€", true);
        Refrescar();
    }

    private void MejorarAutoGen()
    {
        int coste = MineralUpgradeManager.Instance.GetCosteSiguienteAutoGen(mineralActual);
        if (coste < 0) { MostrarFeedback("¡Ya está al nivel máximo!", false); return; }
        if (!EuroManager.Instance.GastarEuros(coste))
        { MostrarFeedback($"Necesitas {coste}€", false); return; }
        MineralUpgradeManager.Instance.AnadirAutoGenerador(mineralActual);
        AutoGenManager.Instance?.ActualizarAutoGeneradores(mineralActual);
        MostrarFeedback($"¡Auto-generador añadido! Coste: {coste}€", true);
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
}