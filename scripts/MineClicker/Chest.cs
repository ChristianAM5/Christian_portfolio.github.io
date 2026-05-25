using UnityEngine;

// La clase Chest hereda de MonoBehaviour y "firma el contrato" de IInteractable
public class Chest : MonoBehaviour, IInteractable
{
    [Header("Estado y Datos")]
    // Propiedad que indica si el cofre ya fue abierto. El 'private set' impide que otros scripts lo cambien por error.
    public bool IsOpened { get; private set; }
    // Identificador único para guardar el estado del cofre en el futuro
    public string ChestID { get; private set; }

    [Header("Visual y Recompensas")]
    public GameObject itemPrefab; // El objeto (loot) que el cofre soltará al abrirse
    public Sprite openedSprite;   // La imagen del cofre abierto

    // Se ejecuta al iniciar el juego
    void Start()
    {
        // Si el ChestID es nulo, genera uno único basado en su posición/nombre (usa un Helper global)
        ChestID ??= GlobalHelper.GenerateUniqueID(gameObject);
    }

    // --- IMPLEMENTACIÓN DE IINTERACTABLE ---

    /// <summary>
    /// Determina si el jugador puede interactuar con el cofre.
    /// </summary>
    /// <returns>True si el cofre está cerrado, False si ya está abierto.</returns>
    public bool CanInteract()
    {
        return !IsOpened;
    }

    /// <summary>
    /// Método principal que llama el jugador al pulsar el botón de acción.
    /// </summary>
    public void Interact()
    {
        // Seguridad: si ya está abierto, no hace nada
        if (!CanInteract()) return;

        // Lógica interna para abrirlo
        OpenChest();
    }

    // --- LÓGICA INTERNA DEL COFRE ---

    private void OpenChest()
    {
        SetOpened(true); // Cambia el estado y la imagen
        SoundEffectManager.Play("Chest"); // Reproduce el sonido de apertura

        // Si hay un objeto asignado dentro del cofre...
        if (itemPrefab)
        {
            // Crea el objeto en el mundo un poco más abajo del cofre
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);

            // Llama a un componente del objeto soltado para que haga un efecto visual de "rebote"
            droppedItem.GetComponent<BounceEffect>().StartBounce();
        }
    }

    /// <summary>
    /// Cambia el estado visual y lógico del cofre.
    /// </summary>
    /// <param name="opened">¿Debe estar abierto?</param>
    public void SetOpened(bool opened)
    {
        IsOpened = opened; // Actualiza la variable lógica

        if (IsOpened)
        {
            // Cambia el sprite del SpriteRenderer por el de 'cofre abierto'
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }
}
