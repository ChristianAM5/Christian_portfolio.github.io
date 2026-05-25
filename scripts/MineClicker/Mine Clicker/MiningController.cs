using UnityEngine;

// Gestiona el click izquierdo del jugador para picar minerales.
// Lee la zona actual del ZoneManager y añade el mineral correspondiente
// al inventario mediante el InventoryController.

public class MiningController : MonoBehaviour
{
    // Referencias a otros scripts
    private InventoryController inventoryController;
    private ZoneManager zoneManager;

    // Prefabs de minerales
    [Header("Prefabs de minerales (en el mismo orden que MineralType)")]
    public GameObject prefabCuarzo;
    public GameObject prefabCarbon;
    public GameObject prefabBauxita;
    public GameObject prefabHalita;
    public GameObject prefabCobre;

    // Variables de cantidad por click
    // Estas se modifican con las mejoras del NPC
    [HideInInspector] public int cantidadCuarzo   = 1;
    [HideInInspector] public int cantidadCarbon    = 1;
    [HideInInspector] public int cantidadBauxita   = 1;
    [HideInInspector] public int cantidadHalita    = 1;
    [HideInInspector] public int cantidadCobre     = 1;

    [Header("Efectos")]
    // Tiempo mínimo entre clicks para evitar spam
    public float cooldownEntreClicks = 0.2f;
    private float tiempoUltimoClick = 0f;

    void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
        zoneManager         = FindFirstObjectByType<ZoneManager>();
    }

    void Update()
    {

    	// No minamos si hay cualquier menú abierto
    	if (PauseController.IsGamePaused) return;

        // Solo procesamos el click si ha pasado el cooldown
        if (Input.GetMouseButtonDown(0) &&
            Time.time - tiempoUltimoClick >= cooldownEntreClicks)
        {
            tiempoUltimoClick = Time.time;
            IntentarPicar();
        }

    }


    // Comprueba la zona actual y añade el mineral correspondiente al inventario.

    private void IntentarPicar()
    {
        if (zoneManager == null) return;

        // Comprobamos si la zona está desbloqueada
        if (!zoneManager.ZonaActualDesbloqueada()) return;

        MineralType? tipoMineral = zoneManager.GetMineralDeZonaActual();

        if (tipoMineral == null)
        {
            Debug.Log("[MiningController] Esta zona no tiene mineral.");
            return;
        }

        // Obtenemos el prefab y la cantidad según el tipo
        GameObject prefab   = GetPrefabDeMineral(tipoMineral.Value);
        int cantidad        = GetCantidadDeMineral(tipoMineral.Value);

        if (prefab == null)
        {
            Debug.LogWarning($"[MiningController] No hay prefab asignado para {tipoMineral}");
            return;
        }

        // Intentamos añadir al inventario
        bool exito = inventoryController.AddMineral(prefab, cantidad);

	    if (exito)
    	    SoundEffectManager.Play("Mine", true);

        if (!exito)
        {
            Debug.Log("[MiningController] Inventario lleno para este mineral.");
        }
    }

    private GameObject GetPrefabDeMineral(MineralType tipo)
    {
        switch (tipo)
        {
            case MineralType.Cuarzo:  return prefabCuarzo;
            case MineralType.Carbon:  return prefabCarbon;
            case MineralType.Bauxita: return prefabBauxita;
            case MineralType.Halita:  return prefabHalita;
            case MineralType.Cobre:   return prefabCobre;
            default: return null;
        }
    }
    
    private int GetCantidadDeMineral(MineralType tipo)
    {
        if (MineralUpgradeManager.Instance != null)
            return MineralUpgradeManager.Instance.GetCantidadPorClick(tipo);
        return 1; // fallback por si el manager no está listo
    }

}

