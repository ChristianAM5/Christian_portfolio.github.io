using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Se ha modificado para que los slots tengan capacidad
// para agrupar minerales iguales

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    void Start()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();
    }

    // ─── Añadir mineral con stacking ──────────────────────────────

    public bool AddMineral(GameObject mineralPrefab, int cantidad)
    {
        MineralItem mineralData = mineralPrefab.GetComponent<MineralItem>();
        if (mineralData == null) return AddItem(mineralPrefab);

        // Buscar slot existente con este mineral
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem == null) continue;

            MineralItem mineralEnSlot = slot.currentItem.GetComponent<MineralItem>();
            if (mineralEnSlot == null) continue;

            if (mineralEnSlot.mineralType == mineralData.mineralType)
            {
                int capacidad = MineralUpgradeManager.Instance != null
                    ? MineralUpgradeManager.Instance.GetCapacidad(mineralEnSlot.mineralType)
                    : 50;

                if (mineralEnSlot.quantity >= capacidad)
                {
                    // Slot lleno, notificamos
                    ItemPickupUIController.Instance?.ShowItemPickup(
                        $"{mineralData.mineralType} lleno", mineralPrefab.GetComponent<Image>()?.sprite, 0);
                    return false;
                }

                // Solo añadimos lo que cabe hasta el límite
                int caben       = capacidad - mineralEnSlot.quantity;
                int cantidadReal = Mathf.Min(cantidad, caben);

                mineralEnSlot.quantity += cantidadReal;
                slot.UpdateQuantityDisplay();
                MostrarPopupMineral(mineralEnSlot, cantidadReal);

                // Avisamos si se ha llenado
                if (mineralEnSlot.quantity >= capacidad)
                {
                    ItemPickupUIController.Instance?.ShowItemPickup(
                        $"{mineralData.mineralType} lleno", mineralPrefab.GetComponent<Image>()?.sprite, 0);
                }

                return true;
            }
        }

        // No existe slot con este mineral, buscamos uno vacío
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null) continue;

            GameObject nuevoItem = Instantiate(mineralPrefab, slotTransform);
            nuevoItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            MineralItem nuevoMineral = nuevoItem.GetComponent<MineralItem>();
            nuevoMineral.quantity = cantidad;

            slot.currentItem = nuevoItem;
            slot.UpdateQuantityDisplay();
            MostrarPopupMineral(nuevoMineral, cantidad);
            return true;
        }

        // Sin slots disponibles
        ItemPickupUIController.Instance?.ShowItemPickup(
            "¡Inventario lleno!", mineralPrefab.GetComponent<Image>()?.sprite, 0);
        return false;
    }

    // ─── Sistema antiguo de items (no minerales) ──────────────────

    public bool AddItem(GameObject itemPrefab)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                return true;
            }
        }
        Debug.Log("[Inventario] No hay slots disponibles.");
        return false;
    }

    // ─── Consultas de mineral ─────────────────────────────────────

    public int GetCantidadMineral(MineralType tipo)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem == null) continue;

            MineralItem mineral = slot.currentItem.GetComponent<MineralItem>();
            if (mineral != null && mineral.mineralType == tipo)
                return mineral.quantity;
        }
        return 0;
    }

    public bool GastarMineral(MineralType tipo, int cantidad)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem == null) continue;

            MineralItem mineral = slot.currentItem.GetComponent<MineralItem>();
            if (mineral == null || mineral.mineralType != tipo) continue;

            if (mineral.quantity < cantidad)
            {
                Debug.Log($"[Inventario] No hay suficiente {tipo}.");
                return false;
            }

            mineral.quantity -= cantidad;

            if (mineral.quantity <= 0)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }

            slot.UpdateQuantityDisplay();
            return true;
        }

        Debug.Log($"[Inventario] No se encontró {tipo} en el inventario.");
        return false;
    }

    // ─── Guardado y carga ─────────────────────────────────────────

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem == null) continue;

            Item item = slot.currentItem.GetComponent<Item>();
            if (item == null) continue;

            MineralItem mineral = slot.currentItem.GetComponent<MineralItem>();

            invData.Add(new InventorySaveData
            {
                itemID    = item.ID,
                slotIndex = slotTransform.GetSiblingIndex(),
                quantity  = mineral != null ? mineral.quantity : 1
            });
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        // Limpiamos slots actuales
        foreach (Transform child in inventoryPanel.transform)
            Destroy(child.gameObject);

        // Recreamos slots vacíos
        for (int i = 0; i < slotCount; i++)
            Instantiate(slotPrefab, inventoryPanel.transform);

        if (inventorySaveData == null) return;

        // Rellenamos con los datos guardados
        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex >= slotCount) continue;

            Slot slot = inventoryPanel.transform
                .GetChild(data.slotIndex).GetComponent<Slot>();

            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
            if (itemPrefab == null) continue;

            GameObject item = Instantiate(itemPrefab, slot.transform);
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            // Restaurar cantidad si es mineral
            MineralItem mineral = item.GetComponent<MineralItem>();
            if (mineral != null && data.quantity > 0)
                mineral.quantity = data.quantity;
            slot.currentItem = item;
            slot.UpdateQuantityDisplay();
        }
    }

    // ─── Helper popup ─────────────────────────────────────────────

    private void MostrarPopupMineral(MineralItem mineral, int cantidadRecogida)
    {
        if (ItemPickupUIController.Instance == null) return;

        Sprite icono = mineral.GetComponent<Image>()?.sprite;
        ItemPickupUIController.Instance.ShowItemPickup(
            mineral.Name, icono, cantidadRecogida);
    }
}