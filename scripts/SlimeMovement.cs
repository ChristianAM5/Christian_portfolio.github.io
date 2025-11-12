using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SlimeMovement : MonoBehaviour
{
    Camera cam;
    NavMeshAgent agent;
    public LayerMask ground;

    private bool isStunned = false; // Paralizar movimiento cuando le golpee una flecha
    
    void Start()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }

    
    void Update()
    {
	// Si está paralizado, no procesamos movimiento
        if (isStunned) return;


        //Comprobamos si el jugador está clickando el botón
        if(Input.GetMouseButtonDown(1))
        {
            //Lo está clickando, así que creamos un raycast....
            RaycastHit hit;
            //... para lanzarlo desde la cámara hasta la posición del cursor ("mousePosition")
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            //Comprobamos si nuestro raycast está golpeando algo (parámetro "ray"), luego si ese algo está en la capa ground (parámetro "ground")
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                //Nos llevamos el agent hasta la posición que ha alcanzado el hit ("hit.point")
                agent.SetDestination(hit.point);
            }
        }
    }

	// Método público para aplicar el "stun"

    public IEnumerator Stun(float duration)
    {
        if (isStunned) yield break; // evita solaparse si recibe varios golpes
        isStunned = true;

        // Guardamos la velocidad actual para restaurarla luego
        float originalSpeed = agent.speed;
        agent.isStopped = true; // Detiene el NavMeshAgent
        agent.speed = 0;

        // Esperamos el tiempo de parálisis
        yield return new WaitForSeconds(duration);

        // Reactivamos el movimiento
        agent.isStopped = false;
        agent.speed = originalSpeed;
        isStunned = false;
    }
}
