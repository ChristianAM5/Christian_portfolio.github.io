using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class CollectableController : MonoBehaviour
{
    //Script que controla al player
    public RigidbodyFirstPersonController rigidbodyFirstPersonController;
    private bool collected = false; // evita recogerla dos veces
    public string itemId;
    public GameObject warningLight;
    void Start()
    {
        if (itemId == "tnt")
        {
            rigidbodyFirstPersonController = GameObject.FindGameObjectWithTag("Player").GetComponent<RigidbodyFirstPersonController>();
            rigidbodyFirstPersonController.tntTargetCount++;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!collected && other.CompareTag("Player"))
        {
            collected = true; // marca como recogida
            if (itemId == "flecha")
            {
                rigidbodyFirstPersonController = other.GetComponent<RigidbodyFirstPersonController>();
                rigidbodyFirstPersonController.RecogerFlecha();
            }
            else if (itemId == "tnt")
            {
                rigidbodyFirstPersonController.RecogerTnt();
                Instantiate(warningLight, new Vector3(transform.position.x,16.8f,transform.position.z), warningLight.transform.rotation);
            }
            Destroy(gameObject);
        }
    }
    
}