using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Opciones : MonoBehaviourPunCallbacks
{
    public void Reiniciar()
    {
        Debug.Log("Reiniciar Laberinto");
        if (GameConfig.singleScreenMode)
            SceneManager.LoadScene("Nivel_1_Friendless");
        else
            SceneManager.LoadScene("Nivel_1");
    }

    public void Salir()
    {
        Debug.Log("Salir al inicio");

        // Limpiar el chat antes de salir de la escena
        CtrlChat chat = FindObjectOfType<CtrlChat>();
        if (chat != null)
        {
            Debug.Log("🧹 [Opciones] Limpiando CtrlChat antes de salir");
            chat.LimpiarCompletamente();
        }

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }
        else if (GameConfig.singleScreenMode)
        {
            SceneManager.LoadScene("Master_MainMenu");
        }
        else
            SceneManager.LoadScene("MainMenu");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Loby");
    }
}