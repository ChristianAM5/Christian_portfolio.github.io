using UnityEngine;
using System; // Necesario para usar 'Action', que son nuestros eventos

public class HealthSystem : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f; // La vida máxima que puede tener el personaje
    public float currentHealth;    // La vida que tiene en el momento exacto

    [Header("Audio")]
    // Referencia al componente que reproduce sonido y la lista de clips disponibles
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] damageSounds;

    // --- EVENTOS (La clave del desacoplamiento) ---
    // OnHealthChanged: Avisa a otros (como la barra de vida) cuánto cambió la salud.
    // OnDeath: Avisa a otros (como el Game Over) que el personaje murió.
    public event Action<float, float> OnHealthChanged; // Envía (vidaActual, vidaMáxima)
    public event Action OnDeath;

    private void Awake()
    {
        // Al iniciar, nos aseguramos de que el personaje empiece con la vida llena.
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Función principal para recibir daño.
    /// </summary>
    /// <param name="amount">Cantidad base de daño.</param>
    /// <param name="type">Opcional: Tipo de daño para aplicar multiplicadores.</param>
    public void TakeDamage(float amount, DamageType type = null)
    {
        // Si ya está muerto, no procesamos más daño.
        if (currentHealth <= 0) return;

        // Lógica de Multiplicador: Si el daño viene con un "tipo" (ej. fuego, explosión),
        // multiplicamos el daño base. Si no, usamos el valor original.
        float finalDamage = type != null ? amount * type.damageMultiplier : amount;

        // Restamos la vida y usamos Clamp para que nunca sea menor a 0 ni mayor al máximo.
        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // DISPARAR EVENTO: "?." significa "si alguien está escuchando, avísales".
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Efecto visual/sonoro inmediato
        PlayDamageSound();

        // Verificamos si la vida llegó a cero para ejecutar la muerte.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Función para curar al personaje (usada por los Pickups).
    /// </summary>
    public void Heal(float amount)
    {
        // MoveTowards incrementa el valor asegurándose de no pasarse del máximo.
        currentHealth = Mathf.MoveTowards(currentHealth, maxHealth, amount);

        // Avisamos a la UI que la vida subió.
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        // Log para depuración en la consola de Unity.
        Debug.Log($"{gameObject.name} ha muerto.");

        // Avisamos a todos los sistemas interesados (Cámara, UI, GameManager) que morimos.
        OnDeath?.Invoke();
    }

    private void PlayDamageSound()
    {
        // Verificación de seguridad: ¿Tenemos un AudioSource y sonidos asignados?
        if (audioSource != null && damageSounds.Length > 0)
        {
            // Seleccionamos un sonido aleatorio del array para que no sea repetitivo.
            AudioClip clip = damageSounds[UnityEngine.Random.Range(0, damageSounds.Length)];

            // Variación de tono (Pitch): Hace que el mismo sonido suene ligeramente 
            // más grave o agudo cada vez, dando realismo.
            audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);

            // PlayOneShot permite que el sonido suene sin interrumpir otros sonidos previos.
            audioSource.PlayOneShot(clip);
        }
    }
}
