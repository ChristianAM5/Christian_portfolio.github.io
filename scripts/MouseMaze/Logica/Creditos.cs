using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Creditos : MonoBehaviour
{

    void Start()
    {
        Invoke("WaitForEnd", 10);
    }


    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SceneManager.LoadScene("Master_MainMenu");
        }

        if (Gamepad.current != null && (Gamepad.current.startButton.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame))
        {
            SceneManager.LoadScene("Master_MainMenu");
        }
    }


    public void WaitToEnd()
    {
        SceneManager.LoadScene("Testeo_Inicio");
    }
}
