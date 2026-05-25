using UnityEngine;
using UnityEngine.InputSystem; // Necesario para usar el nuevo sistema de entrada de Unity

public class InteractionDetector : MonoBehaviour
{
    // Almacena la referencia al objeto interactuable más cercano
    private IInteractable interactableInRange = null;

    // Icono visual (ej: podría ser una letra "E") que aparece sobre el jugador cuando puede interactuar
    public GameObject interactionIcon;

    void Start()
    {
        // Al empezar, nos aseguramos de que el icono de interacción esté oculto
        interactionIcon.SetActive(false);
    }

    /// <summary>
    /// Este método es llamado por el componente Player Input cuando el jugador pulsa la tecla de interacción.
    /// </summary>
    public void OnInteract(InputAction.CallbackContext context)
    {
        // Solo actuamos cuando la tecla se ha pulsado por completo (performed)
        if (context.performed)
        {
            // El símbolo '?' (null-conditional) hace que solo llame a Interact() si hay algo en rango
            interactableInRange?.Interact();

            // Si tras interactuar el objeto ya no permite más interacciones (ej: cofre ya abierto)...
            if (interactableInRange != null && !interactableInRange.CanInteract())
            {
                // Escondemos el icono inmediatamente
                interactionIcon.SetActive(false);
            }
        }
    }

    // Se activa cuando el jugador entra en el área (Trigger) de un objeto
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Intentamos obtener el componente IInteractable del objeto con el que chocamos
        // y comprobamos si realmente se puede interactuar con él en ese momento
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            // Guardamos el objeto como nuestro objetivo actual y mostramos el icono visual
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }

    // Se activa cuando el jugador se aleja del objeto
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Si el objeto del que nos alejamos es el mismo que teníamos guardado...
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            // Limpiamos la referencia y ocultamos el icono
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}

