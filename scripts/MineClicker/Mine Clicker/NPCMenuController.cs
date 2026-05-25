using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Controlador principal del menú de NPC
// Contiene las páginas: Intro, Vender, Zonas, Mejoras

public class NPCMenuController : MonoBehaviour
{
    [Header("Páginas del menú")]
    public GameObject paginaIntro;    // Texto dialogo NPC
    public GameObject paginaVender;   // Solo NPC central, y QuickAccess
    public GameObject paginaZonas;    // Solo NPC central
    public GameObject paginaMejoras;  // Todos los NPCs

    [Header("Botones de pestaña")]
    public Button btnIntro;           
    public Button btnVender;
    public Button btnZonas;
    public Button btnMejoras;
    public Button btnCerrar;

    [Header("Texto de introducción")]
    // El TextMeshPro que está DENTRO de paginaIntro
    public TextMeshProUGUI textoIntro;

    [Header("Controladores de cada página")]
    public NPCVenderController  ctrlVender;
    public NPCZonasController   ctrlZonas;
    public NPCMejorasController ctrlMejoras;

    // Conectar botones a sus funciones
    private void Start()
    {
        btnIntro?.onClick.AddListener(()   => MostrarPagina(0));
        btnVender?.onClick.AddListener(()  => MostrarPagina(1));
        btnZonas?.onClick.AddListener(()   => MostrarPagina(2));
        btnMejoras?.onClick.AddListener(() => MostrarPagina(3));
        btnCerrar?.onClick.AddListener(Cerrar);
    }

    // Llamado al abrir el menú con: mineral = mineral del NPC,
    // esNPCCentral = si es NPC principal y esMenuQuickAccess = acceso rápido (solo vender)

    public void Inicializar(MineralType mineral, bool esNPCCentral, bool esMenuQuickAccess)
    {

        ResetUI();  // Oculta todo y activa botones por defecto

        // Si es acceso rápido solo vender
        if (esMenuQuickAccess)
        {
            btnIntro?.gameObject.SetActive(false);
            btnZonas?.gameObject.SetActive(false);
            btnMejoras?.gameObject.SetActive(false);
            btnVender?.gameObject.SetActive(true);

            paginaIntro?.SetActive(false);
            paginaZonas?.SetActive(false);
            paginaMejoras?.SetActive(false);
            paginaVender?.SetActive(true);

            // Inicializamos y refrescamos la página de vender
            ctrlVender?.Inicializar();
            ctrlVender?.Refrescar();
            return;
        }

        // Mostramos u ocultamos pestañas según tipo de NPC
        btnVender?.gameObject.SetActive(esNPCCentral);
        btnZonas?.gameObject.SetActive(esNPCCentral);
        paginaVender?.SetActive(false);
        paginaZonas?.SetActive(false);

        // Notificamos a los subcontroladores
        ctrlMejoras?.Inicializar(mineral);

        // Si NPC central, inicializamos vender y zonas
        if (esNPCCentral)
        {
            ctrlVender?.Inicializar();   // sin mineral, vende todos
            ctrlZonas?.Inicializar();
        }

        // Siempre abrimos en la página de Intro
        MostrarPagina(0);
    }


    // 0=Intro, 1=Vender, 2=Zonas, 3=Mejoras

    public void MostrarPagina(int index)
    {
        paginaIntro?.SetActive(index == 0);
        paginaVender?.SetActive(index == 1);
        paginaZonas?.SetActive(index == 2);
        paginaMejoras?.SetActive(index == 3);

        // Refrescar contenido dinámico
        if (index == 1) ctrlVender?.Refrescar();
        if (index == 2) ctrlZonas?.Refrescar();
        if (index == 3) ctrlMejoras?.Refrescar();
    }

    // Permite asignar el texto de intro desde NPCInteractable.
    public void SetTextoIntro(string texto)
    {
        if (textoIntro != null)
            textoIntro.text = texto;
    }

    private void ResetUI()
{
    // Activar todos los botones
    btnIntro?.gameObject.SetActive(true);
    btnVender?.gameObject.SetActive(true);
    btnZonas?.gameObject.SetActive(true);
    btnMejoras?.gameObject.SetActive(true);

    // Ocultar todas las páginas
    paginaIntro?.SetActive(false);
    paginaVender?.SetActive(false);
    paginaZonas?.SetActive(false);
    paginaMejoras?.SetActive(false);
}

    public void Cerrar()
    {
        gameObject.SetActive(false);        // Ocultamos menú
        PauseController.SetPause(false);    // Reanudamos juego
    }
}