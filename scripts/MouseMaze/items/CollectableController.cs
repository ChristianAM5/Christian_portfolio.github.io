using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Photon.Pun;

public class CollectableController : MonoBehaviourPunCallbacks
{
    private bool collected = false;
    public string itemId;
    public GameObject warningLight;
    private RigidbodyFirstPersonController RigidbodyFirstPersonController;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            RigidbodyFirstPersonController = other.gameObject.GetComponent<RigidbodyFirstPersonController>();

            if (RigidbodyFirstPersonController == null) return;

            // Verificar que sea el dueño del personaje el que ejecuta esto
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && !pv.IsMine) return;

            collected = true;

            if (itemId == "flecha")
            {
                RigidbodyFirstPersonController.RecogerFlecha();
            }
            else if (itemId == "tnt")
            {
                if (PhotonNetwork.IsConnected) // ONLINE
                {
                    if (GameManager_Network.Instance != null && GameManager_Network.Instance.photonView != null)
                        GameManager_Network.Instance.photonView.RPC("ActualizarTNTUI", RpcTarget.AllBuffered);
                    else
                        Debug.LogError("Falta el GameManager o el PhotonView en la escena.");
                }
                else
                {
                    // Offline: llamada directa al jugador
                    RigidbodyFirstPersonController.RecogerTnt();
                }

                // --- CORRECCIÓN LUZ DE ADVERTENCIA ---
                if (warningLight != null)
                {
                    Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);

                    if (PhotonNetwork.IsConnected) // ONLINE
                    {
                        // IMPORTANTE: Ya no comprobamos si es MasterClient. 
                        // El jugador que coge la dinamita instancia la luz para todos.
                        PhotonNetwork.Instantiate("WarningLight Variant", spawnPos, Quaternion.identity);
                    }
                    else // OFFLINE
                    {
                        Instantiate(warningLight, spawnPos, Quaternion.identity);
                    }
                }
            }

            // --- LÓGICA DE DESTRUCCIÓN ---
            if (!PhotonNetwork.IsConnected)
            {
                Destroy(gameObject);
            }
            else if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                var sm = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
                if (sm != null)
                {
                    sm.photonView.RPC("RPC_DestruirObjeto", photonView.Owner, photonView.ViewID);
                }
            }
        }
    }
}