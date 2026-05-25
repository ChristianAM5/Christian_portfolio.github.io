using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class ExitLevelController : MonoBehaviourPunCallbacks
    {
        public int level;

        // Ratones que están físicamente dentro del collider ahora mismo
        private HashSet<RigidbodyFirstPersonController> playersInZone 
            = new HashSet<RigidbodyFirstPersonController>();

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            var player = other.GetComponent<RigidbodyFirstPersonController>();
            if (player != null && !player.isDead)
            {
                playersInZone.Add(player);
                CheckWinCondition();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            var player = other.GetComponent<RigidbodyFirstPersonController>();
            if (player != null)
                playersInZone.Remove(player);
        }

        void CheckWinCondition()
        {
            // Busca todos los ratones en escena
            var allPlayers = FindObjectsOfType<RigidbodyFirstPersonController>();

            int alivePlayers = 0;
            int alivePlayersInZone = 0;

            foreach (var p in allPlayers)
            {
                if (p.isDead) continue;
                alivePlayers++;
                if (playersInZone.Contains(p)) alivePlayersInZone++;
            }

            if (alivePlayers == 0) return; // Todos muertos, no hay victoria de ratones

            // ¿Están TODOS los ratones vivos dentro de la zona?
            if (alivePlayersInZone < alivePlayers)
            {
                Debug.Log($"[Exit] {alivePlayersInZone}/{alivePlayers} ratones en zona. Esperando...");
                return;
            }

            // TNTs recogidas globales
            int tntRecogidas = PhotonNetwork.IsConnected
                ? GameManager_Network.Instance.tntGlobal    // online: variable global compartida
                : allPlayers[0].tnt;                        // offline: el único jugador

            if (tntRecogidas < SpawnManager.totalTNTsEnMapa)
            {
                Debug.Log($"[Exit] TNTs: {tntRecogidas}/{SpawnManager.totalTNTsEnMapa}. Faltan TNTs.");
                return;
            }

            Debug.Log("[Exit] ¡Victoria ratones!");

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("RPC_PedirCargaEscena", RpcTarget.MasterClient, "Gana_Raton");
            }
            else // OFFLINE
            {
                SceneManager.LoadScene("Gana_Raton");
            }
        }
        
        [PunRPC]
        void RPC_PedirCargaEscena(string escena)
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(escena);
        }
    }
}