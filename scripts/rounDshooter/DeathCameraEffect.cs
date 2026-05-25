using UnityEngine;

/// <summary>
/// Este script se encarga de crear un efecto visual cuando el jugador muere.
/// Hace que la cámara caiga al suelo y se incline, simulando que el personaje se desploma.
/// </summary>
public class DeathCameraEffect : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("La cámara que se moverá al morir (generalmente la Main Camera).")]
    [SerializeField] private Transform cameraTransform;

    private HealthSystem health; // Referencia al sistema de vida para saber cuándo morir.

    [Header("Ajustes de Caída")]
    [SerializeField] private float fallSpeed = 4f;      // Velocidad de la transición.
    [SerializeField] private float tiltAngle = 70f;     // Cuánto se inclina la cabeza hacia un lado.
    [SerializeField] private float targetHeight = 0.3f; // Cómo de cerca del suelo queda la vista.

    private bool isDead = false;          // Estado para controlar cuándo ejecutar el efecto.
    private Quaternion targetRotation;    // Almacena la rotación final deseada.
    private Vector3 targetPosition;       // Almacena la posición final deseada.

    void Start()
    {
        // Buscamos el componente HealthSystem en el mismo objeto.
        health = GetComponent<HealthSystem>();

        // Suscripción al evento: "Cuando el HealthSystem avise que murió, ejecuta mi función".
        if (health != null)
        {
            health.OnDeath += StartDeathSequence;
        }
    }

    void StartDeathSequence()
    {
        // Activamos el estado de muerte para que el Update empiece a trabajar.
        isDead = true;

        // 1. Calculamos la ROTACIÓN final:
        // Queremos que mantenga su dirección (Y), pero que se incline en el eje Z (de lado).
        targetRotation = Quaternion.Euler(0, cameraTransform.localEulerAngles.y, tiltAngle);

        // 2. Calculamos la POSICIÓN final:
        // Mantenemos su X y Z local, pero cambiamos la altura (Y) a targetHeight.
        targetPosition = new Vector3(cameraTransform.localPosition.x, targetHeight, cameraTransform.localPosition.z);

        // --- DESACTIVAR CONTROLES (Para que el jugador no se mueva estando muerto) ---

        // Desactivamos el CharacterController para que pierda las colisiones y gravedad de movimiento.
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Si usamos físicas (Rigidbody), lo ponemos en modo cinemático para que no ruede por el mapa.
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // NOTA: Aquí es donde desactivaríamos los scripts de 'PlayerMovement' o 'MouseLook'.
    }

    void Update()
    {
        // Si no estamos muertos, no hacemos nada (ahorro de recursos).
        if (!isDead) return;

        // Slerp (Spherical Linear Interpolation): Mueve la rotación de forma suave y natural.
        cameraTransform.localRotation = Quaternion.Slerp(
            cameraTransform.localRotation,
            targetRotation,
            Time.deltaTime * fallSpeed
        );

        // Lerp (Linear Interpolation): Mueve la posición suavemente hacia el objetivo en el suelo.
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetPosition,
            Time.deltaTime * fallSpeed
        );
    }

    private void OnDestroy()
    {
        // ¡IMPORTANTE! Siempre hay que desuscribirse de los eventos cuando el objeto se destruye
        // para evitar errores de memoria o fugas de datos (Memory Leaks).
        if (health != null) health.OnDeath -= StartDeathSequence;
    }
}

/* Conceptos: 
Diferencia entre Lerp y Slerp:
    Lerp: Ideal para posiciones (líneas rectas).
    Slerp: Ideal para rotaciones, ya que hace que el giro sea más orgánico y no se vea "tieso".

Local vs World Space: Nota que usamos localPosition y localRotation. Esto es crucial porque si moviéramos 
la cámara en coordenadas globales, al morir el jugador la cámara podría salir disparada hacia el centro 
del mapa (0,0,0) en lugar de quedarse "dentro" del cuerpo del personaje.
*/