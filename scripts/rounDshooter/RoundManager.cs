using UnityEngine;
using System.Collections;
 
/// <summary>
/// Sistema central del juego. Gestiona:
///   - Rondas de 30 segundos infinitas
///   - Escalado de enemigos por ronda
///   - Desbloqueo de armas por ronda
///   - Conteo de bajas y puntos de habilidad
///   - Pausa entre rondas (timeScale = 0)
///   - Datos para la pantalla de muerte (kills, tiempo, ronda)
/// </summary>
/// 
public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }
 
    // ── Configuración ────────────────────────────────────────────────────────
    [Header("Rondas")]
    [SerializeField] private float roundDuration       = 30f;
    [SerializeField] private float betweenRoundDelay   = 1f; // pausa antes de mostrar la UI
 
    [Header("Escalado de Enemigos por Ronda")]
    [Tooltip("+X% de vida por cada ronda adicional. Ej: 0.15 = +15%")]
    [SerializeField] private float healthScalePerRound  = 0.15f;
    [Tooltip("+X% de velocidad. Ej: 0.08 = +8%")]
    [SerializeField] private float speedScalePerRound   = 0.08f;
    [Tooltip("+X% de daño. Ej: 0.10 = +10%")]
    [SerializeField] private float damageScalePerRound  = 0.10f;
    [Tooltip("-X% de tiempo entre spawns (más rápido). Ej: 0.10 = -10%")]
    [SerializeField] private float spawnRateScale       = 0.10f;
    [Tooltip("El intervalo de spawn no bajará de este mínimo (segundos).")]
    [SerializeField] private float minSpawnInterval     = 0.8f;
 
    [Header("Desbloqueo de Armas")]
    [Tooltip("Ronda mínima para desbloquear cada arma. El índice coincide con el array de WeaponManager.\n" +
             "Índice 0 = Pistola (siempre disponible, pon 0 o 1)\n" +
             "Índice 1 = Rifle (ronda 3)\n" +
             "Índice 2 = Francotirador (ronda 6)\n" +
             "Índice 3 = Lanzacohetes (ronda 10)")]
    [SerializeField] private int[] weaponUnlockRounds = { 1, 3, 6, 10 };
 
    [Header("Puntos de Habilidad")]
    [SerializeField] private int killsPerSkillPoint = 5;

    [Header("Audio")]
    [SerializeField] private AudioClip spendPointSound;

    [Range(0, 1)]
    [SerializeField] private float volume = 1f;
 
    // ── Estado público (leído por UIs) ────────────────────────────────────────
    public int   CurrentRound        { get; private set; } = 0;
    public float RoundTimeRemaining  { get; private set; } = 0f;
    public bool  IsPlaying           { get; private set; } = false;
    public int   TotalKills          { get; private set; } = 0;
    public int   SkillPoints         { get; private set; } = 0;
    public float TotalPlayTime       { get; private set; } = 0f;
 
    // ── Estado interno ────────────────────────────────────────────────────────
    private int killsUntilNextPoint;
    private WeaponManager weaponManager;
    private EnemySpawner  enemySpawner;
    private bool gameOver = false;
 
    // ── Inicialización ────────────────────────────────────────────────────────
 
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        killsUntilNextPoint = killsPerSkillPoint;
    }
 
    private void Start()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();
        enemySpawner  = FindFirstObjectByType<EnemySpawner>();
 
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
 
        // Arma inicial desbloqueada siempre (índice 0 = pistola)
        weaponManager?.UnlockWeapon(0);
 
        StartRound(1);
    }
 
    // ── Loop principal ────────────────────────────────────────────────────────
 
    private void Update()
    {
        if (!IsPlaying || gameOver) return;
 
        TotalPlayTime      += Time.deltaTime;
        RoundTimeRemaining -= Time.deltaTime;
 
        if (RoundTimeRemaining <= 0f)
        {
            RoundTimeRemaining = 0f;
            EndRound();
        }
    }
 
    // ── Gestión de rondas ─────────────────────────────────────────────────────
 
    private void StartRound(int roundNumber)
    {
        CurrentRound       = roundNumber;
        RoundTimeRemaining = roundDuration;
        IsPlaying          = true;
 
        ApplyEnemyScaling();
        CheckWeaponUnlocks();
 
        GameEvents.RoundStarted(CurrentRound);
        Debug.Log($"[RoundManager] ▶ Ronda {CurrentRound} iniciada.");
    }
 
    private void EndRound()
    {
        IsPlaying = false;
        GameEvents.RoundEnded(CurrentRound);
        Debug.Log($"[RoundManager] ■ Ronda {CurrentRound} terminada. Bajas totales: {TotalKills}");
 
        StartCoroutine(PauseBeforeUI());
    }
 
    private IEnumerator PauseBeforeUI()
    {
        // Esperamos un momento para que el jugador vea el final de acción
        yield return new WaitForSeconds(betweenRoundDelay);
 
        // Congelamos todo: enemigos, jugador, spawner. La UI usa unscaledTime.
        Time.timeScale = 0f;
        // GameEvents.OnRoundEnded ya fue disparado; la BetweenRoundsUI lo habrá recibido.
    }
 
    /// <summary>
    /// Llamado por el botón "Continuar" de la UI entre rondas.
    /// </summary>
    public void StartNextRound()
    {
        Time.timeScale = 1f;
        StartRound(CurrentRound + 1);
    }
 
    // ── Escalado de enemigos ──────────────────────────────────────────────────
 
    private void ApplyEnemyScaling()
    {
        if (enemySpawner == null) return;
 
        int extraRounds = CurrentRound - 1; // la ronda 1 no escala (x1.0)
 
        float healthMult = 1f + extraRounds * healthScalePerRound;
        float speedMult  = 1f + extraRounds * speedScalePerRound;
        float damageMult = 1f + extraRounds * damageScalePerRound;
 
        // El intervalo de spawn se reduce con cada ronda, hasta el mínimo
        float spawnMult = extraRounds * 0.1f;
 
        enemySpawner.SetRoundScaling(healthMult, speedMult, damageMult, spawnMult);
    }
 
    // ── Desbloqueo de armas ───────────────────────────────────────────────────
 
    private void CheckWeaponUnlocks()
    {
        if (weaponManager == null) return;
 
        for (int i = 0; i < weaponUnlockRounds.Length; i++)
        {
            if (CurrentRound >= weaponUnlockRounds[i])
                weaponManager.UnlockWeapon(i);
        }
    }
 
    // ── Puntos de habilidad ───────────────────────────────────────────────────
 
    private void HandleEnemyKilled()
    {
        TotalKills++;
        killsUntilNextPoint--;
 
        if (killsUntilNextPoint <= 0)
        {
            SkillPoints++;
            killsUntilNextPoint = killsPerSkillPoint;
            GameEvents.SkillPointsChanged(SkillPoints);
            Debug.Log($"[RoundManager] ★ Punto de habilidad ganado. Total: {SkillPoints}");
        }
    }
 
    /// <summary>
    /// Añade puntos de habilidad de bonus (cofre, evento especial, etc.).
    /// </summary>
    public void AddBonusSkillPoints(int amount)
    {
        SkillPoints += amount;
        GameEvents.SkillPointsChanged(SkillPoints);
        Debug.Log($"[RoundManager] ★ +{amount} puntos de habilidad (bonus). Total: {SkillPoints}");
    }
 
    /// <summary>
    /// Gasta un punto de habilidad. Devuelve false si no hay suficientes.
    /// Llamado por la UI de mejoras antes de aplicar una mejora.
    /// </summary>
    public bool SpendSkillPoint()
    {
        if (SkillPoints <= 0) return false;
        SkillPoints--;

        // SONIDO
        if (spendPointSound != null)
        {
            AudioSource.PlayClipAtPoint(
                spendPointSound,
                Camera.main.transform.position,
                volume
            );
        }

        GameEvents.SkillPointsChanged(SkillPoints);
        return true;
    }
 
    // ── Muerte del jugador ────────────────────────────────────────────────────
 
    /// <summary>
    /// Llamado por DeathManagerUI al detectar la muerte del jugador.
    /// Para los spawns y guarda el estado para la pantalla de muerte.
    /// </summary>
    public void HandlePlayerDeath()
    {
        if (gameOver) return;
        gameOver  = true;
        IsPlaying = false;
        enemySpawner?.SetSpawningEnabled(false);
        StopAllCoroutines();
        Debug.Log($"[RoundManager] GAME OVER — Ronda: {CurrentRound} | Bajas: {TotalKills} | Tiempo: {TotalPlayTime:F0}s");
    }
 
    // ── Limpieza ──────────────────────────────────────────────────────────────
 
    private void OnDestroy()
    {
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
    }
}
