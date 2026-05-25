using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Necesario para usar Corrutinas (procesos que esperan tiempo)

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    // Referencia al script de lógica del jugador. Es privada porque la buscaremos por código.
    private HealthSystem playerHealth;
    // La barra de UI de Unity (Slider).
    [SerializeField] private Slider healthSlider;

    [Header("Settings")]
    // Velocidad a la que se mueve la barrita. Un valor más bajo la hace más suave.
    [SerializeField] private float updateSpeed = 5f;

    [Header("Critical Health Effect")]
    // Imagen roja que cubre la pantalla cuando nos queda poca vida.
    [SerializeField] private Image criticalVignette;
    // Porcentaje (0.3 = 30%) para activar el efecto visual de peligro.
    [SerializeField] private float criticalThreshold = 0.3f;

    // El valor real de vida hacia el que la barra debe moverse visualmente.
    private float targetValue;

    private void Awake()
    {
        // Si olvidamos arrastrar el Slider en el Inspector, el script intenta encontrarlo en el mismo objeto.
        if (healthSlider == null) healthSlider = GetComponent<Slider>();
    }

    private void Start()
    {
        // Iniciamos una Corrutina. Esto es como un "hilo" secundario que buscará al jugador
        // sin detener el resto del juego.
        StartCoroutine(FindPlayerRoutine());
    }

    private IEnumerator FindPlayerRoutine()
    {
        // Bucle "mientras": Si el jugador no ha aparecido aún, sigue buscando.
        // Útil si el jugador hace respawn o tarda en cargar.
        while (playerHealth == null)
        {
            // Buscamos cualquier objeto que tenga la etiqueta "Player".
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                // Si encontramos el objeto, intentamos obtener su componente de salud.
                playerHealth = playerObj.GetComponent<HealthSystem>();

                if (playerHealth != null)
                {
                    // ¡CONEXIÓN CLAVE!: Nos suscribimos al evento "OnHealthChanged".
                    // "Cuando la vida cambie en el Player, avísame a mí (UpdateHealthBar)".
                    playerHealth.OnHealthChanged += UpdateHealthBar;

                    // Ajustamos los valores iniciales de la barra.
                    healthSlider.maxValue = 100f;
                    healthSlider.value = 100f;
                    targetValue = 100f;

                    Debug.Log("<color=green>HealthBarUI:</color> Jugador encontrado y conectado.");
                }
            }

            if (playerHealth == null)
            {
                // Si no lo encontramos en este frame, esperamos al siguiente y volvemos a intentar.
                yield return null;
            }
        }
    }

    private void OnDisable()
    {
        // IMPORTANTE: Si este objeto se destruye o desactiva, debemos desuscribirnos
        // del evento para evitar errores de memoria (Memory Leaks).
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    // Este método se ejecuta automáticamente cada vez que el Player recibe daño o cura.
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        targetValue = currentHealth; // No cambiamos el valor visual aquí, solo el objetivo.

        // Lógica de la viñeta de sangre/peligro
        float healthPercent = currentHealth / maxHealth;

        if (healthPercent <= criticalThreshold)
        {
            // InverseLerp calcula cómo de cerca estamos del 0 dentro del rango crítico.
            // Si el umbral es 30% y tenemos 15%, el alpha será 0.5 (mitad de intensidad).
            float alpha = Mathf.InverseLerp(criticalThreshold, 0, healthPercent);

            // Aplicamos el color rojo con la transparencia calculada (máximo 60%).
            criticalVignette.color = new Color(1, 0, 0, alpha * 0.6f);
        }
        else
        {
            // Si tenemos más del 30%, la viñeta es totalmente transparente.
            criticalVignette.color = new Color(1, 0, 0, 0);
        }
    }

    private void Update()
    {
        // LERPIZACIÓN: Para que la barra no "salte" de un valor a otro,
        // usamos Mathf.Lerp para que se deslice suavemente hacia el targetValue.
        if (healthSlider != null && healthSlider.value != targetValue)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * updateSpeed);
        }
    }
}