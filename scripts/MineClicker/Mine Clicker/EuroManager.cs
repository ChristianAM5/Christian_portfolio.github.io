using UnityEngine;
using TMPro;

// Gestiona la moneda global del juego (Euros).
// Centralizado para que cualquier sistema pueda consultarlo
// y para facilitar añadir multiplicadores.

public class EuroManager : MonoBehaviour
{
    public static EuroManager Instance { get; private set; }

    // Multiplicador global de ganancias 
    [Header("Multiplicadores globales")]
    public float multiplicadorGanancias = 1f;

    // Euros actuales
    [Header("Estado actual")]
    [SerializeField] private int euros = 0;
    public int Euros => euros;

    // Referencia al texto de la UI
    [Header("UI")]
    // Arrastrar un TextMeshPro que muestre los euros en pantalla
    public TextMeshProUGUI textoEuros;

    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        RefrescarUI();
    }

    // Añade euros aplicando el multiplicador global
    public void AnadirEuros(int cantidad)
    {
        int cantidadFinal = Mathf.RoundToInt(cantidad * multiplicadorGanancias);
        euros += cantidadFinal;
        RefrescarUI();
        Debug.Log($"[EuroManager] +{cantidadFinal}€ | Total: {euros}€");
    }


    // Intenta gastar euros. Devuelve false si no hay suficientes.

    public bool GastarEuros(int cantidad)
    {
        if (euros < cantidad)
        {
            Debug.Log($"[EuroManager] No hay suficientes euros. " +
                      $"Tienes {euros}€, necesitas {cantidad}€.");
            return false;
        }
        euros -= cantidad;
        RefrescarUI();
        return true;
    }

    // Fuerza un valor concreto (usado al cargar partida)
    public void SetEuros(int cantidad)
    {
        euros = cantidad;
        RefrescarUI();
    }

    private void RefrescarUI()
    {
        if (textoEuros != null)
            textoEuros.text = $"{euros}€";
    }
}
