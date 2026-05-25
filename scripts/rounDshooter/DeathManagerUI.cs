using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar la escena
using UnityEngine.UI; // Necesario para interactuar con Componentes de UI (Button)
using System.Collections; // Necesario para usar Corrutinas (IEnumerator)

/// <summary>
/// Este script gestiona la pantalla de "Game Over".
/// Se encarga de escuchar cuando el jugador muere y mostrar el menu tras una breve espera.
/// </summary>
public class DeathManagerUI : MonoBehaviour
{
[Header("Configuración de UI")]
    [Tooltip("El panel que contiene los botones y textos de muerte")]
    [SerializeField] private GameObject deathScreenPanel;

    [Tooltip("Botón que el jugador presionará para reintentar")]
    [SerializeField] private Button restartButton;

    [Tooltip("Segundos que esperamos antes de mostrar la UI (para ver la animación de caída)")]
    [SerializeField] private float delayBeforeShow = 3.0f;

    [Header("Textos de muerte")]
    [SerializeField] private TMPro.TextMeshProUGUI roundReachedText;
    [SerializeField] private TMPro.TextMeshProUGUI killsText;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;

    private HealthSystem playerHealth;

    private void Start()
    {
        SetupPlayerConnection();

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(false);
    }

    private void SetupPlayerConnection()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<HealthSystem>();

            if (playerHealth != null)
            {
                playerHealth.OnDeath += HandleDeath;
            }
        }
    }

    /// <summary>
    /// Este método se dispara inmediatamente cuando el jugador se queda sin vida.
    /// </summary>
    private void HandleDeath()
    {
        // 1. CONEXIÓN CRUCIAL: Avisamos al RoundManager para que detenga el tiempo de la ronda
        // e inactive el Spawner de enemigos al instante.
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.HandlePlayerDeath();
        }

        // 2. CONEXIÓN CRUCIAL: Disparamos el evento global de muerte del jugador.
        // Esto le dice a 'BetweenRoundsUI' que active su flag 'gameOver' y no muestre el panel de mejoras.
        GameEvents.PlayerDeath();

        // 3. Comenzamos la secuencia de espera visual
        StartCoroutine(DeathSequenceRoutine());
    }

    /// <summary>
    /// Secuencia temporal: Espera en tiempo real -> Congela el juego -> Muestra UI
    /// </summary>
    private IEnumerator DeathSequenceRoutine()
    {
        // Esperamos los segundos configurados de forma normal para que la cámara/físicas 
        // del jugador hagan la animación de caer al suelo.
        yield return new WaitForSeconds(delayBeforeShow);

        // 4. NUEVO: Congelamos por completo el movimiento de los enemigos y proyectiles
        Time.timeScale = 0f;

        // Activamos visualmente el panel de muerte
        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(true);

        if (RoundManager.Instance != null)
        {
            roundReachedText.text = $"Ronda alcanzada: {RoundManager.Instance.CurrentRound}";
            killsText.text = $"Enemigos eliminados: {RoundManager.Instance.TotalKills}";

            float t = RoundManager.Instance.TotalPlayTime;
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);

            timeText.text = $"Tiempo total: {minutes}m {seconds}s";
        }

        // Liberamos el cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        // Importante: devolvemos el tiempo a la normalidad antes de recargar la escena
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandleDeath;
        }
    }
}