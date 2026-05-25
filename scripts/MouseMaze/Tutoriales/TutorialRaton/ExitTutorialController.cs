using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class ExitTutorialController : MonoBehaviour
    {
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

            // TNTs recogidas globales
            int tntRecogidas = allPlayers[0].tnt;
            if (tntRecogidas < SpawnManager.totalTNTsEnMapa)
            {
                Debug.Log($"[Exit] TNTs: {tntRecogidas}/{SpawnManager.totalTNTsEnMapa}. Faltan TNTs.");
                return;
            }
            //Si se añade pantalla de tutoriales, cambiar la línea de abajo
            SceneManager.LoadScene("Menu_Tutoriales");
        }
    }
}