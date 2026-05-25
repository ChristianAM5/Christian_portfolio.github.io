using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class SlimeSelectionManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static SlimeSelectionManager Instance { get; set; }

    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();

    public LayerMask slime;
    public LayerMask ground;
    public GameObject groundMarker;
    public bool allowMultiselect;
    public Camera cam;

    // private PhotonView photonView; lo hereda de puncallbacks
    private bool esJugadorSlime = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // photonView = GetComponent<PhotonView>(); lo hereda de puncallbacks
        // En Online el script se desactiva solo
        esJugadorSlime = !PhotonNetwork.IsConnected || (photonView != null && photonView.IsMine);

        if (!esJugadorSlime)
        {
            Debug.Log("No soy el jugador slime, desactivando SlimeSelectionManager");
            enabled = false;
            return;
        }

        Debug.Log("SOY EL JUGADOR SLIME");
	
    // FORZAR USO SOLO DE TECLADO + RATÓN
    PlayerInput playerInputComp = GetComponent<PlayerInput>();
    if (playerInputComp != null)
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        var mouse = UnityEngine.InputSystem.Mouse.current;
        
        if (keyboard != null && mouse != null)
        {
            // Quitar todos los dispositivos
            playerInputComp.user.UnpairDevices();
            
            // Emparejar solo teclado y ratón (método correcto)
            UnityEngine.InputSystem.Users.InputUser.PerformPairingWithDevice(keyboard, playerInputComp.user);
            UnityEngine.InputSystem.Users.InputUser.PerformPairingWithDevice(mouse, playerInputComp.user);
            
            Debug.Log($"Slime emparejado con teclado: {keyboard.name} y ratón: {mouse.name}");
        }
        else
        {
            Debug.LogError("No se encontró teclado o ratón");
        }
        
        playerInputComp.neverAutoSwitchControlSchemes = true;
    }

        // BÚSQUEDA MEJORADA DE LA CÁMARA
        if (cam == null)
        {
            // Buscar MainCamera_B (incluyendo inactivas)
            Camera[] allCams = FindObjectsOfType<Camera>(true);
            
            foreach (Camera c in allCams)
            {
                // Buscar específicamente MainCamera_B
                if (c.name == "MainCamera_B")
                {
                    cam = c;
                    Debug.Log($"Cámara encontrada: {c.name}");
                    break;
                }
            }
            
            // Si no se encuentra, buscar cualquier cámara que no sea de jugador
            if (cam == null)
            {
                foreach (Camera c in allCams)
                {
                    if (c.transform.parent == null || !c.transform.parent.name.Contains("Player"))
                    {
                        cam = c;
                        Debug.Log($"Usando cámara alternativa: {c.name}");
                        break;
                    }
                }
            }
            
            if (cam == null)
            {
                Debug.LogError("NO SE ENCONTRÓ CÁMARA PARA EL SLIME");
            }
        }
        else
        {
            Debug.Log($"Cámara ya asignada: {cam.name}");
        }
    }

    public void SelectUnit(InputAction.CallbackContext callbackContext)
    {
        if (!esJugadorSlime || !callbackContext.performed) return;

        if (cam == null)
        {
            Debug.LogError(" Cámara es null en SelectUnit");
            return;
        }

        Debug.Log("🖱️ Intentando seleccionar slime...");
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, slime))
        {
            Debug.Log($"Slime golpeado: {hit.collider.name}");
            
            if (allowMultiselect)
            {
                MultiSelect(hit.collider.gameObject);
            }
            else
            {
                SelectByClicking(hit.collider.gameObject);
            }
        }
        else
        {
            Debug.Log("Raycast no golpeó ningún slime (layer correcto?)");
            if (!allowMultiselect)
            {
                DeselectAll();
            }
        }
    }

    public void AllowMultiselect(InputAction.CallbackContext callbackContext)
    {
        if (!esJugadorSlime) return;

        if (callbackContext.performed)
        {
            allowMultiselect = true;
            Debug.Log("Multiselect activo");
        }
        else
        {
            allowMultiselect = false;
            Debug.Log("Multiselect inactivo");
        }
    }

    void MultiSelect(GameObject unit)
    {
        if (unit.CompareTag("Enemy"))
        {
            if (!unitsSelected.Contains(unit))
            {
                unitsSelected.Add(unit);
                TriggerSelectionIndicator(unit, true);
                EnableUnitMovement(unit, true);
            }
            else
            {
                EnableUnitMovement(unit, false);
                TriggerSelectionIndicator(unit, false);
                unitsSelected.Remove(unit);
            }
        }
    }

    void DeselectAll()
    {
        foreach (var unit in unitsSelected)
        {
            EnableUnitMovement(unit, false);
            TriggerSelectionIndicator(unit, false);
        }
        unitsSelected.Clear();
    }

    void SelectByClicking(GameObject unit)
    {
        DeselectAll();

        if (unit.CompareTag("Enemy"))
        {
            unitsSelected.Add(unit);
            TriggerSelectionIndicator(unit, true);
            EnableUnitMovement(unit, true);
        }
    }

    void EnableUnitMovement(GameObject unit, bool shouldMove)
    {
        SlimeMovement movement = unit.GetComponent<SlimeMovement>();
        if (movement != null)
        {
            movement.enabled = shouldMove;
        }
    }

    void TriggerSelectionIndicator(GameObject unit, bool isVisible)
    {
        if (unit.transform.childCount > 0)
        {
            unit.transform.GetChild(0).gameObject.SetActive(isVisible);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // No necesita sincronizar nada por ahora
    }
}