using UnityEngine;

// Llevar registro de en qué zona se encuentra el jugador en todo momento.
// Se actualiza desde MapTransition cada vez que el jugador cruza un waypoint.
// También define qué mineral corresponde a cada zona.

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance { get; private set; }

    [Header("Nombres de las zonas")]
    // Los nombres deben coincidir con el mapbounds
    public string zonaCentral  = "ZonaCuarzo";
    public string zonaIzquierda = "ZonaCarbon";
    public string zonaDerecha  = "ZonaBauxita";
    public string zonaArriba   = "ZonaHalita";
    public string zonaAbajo    = "ZonaCobre";

    [Header("Zona actual")]
    // Zona en la que se encuentra el jugador ahora mismo
    public string zonaActual;

    private void Awake()
    {
        // Asignar zona central en caso de nueva partida
        if (string.IsNullOrEmpty(zonaActual))
        {
            zonaActual = zonaCentral;
            Debug.Log("[ZoneManager] Zona inicial asignada: " + zonaActual);
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Llamado desde MapTransition cuando el jugador entra en una nueva zona.

    public void SetZonaActual(string nombreZona)
    {
        zonaActual = nombreZona;
        Debug.Log($"[ZoneManager] Zona actual: {zonaActual}");
    }

    // Devuelve el tipo de mineral que corresponde a la zona actual.
    // Devuelve null si la zona no tiene mineral asignado.

    public MineralType? GetMineralDeZonaActual()
    {
        if (zonaActual == zonaCentral)  return MineralType.Cuarzo;
        if (zonaActual == zonaIzquierda)   return MineralType.Carbon;
        if (zonaActual == zonaDerecha)  return MineralType.Bauxita;
        if (zonaActual == zonaArriba)   return MineralType.Halita;
        if (zonaActual == zonaAbajo)    return MineralType.Cobre;

        return null; // Zona sin mineral
    }


    // Comprueba si la zona actual está desbloqueada.
    
    public bool ZonaActualDesbloqueada()
    {
        return ZoneUnlockManager.Instance != null &&
               ZoneUnlockManager.Instance.EstaDesbloqueada(zonaActual);
    }
}

