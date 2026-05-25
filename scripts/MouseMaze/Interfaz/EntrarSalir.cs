using UnityEngine;
using UnityEngine.SceneManagement;

public class EntrarSalir : MonoBehaviour
{
    public void Empezar()
    {
        // si esta en una pantalla llevar a animcacion inicial
        if (GameConfig.singleScreenMode)
            SceneManager.LoadScene("Animacion_Inicial");
        else
            SceneManager.LoadScene("LobbyScene");
    }

    public void Salir()
    {
        SceneManager.LoadScene("Master_MainMenu");
    }

    public void Creditos()
    {
        SceneManager.LoadScene("Creditos");
    }
}
