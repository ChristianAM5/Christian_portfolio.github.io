using UnityEngine;

// Cheat: introduce el código para recibir 50 de cada mineral.
// ↑ ↑ ↓ ↓ ← → ← → B A

public class Cheat : MonoBehaviour
{
    // Secuencia del código
    private KeyCode[] Code = new KeyCode[]
    {
        KeyCode.UpArrow,    KeyCode.UpArrow,
        KeyCode.DownArrow,  KeyCode.DownArrow,
        KeyCode.LeftArrow,  KeyCode.RightArrow,
        KeyCode.LeftArrow,  KeyCode.RightArrow,
        KeyCode.B,          KeyCode.A
    };

    // Cantidad de cada mineral que se otorga
    private const int CANTIDAD_CHEAT = 50;

    private int indicePulso = 0;
    private InventoryController inventoryController;

    [Header("Prefabs de minerales")]
    public GameObject prefabCuarzo;
    public GameObject prefabCarbon;
    public GameObject prefabBauxita;
    public GameObject prefabHalita;
    public GameObject prefabCobre;

    private void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
    }

    private void Update()
    {
        // Comprobamos si se ha pulsado la siguiente tecla de la secuencia
        if (Input.GetKeyDown(Code[indicePulso]))
        {
            indicePulso++;

            // Si se ha completado la secuencia
            if (indicePulso >= Code.Length)
            {
                ActivarCheat();
                indicePulso = 0;
            }
        }
        else if (Input.anyKeyDown)
        {
            // Si se pulsa una tecla incorrecta reiniciamos
            indicePulso = 0;

            // Comprobamos si la tecla incorrecta es la primera de la secuencia
            if (Input.GetKeyDown(Code[0]))
                indicePulso = 1;
        }
    }

    private void ActivarCheat()
    {
        Debug.Log("[CHEAT] Código activado! +50 de cada mineral.");

        // Añadimos 50 de cada mineral al inventario
        AnadirMineral(prefabCuarzo,  MineralType.Cuarzo);
        AnadirMineral(prefabCarbon,  MineralType.Carbon);
        AnadirMineral(prefabBauxita, MineralType.Bauxita);
        AnadirMineral(prefabHalita,  MineralType.Halita);
        AnadirMineral(prefabCobre,   MineralType.Cobre);

        // Notificación en pantalla
        ItemPickupUIController.Instance?.ShowItemPickup(
            "¡CHEAT ACTIVADO! +50 de todo", null, 0);
    }

    private void AnadirMineral(GameObject prefab, MineralType tipo)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[CHEAT] Prefab de {tipo} no asignado.");
            return;
        }

        bool exito = inventoryController.AddMineral(prefab, CANTIDAD_CHEAT);

        if (!exito)
            Debug.Log($"[CHEAT] No se pudo añadir {tipo}, inventario lleno.");
    }
}
