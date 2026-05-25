using UnityEngine;
using TMPro;

// Cofre que acumula minerales generados por las criaturas.
// Respeta la capacidad máxima del slot de inventario de ese mineral.
// Al interactuar recoge solo lo que quepa en el inventario.

public class AutoGenChest : MonoBehaviour, IInteractable
{
    [Header("Mineral de esta zona")]
    public MineralType tipoMineral;

    [Header("UI del cofre")]
    public TextMeshPro textoContador;   // Texto con la cantidad del cofre

    [Header("Sprites")]
    public Sprite spriteCerrado;
    public Sprite spriteAbierto;

    // Cantidad acumulada dentro del cofre
    private int cantidadAcumulada = 0;

    // Propiedad pública para que SaveController pueda leerla
    public int CantidadAcumulada => cantidadAcumulada;

    // Capacidad máxima del cofre = capacidad del slot mejorado
    private int CapacidadMaxima => MineralUpgradeManager.Instance != null
        ? MineralUpgradeManager.Instance.GetCapacidad(tipoMineral)
        : 50; // fallback si el manager no está listo

    private InventoryController inventoryController;

    private void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
        ActualizarVisual();
    }


    // Llamado por AutoGenCreature cada X segundos.
    // Solo acumula si no se ha superado la capacidad máxima.

    public void AnadirMineral(MineralType tipo, int cantidad)
    {
        int espacioDisponible = CapacidadMaxima - cantidadAcumulada;

        if (espacioDisponible <= 0)
        {
            Debug.Log($"[Cofre] Cofre de {tipo} lleno ({cantidadAcumulada}/{CapacidadMaxima})");
            return;
        }

        // Solo añadimos lo que cabe
        int cantidadReal = Mathf.Min(cantidad, espacioDisponible);
        cantidadAcumulada += cantidadReal;

        // Notificación al llenarse el cofre
        if (cantidadAcumulada >= CapacidadMaxima)
        {
            GameObject prefab = AutoGenManager.Instance?.GetPrefab(tipo);
            Sprite icono = prefab != null
                ? prefab.GetComponent<SpriteRenderer>()?.sprite
                : null;

            ItemPickupUIController.Instance?.ShowItemPickup("Cofre lleno", icono, 0);
        }

        Debug.Log($"[Cofre] +{cantidadReal} de {tipo}. " + $"Total: {cantidadAcumulada}/{CapacidadMaxima}");

        ActualizarVisual();
    }

    // IInteractable

    public bool CanInteract()
    {
        return cantidadAcumulada > 0;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        RecogerHastaLlenarSlot();
    }

    // Lógica de recogida

    private void RecogerHastaLlenarSlot()
    {
        GameObject prefab = AutoGenManager.Instance?.GetPrefab(tipoMineral);
        if (prefab == null) return;

        // Calculamos cuánto cabe en el inventario
        int enInventario   = inventoryController.GetCantidadMineral(tipoMineral);
        int capacidadSlot  = MineralUpgradeManager.Instance.GetCapacidad(tipoMineral);
        int espacioEnSlot  = capacidadSlot - enInventario;

        
        if (espacioEnSlot <= 0)
        {
            // notificacion si inventario del mineral lleno al recoger del cofre
            Sprite icono = prefab != null
                ? prefab.GetComponent<SpriteRenderer>()?.sprite
                : null;

            string nombreMineral = tipoMineral.ToString();
            ItemPickupUIController.Instance?.ShowItemPickup($"{nombreMineral} lleno", icono, 0);

            Debug.Log("[Cofre] Slot de inventario lleno, no se puede recoger.");
            return;
        }

        // Recogemos el mínimo entre lo que hay en el cofre
        // y lo que cabe en el inventario
        int cantidadARecoger = Mathf.Min(cantidadAcumulada, espacioEnSlot);

        bool exito = inventoryController.AddMineral(prefab, cantidadARecoger);

        if (exito)
        {
            cantidadAcumulada -= cantidadARecoger;
            Debug.Log($"[Cofre] Recogidas {cantidadARecoger} de {tipoMineral}. " +
                      $"Quedan en cofre: {cantidadAcumulada}");
        }

        ActualizarVisual();
    }

    // Visual

    private void ActualizarVisual()
    {
        // Texto flotante: muestra cantidad/capacidad
        if (textoContador != null)
        {
            textoContador.text = cantidadAcumulada > 0
                ? $"{cantidadAcumulada}/{CapacidadMaxima}"
                : "";
        }

        // Sprite abierto/cerado según si tiene contenido
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = cantidadAcumulada > 0 ? spriteAbierto : spriteCerrado;
    }

    // Método para restaurar al cargar partida
    public void CargarCantidad(int cantidad)
    {
        cantidadAcumulada = cantidad;
        ActualizarVisual();
    }
}
