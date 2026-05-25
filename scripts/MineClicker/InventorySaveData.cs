using UnityEngine;

[System.Serializable]
public class InventorySaveData
{
    public int itemID;
    public int slotIndex;   // Posición del slot dentro del inventario donde está colocado
    public int quantity;    // Cantidad de ese mineral
}