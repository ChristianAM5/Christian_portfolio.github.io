using UnityEngine;
using UnityEngine.AI;
 
/// <summary>
/// Spawner de enemigos. Genera enemigos en posiciones NavMesh aleatorias.
/// RoundManager le pasa multiplicadores de escalado al inicio de cada ronda.
/// </summary>

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject normalEnemyPrefab;
    [SerializeField] private GameObject tankEnemyPrefab;
    [SerializeField] private GameObject fastEnemyPrefab;
 
    [Header("Spawn Timing (valores base — ronda 1)")]
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 7f;
 
    [Header("Posiciones de Spawn")]
    [SerializeField] private float spawnRadius          = 15f;
    [SerializeField] private float minDistanceFromPlayer = 5f;
    [SerializeField] private int   maxEnemies            = 8;
 
    [Header("NavMesh")]
    [SerializeField] private float navMeshSampleRadius = 2f;
 
    // ── Estado interno ────────────────────────────────────────────────────────
    private float nextSpawnTime;
    private Transform playerTransform;
    private bool spawningEnabled = true;
 
    // Multiplicadores aplicados por RoundManager al inicio de cada ronda
    private float currentHealthMult  = 1f;
    private float currentSpeedMult   = 1f;
    private float currentDamageMult  = 1f;
    // spawnIntervalMult < 1 = spawns más frecuentes
    private float currentSpawnMult   = 1f;
 
    // ── Inicio ────────────────────────────────────────────────────────────────
 
    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning("[EnemySpawner] No se encontró el Player.");
 
        ScheduleNextSpawn();
    }
 
    void Update()
    {
        // Solo spawnea si está habilitado Y la ronda está activa
        if (!spawningEnabled) return;
        if (RoundManager.Instance != null && !RoundManager.Instance.IsPlaying) return;
 
        if (Time.time >= nextSpawnTime)
        {
            TrySpawnEnemy();
            ScheduleNextSpawn();
        }
    }
 
    // ── API pública ───────────────────────────────────────────────────────────
 
    /// <summary>
    /// Llamado por RoundManager al inicio de cada ronda para escalar los enemigos.
    /// spawnIntervalMult: valor entre 0-1, cuanto más bajo más rápido spawnan.
    /// </summary>
    public void SetRoundScaling(float healthMult, float speedMult,
                                float damageMult, float spawnIntervalMult)
    {
        currentHealthMult = healthMult;
        currentSpeedMult  = speedMult;
        currentDamageMult = damageMult;
        currentSpawnMult  = spawnIntervalMult;
 
        // Ajustamos maxEnemies con la ronda actual para mayor presión
        int round = RoundManager.Instance != null ? RoundManager.Instance.CurrentRound : 1;
        maxEnemies = Mathf.Min(8 + (round - 1), 20); // +1 por ronda, máximo 20
 
        Debug.Log($"[EnemySpawner] Escalado ronda {round}: " +
                  $"vida×{healthMult:F2} vel×{speedMult:F2} " +
                  $"daño×{damageMult:F2} spawn×{spawnIntervalMult:F2} " +
                  $"maxEnemigos={maxEnemies}");
    }
 
    /// <summary>Activa o desactiva el spawner (usado al morir el jugador).</summary>
    public void SetSpawningEnabled(bool enabled) => spawningEnabled = enabled;
 
    // ── Lógica de spawn ───────────────────────────────────────────────────────
 
    private void ScheduleNextSpawn()
    {
        // Aplicamos el multiplicador de spawn: un valor menor acorta el intervalo
        float interval = Mathf.Max(0.8f, Random.Range(minSpawnInterval - currentSpawnMult, maxSpawnInterval - currentSpawnMult));
        nextSpawnTime = Time.time + interval;
    }
 
    private void TrySpawnEnemy()
    {
        EnemyAI[] active = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        if (active.Length >= maxEnemies) return;
 
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 candidate = GetRandomPositionAround(transform.position, spawnRadius);
 
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit,
                                       navMeshSampleRadius, NavMesh.AllAreas))
            {
                Vector3 spawnPos = hit.position;
 
                if (playerTransform != null &&
                    Vector3.Distance(spawnPos, playerTransform.position) < minDistanceFromPlayer)
                    continue;
 
                SpawnEnemy(spawnPos);
                return;
            }
        }
 
        Debug.LogWarning("[EnemySpawner] No se encontró posición NavMesh válida.");
    }

    private GameObject GetRandomEnemyPrefab()
    {
        float r = Random.value;

        if (r < 0.5f) // mas posibilidad del normal
            return normalEnemyPrefab;
        else if (r < 0.75f)
            return tankEnemyPrefab;
        else
            return fastEnemyPrefab;
    }
 
    private Vector3 GetRandomPositionAround(Vector3 center, float radius)
    {
        Vector2 rnd = Random.insideUnitCircle * radius;
        return new Vector3(center.x + rnd.x, center.y + 5f, center.z + rnd.y);
    }
 
    private void SpawnEnemy(Vector3 position)
    {
 
        GameObject prefab = GetRandomEnemyPrefab();

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
 
        // Inyectamos los multiplicadores de escalado al enemigo recién creado
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
            ai.Initialize(currentHealthMult, currentSpeedMult, currentDamageMult);
    }
 
    // ── Gizmos ────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
    }
}
 