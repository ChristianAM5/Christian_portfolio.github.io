using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Photon.Pun;

public class SlimeStatus : MonoBehaviourPun, IPunObservable
{
    public float stunDuration = 10f;
    public float blinkInterval = 0.2f;

    // Variable sincronizada
    public bool isStunned = false;

    // Para sincronización
    public bool stunStateChanged = false;
    public float stunTimer = 0f;
    private NavMeshAgent agent;
    private Renderer slimeRenderer;
    private SlimeKills slimeKills;
    private Animator slimeStun;
    private SlimeDash slimeDash; // Referencia al componente de dash
    private Collider slimeCollider;

    void Start()
    {
        // Obtener referencias locales (cada cliente obtiene SUS componentes)
        agent = GetComponent<NavMeshAgent>();
        slimeRenderer = GetComponentInChildren<Renderer>();
        slimeStun = GetComponent<Animator>();
        slimeKills = GetComponent<SlimeKills>();
        slimeDash = GetComponent<SlimeDash>(); // Obtener referencia
        slimeCollider = GetComponent<Collider>();
    }

    void Update()
    {
        bool isMine = !PhotonNetwork.IsConnected || photonView.IsMine;
        // Solo el dueño del slime maneja el temporizador
        if (isMine && isStunned)
        {
            stunTimer += Time.deltaTime;
            if (stunTimer >= stunDuration)
            {
                    if (PhotonNetwork.IsConnected)
                        photonView.RPC("RPC_EndStun", RpcTarget.All);
                    else
                        RPC_EndStun(); // directo en offline
            }
        }

        // El parpadeo lo maneja cada cliente localmente
        // (no necesita red, solo es visual)
        if (isStunned && slimeRenderer != null)
        {
            // El parpadeo ya lo maneja la corrutina
        }
    }

    // Método público para aplicar aturdimiento (lo llama la flecha)
    public void ApplyStun()
    {
        if (PhotonNetwork.IsConnected)
            photonView.RPC("RPC_ApplyStun", RpcTarget.All);
        else
            RPC_ApplyStun(); // llamada directa en offline
    }

    [PunRPC]
    public void RPC_ApplyStun()
    {
        // Si ya está aturdido, no hacer nada
        if (isStunned) return;

        // Detener corrutinas anteriores si existen
        StopAllCoroutines();

        // Iniciar aturdimiento
        StartCoroutine(StunCoroutine());
    }

    [PunRPC]
    public void RPC_EndStun()
    {
        StopAllCoroutines();
        StartCoroutine(EndStunCoroutine());
    }

    private IEnumerator StunCoroutine()
    {
        isStunned = true;
        stunTimer = 0f;

        // Animación
        if (slimeStun != null)
            slimeStun.SetBool("isStunned", true);

        // Detener dash si existe (solo si soy dueño)
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            if (slimeDash != null)
            {
                slimeDash.StopDash();
            }

            // Detener el agente
            if (agent != null)
            {
                agent.isStopped = true;
                agent.speed = 0;
            }
        }

        // Ahora sí guardamos la velocidad correcta (ya restaurada por StopDash)
        float originalSpeed = agent.speed;
        agent.isStopped = true;
        agent.speed = 0;
        if (slimeCollider != null) slimeCollider.enabled = false;

        StartCoroutine(BlinkCoroutine());

        // El dueño maneja el tiempo, los demás solo esperan
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            // Los clientes no dueños esperan a que el dueño termine
            yield break;
        }

        // El dueño espera el tiempo y luego llama al RPC para terminar
        // Esto ya se maneja en Update()
    }

    private IEnumerator EndStunCoroutine()
    {
        isStunned = false;

        // Animación
        if (slimeStun != null)
            slimeStun.SetBool("isStunned", false);

        // Restaurar renderer
        if (slimeCollider != null) slimeCollider.enabled = true;
        if (slimeRenderer != null)
            slimeRenderer.enabled = true;

        // Restaurar movimiento (solo si soy dueño)
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            if (agent != null)
            {
                agent.isStopped = false;
                // Restaurar velocidad original (deberías guardarla)
                agent.speed = 3.5f; // O la velocidad que tenga por defecto
            }
        }

        yield break;
    }

    private IEnumerator BlinkCoroutine()
    {
        while (isStunned)
        {
            if (slimeRenderer != null)
            {
                slimeRenderer.enabled = !slimeRenderer.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // Sincronizar el estado de aturdimiento
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // El dueño escribe
            stream.SendNext(isStunned);
            stream.SendNext(stunTimer);
        }
        else
        {
            // Los demás reciben
            bool newStunState = (bool)stream.ReceiveNext();
            float newStunTimer = (float)stream.ReceiveNext();

            // Si el estado cambió, actualizar
            if (newStunState != isStunned)
            {
                if (newStunState)
                {
                    StartCoroutine(StunCoroutine());
                }
                else
                {
                    StartCoroutine(EndStunCoroutine());
                }
            }

            // Sincronizar timer para clientes no dueños
            if (!photonView.IsMine)
            {
                stunTimer = newStunTimer;
            }

        }
    }

    // Método para verificar si está aturdido (útil para otros scripts)
    public bool IsStunned()
    {
        return isStunned;
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}