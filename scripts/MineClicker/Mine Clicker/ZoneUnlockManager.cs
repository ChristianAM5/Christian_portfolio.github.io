using UnityEngine;
using System.Collections.Generic;

// Gestiona qué zonas están desbloqueadas.
// Comprueba si el jugador tiene los materiales necesarios y destruye las vallas al desbloquear.

public class ZoneUnlockManager : MonoBehaviour
{
    public static ZoneUnlockManager Instance { get; private set; }

    [Header("Referencias")]
    private InventoryController inventoryController;

    // Vallas que bloquean el paso
    [Header("Vallas de bloqueo (arrastra los GameObjects)")]
    public GameObject vallaCarbon;
    public GameObject vallabauxita;
    public GameObject vallaHalita;
    public GameObject vallaCobre;

    // Costes de desbloqueo 
    // Cada zona tiene una lista de (MineralType, cantidad requerida).

    // Zona Carbón: 50 cuarzo
    private static readonly (MineralType tipo, int cantidad)[] costeCarbon =
    {
        (MineralType.Cuarzo, 50)
    };

    // Zona Bauxita: 100 cuarzo + 50 carbón
    private static readonly (MineralType tipo, int cantidad)[] costeBauxita =
    {
        (MineralType.Cuarzo,  100),
        (MineralType.Carbon,   50)
    };

    // Zona Halita: 150 cuarzo + 100 carbón + 50 bauxita
    private static readonly (MineralType tipo, int cantidad)[] costeHalita =
    {
        (MineralType.Cuarzo,  150),
        (MineralType.Carbon,  100),
        (MineralType.Bauxita,  50)
    };

    // Zona Cobre: 200 cuarzo + 150 carbón + 100 bauxita + 50 halita
    private static readonly (MineralType tipo, int cantidad)[] costeCobre =
    {
        (MineralType.Cuarzo,  200),
        (MineralType.Carbon,  150),
        (MineralType.Bauxita, 100),
        (MineralType.Halita,   50)
    };

    // Estado de desbloqueo
    // La zona central siempre está desbloqueada desde el inicio.

    private Dictionary<string, bool> zonasDesbloqueadas;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
        InicializarZonas();
    }

    private void InicializarZonas()
    {
        // Por defecto solo la central está desbloqueada
        zonasDesbloqueadas = new Dictionary<string, bool>
        {
            { ZoneManager.Instance.zonaCentral,  true  },
            { ZoneManager.Instance.zonaIzquierda,   false },
            { ZoneManager.Instance.zonaDerecha,  false },
            { ZoneManager.Instance.zonaArriba,   false },
            { ZoneManager.Instance.zonaAbajo,    false }
        };
    }

    // Devuelve si una zona está desbloqueada
    public bool EstaDesbloqueada(string nombreZona)
    {
        if (zonasDesbloqueadas.TryGetValue(nombreZona, out bool desbloqueada))
            return desbloqueada;
        return false;
    }

    // Métodos de desbloqueo

    // Devuelve un string con los materiales que faltan para desbloquear
    // la zona. Si está vacío, el jugador puede pagar.

    public string GetMaterialesFaltantes(string nombreZona)
    {
        var costes = GetCostesDeZona(nombreZona);
        if (costes == null) return "";

        List<string> faltantes = new List<string>();

        foreach (var (tipo, cantidad) in costes)
        {
            int enInventario = inventoryController.GetCantidadMineral(tipo);
            if (enInventario < cantidad)
                // Añadimos cada faltante como elemento de la lista
                faltantes.Add($"{tipo}: {enInventario}/{cantidad}");
        }

        // Unimos todo con " | " para que quede horizontal
        return faltantes.Count > 0
            ? "Te faltan: " + string.Join(" | ", faltantes)
            : "";
    }


    // Intenta desbloquear una zona.
  
    public bool IntentarDesbloquear(string nombreZona, out string mensajeError)
    {
        mensajeError = "";

        if (EstaDesbloqueada(nombreZona))
        {
            mensajeError = "Esta zona ya está desbloqueada.";
            return false;
        }

        var costes = GetCostesDeZona(nombreZona);
        if (costes == null)
        {
            mensajeError = "Zona desconocida.";
            return false;
        }

        // Comprobamos si tiene todos los materiales
        string faltantes = GetMaterialesFaltantes(nombreZona);
        if (!string.IsNullOrEmpty(faltantes))
        {
            mensajeError = $"Te faltan materiales:\n{faltantes}";
            return false;
        }

        // Gastamos los materiales
        foreach (var (tipo, cantidad) in costes)
            inventoryController.GastarMineral(tipo, cantidad);

        // Marcamos como desbloqueada y destruimos la valla
        zonasDesbloqueadas[nombreZona] = true;
        DestruirValla(nombreZona);

        Debug.Log($"[ZoneUnlock] Zona {nombreZona} desbloqueada.");
        return true;
    }

    // Destruye el GameObject de la valla correspondiente

    private void DestruirValla(string nombreZona)
    {
        ZoneManager zm = ZoneManager.Instance;
        if      (nombreZona == zm.zonaIzquierda)  DestruirSi(ref vallaCarbon);
        else if (nombreZona == zm.zonaDerecha) DestruirSi(ref vallabauxita);
        else if (nombreZona == zm.zonaArriba)  DestruirSi(ref vallaHalita);
        else if (nombreZona == zm.zonaAbajo)   DestruirSi(ref vallaCobre);
    }

    private void DestruirSi(ref GameObject valla)
    {
        if (valla != null)
        {
            Destroy(valla);
            valla = null;
        }
    }

    // Helper de costes

    private (MineralType, int)[] GetCostesDeZona(string nombreZona)
    {
        ZoneManager zm = ZoneManager.Instance;
        if (nombreZona == zm.zonaIzquierda)   return costeCarbon;
        if (nombreZona == zm.zonaDerecha)  return costeBauxita;
        if (nombreZona == zm.zonaArriba)   return costeHalita;
        if (nombreZona == zm.zonaAbajo)    return costeCobre;
        return null;
    }

    // Guardado / Carga

    public List<ZoneSaveData> GetSaveData()
    {
        List<ZoneSaveData> lista = new List<ZoneSaveData>();
        foreach (var kvp in zonasDesbloqueadas)
            lista.Add(new ZoneSaveData { nombreZona = kvp.Key, desbloqueada = kvp.Value });
        return lista;
    }

    public void LoadSaveData(List<ZoneSaveData> datos)
    {
        if (datos == null) return;
        foreach (ZoneSaveData d in datos)
        {
            zonasDesbloqueadas[d.nombreZona] = d.desbloqueada;
            // Si estaba desbloqueada, destruimos la valla directamente
            if (d.desbloqueada)
                DestruirValla(d.nombreZona);
        }
    }
}

// Clase serializable para guardado
[System.Serializable]
public class ZoneSaveData
{
    public string nombreZona;
    public bool   desbloqueada;
}

