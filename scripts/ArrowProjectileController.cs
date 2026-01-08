using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class ArrowProjectileController : MonoBehaviour
    {
        private bool hasHit = false; // evita múltiples colisiones

        private void OnCollisionEnter(Collision collision)
        {
            // Si golpea al slime
            if (collision.collider.CompareTag("Enemy"))
            {
                SlimeStatus status = collision.collider.GetComponent<SlimeStatus>();
                if (status != null)
                {
                    status.ApplyStun();
                }

                Destroy(gameObject); // destruye la flecha al impactar
            }

            // --- Impacta en un muro ---
            else if ((!hasHit) &&
             (collision.collider.CompareTag("Wall") ||
              collision.collider.CompareTag("Ground") ||
              collision.collider.CompareTag("Ceiling")))
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

            // En el arrowprojectilecontroller debajo de  col.isTrigger = true; en el enumerator
            Collider playerCol = FindObjectOfType<RigidbodyFirstPersonController>().GetComponent<Collider>();
            Physics.IgnoreCollision(col, playerCol, false);

            // Solo añade CollectableController si no existe
            if (GetComponent<CollectableController>() == null)
        {
            CollectableController arrowCtrl = gameObject.AddComponent<CollectableController>();
                arrowCtrl.rigidbodyFirstPersonController = FindObjectOfType<RigidbodyFirstPersonController>();
                arrowCtrl.itemId = "flecha";
        }

            // Desactivamos este script para evitar más colisiones
            this.enabled = false;
        }
    }
}
