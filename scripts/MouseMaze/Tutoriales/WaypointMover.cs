using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointMover : MonoBehaviour
{
    NavMeshAgent agente;
    int indiceActual = -1;
    public List<Transform> targets = new List<Transform>();

    [SerializeField] GameObject warningLight;
    // Start is called before the first frame update
    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //calculamos la distancia que queda por recorrer antes del siguiente punto
        if (agente.remainingDistance < 1)
        {
            //instanciamos un warning light
            Instantiate(warningLight, agente.destination, warningLight.transform.rotation);

            if (indiceActual >= targets.Count - 1)
            {
                indiceActual = 0;
            }
            else
            {
                indiceActual++;
            }

            agente.SetDestination(targets[indiceActual].transform.position);
        }
    }
}
