using UnityEngine;

// La clase implementa 'IInteractable', lo que sugiere un sistema de interacci�n 
// por Raycast o proximidad, permitiendo que el jugador "active" objetos.
public class KeyPickUp : MonoBehaviour, IInteractable
{
    // [SerializeField] permite ver estas variables en el Inspector de Unity sin hacerlas p�blicas.
    [SerializeField] private KeyType type; // Define qu� tipo de llave es (Roja, Azul, etc.)
    [SerializeField] private MeshRenderer keyRenderer; // Referencia al componente que dibuja la malla 3D


    [Header("Efectos")]
    [SerializeField] private AudioClip pickupSound;

    [Range(0, 1)]
    [SerializeField] private float volume = 1f;

    // Esta propiedad cumple con la interfaz IInteractable.
    // Es el texto que aparecer� en la pantalla del alumno/jugador al mirar la llave.
    public string InteractionPrompt => $"Recoger llave {type}";

    // --- OPTIMIZACI�N VISUAL ---

    // OnValidate se ejecuta autom�ticamente cuando cambias algo en el Inspector.
    // �Ideal para ver cambios de color en tiempo real sin darle a Play!
    private void OnValidate()
    {
        UpdateKeyVisuals();
    }

    private void Start()
    {
        // Aseguramos que al empezar la partida, la llave tenga el color correcto.
        UpdateKeyVisuals();
    }

    private void UpdateKeyVisuals()
    {
        if (keyRenderer == null) return;

        // USO DE MATERIAL PROPERTY BLOCK:
        // En lugar de hacer "renderer.material.color = ...", que crea una copia del material 
        // y gasta memoria, usamos un PropertyBlock. Esto modifica la instancia en la GPU,
        // permitiendo que muchas llaves compartan el mismo material pero se vean de distinto color.
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        // Obtenemos el color correspondiente al Enum de la llave.
        Color targetColor = GetColorFromType(type);

        // Aplicamos el color al bloque de propiedades y luego lo pasamos al renderer.
        keyRenderer.GetPropertyBlock(propBlock);

        // "_BaseColor" es el nombre est�ndar en el Shader de URP (Universal Render Pipeline).
        propBlock.SetColor("_BaseColor", targetColor);

        keyRenderer.SetPropertyBlock(propBlock);
    }

    // Un m�todo auxiliar que traduce un tipo (Enum) a un color real (RGBA).
    private Color GetColorFromType(KeyType t)
    {
        // Usamos una expresi�n switch (C# moderno) para asignar colores r�pidamente.
        return t switch
        {
            KeyType.Red => Color.red,
            KeyType.Blue => Color.blue,
            KeyType.Green => Color.green,
            KeyType.Gold => new Color(1f, 0.84f, 0f), // Oro personalizado (R:1, G:0.84, B:0)
            _ => Color.white // Color por defecto si no coincide ninguno
        };
    }

    // --- L�GICA DE INTERACCI�N ---

    public void Interact()
    {
        // Buscamos el inventario del jugador. 
        // Nota: FindFirstObjectByType es la versi�n moderna y eficiente en Unity 6 para buscar objetos.
        PlayerInventory inv = Object.FindFirstObjectByType<PlayerInventory>();

        if (inv != null)
        {
            // A�adimos la llave al inventario y eliminamos el objeto de la escena.
            inv.AddKey(type);

            // SONIDO
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
            }
            
            Destroy(gameObject);
        }
    }
}