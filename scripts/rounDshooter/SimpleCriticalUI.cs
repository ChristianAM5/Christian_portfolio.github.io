using UnityEngine;
using UnityEngine.UI; // Necesario para manipular componentes de Interfaz de Usuario

public class SimpleCriticalUI : MonoBehaviour
{
    // --- SECCIÓN DE REFERENCIAS ---
    [Header("Referencias")]
    // Referencia al script de vida del jugador. Es privado para mantener el encapsulamiento.
    private HealthSystem healthSystem;

    // La imagen (usualmente un degradado rojo) que cubrirá la pantalla.
    [SerializeField] private Image vignetteImage;

    // --- SECCIÓN DE AJUSTES ---
    [Header("Ajustes")]
    // Umbral de activación (0.3f significa que el efecto empieza al 30% de vida o menos).
    [Range(0, 1)][SerializeField] private float threshold = 0.3f;

    // Velocidad a la que el efecto de latido "parpadea".
    [SerializeField] private float pulseSpeed = 5f;

    // Opacidad máxima que alcanzará la viñeta para no cegar totalmente al jugador.
    [SerializeField] private float maxAlpha = 0.6f;

    // Variable interna para cachear el porcentaje de vida actual (0 a 1).
    private float currentHealthPercent = 1f;

    private void Start()
    {
        // 1. CONEXIÓN DINÁMICA: Buscamos al jugador por su etiqueta. 
        // Esto permite que el script funcione sin importar en qué escena estemos.
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            healthSystem = playerObj.GetComponent<HealthSystem>();
        }

        // 2. SUSCRIPCIÓN A EVENTOS: Aquí ocurre la magia del desacoplamiento.
        if (healthSystem != null)
        {
            // Nos suscribimos al evento 'OnHealthChanged'. Cuando la vida cambie, 
            // el HealthSystem nos avisará y nosotros actualizaremos 'currentHealthPercent'.
            healthSystem.OnHealthChanged += (curr, max) => currentHealthPercent = curr / max;
            currentHealthPercent = 1f;
        }
        else
        {
            Debug.LogWarning("No se encontró un objeto con el Tag 'Player' o no tiene el HealthSystem.");
        }

        // 3. ESTADO INICIAL: Aseguramos que la viñeta sea invisible al empezar.
        Color c = vignetteImage.color;
        c.a = 0;
        vignetteImage.color = c;
    }

    private void Update()
    {
        // ¿Estamos en estado crítico? (Vida menor al umbral y mayor a 0)
        if (currentHealthPercent <= threshold && currentHealthPercent > 0)
        {
            // --- LÓGICA DEL LATIDO (FEEDBACK VISUAL) ---

            // Usamos la función Matemática Seno (Sin) para crear una oscilación suave.
            // Mathf.Sin devuelve valores entre -1 y 1. 
            // Sumamos 1 y dividimos entre 2 para que el rango sea de 0 a 1 (ideal para la opacidad).
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

            Color c = vignetteImage.color;
            c.a = pulse * maxAlpha; // Multiplicamos por la opacidad máxima permitida.
            vignetteImage.color = c;
        }
        else if (vignetteImage.color.a > 0)
        {
            // --- LIMPIEZA SUAVE ---

            // Si nos curamos o morimos, no queremos que la viñeta desaparezca de golpe.
            // MoveTowards reduce el valor de 'alpha' gradualmente hacia 0.
            Color c = vignetteImage.color;
            c.a = Mathf.MoveTowards(c.a, 0, Time.deltaTime * 2f);
            vignetteImage.color = c;
        }
    }
}