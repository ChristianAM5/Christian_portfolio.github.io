using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public PlayerController PlayerController; 
    private bool collected = false; // evita recogerla dos veces

    private void OnTriggerEnter(Collider other) 
    { 
        if (!collected && other.CompareTag("Player")) 
        { 
            collected = true; // marca como recogida
            PlayerController.RecogerFlecha(); 
            Destroy(gameObject); 
        } 
    }
}
