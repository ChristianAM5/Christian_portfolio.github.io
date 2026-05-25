using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HealthSystem))]

public class EnemyAI : MonoBehaviour
{
    [Header("Combate")]
    [SerializeField] private float damagePerHit   = 15f;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private float attackRange    = 1.5f;
    [SerializeField] private DamageType damageType;
 
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 3.5f;
    [Tooltip("Porcentaje de vida (0 a 1) por debajo del cual el enemigo huirá. 0.25f = 25% de vida.")]
    [SerializeField] private float fleeHealthThreshold = 0.25f;
    [Tooltip("Distancia a la que intentará alejarse del jugador al huir.")]
    [SerializeField] private float fleeDistance = 10f;
    [Tooltip("Velocidad reducida cuando está huyendo con la animación de caminar.")]
    [SerializeField] private float walkSpeed = 1.8f;

    [Header("Feedback Visual")]
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private Color hitColor    = Color.red;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [Tooltip("Velocidad a la que parpadeará en rojo mientras huye. Números más altos = parpadeo más rápido.")]
    [SerializeField] private float fleeFlashSpeed = 8f; // NUEVO: Velocidad del parpadeo constante
 
    // ── Referencias internas ──────────────────────────────────────────────────
    private NavMeshAgent  agent;
    private HealthSystem  healthSystem;
    private Transform     playerTransform;
    private HealthSystem  playerHealthSystem;
    private Animator      animator; 
 
    private float nextDamageTime;
    private float flashTimer;
    private bool  isFlashing;
    private bool  isFleeing = false; 

    private float fleeFlashCooldownTimer = 0f;
    private Color baseColor;
 
    public void Initialize(float healthMult, float speedMult, float damageMult)
    {
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.maxHealth     *= healthMult;
            healthSystem.currentHealth  = healthSystem.maxHealth;
        }
 
        moveSpeed    *= speedMult;
        walkSpeed    *= speedMult; 
        damagePerHit *= damageMult;
    }
 
    void Start()
    {
        agent        = GetComponent<NavMeshAgent>();
        healthSystem = GetComponent<HealthSystem>();
        animator     = GetComponent<Animator>(); 

        agent.speed           = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
 
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform    = playerObj.transform;
            playerHealthSystem = playerObj.GetComponent<HealthSystem>();
        }
        else
        {
            Debug.LogWarning("[EnemyAI] No se encontró el Player.");
        }
 
        healthSystem.OnDeath += HandleDeath;

        baseColor = enemyRenderer.material.color;
 
        if (enemyRenderer != null)
            enemyRenderer.material.color = baseColor;
    }
 
    private void OnEnable()
    {
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= HandleHit;
            healthSystem.OnHealthChanged += HandleHit;
        }
    }

    void Update()
    {
        if (playerTransform == null || playerHealthSystem == null) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        // Verificamos si la vida cae por debajo del umbral para empezar a huir
        float currentHealthPercent = healthSystem.currentHealth / healthSystem.maxHealth;
        if (!isFleeing && currentHealthPercent <= fleeHealthThreshold)
        {
            StartFleeing();
        }

        if (isFleeing)
        {
            UpdateFleeBehavior();
            
            // NUEVO: Forzamos el mismo destello de disparo de forma repetida
            fleeFlashCooldownTimer -= Time.deltaTime;
            if (fleeFlashCooldownTimer <= 0f)
            {
                // Activamos exactamente las mismas variables del golpe físico
                enemyRenderer.material.color = hitColor;
                isFlashing = true;
                flashTimer = hitFlashDuration; // Dura exactamente lo mismo que un balazo

                // Tiempo de espera entre destello y destello (ajustable con fleeFlashSpeed)
                // Si fleeFlashSpeed es 4f, parpadeará cada 0.25 segundos.
                fleeFlashCooldownTimer = 1f / fleeFlashSpeed; 
            }
        }
        else
        {
            UpdateAttackBehavior();
        }
 
        // Lógica de apagado del destello (La dejamos fuera para que afecte tanto a huidas como a disparos)
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                isFlashing = false;
                if (enemyRenderer != null)
                    enemyRenderer.material.color = normalColor;
            }
        }
    }
 
    // ── Lógicas de Estado ──────────────────────────────────────────────────────

    private void StartFleeing()
    {
        isFleeing = true;
        agent.speed = walkSpeed; 
        agent.stoppingDistance = 0f; 

        if (animator != null)
        {
            animator.SetBool("Run", false);
            animator.SetBool("Walk", true);
        }
    }

    private void UpdateFleeBehavior()
    {
        Vector3 dirToPlayer = transform.position - playerTransform.position;
        Vector3 idealFleeTarget = transform.position + dirToPlayer.normalized * fleeDistance;

        if (NavMesh.SamplePosition(idealFleeTarget, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void UpdateAttackBehavior()
    {
        agent.SetDestination(playerTransform.position);
 
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist <= attackRange && Time.time >= nextDamageTime)
        {
            playerHealthSystem.TakeDamage(damagePerHit, damageType);
            nextDamageTime = Time.time + damageCooldown;
        }
    }

    // ── Handlers ──────────────────────────────────────────────────────────────
 
    private void HandleHit(float current, float max)
    {
        if (enemyRenderer == null) return;
        
        // Si no está huyendo, que haga el parpadeo rápido clásico de impacto
        if (!isFleeing)
        {
            enemyRenderer.material.color = hitColor;
            isFlashing = true;
            flashTimer = hitFlashDuration;
        }
    }
 
    private void HandleDeath()
    {
        GameEvents.EnemyKilled();
 
        agent.isStopped = true;
        agent.enabled   = false;
 
        Destroy(gameObject, 0.15f);
    }
 
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath          -= HandleDeath;
            healthSystem.OnHealthChanged  -= HandleHit;
        }
    }
 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}