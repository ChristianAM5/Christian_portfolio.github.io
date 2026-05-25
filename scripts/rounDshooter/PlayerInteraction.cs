using UnityEngine;
using TMPro; // Necesario para manipular elementos de texto de TextMeshPro
using UnityEngine.UI; // Necesario para manipular componentes de imagen (UI)

/// <summary>
/// Este script se encarga de detectar objetos interactuables frente al jugador 
/// mediante un rayo (Raycast) y gestionar la interfaz visual de interacción.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración")]
    // Distancia máxima a la que el jugador puede alcanzar un objeto
    [SerializeField] private float interactionDistance = 3f;
    // Filtro para que el rayo solo choque con objetos en una capa específica (ej. "Interactable")
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI de Interacción")]
    // La imagen del punto de mira (crosshair) en el centro de la pantalla
    [SerializeField] private Image crosshair;
    // El texto que indica qué hace el objeto (ej. "Presiona E para abrir")
    [SerializeField] private TextMeshProUGUI promptText;
    // Colores para dar feedback visual al jugador
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color interactColor = Color.green;

    // Referencia a la interfaz del objeto que estamos mirando actualmente
    private IInteractable _currentInteractable;
    // Referencia a la cámara principal del jugador
    private Camera _cam;

    void Start()
    {
        // Buscamos la cámara en los hijos del jugador (estándar en FPS)
        _cam = GetComponentInChildren<Camera>();
        // Inicializamos la interfaz en modo "reposo"
        UpdateUI(false);
    }

    void Update()
    {
        // En cada frame, lanzamos un rayo para ver qué tiene delante el jugador
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        // Creamos un rayo que sale del centro exacto de la cámara (punto 0.5, 0.5 de la pantalla)
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Lanzamos el rayo físicamente en la escena
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            // Si el rayo toca algo, buscamos si ese objeto (o sus padres) tiene el componente IInteractable
            // Usamos GetComponentInParent por si el Collider está en un hijo del modelo 3D
            _currentInteractable = hit.collider.GetComponentInParent<IInteractable>();

            if (_currentInteractable != null)
            {
                // Si encontramos una interfaz válida, activamos la UI de interacción
                UpdateUI(true);
                return; // Salimos del método para evitar que se resetee abajo
            }
        }

        // Si el rayo no toca nada o el objeto no es interactuable, limpiamos la referencia
        _currentInteractable = null;
        UpdateUI(false);
    }

    /// <summary>
    /// Cambia el estado visual de la UI dependiendo de si estamos mirando un objeto o no.
    /// </summary>
    private void UpdateUI(bool isLookingAtInteractable)
    {
        if (isLookingAtInteractable && _currentInteractable != null)
        {
            // Cambiamos el color de la mira y mostramos el texto personalizado del objeto
            crosshair.color = interactColor;
            promptText.text = _currentInteractable.InteractionPrompt;
        }
        else
        {
            // Volvemos al estado normal y vaciamos el texto
            crosshair.color = normalColor;
            promptText.text = "";
        }
    }

    /// <summary>
    /// Este método es llamado automáticamente por el sistema de Unity "Input System" 
    /// (si usan SendMessages) al presionar el botón de Interactuar.
    /// </summary>
    private void OnInteract()
    {
        // Si hay algo frente a nosotros, ejecutamos su método Interact()
        if (_currentInteractable != null)
        {
            _currentInteractable.Interact();
        }
    }
}
