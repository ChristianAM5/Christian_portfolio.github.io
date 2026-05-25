using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Clase encargada de actualizar la interfaz visual del inventario.
/// Sigue un modelo de "Observador": no pregunta cada frame si hay llaves,
/// sino que espera a que el PlayerInventory le avise.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Configuración de UI")]
    [Tooltip("El prefab que contiene un componente Image para representar la llave.")]
    [SerializeField] private GameObject keyIconPrefab;

    [Tooltip("El objeto (generalmente un Horizontal/Vertical Layout Group) donde caerán los iconos.")]
    [SerializeField] private Transform container;

    private void Start()
    {
        // PASO 1: Localizar la fuente de los datos (el inventario del jugador).
        // Usamos FindFirstObjectByType (sustituto moderno de FindObjectOfType en Unity 6).
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();

        if (inventory != null)
        {
            // PASO 2: Suscripción al evento.
            // "Escuchamos" el evento OnKeyCollected. Cuando el inventario diga "¡Tengo una llave!",
            // nosotros ejecutaremos automáticamente el método AddKeyIcon.
            inventory.OnKeyCollected += AddKeyIcon;
        }
        else
        {
            Debug.LogWarning("InventoryUI: No se encontró un PlayerInventory en la escena.");
        }
    }

    /// <summary>
    /// Método que se ejecuta cuando el evento OnKeyCollected se dispara.
    /// </summary>
    /// <param name="type">El tipo de llave (Enum) que recibimos desde el evento.</param>
    private void AddKeyIcon(KeyType type)
    {
        // 1. Instanciamos el icono visual dentro del contenedor de la UI.
        // Al pasarle 'container' como segundo argumento, se vuelve su hijo automáticamente.
        GameObject newIcon = Instantiate(keyIconPrefab, container);

        // 2. Personalizamos el icono.
        // Buscamos el componente Image para cambiarle el color según el tipo de llave.
        Image iconImage = newIcon.GetComponent<Image>();

        if (iconImage != null)
        {
            // Llamamos a nuestra función auxiliar para obtener el color correcto.
            iconImage.color = GetColorFromType(type);
        }
    }

    /// <summary>
    /// Lógica de negocio visual: Traduce un tipo de llave (lógica) a un color (visual).
    /// </summary>
    private Color GetColorFromType(KeyType t)
    {
        // Usamos una expresión switch (C# moderno) para asignar colores.
        return t switch
        {
            KeyType.Red => Color.red,
            KeyType.Blue => Color.blue,
            KeyType.Green => Color.green,
            KeyType.Gold => new Color(1f, 0.84f, 0f), // Color dorado personalizado (R, G, B)
            _ => Color.white               // Color por defecto
        };
    }

    private void OnDestroy()
    {
        // BUENA PRÁCTICA: Siempre desuscribirse de los eventos al destruir el objeto
        // para evitar errores de memoria (Memory Leaks).
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.OnKeyCollected -= AddKeyIcon;
        }
    }
}
