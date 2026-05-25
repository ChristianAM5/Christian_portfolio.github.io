using UnityEngine;
using UnityEngine.Rendering;

// Este componente permite que un objeto (un botiquín) cure al jugador cuando este lo toca.
public class HealthPickup : MonoBehaviour
{
    // [Header] ayuda a organizar el Inspector de Unity para que los alumnos no se pierdan.
    [Header("Ajustes de Curación")]

    // Cantidad de puntos de vida que recuperará el jugador.
    [SerializeField] private float healAmount = 25f;

    // Variable para decidir si el objeto desaparece tras usarse (útil para items de un solo uso).
    [SerializeField] private bool destroyOnUse = true;

    [Header("Efectos")]

    // Referencia al archivo de audio que sonará al curarse.
    [SerializeField] private AudioClip healSound;

    // [Range] crea un deslizador (slider) en el Inspector para evitar valores fuera de 0 y 1.
    [Range(0, 1)][SerializeField] private float volume = 1f;

    // OnTriggerEnter se ejecuta cuando un objeto con Rigidbody entra en el área "Trigger" de este objeto.
    private void OnTriggerEnter(Collider other)
    {
        // 1. Verificamos si lo que entró en el área tiene la etiqueta "Player".
        if (other.CompareTag("Player"))
        {
            // 2. Intentamos obtener el componente 'HealthSystem' del objeto que entró (el jugador).
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();

            // 3. Verificamos si el jugador realmente tiene un sistema de salud (evita errores de consola).
            if (playerHealth != null)
            {
                // COMPROBACIÓN LÓGICA: ¿El jugador realmente necesita curarse?
                // Solo actuamos si la vida actual es menor que la vida máxima.
                if (playerHealth.currentHealth < playerHealth.maxHealth)
                {
                    // 4. Aplicamos la curación llamando al método Heal del jugador.
                    playerHealth.Heal(healAmount);

                    // 5. Feedback auditivo:
                    if (healSound != null)
                    {
                        // PlayClipAtPoint crea un objeto temporal que reproduce el sonido en la posición del pickup.
                        // Esto permite que el sonido siga sonando incluso si destruimos el pickup inmediatamente.
                        AudioSource.PlayClipAtPoint(healSound, transform.position, volume);
                    }

                    // 6. Finalmente, eliminamos el botiquín de la escena.
                    Destroy(gameObject);
                }
                else
                {
                    // Si el jugador está lleno, no hacemos nada y enviamos un mensaje a consola (opcional).
                    Debug.Log("El jugador ya tiene la vida al máximo, botiquín ignorado.");
                }
            }
        }
    }
}