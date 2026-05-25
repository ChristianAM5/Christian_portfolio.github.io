using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class SlimeDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDistance = 10f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 2f;

    [Header("References")]
    private NavMeshAgent agent;
    private bool canDash = true;
    private Animator walkSpeed;

    // Variables para guardar valores originales
    private float originalSpeed;
    private float originalAcceleration;
    private float originalWalkSpeed;
    private bool isDashing = false;
    private Coroutine dashCoroutine;

    // Componentes de Photon
    private PhotonView photonView;
    private bool isMultiplayer = false;

    // Para saber a qu� jugador pertenece este slime
    [HideInInspector]
    public int ownerPlayerId = -1; // -1 = sin due�o, 0 = Jugador A, 1 = Jugador B

    void Awake()
    {
        // Detectar modo multijugador
        isMultiplayer = PhotonNetwork.IsConnected;
        photonView = GetComponent<PhotonView>();

        // En online, el ownerPlayerId lo determina Photon
        if (isMultiplayer && photonView != null)
        {
            ownerPlayerId = photonView.Owner.ActorNumber;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        walkSpeed = GetComponent<Animator>();

        // Guardar valores originales al inicio
        if (agent != null)
        {
            originalSpeed = agent.speed;
            originalAcceleration = agent.acceleration;
        }

        if (walkSpeed != null)
        {
            originalWalkSpeed = walkSpeed.speed;
        }
    }

    void Update()
    {
        // Verificar si podemos procesar input para este slime
        if (ShouldProcessInput())
        {
            // Input de dash (ESPACIO)
            if (canDash && Input.GetKeyDown(KeyCode.Space))
            {
                RequestDash();
            }
        }
    }

    /// <summary>
    /// Determina si este slime debe procesar input del jugador
    /// </summary>
    private bool ShouldProcessInput()
    {
        // MODO OFFLINE (pantalla dividida)
        if (!isMultiplayer)
        {
            // En offline, usamos SlimeSelectionManager como antes
            return (SlimeSelectionManager.Instance != null &&
                    SlimeSelectionManager.Instance.unitsSelected.Contains(gameObject));
        }

        // MODO ONLINE
        else
        {
            // Solo procesar input si:
            // 1. Soy el due�o de este slime (photonView.IsMine)
            // 2. Y estoy seleccionado (usando tu sistema de selecci�n)
            if (photonView != null && photonView.IsMine)
            {
            return (SlimeSelectionManager.Instance != null &&
                SlimeSelectionManager.Instance.unitsSelected.Contains(gameObject)); // dashean todos antes
            }
            return false;
        }
    }

    /// <summary>
    /// Solicita iniciar un dash (maneja la l�gica de red)
    /// </summary>
    public void RequestDash()
    {
        // MODO ONLINE: Usar RPC para sincronizar
        if (isMultiplayer && photonView != null)
        {
            photonView.RPC("RPC_StartDash", RpcTarget.All);
        }
        // MODO OFFLINE: Llamar directamente
        else
        {
            StartDash();
        }
    }

    [PunRPC]
    private void RPC_StartDash()
    {
        StartDash();
    }

    /// <summary>
    /// Inicia el dash (l�gica com�n)
    /// </summary>
    private void StartDash()
    {
        // Si ya est� en dash o no puede, salir
        if (!canDash || isDashing) return;

        Vector3 dashDirection = transform.forward;

        // Usar direcci�n de movimiento si existe
        if (agent != null && agent.velocity.magnitude > 0.1f)
        {
            dashDirection = agent.velocity.normalized;
        }

        Vector3 dashDestination = transform.position + dashDirection * dashDistance;

        // Validar destino en NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dashDestination, out hit, dashDistance, NavMesh.AllAreas))
        {
            dashDestination = hit.position;
        }

        // Establecer destino (solo si somos due�os en online, o siempre en offline)
        if (agent != null)
        {
            if (!isMultiplayer || (isMultiplayer && photonView.IsMine))
            {
                agent.SetDestination(dashDestination);
            }
        }

        // Iniciar corrutina de dash
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
        }
        dashCoroutine = StartCoroutine(DashSpeedBoost());
    }

    IEnumerator DashSpeedBoost()
    {
        canDash = false;
        isDashing = true;

        // Aplicar efectos de dash (TODOS los ven)
        ApplyDashEffects(true);

        // Esperar duraci�n del dash
        yield return new WaitForSeconds(dashDuration);

        // Restaurar velocidad original
        RestoreOriginalSpeed();
        isDashing = false;

        // Esperar cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
        dashCoroutine = null;
    }

    /// <summary>
    /// Aplica los efectos visuales y de movimiento del dash
    /// </summary>
    private void ApplyDashEffects(bool startDash)
    {
        if (startDash)
        {
            // Aumentar velocidad para el dash
            if (agent != null)
            {
                agent.speed = dashSpeed;
                agent.acceleration = dashSpeed * 2;
            }

            if (walkSpeed != null)
            {
                walkSpeed.speed = dashSpeed;
            }

            // Aqu� podr�as a�adir efectos visuales (part�culas, sonidos)
            // que TODOS los jugadores deben ver
            if (isMultiplayer)
            {
                // Por ejemplo, activar un sistema de part�culas
                // particleSystem.Play();
            }
        }
        else
        {
            RestoreOriginalSpeed();
        }
    }

    /// <summary>
    /// M�todo p�blico para detener el dash externamente (desde SlimeStatus)
    /// Ahora con soporte para red
    /// </summary>
    public void StopDash()
    {
        // MODO ONLINE: Sincronizar la detenci�n
        if (isMultiplayer && photonView != null)
        {
            photonView.RPC("RPC_StopDash", RpcTarget.All);
        }
        // MODO OFFLINE: Llamar directamente
        else
        {
            InternalStopDash();
        }
    }

    [PunRPC]
    private void RPC_StopDash()
    {
        InternalStopDash();
    }

    private void InternalStopDash()
    {
        if (isDashing && dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            RestoreOriginalSpeed();
            isDashing = false;
            dashCoroutine = null;

            // Iniciar cooldown despu�s de ser interrumpido
            StartCoroutine(DashCooldownAfterInterrupt());
        }
    }

    private IEnumerator DashCooldownAfterInterrupt()
    {
        canDash = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void RestoreOriginalSpeed()
    {
        if (agent != null)
        {
            agent.speed = originalSpeed;
            agent.acceleration = originalAcceleration;
        }

        if (walkSpeed != null)
        {
            walkSpeed.speed = originalWalkSpeed;
        }
    }

    public bool CanDash()
    {
        return canDash;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    /// <summary>
    /// Para identificar qu� jugador controla este slime en online
    /// </summary>
    public void SetOwnerPlayerId(int playerId)
    {
        ownerPlayerId = playerId;
    }
}