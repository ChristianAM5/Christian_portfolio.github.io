using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // ─── Datos existentes ─────────────────────────────────────────
    public Vector3 playerPosition;
    public string  mapBoundary;
    public List<InventorySaveData> inventorySaveData;
    public List<InventorySaveData> hotbarSaveData;
    public List<ChestSaveData>     chestSaveData;

    // ─── Datos nuevos ─────────────────────────────────────────────
    public int eurosGuardados;
    public List<ZoneSaveData>    zonasDesbloqueadas;
    public List<UpgradeSaveData> mejorasGuardadas;
    public List<AutoGenSaveData> autoGenGuardados;
    public List<string> cartelesResueltos;
    public List<string> recompensasAcertijos;

    public float multiplicadorCantidad  = 1f;
    public float multiplicadorPrecio    = 1f;
    public float multiplicadorCapacidad = 1f;
}

// ─── Clases de guardado ───────────────────────────────────────────

[System.Serializable]
public class ChestSaveData
{
    public string chestID;
    public bool   isOpened;
}

[System.Serializable]
public class AutoGenSaveData
{
    public MineralType mineralType;
    public int         numGeneradores;
    public int         cantidadEnCofre;
}