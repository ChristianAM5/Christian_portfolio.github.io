using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SlimeStatus : MonoBehaviour
{
    public float stunDuration = 10f;
    public float blinkInterval = 0.2f; // cada cuánto parpadea

    public bool isStunned = false;
    private NavMeshAgent agent;
    private Renderer slimeRenderer;
    private SlimeKills slimeKills;
    private Animator slimeStun;
	private Collider slimeCollider;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        slimeRenderer = GetComponentInChildren<Renderer>(); // Asume que el mesh está en un hijo
        slimeStun = GetComponent<Animator>();

        slimeKills = GetComponent<SlimeKills>();
		slimeCollider = GetComponent<Collider>();
    }

    public void ApplyStun()
    {
        slimeStun.SetBool("isStunned", true);
        if (!isStunned)
            StartCoroutine(StunCoroutine());
        //slimeStun.SetTrigger("Stun_recover");
    }

    private IEnumerator StunCoroutine()
    {
        isStunned = true;
		// Eliminar collider
		if (slimeCollider != null)
            slimeCollider.enabled = false;
		
        float originalSpeed = agent.speed;
        agent.isStopped = true;
        agent.speed = 0;

        // Coroutine para parpadear mientras dura el stun
        StartCoroutine(BlinkCoroutine());

        yield return new WaitForSeconds(stunDuration);

        slimeStun.SetBool("isStunned", false);
        agent.isStopped = false;
        agent.speed = originalSpeed;
        isStunned = false;

		// Reactivar el collider
        if (slimeCollider != null)
            slimeCollider.enabled = true;

	// Asegurarnos de que el slime quede visible al final
        if (slimeRenderer != null)
            slimeRenderer.enabled = true;
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

    IEnumerator PlayAndFreeze(string triggerName)
    {
        slimeStun.SetTrigger(triggerName);

        // Esperar a que empiece la animación
        yield return null;

        // Obtener info de la animación actual
        AnimatorStateInfo info = slimeStun.GetCurrentAnimatorStateInfo(0);

        // Esperar a que termine
        yield return new WaitForSeconds(info.length);

        // Congelar animación en el último frame
        slimeStun.speed = 0f;
    }
}

