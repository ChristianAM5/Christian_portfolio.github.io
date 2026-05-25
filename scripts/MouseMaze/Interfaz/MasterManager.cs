using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MasterManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button[] buttons;


    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float navigationCooldown = 0.3f;

    private int currentIndex = 0;
    private float lastNavigationTime = 0f;
    private InputAction navigateAction;

    private void Start()
    {

        // Configurar input
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

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

    public void DebugReference()
    {
        Debug.Log("Botón pulsado");
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
            Debug.Log($"Navegando a boton: {currentIndex}");
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


    public void Online()
    {
        SceneManager.LoadScene("Loby");
    }

    public void Offline()
    {
        // Modo local (2 pantallas)
        GameConfig.singleScreenMode = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void OfflineSolo()
    {
        // Modo solo (1 pantalla)
        GameConfig.singleScreenMode = true;
        SceneManager.LoadScene("MainMenu");
    }

    public void MenuOffline()
    {
        SceneManager.LoadScene("Single_Multi_Offline");
    }
    public void MenuTutorial()
    {
        SceneManager.LoadScene("Menu_Tutoriales");
    }

    public void TutorialRaton()
    {
        SceneManager.LoadScene("TutotialRaton");
    }

    public void TutorialSlime()
    {
        SceneManager.LoadScene("Tutorial_Slime");
    }

    public void Salir()
    {
        GameConfig.singleScreenMode = true;

        SceneManager.LoadScene("Master_MainMenu");
    }
    public void ExitJogo()
    {
        Application.Quit();
    }



}