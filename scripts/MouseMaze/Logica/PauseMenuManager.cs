using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections; // <-- IMPORTANTE: Añadido para poder usar IEnumerator

public class PauseMenuManager : MonoBehaviourPunCallbacks
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private bool OnMenu;

    [Header("Chat Settings")]
    [SerializeField] private GameObject chatHintText; // Objeto "Tab to open chat"

    [Header("Navegación")]
    private string nextBackScene = "Master_MainMenu";
    private bool isPaused = false;
    public static bool GameIsPaused = false;
    private PlayerInput localPlayerInput;

    // Control para evitar que intente despausar varias veces a la vez
    private bool isUnpausing = false;

    // NUEVO: Control para saber si estamos saliendo voluntariamente desde el menú de pausa
    private bool manualLeaveFromPause = false;

    private static PauseMenuManager _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        CacheLocalPlayerInput();
    }

    void Update()
    {
        if (localPlayerInput == null)
            CacheLocalPlayerInput();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Si está pausado, esperamos a que suelte la tecla para evitar conflictos con MouseLook
            if (isPaused && !isUnpausing)
                StartCoroutine(UnpauseRoutine());
            // Si no está pausado, pausamos instantáneamente
            else if (!isPaused)
                TogglePause();
        }
    }

    // <-- NUEVA CORRUTINA -->
    private IEnumerator UnpauseRoutine()
    {
        isUnpausing = true;

        // 1. Esperamos hasta que el jugador levante por completo el dedo de la tecla Escape
        yield return new WaitUntil(() => !Input.GetKey(KeyCode.Escape));

        // 2. Esperamos un frame extra por seguridad, para que MouseLook no lo detecte
        yield return new WaitForEndOfFrame();

        // 3. Ahora sí, despausamos el juego
        if (isPaused)
            TogglePause();

        isUnpausing = false;
    }

    private void CacheLocalPlayerInput()
    {
        var allPlayerInputs = FindObjectsOfType<PlayerInput>();
        foreach (var pi in allPlayerInputs)
        {
            var pun = pi.GetComponent<MonoBehaviourPun>();
            if (pun != null && pun.photonView != null && pun.photonView.IsMine)
            {
                localPlayerInput = pi;
                return;
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        GameIsPaused = isPaused;

        pausePanel.SetActive(isPaused);
        settingsPanel.SetActive(false);

        // Limpieza de selección de UI
        if (!isPaused && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (localPlayerInput != null)
        {
            if (isPaused) localPlayerInput.DeactivateInput();
            else localPlayerInput.ActivateInput();
        }

        FreezeLocalPlayer(isPaused);

        // Gestión de cursor según estado de pausa y tipo de escena
        if (!OnMenu)
        {
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
        }
    }

    private void FreezeLocalPlayer(bool freeze)
    {
        if (localPlayerInput == null) return;
        Rigidbody rb = localPlayerInput.GetComponent<Rigidbody>();
        if (rb != null && freeze) rb.velocity = Vector3.zero;
    }

    public void SetOnMenu(bool state)
    {
        OnMenu = state;
        if (state)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isPaused = false;
            GameIsPaused = false;
        }
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void LeaveGame()
    {
        isPaused = false;
        GameIsPaused = false;
        Time.timeScale = 1f;

        // Indicamos que queremos forzar la salida a otra escena desde el menú de pausa
        manualLeaveFromPause = true;

        if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
        else if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        else LoadParentScene();
    }

    // NUEVO: El PauseMenuManager solo fuerza la carga de escena si NO estamos en un menú, 
    // o si le hemos dado explícitamente a LeaveGame().
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (!OnMenu || manualLeaveFromPause)
        {
            manualLeaveFromPause = false;
            LoadParentScene();
        }
    }

    // NUEVO: Igual que arriba, evitamos pisar la lógica de CtrlConexion.
    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (!OnMenu || manualLeaveFromPause)
            {
                manualLeaveFromPause = false;
                LoadParentScene();
            }
        }
    }

    private void LoadParentScene()
    {
        Time.timeScale = 1f;
        string sceneToLoad = (string.IsNullOrEmpty(nextBackScene) || nextBackScene == SceneManager.GetActiveScene().name)
                             ? "Master_MainMenu"
                             : nextBackScene;

        SceneManager.LoadScene(sceneToLoad);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneConfig config = Object.FindAnyObjectByType<SceneConfig>();

        if (config != null)
        {
            OnMenu = config.isMenuScene;
            if (scene.name != config.parentSceneName)
                nextBackScene = config.parentSceneName;
        }
        else
        {
            OnMenu = false;
            nextBackScene = "Master_MainMenu";
        }

        // Reset de estados
        isPaused = false;
        GameIsPaused = false;
        Time.timeScale = 1f;
        isUnpausing = false;
        manualLeaveFromPause = false; // Reset de la bandera

        // Reset UI
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            RectTransform rt = pausePanel.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Cursor según escena
        Cursor.lockState = OnMenu ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = OnMenu;

        // Gestión del Chat Hint: Solo activo si estamos en una sala Online
        if (chatHintText != null)
        {
            bool shouldShowChat = PhotonNetwork.InRoom && !OnMenu;
            chatHintText.SetActive(shouldShowChat);
        }

        localPlayerInput = null;
        CacheLocalPlayerInput();
    }

    public override void OnJoinedRoom()
    {
        if (chatHintText != null)
            chatHintText.SetActive(true);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
}