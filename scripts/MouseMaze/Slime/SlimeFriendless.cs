using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SlimeFriendless : MonoBehaviour
{
    [Header("Behaviour Mode")]
    [Tooltip("True = siempre persigue al jugador. False = patrulla + persecución.")]
    public bool alwaysChase = false;

    [Header("Movement")]
    public float patrolSpeed    = 3.5f;
    public float chaseSpeed     = 6f;
    public float visionRange    = 8f;
    public float waypointRadius = 1f;   // distancia para considerar que llegó al waypoint

    [Header("Patrol Waypoints")]
    [Tooltip("Arrastra aquí los GameObjects vacíos que marcan la ruta.")]
    public Transform[] waypoints;

    [Header("Dash")]
    public bool canUseDash = true;
    [Tooltip("Cada cuántos segundos (aprox) intentará hacer dash mientras persigue.")]
    public float dashCheckInterval = 3f;

    [Header("References")]
    [Tooltip("Arrastra aquí el Transform del jugador.")]
    public Transform player;

    private NavMeshAgent agent;
    private SlimeDash    slimeDash;
    private Animator     anim;

    private enum State { Patrol, Chase, ReturnToPatrol }
    private State state = State.Patrol;

    private int   currentWaypointIndex = 0;
    private float dashTimer            = 0f;

    void Start()
    {
        agent     = GetComponent<NavMeshAgent>();
        slimeDash = GetComponent<SlimeDash>();
        anim      = GetComponent<Animator>();

        // Si no se asignó jugador, búscalo por tag
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (alwaysChase)
        {
            state = State.Chase;
            agent.speed = chaseSpeed;
        }
        else
        {
            state = State.Patrol;
            agent.speed = patrolSpeed;
            GoToCurrentWaypoint();
        }
    }

    void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol:
                UpdatePatrol(distToPlayer);
                break;

            case State.Chase:
                UpdateChase(distToPlayer);
                break;

            case State.ReturnToPatrol:
                UpdateReturnToPatrol(distToPlayer);
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  ESTADOS
    // ═══════════════════════════════════════════════════════════

    // ── 1. PATRULLA ─────────────────────────────────────────────
    void UpdatePatrol(float distToPlayer)
    {
        // ¿Jugador en rango? → perseguir
        if (distToPlayer <= visionRange)
        {
            EnterChase();
            return;
        }

        // ¿Llegó al waypoint? → siguiente
        if (!agent.pathPending && agent.remainingDistance <= waypointRadius)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            GoToCurrentWaypoint();
        }
    }

    // ── 2. PERSECUCIÓN ──────────────────────────────────────────
    void UpdateChase(float distToPlayer)
    {
        // Modo "siempre sigue": nunca sale de Chase
        if (!alwaysChase && distToPlayer > visionRange)
        {
            EnterReturnToPatrol();
            return;
        }

        agent.SetDestination(player.position);

        // Lógica de dash
        if (canUseDash && slimeDash != null)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashCheckInterval)
            {
                dashTimer = 0f;
                if (slimeDash.CanDash())
                    TriggerAIDash();
            }
        }
    }

    // ── 3. VOLVER A PATRULLA ────────────────────────────────────
    void UpdateReturnToPatrol(float distToPlayer)
    {
        // Si el jugador vuelve a acercarse, perseguir de nuevo
        if (distToPlayer <= visionRange)
        {
            EnterChase();
            return;
        }

        // ¿Llegó al waypoint más cercano?
        if (!agent.pathPending && agent.remainingDistance <= waypointRadius)
        {
            state = State.Patrol;
            agent.speed = patrolSpeed;
            GoToCurrentWaypoint();
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  TRANSICIONES
    // ═══════════════════════════════════════════════════════════
    void EnterChase()
    {
        state = State.Chase;
        agent.speed = chaseSpeed;
        dashTimer = 0f;
        agent.SetDestination(player.position);
    }

    void EnterReturnToPatrol()
    {
        state = State.ReturnToPatrol;
        agent.speed = patrolSpeed;
        currentWaypointIndex = GetNearestWaypointIndex();
        GoToCurrentWaypoint();
    }

    // ═══════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════
    void GoToCurrentWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    int GetNearestWaypointIndex()
    {
        int   nearest = 0;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float d = Vector3.Distance(transform.position, waypoints[i].position);
            if (d < minDist)
            {
                minDist = d;
                nearest = i;
            }
        }
        return nearest;
    }

    void TriggerAIDash()
    {
        // Llama al mismo método interno de SlimeDash usando reflexión
        // para no alterar su script original
        var method = slimeDash.GetType()
                               .GetMethod("StartDash",
                                          System.Reflection.BindingFlags.NonPublic |
                                          System.Reflection.BindingFlags.Instance);
        method?.Invoke(slimeDash, null);
    }

    // ═══════════════════════════════════════════════════════════
    //  GIZMOS (visualizar rango en el editor)
    // ═══════════════════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            Gizmos.DrawLine(waypoints[i].position,
                            waypoints[(i + 1) % waypoints.Length].position);
        }
    }
}
