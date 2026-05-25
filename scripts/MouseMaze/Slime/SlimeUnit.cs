using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeUnit : MonoBehaviour
{
    void Start()
    {
        // Solo añadirse a la lista si el SlimeSelectionManager existe
        if (SlimeSelectionManager.Instance != null)
        {
            SlimeSelectionManager.Instance.allUnitsList.Add(gameObject);
        }
    }

    void OnDestroy()
    {
        // Solo quitarse de la lista si el SlimeSelectionManager existe
        if (SlimeSelectionManager.Instance != null)
        {
            SlimeSelectionManager.Instance.allUnitsList.Remove(gameObject);
        }
    }
}
