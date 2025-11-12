using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float velocidad = 20.0f;
    //public float velocidadGiro = 45.0f;
    public float horizontalInput;
    public float forwardInput;

    public string inputID;

    public GameObject crossbow; // La ballesta 
    public GameObject arrowPrefab; // El proyectil
    public Camera mainCamera; // Flecha desde la ballesta al centro de la camara
    public int arrows = 0; // Número de flechas disponibles 
    public float arrowSpeed = 20.0f;
   

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal" + inputID);
        forwardInput = Input.GetAxis("Vertical" + inputID);
        transform.Translate(Vector3.forward * Time.deltaTime * velocidad * forwardInput);
        transform.Translate(Vector3.right * Time.deltaTime * velocidad * horizontalInput);
        //transform.Rotate(Vector3.up, velocidadGiro * horizontalInput * Time.deltaTime);
	//crossbow.transform.position = transform.position + new Vector3(-0.5f, 0.2f, 0);

	// Si el jugador tiene al menos una flecha, puede disparar 
	if (arrows > 0) 
	// tecla espacio disparar 
	{ 
		crossbow.SetActive(true); 
		if (Input.GetKeyDown(KeyCode.Space)) 
			{ 
			DispararFlecha(); 
			} 
	} 
	if (arrows == 0) 
	{ 
	crossbow.SetActive(false); 
	} 

	}

	private void DispararFlecha() 
	{ 
		arrows--; 
		// Posicion inicial: punta de la ballesta
		Vector3 spawnPos = crossbow.transform.position + crossbow.transform.forward * 0.8f; 

   		// Dirección: desde la ballesta hacia el centro de la cámara
  		Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // centro de la pantalla
  		Vector3 targetDirection = ray.direction;

		// Crear la flecha
		GameObject projectile = Instantiate(arrowPrefab, spawnPos, Quaternion.LookRotation(targetDirection));
    		Rigidbody projRb = projectile.GetComponent<Rigidbody>();
    		projRb.velocity = targetDirection * arrowSpeed; 

	}    

	// Método para recoger una flecha (se llama desde el powerup) 
	public void RecogerFlecha() 
	{ 
	arrows++; 
	}

}
