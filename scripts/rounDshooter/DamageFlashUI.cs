using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Este script se encarga de mostrar un "flash" visual en la pantalla (UI)
/// cada vez que el jugador recibe daño.
/// </summary>
public class DamageFlashUI : MonoBehaviour
{
    [Header("Referencias")]
    // Referencia al sistema de vida del jugador. Es privado porque lo buscaremos por código.
    private HealthSystem healthSystem;

    // El componente 'Image' que cubrirá la pantalla (normalmente un panel rojo transparente).
    [SerializeField] private Image flashImage;

    [Header("Configuración")]
    // Cuánto tiempo durará el efecto de desvanecimiento.
    [SerializeField] private float flashDuration = 0.2f;

    // Color inicial del flash (rojo por defecto con algo de transparencia/alpha).
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.4f);

    private float lastHealth;       // Guarda la vida que teníamos en el último cambio para comparar.
    private Coroutine flashRoutine; // Referencia a la corrutina para poder reiniciarla si nos golpean rápido.

    private void Start()
    {
        // Al empezar, intentamos conectar con el jugador.
        SetupPlayerReference();
    }

    // OnEnable se ejecuta cada vez que el objeto se activa.
    // Es vital en Unity 6 si usamos pools de objetos o cambiamos de escena.
    private void OnEnable()
    {
        SetupPlayerReference();
    }

    /// <summary>
    /// Busca al jugador en la escena y se suscribe a sus eventos de vida.
    /// </summary>
    private void SetupPlayerReference()
    {
        // Buscamos al objeto que tenga la etiqueta "Player". 
        // Esto permite que el sistema funcione con cualquier controlador (CharacterController, etc.)
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            // Intentamos obtener el componente lógico de salud.
            healthSystem = playerObj.GetComponent<HealthSystem>();

            if (healthSystem != null)
            {
                // DESUSCRIPCIÓN PREVENTIVA: Antes de suscribirnos, nos aseguramos de no estar ya suscritos.
                // Esto evita que el método HandleHealthChanged se ejecute dos veces por un solo golpe.
                healthSystem.OnHealthChanged -= HandleHealthChanged;

                // SUSCRIPCIÓN AL EVENTO: "Cuando la vida cambie, avísame ejecutando HandleHealthChanged".
                healthSystem.OnHealthChanged += HandleHealthChanged;

                // Inicializamos la referencia de vida actual.
                lastHealth = 100f;
            }
        }
    }

    /// <summary>
    /// Método que reacciona al evento del HealthSystem.
    /// </summary>
    /// <param name="current">Vida actual tras el cambio.</param>
    /// <param name="max">Vida máxima del jugador.</param>
    private void HandleHealthChanged(float current, float max)
    {
        // Lógica de detección de daño:
        // Si la vida actual es menor que la que teníamos antes, es que nos han hecho daño.
        if (current < lastHealth)
        {
            // Si ya había un flash en curso, lo detenemos para empezar uno nuevo (feedback inmediato).
            if (flashRoutine != null) StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        // Actualizamos nuestro registro para la siguiente comparación.
        lastHealth = current;
    }

    /// <summary>
    /// Corrutina que gestiona el efecto visual de "parpadeo" de forma suave.
    /// </summary>
    private IEnumerator FlashRoutine()
    {
        float elapsed = 0;

        // Ponemos el color inicial (el rojo con el alpha configurado).
        flashImage.color = flashColor;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;

            // Calculamos el desvanecimiento (Lerp): 
            // Vamos desde el Alpha original hasta 0 (totalmente transparente).
            float alpha = Mathf.Lerp(flashColor.a, 0, elapsed / flashDuration);

            // Aplicamos el nuevo color con el alpha calculado.
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);

            // Esperamos al siguiente frame.
            yield return null;
        }

        // Al finalizar, nos aseguramos de que el alpha sea exactamente 0.
        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
    }

    private void OnDestroy()
    {
        // BUENA PRÁCTICA: Cuando este objeto de UI se destruye, debemos limpiar la suscripción.
        // Si no lo hacemos, el HealthSystem intentará llamar a un objeto que ya no existe (Memory Leak).
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= HandleHealthChanged;
    }
}

/*
Conceptos clave para explicar a los alumnos:
 
Suscripción a Eventos (+=): es como "apuntarse a una lista de correo". La UI no está preguntando 
cada segundo "¿te han pegado?", sino que se queda dormida hasta que el HealthSystem envía un "correo" masivo a 
todos los interesados.

Lerp (Linear Interpolation): Es la herramienta matemática para transiciones suaves. En este caso, para que el rojo 
no desaparezca de golpe, sino que se desvanezca elegantemente.

Desacoplamiento: El script busca el Tag "Player". Esto significa que si mañana cambian al soldado por un tanque, 
mientras el tanque tenga el Tag "Player" y el componente HealthSystem, la UI seguirá funcionando sin cambiar ni una 
línea de código.

Limpieza (OnDestroy): Siempre hay que recalcar que un buen programador limpia lo que ensucia. Dejar eventos colgados 
es una de las causas principales de errores (crashes) en proyectos grandes de Unity.
*/