using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowProjectileController : MonoBehaviour
{
    private bool hasHit = false; // evita múltiples colisiones

    private void OnCollisionEnter(Collision collision)
    {
        // Si golpea al slime
        if (collision.collider.CompareTag("Enemy"))
        {
            SlimeMovement slime = collision.collider.GetComponent<SlimeMovement>();
            if (slime != null)
            {
                slime.StartCoroutine(slime.Stun(5f)); // paraliza 5 segundos
            }

            Destroy(gameObject); // destruye la flecha al impactar
        }
	
	// --- Impacta en un muro ---
        else if (collision.collider.CompareTag("Wall") && !hasHit)
        {
            hasHit = true;

            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero; // detiene el movimiento
            rb.isKinematic = true;      // desactiva la física

	    // Ajustar orientación con el ángulo del impacto
            ContactPoint contact = collision.contacts[0];
            transform.rotation = Quaternion.LookRotation(-contact.normal);

	    // Colocarla justo en el punto de contacto (ligeramente hundida)
            transform.position = contact.point + (-contact.normal * 0.05f);

	    // Fijar la flecha al muro
            transform.SetParent(collision.collider.transform);

	    // Espera un frame antes de activar el trigger y hacerlo recogible
            StartCoroutine(EnablePickupNextFrame());
        }
    }

        private IEnumerator EnablePickupNextFrame()
    {
        yield return null; // espera 1 frame

        // Ahora hacemos el collider trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Solo añade ArrowController si no existe
        if (GetComponent<ArrowController>() == null)
        {
            ArrowController arrowCtrl = gameObject.AddComponent<ArrowController>();
            arrowCtrl.PlayerController = FindObjectOfType<PlayerController>();
        }

        // Desactivamos este script para evitar más colisiones
        this.enabled = false;
    }
}

