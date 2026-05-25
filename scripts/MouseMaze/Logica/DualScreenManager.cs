using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DualScreenManager : MonoBehaviour
{
    [Header("Camaras")]
    public Camera mainCamera; //Display 1 SLIME
    public Camera secondCamera; //Display 2 RATON

    [Header("Player Inputs")]
    public PlayerInput playerKeyboardMouse; // Player que usar� teclado + rat�n
    public PlayerInput playerGamepad;      // Player que usar� mando

    void Start()
    {
        StartCoroutine(ConfigureDualScreens());
    }

    IEnumerator ConfigureDualScreens()
    {
        // Esperar dos frames para que Unity inicialice los displays
        yield return new WaitForSeconds(2 * Time.deltaTime);

        Debug.Log($"Displays conectados: {Display.displays.Length}");

        // Activar todos los displays disponibles
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
            Debug.Log($"Display {i+1} activado: {Display.displays[i].systemWidth}x{Display.displays[i].systemHeight}");
        }

        // Configurar c�maras
        if (mainCamera != null)
        {
            mainCamera.targetDisplay = 1; // Se renderiza en la pantalla principal
            //mainCamera.backgroundColor = Color.blue;
        }

        if (secondCamera != null && Display.displays.Length > 1)
        {
            secondCamera.targetDisplay = 0; // Se renderiza en la segunda pantalla
            //secondCamera.backgroundColor = Color.red;

            // Crear un cubo para ver en la segunda pantalla
            /*
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(0, 0, 5);
            cube.name = "SecondScreenCube";*/
        }
        else if (secondCamera != null)
        {
            // Si solo hay una pantalla, desactivar la segunda c�mara
            secondCamera.gameObject.SetActive(false);
            Debug.Log("Solo hay una pantalla disponible");
        }

        // Configurar Input System
        AssignInputs();
    }

    private void AssignInputs()
    {
        // Gamepad al primer PlayerInput
        if (playerGamepad != null)
        {
            if (Gamepad.all.Count > 0)
            {
                playerGamepad.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
            }
            else
            {
                Debug.LogWarning("No se ha detectado ning�n gamepad");
            }
        }

        // Keyboard + Mouse al segundo PlayerInput
        if (playerKeyboardMouse != null)
        {
            var kbMouse = new InputDevice[] { Keyboard.current, Mouse.current };
            playerKeyboardMouse.SwitchCurrentControlScheme("Keyboard Mouse", kbMouse);
        }
    }


    void Update()
    {
        // Tecla para debug
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"Displays activos: {Display.displays.Length}");
            for (int i = 0; i < Display.displays.Length; i++)
            {
                Debug.Log($"Display {i}: {Display.displays[i].active}");
            }
        }

        // A�ade esto al Update del script
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (secondCamera != null)
            {
                // Cambiar color para ver que funciona
                secondCamera.backgroundColor =
                    new Color(Random.value, Random.value, Random.value);
                Debug.Log("Color cambiado en segunda c�mara");
            }
        }
    }
}