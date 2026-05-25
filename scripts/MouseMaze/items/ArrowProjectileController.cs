using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Photon.Pun;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class ArrowProjectileController : MonoBehaviourPun, IPunOwnershipCallbacks
    {
        private bool hasHit = false;
        private PhotonView pv;

        private void Awake()
        {
            pv = GetComponent<PhotonView>();
            // Registrar para callbacks de ownership
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            // Limpiar callbacks
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void FixedUpdate()
        {
            if (hasHit) return;
            if (pv != null && !pv.IsMine) return;

            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 velocity = rb.velocity;
            float distancia = velocity.magnitude * Time.fixedDeltaTime * 3f;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, velocity.normalized, out hit, distancia))
                ProcesarImpacto(hit.collider, hit.point, hit.normal);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasHit) return;
            if (pv != null && !pv.IsMine) return;

            ContactPoint contact = collision.contacts[0];
            ProcesarImpacto(collision.collider, contact.point, contact.normal);
        }

        private void ProcesarImpacto(Collider collider, Vector3 point, Vector3 normal)
        {
            if (collider.CompareTag("Enemy"))
            {
                SlimeStatus status = collider.GetComponent<SlimeStatus>();
                if (status != null) status.ApplyStun();

                if (PhotonNetwork.IsConnected)
                    PhotonNetwork.Destroy(gameObject);
                else
                    Destroy(gameObject);
                return;
            }

            // Online: si golpea a otro jugador, destruir la flecha
            if (collider.CompareTag("Player"))
            {
                if (PhotonNetwork.IsConnected)
                    PhotonNetwork.Destroy(gameObject);
                else
                    Destroy(gameObject);
                return;
            }

            if (!hasHit && (collider.CompareTag("Wall") || collider.CompareTag("Ground") || collider.CompareTag("Ceiling")))
            {
                hasHit = true;

                Vector3 hitPoint = point + normal * 0.05f;
                Quaternion hitRotation = Quaternion.LookRotation(-normal);

                FijarFlecha(hitPoint, hitRotation);

                if (PhotonNetwork.IsConnected && pv.IsMine)
                    pv.RPC("RPC_FijarFlecha", RpcTarget.OthersBuffered, hitPoint, hitRotation);

                StartCoroutine(EnablePickupNextFrame());
            }
        }

        // RPC para sincronizar la flecha fijada en los demás clientes
        [PunRPC]
        private void RPC_FijarFlecha(Vector3 position, Quaternion rotation)
        {
            hasHit = true;
            FijarFlecha(position, rotation);
            StartCoroutine(EnablePickupNextFrame());
        }

        // Lógica compartida para fijar la flecha (local y remoto)
        private void FijarFlecha(Vector3 position, Quaternion rotation)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            // Ajustar orientación y posición
            transform.position = position;
            transform.rotation = rotation;
        }

        private IEnumerator EnablePickupNextFrame()
        {
            yield return null; // espera 1 frame

            // Hacer trigger
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;

            // Re-habilitar colisión con jugadores
            RigidbodyFirstPersonController[] allPlayers = FindObjectsOfType<RigidbodyFirstPersonController>();
            foreach (var player in allPlayers)
            {
                Collider playerCol = player.GetComponent<Collider>();
                if (playerCol != null)
                {
                    Physics.IgnoreCollision(col, playerCol, false);
                }
            }

            // CAMBIO IMPORTANTE: Ya no asignamos rigidbodyFirstPersonController
            // porque CollectableController ahora lo busca automáticamente
            if (GetComponent<CollectableController>() == null)
            {
                CollectableController arrowCtrl = gameObject.AddComponent<CollectableController>();
                arrowCtrl.itemId = "flecha";
            }

            // Desactivar este script
            this.enabled = false;
        }

        // Callbacks de ownership (opcional, para debug)
        public void OnOwnershipRequest(PhotonView targetView, Photon.Realtime.Player requestingPlayer)
        {
            // Lógica para aceptar/rechazar transferencia
        }

        public void OnOwnershipTransfered(PhotonView targetView, Photon.Realtime.Player previousOwner)
        {
            Debug.Log($"Flecha ahora es de {targetView.Owner}");
        }

        public void OnOwnershipTransferFailed(PhotonView targetView, Photon.Realtime.Player senderOfFailedRequest)
        {
            // Si falla, intentar de nuevo o destruir
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}