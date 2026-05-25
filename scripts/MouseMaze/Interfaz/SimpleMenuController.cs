using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

public class SimpleMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button[] buttons;
    
    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float navigationCooldown = 0.3f;
    
    private int currentIndex = 0;
    private float lastNavigationTime = 0f;
    private InputAction navigateAction;

    // Gestionar offline de una pantalla
    private void Awake()
    {
        // OFFLINE IA
        if (GameConfig.singleScreenMode)
        {
            string[] objectsToDisable = { "InicioS", "CameraSlime", "DualScreenManager", "Gana_RatonS", "Gana_SlimeS" };
            foreach (string objName in objectsToDisable)
            {
                GameObject obj = GameObject.Find(objName);
                if (obj != null)
                    obj.SetActive(false);
                else
                    Debug.LogWarning($"[SingleScreen] No se encontró el objeto: {objName}");
            }

            // ── Reasignar CameraRaton al Display 1 ──
            GameObject cameraRatonObj = GameObject.Find("CameraRaton");
            if (cameraRatonObj != null)
            {
                Camera cam = cameraRatonObj.GetComponent<Camera>();
                if (cam != null)
                    cam.targetDisplay = 0; // Display 1 en Unity = índice 0
                else
                    Debug.LogWarning("[SingleScreen] CameraRaton no tiene componente Camera!");
            }
            else
            {
                Debug.LogWarning("[SingleScreen] No se encontró el objeto: CameraRaton");
            }

            // ── Reasignar Canvas InicioR al Display 1 ──
            GameObject inicioRObj = GameObject.Find("InicioR");
            if (inicioRObj != null)
            {
                Canvas canvas = inicioRObj.GetComponent<Canvas>();
                if (canvas != null)
                    canvas.targetDisplay = 0; // Display 1 = índice 0
                else
                    Debug.LogWarning("[SingleScreen] InicioR no tiene componente Canvas!");
            }
            else
            {
                Debug.LogWarning("[SingleScreen] No se encontró el objeto: InicioR");
            }

            // ── Reasignar Canvas GanaRatonR al Display 1 ──
            GameObject GanaRatonRObj = GameObject.Find("Gana_RatonR");
            if (GanaRatonRObj != null)
            {
                Canvas canvas = GanaRatonRObj.GetComponent<Canvas>();
                if (canvas != null)
                    canvas.targetDisplay = 0; // Display 1 = índice 0
                else
                    Debug.LogWarning("[SingleScreen] GanaRatonR no tiene componente Canvas!");
            }
            else
            {
                Debug.LogWarning("[SingleScreen] No se encontró el objeto: GanaRatonR");
            }

            // ── Reasignar Canvas GanaSlimeR al Display 1 ──
            GameObject GanaSlimeRObj = GameObject.Find("Gana_SlimeR");
            if (GanaSlimeRObj != null)
            {
                Canvas canvas = GanaSlimeRObj.GetComponent<Canvas>();
                if (canvas != null)
                    canvas.targetDisplay = 0; // Display 1 = índice 0
                else
                    Debug.LogWarning("[SingleScreen] GanaSlimeR no tiene componente Canvas!");
            }
            else
            {
                Debug.LogWarning("[SingleScreen] No se encontró el objeto: GanaSlimeR");
            }
        }

        // ONLINE
        if (PhotonNetwork.IsConnected)
        {
            RolJugador rol = ObtenerRolDesdePhoton();

                if (rol == RolJugador.Slime)
                {
                    // Desactivar objetos del ratón y dual screen
                    string[] objectsToDisable = { "CameraRaton", "DualScreenManager", "Gana_RatonR", "Gana_SlimeR", "Reiniciar" };
                    foreach (string objName in objectsToDisable)
                    {
                        GameObject obj = GameObject.Find(objName);
                        if (obj != null)
                            obj.SetActive(false);
                        else
                            Debug.LogWarning($"[OnlineTemporal-Slime] No se encontró: {objName}");
                    }
                    // El slime ya usa Display 1, no hace falta reasignar cámaras ni canvases
                }
                else
                {
                    // Desactivar objetos del slime y dual screen
                    string[] objectsToDisable = { "CameraSlime", "DualScreenManager", "Gana_RatonS", "Gana_SlimeS", "Reiniciar" };
                    foreach (string objName in objectsToDisable)
                    {
                        GameObject obj = GameObject.Find(objName);
                        if (obj != null)
                            obj.SetActive(false);
                        else
                            Debug.LogWarning($"[OnlineTemporal-Raton] No se encontró: {objName}");
                    }

                    // CameraRaton pasa a Display 1 (índice 0), ya que en la escena dual era Display 2
                    GameObject cameraRatonObj = GameObject.Find("CameraRaton");
                    if (cameraRatonObj != null)
                    {
                        Camera cam = cameraRatonObj.GetComponent<Camera>();
                        if (cam != null) cam.targetDisplay = 0;
                        else Debug.LogWarning("[OnlineTemporal-Raton] CameraRaton no tiene Camera!");
                    }
                    else Debug.LogWarning("[OnlineTemporal-Raton] No se encontró: CameraRaton");

                    // Canvases del ratón también al Display 1
                    string[] canvasNames = { "Gana_RatonR", "Gana_SlimeR" };
                    foreach (string canvasName in canvasNames)
                    {
                        GameObject obj = GameObject.Find(canvasName);
                        if (obj != null)
                        {
                            Canvas canvas = obj.GetComponent<Canvas>();
                            if (canvas != null) canvas.targetDisplay = 0;
                            else Debug.LogWarning($"[OnlineTemporal-Raton] {canvasName} no tiene Canvas!");
                        }
                        else Debug.LogWarning($"[OnlineTemporal-Raton] No se encontró: {canvasName}");
                    }
                }
        }
    }
    

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // Configurar input
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerInput != null) playerInput.SwitchCurrentActionMap("EntrarSalir"); // Cambia el input del jugador por si acaso

        if (playerInput != null)
        {
            navigateAction = playerInput.actions["Navigate"];
        }

        // Seleccionar primer bot�n
        if (buttons.Length > 0)
        {
            currentIndex = 0;
            SelectButton(currentIndex);
        }

        // Configurar EventSystem si no existe
        if (EventSystem.current == null)
        {
            Debug.LogError("No hay EventSystem en la escena!");
        }
    }

    private void Update()
    {
        if (navigateAction != null && Time.time - lastNavigationTime > navigationCooldown)
        {
            HandleNavigation();
        }
    }

    private void HandleNavigation()
    {
        Vector2 input = navigateAction.ReadValue<Vector2>();
        
        // Debug para ver el input
        if (input.magnitude > 0.1f)
        {
            Debug.Log($"Input recibido: {input}");
        }

        if (input.y < -0.5f) // Abajo
        {
            Navigate(1);
            lastNavigationTime = Time.time;
        }
        else if (input.y > 0.5f) // Arriba
        {
            Navigate(-1);
            lastNavigationTime = Time.time;
        }
    }

    private void Navigate(int direction)
    {
        int newIndex = currentIndex + direction;
        
        if (newIndex >= 0 && newIndex < buttons.Length)
        {
            currentIndex = newIndex;
            SelectButton(currentIndex);
            Debug.Log($"Navegando a bot�n: {currentIndex}");
        }
    }

    private void SelectButton(int index)
    {
        // Limpiar y resetear selecci�n
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
        }
        
        buttons[index].Select();
    }

    // Este m�todo se llama autom�ticamente por el Input System
    public void Submit(InputAction.CallbackContext context)
    {
        
        Debug.Log("Bot�n Submit presionado!");
        if (buttons[currentIndex] != null)
        {
            buttons[currentIndex].onClick.Invoke();
        }
        
    }

    // Para debug
    public void OnNavigateDebug(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 value = context.ReadValue<Vector2>();
            Debug.Log($"Navigate action triggered: {value}");
        }
    }

    private RolJugador ObtenerRolDesdePhoton()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
            return RolJugador.SinAsignar;

        object slimeActorObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("SlimePlayerActorID", out slimeActorObj))
        {
            return PhotonNetwork.LocalPlayer.ActorNumber == (int)slimeActorObj
                ? RolJugador.Slime
                : RolJugador.Raton;
        }
        return RolJugador.SinAsignar;
    }

}