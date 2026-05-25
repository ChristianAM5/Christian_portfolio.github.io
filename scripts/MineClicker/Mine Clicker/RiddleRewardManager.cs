using UnityEngine;
using System.Collections.Generic;

public class RiddleRewardManager : MonoBehaviour
{
    public static RiddleRewardManager Instance { get; private set; }

    // Recompensas ya aplicadas (para evitar duplicados)
    private HashSet<string> recompensasAplicadas = new HashSet<string>();

    // Acceso rápido a los scripts necesarios
    private MineralUpgradeManager upgradeManager;
    private DuelEnemySpawner duelSpawner;
    private CentralNPCQuickAccess quickAccess;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        upgradeManager = MineralUpgradeManager.Instance;
        duelSpawner    = FindFirstObjectByType<DuelEnemySpawner>(FindObjectsInactive.Include);
        quickAccess    = FindFirstObjectByType<CentralNPCQuickAccess>(FindObjectsInactive.Include);
    }

    // Llamar a las funciones que aplican la recompensa dependiendo del acertijo/cartel completado
    public void AplicarRecompensa(string cartelID)
    {
        if (recompensasAplicadas.Contains(cartelID))
            return;

        recompensasAplicadas.Add(cartelID);

        switch (cartelID)
        {
            case "CartelCuarzo":
                ActivarNPCEnemigo();
                break;

            case "CartelCarbon":
                DuplicarCantidadClick();
                break;

            case "CartelBauxita":
                ActivarAccesoNPCcentral();
                break;

            case "CartelHalita":
                DuplicarPrecioMinerales();
                break;

            case "CartelCobre":
                DuplicarCapacidadMinerales();
                break;

            default:
                Debug.LogWarning($"[RiddleReward] Cartel desconocido: {cartelID}");
                break;
        }
    }

    // Funciones que llamana a la recompensa en el script correspondiente
    private void ActivarNPCEnemigo()
    {
        if (duelSpawner != null)
            duelSpawner.ActivarSpawner();
    }

    private void DuplicarCantidadClick()
    {
        if (upgradeManager != null)
            upgradeManager.multiplicadorGlobalCantidad *= 2f;
    }

    private void ActivarAccesoNPCcentral()
    {
        if (quickAccess != null)
            quickAccess.ActivarAcceso();
    }

    private void DuplicarPrecioMinerales()
    {
        if (upgradeManager != null)
            upgradeManager.multiplicadorGlobalPrecio *= 2f;
    }

    private void DuplicarCapacidadMinerales()
    {
        if (upgradeManager != null)
            upgradeManager.multiplicadorGlobalCapacidad *= 2f;
    }

    // Guardado
    public List<string> GetRecompensasGuardadas()
    {
        return new List<string>(recompensasAplicadas);
    }

    // Cargado
    public void LoadRecompensasGuardadas(List<string> lista)
    {
        if (lista == null) return;

        upgradeManager = MineralUpgradeManager.Instance;
        duelSpawner    = FindFirstObjectByType<DuelEnemySpawner>(FindObjectsInactive.Include);
        quickAccess    = FindFirstObjectByType<CentralNPCQuickAccess>(FindObjectsInactive.Include);

        foreach (string id in lista)
        {
            recompensasAplicadas.Add(id);
            AplicarRecompensaSinMultiplicadores(id);
        }
    }

    // Solo activa comportamientos, no toca multiplicadores (ya están restaurados)
    private void AplicarRecompensaSinMultiplicadores(string cartelID)
    {
        switch (cartelID)
        {
            case "CartelCuarzo":  ActivarNPCEnemigo();       break;
            case "CartelBauxita": ActivarAccesoNPCcentral(); break;
            // CartelCarbon, CartelHalita, CartelCobre no hacen nada aquí
            // porque sus efectos (multiplicadores) ya están en el saveData
        }
    }

    // Feedback en el cartel para saber que hace la mejora desbloqueada
    public string GetDescripcionRecompensa(string cartelID)
    {
        switch (cartelID)
        {
            case "CartelCuarzo":
                return "  NPC   duelista   desbloqueado";

            case "CartelCarbon":
                return "  x2   Cantidad   por   click";

            case "CartelBauxita":
                return "  Presiona   N   para   acceder   al   NPC   central";

            case "CartelHalita":
                return "  x2  Precio   venta   de   minerales";

            case "CartelCobre":
                return "  x2   Capacidad   minerales";
        }

        return "Recompensa desconocida";
    }
}
