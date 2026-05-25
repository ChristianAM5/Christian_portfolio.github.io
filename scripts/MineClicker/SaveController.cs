using System.Collections.Generic;
using System.IO;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class SaveController : MonoBehaviour
{
    private string saveLocation;

    // ─── Referencias ──────────────────────────────────────────────
    private InventoryController   inventoryController;
    // private HotbarController      hotbarController;
    private Chest[]               chests;
    private ZoneUnlockManager     zoneUnlockManager;
    private MineralUpgradeManager upgradeManager;
    private AutoGenManager        autoGenManager;

    void Start()
    {
        InitializeComponents();
        StartCoroutine(CargarSiguienteFrame());
    }

    private IEnumerator CargarSiguienteFrame()
    {
        yield return null; // espera un frame para que todos los Start() hayan corrido
        LoadGame();
    }

    private void InitializeComponents()
    {
        saveLocation      = Path.Combine(
            Application.persistentDataPath, "saveData.json");

        inventoryController = FindFirstObjectByType<InventoryController>();
        // hotbarController    = FindFirstObjectByType<HotbarController>();
        zoneUnlockManager   = FindFirstObjectByType<ZoneUnlockManager>();
        upgradeManager      = FindFirstObjectByType<MineralUpgradeManager>();
        autoGenManager      = FindFirstObjectByType<AutoGenManager>();
        chests              = FindObjectsByType<Chest>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    // ─── Guardar ──────────────────────────────────────────────────

    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            // Datos existentes
            playerPosition  = GameObject.FindWithTag("Player").transform.position,
            mapBoundary     = FindFirstObjectByType<CinemachineConfiner2D>()
                                .BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            // hotbarSaveData    = hotbarController.GetHotbarItems(),
            chestSaveData     = GetChestState(),

            // Datos nuevos
            eurosGuardados      = EuroManager.Instance?.Euros ?? 0,
            zonasDesbloqueadas  = zoneUnlockManager?.GetSaveData(),
            mejorasGuardadas    = upgradeManager?.GetSaveData(),
            multiplicadorCantidad  = upgradeManager?.multiplicadorGlobalCantidad  ?? 1f,
            multiplicadorPrecio    = upgradeManager?.multiplicadorGlobalPrecio    ?? 1f,
            multiplicadorCapacidad = upgradeManager?.multiplicadorGlobalCapacidad ?? 1f,
            autoGenGuardados    = GetAutoGenState(),
            cartelesResueltos = GetCartelesResueltos(),
            recompensasAcertijos = RiddleRewardManager.Instance?.GetRecompensasGuardadas()
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log($"[Save] Partida guardada en {saveLocation}");
    }

    // ─── Cargar ───────────────────────────────────────────────────

    public void LoadGame()
    {
        if (!File.Exists(saveLocation))
        {
            // Primera vez que se juega
            NuevaPartida();
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(
            File.ReadAllText(saveLocation));

        // ── Datos existentes ──────────────────────────────────────
        GameObject.FindWithTag("Player").transform.position =
            saveData.playerPosition;

        PolygonCollider2D savedMapBound = GameObject
            .Find(saveData.mapBoundary)
            ?.GetComponent<PolygonCollider2D>();

        if (savedMapBound != null)
        {
            FindFirstObjectByType<CinemachineConfiner2D>()
                .BoundingShape2D = savedMapBound;
            MapController_Manual.Instance?.HighlightArea(saveData.mapBoundary);
            MapController_Dynamic.Instance?.GenerateMap(savedMapBound);
            ZoneManager.Instance?.SetZonaActual(saveData.mapBoundary);
        }

        inventoryController.SetInventoryItems(saveData.inventorySaveData);
        // hotbarController.SetHotbarItems(saveData.hotbarSaveData);
        LoadChestStates(saveData.chestSaveData);
	    LoadCartelesResueltos(saveData.cartelesResueltos);

        // ── Datos nuevos ──────────────────────────────────────────

        // Euros
        if (EuroManager.Instance != null)
            EuroManager.Instance.SetEuros(saveData.eurosGuardados);

        // Zonas desbloqueadas
        if (zoneUnlockManager != null && saveData.zonasDesbloqueadas != null)
            zoneUnlockManager.LoadSaveData(saveData.zonasDesbloqueadas);

        // Mejoras
        if (upgradeManager != null && saveData.mejorasGuardadas != null)
            upgradeManager.LoadSaveData(saveData.mejorasGuardadas);

        // Mejoras globales
        if (upgradeManager != null)
        {
            upgradeManager.multiplicadorGlobalCantidad  = saveData.multiplicadorCantidad;
            upgradeManager.multiplicadorGlobalPrecio    = saveData.multiplicadorPrecio;
            upgradeManager.multiplicadorGlobalCapacidad = saveData.multiplicadorCapacidad;
        }

        // Auto-generadores: los restauramos al final para que
        // upgradeManager ya tenga los niveles cargados
        if (autoGenManager != null && saveData.autoGenGuardados != null)
            LoadAutoGenState(saveData.autoGenGuardados);

	// Recompensas acertijos guardadas
	if (saveData.recompensasAcertijos != null)
   	    RiddleRewardManager.Instance.LoadRecompensasGuardadas(saveData.recompensasAcertijos);


        Debug.Log("[Save] Partida cargada correctamente.");
    }

    // ─── Nueva partida ────────────────────────────────────────────

    private void NuevaPartida()
    {
        SaveGame();
        inventoryController.SetInventoryItems(new List<InventorySaveData>());
        // hotbarController.SetHotbarItems(new List<InventorySaveData>());
        MapController_Dynamic.Instance?.GenerateMap();
        Debug.Log("[Save] Nueva partida iniciada.");
    }

    // ─── Cofres normales ──────────────────────────────────────────

    private List<ChestSaveData> GetChestState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();
        foreach (Chest chest in chests)
        {
            chestStates.Add(new ChestSaveData
            {
                chestID  = chest.ChestID,
                isOpened = chest.IsOpened
            });
        }
        return chestStates;
    }

    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        if (chestStates == null) return;
        foreach (Chest chest in chests)
        {
            ChestSaveData data = chestStates.Find(
                c => c.chestID == chest.ChestID);
            if (data != null)
                chest.SetOpened(data.isOpened);
        }
    }

    // ─── Auto-generadores ─────────────────────────────────────────

    private List<AutoGenSaveData> GetAutoGenState()
    {
        List<AutoGenSaveData> lista = new List<AutoGenSaveData>();
        foreach (MineralType tipo in System.Enum.GetValues(typeof(MineralType)))
        {
            AutoGenChest cofre = autoGenManager.GetCofre(tipo); // ver nota abajo
            lista.Add(new AutoGenSaveData
            {
                mineralType      = tipo,
                numGeneradores   = upgradeManager?.GetNumAutoGeneradores(tipo) ?? 0,
                cantidadEnCofre  = cofre?.CantidadAcumulada ?? 0 
            });
        }
        return lista;
    }

    private void LoadAutoGenState(List<AutoGenSaveData> datos)
    {
        foreach (AutoGenSaveData d in datos)
        {
            // ActualizarAutoGeneradores usa el nivel guardado en upgradeManager
            // que ya fue cargado antes, así que spawneará el número correcto
            autoGenManager.ActualizarAutoGeneradores(d.mineralType);

            // Restaurar cantidad en el cofre
            AutoGenChest cofre = autoGenManager.GetCofre(d.mineralType);
            cofre?.CargarCantidad(d.cantidadEnCofre);
        }
    }
    // ─── Carteles Resueltos ─────────────────────────────────────────

    private List<string> GetCartelesResueltos()
    {
        return RiddleRewardManager.Instance?.GetRecompensasGuardadas() 
            ?? new List<string>();
    }

    private void LoadCartelesResueltos(List<string> resueltos)
    {
        if (resueltos == null) return;
        RiddleSign[] carteles = FindObjectsByType<RiddleSign>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (RiddleSign cartel in carteles)
            if (resueltos.Contains(cartel.gameObject.name))
                Destroy(cartel.gameObject);
    }
    }