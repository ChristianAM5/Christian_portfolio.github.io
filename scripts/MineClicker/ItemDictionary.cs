using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ItemDictionary : MonoBehaviour
{
    public List<Item> itemPrefabs;  // La rellenamos manualmente en el inspector
    private Dictionary<int, GameObject> itemDictionary;
    private void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();
        // Generamos el diccionario autoincrementando los IDs
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (itemPrefabs[i] != null)
            {
                itemPrefabs[i].ID = i + 1;
            }
        }
        foreach (Item item in itemPrefabs)
        {
            itemDictionary[item.ID] = item.gameObject;
        }
    }
    public GameObject GetItemPrefab(int itemID){
        itemDictionary.TryGetValue(itemID, out GameObject prefab);
        if (prefab != null)
        {
            Debug.LogWarning($"Item con ID {itemID} no encontrado en el diccionario.");
        }
        return prefab;
    }
}

