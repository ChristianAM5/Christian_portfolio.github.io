using UnityEngine;
using UnityEngine.InputSystem; // Necesario para usar las nuevas acciones de entrada

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f; // Velocidad de traslación del jugador

    private Rigidbody2D rb;          // Referencia al componente de física
    private Vector2 moveInput;       // Guarda la dirección del movimiento (X, Y)
    private Animator animator;       // Referencia al controlador de animaciones

    [Header("Efectos de Sonido")]
    private bool playingFootsteps = false; // Controla si el sonido de pasos ya está sonando
    public float footstepSpeed = 0.5f;     // Tiempo en segundos entre cada paso

    // Awake/Start se usa para inicializar las referencias a los componentes
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update se ejecuta en cada frame.
    void Update()
    {
        // 1. COMPROBACIÓN DE PAUSA
        // Si el juego está pausado según el PauseController, detenemos al jugador
        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero; // Detiene físicamente al objeto
            animator.SetBool("isWalking", false); // Cambia a animación de "IDLE"
            StopFootsteps(); // Deja de sonar los pasos
            return; // Sale del método para no procesar el movimiento
        }

        // 2. APLICAR MOVIMIENTO
        // Multiplica la dirección recibida por la velocidad configurada
        rb.linearVelocity = moveInput * moveSpeed;

        // 3. CONTROL DE ANIMACIONES
        // Si la velocidad es mayor a 0, activamos la animación de caminar
        animator.SetBool("isWalking", rb.linearVelocity.magnitude > 0);

        // 4. LÓGICA DE SONIDO DE PASOS
        // Si nos movemos y no están sonando los pasos, los iniciamos
        if (rb.linearVelocity.magnitude > 0 && !playingFootsteps)
        {
            StartFootsteps();
        }
        // Si nos detenemos, paramos el sonido
        else if (rb.linearVelocity.magnitude == 0)
        {
            StopFootsteps();
        }
    }

    /// <summary>
    /// Este método debe ser vinculado a una "Action" del Player Input Component.
    /// Se dispara automáticamente cuando el jugador mueve el Stick o pulsa WASD.
    /// </summary>
    public void Move(InputAction.CallbackContext context)
    {
        // Cuando el jugador suelta las teclas (canceled)
        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            // Guardamos la última dirección para que el personaje mire hacia donde iba
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        // Lee el valor del input (un Vector2 con valores entre -1 y 1)
        moveInput = context.ReadValue<Vector2>();

        // Actualiza los parámetros del Animator para el "Blend Tree" (animaciones en 8 direcciones)
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    // Inicia la repetición del sonido de pasos usando InvokeRepeating
    void StartFootsteps()
    {
        playingFootsteps = true;
        // Llama a 'PlayFootstep' cada 'footstepSpeed' segundos
        InvokeRepeating(nameof(PlayFootstep), 0f, footstepSpeed);
    }

    // Detiene la repetición de sonidos
    void StopFootsteps()
    {
        playingFootsteps = false;
        CancelInvoke(nameof(PlayFootstep));
    }

    // Reproduce un clip aleatorio del grupo "Footsteps" con variación de tono
    void PlayFootstep()
    {
        // Usa el gestor de sonidos que vimos anteriormente
        SoundEffectManager.Play("Footsteps", true);
    }
}
