using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityStandardAssets.Characters.FirstPerson;

public class TutorialRatonController : MonoBehaviour
{
    [Header("Interfaz del tutorial")]
    public RawImage[] textos;

    [Header("Panel de Controles (Tecla C)")]
    public GameObject panelControles;

    [Header("Configuración de Control")]
    [SerializeField] private PlayerInput localPlayerInput;

    [Header("Tutorial Section")]
    public List<GameObject> tutorialSection;
    public int sectionIndex;

    [Header("Esferas")]
    [SerializeField] List<GameObject> spheres;

    public bool final = false;

    private void Start()
    {
        Debug.Log("[START] Iniciando TutorialRatonController...");
        if (localPlayerInput == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) localPlayerInput = player.GetComponent<PlayerInput>();
        }

        SetSection();
    }

    private void Update()
    {
        int indiceActual = final ? (textos.Length - 1) : (sectionIndex - 1);
        if (indiceActual < 0) indiceActual = 0;

        // --- TAB ---
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log($"[INPUT] Has pulsado TAB. Indice a evaluar: {indiceActual}");
            if (indiceActual < textos.Length)
            {
                bool objetivoAbierto = textos[indiceActual].gameObject.activeSelf;
                bool controlesAbiertos = panelControles != null && panelControles.activeSelf;

                Debug.Log($"[ESTADO TAB] ObjetivoAbierto: {objetivoAbierto} | ControlesAbiertos: {controlesAbiertos}");

                if (!objetivoAbierto && !controlesAbiertos)
                {
                    Debug.Log("[ACCION] Permiso concedido. Abriendo Objetivo...");
                    AbrirObjetivo(indiceActual);
                }
                else
                {
                    Debug.LogWarning("[DENEGADO] No se abre Objetivo porque ya hay un panel encendido (activeSelf = true).");
                }
            }
            else
            {
                Debug.LogError("[ERROR] Indice fuera de rango para textos.");
            }
        }

        // --- C ---
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("[INPUT] Has pulsado C.");
            if (panelControles != null)
            {
                bool objetivoAbierto = (indiceActual < textos.Length) && textos[indiceActual].gameObject.activeSelf;
                bool controlesAbiertos = panelControles.activeSelf;

                Debug.Log($"[ESTADO C] ObjetivoAbierto: {objetivoAbierto} | ControlesAbiertos: {controlesAbiertos}");

                if (!controlesAbiertos && !objetivoAbierto)
                {
                    Debug.Log("[ACCION] Permiso concedido. Abriendo Controles...");
                    AbrirControles();
                }
                else
                {
                    Debug.LogWarning("[DENEGADO] No se abre Controles porque ya hay un panel encendido (activeSelf = true).");
                }
            }
            else
            {
                Debug.LogError("[ERROR] PanelControles es NULL en el inspector.");
            }
        }
    }

    private void AbrirObjetivo(int indice)
    {
        Debug.Log($"[ACCION] AbrirObjetivo({indice}) invocado.");
        CerrarTodo();

        textos[indice].gameObject.SetActive(true);
        var script = textos[indice].GetComponent<PestaniaControles>();
        if (script != null) script.Abrir();

        PausarJuego();
    }

    private void AbrirControles()
    {
        Debug.Log("[ACCION] AbrirControles() invocado.");
        CerrarTodo();

        panelControles.SetActive(true);
        var script = panelControles.GetComponent<PestaniaControles>();
        if (script != null) script.Abrir();

        PausarJuego();
    }

    public void CerrarTodo()
    {
        Debug.Log("[ACCION] CerrarTodo() ejecutado. Apagando objetos...");
        foreach (var txt in textos)
        {
            if (txt != null) txt.gameObject.SetActive(false);
        }

        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }

        ReanudarJuego();
    }

    public void PausarJuego()
    {
        Debug.Log("[MOTOR] Pausando input y mostrando ratón...");
        if (localPlayerInput != null)
        {
            localPlayerInput.DeactivateInput();
            var fpController = localPlayerInput.GetComponent<RigidbodyFirstPersonController>();
            if (fpController != null) fpController.isTutorialOpen = true;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReanudarJuego()
    {
        Debug.Log("[MOTOR] Reanudando input y bloqueando ratón...");
        if (localPlayerInput != null)
        {
            localPlayerInput.ActivateInput();
            var fpController = localPlayerInput.GetComponent<RigidbodyFirstPersonController>();
            if (fpController != null) fpController.isTutorialOpen = false;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SetText()
    {
        Debug.Log($"[SECCION] SetText() ejecutado. Final={final}, sectionIndex={sectionIndex}");
        if (!final)
        {
            for (int i = 0; i < textos.Length; i++)
            {
                if (i == sectionIndex)
                {
                    textos[i].gameObject.SetActive(true);
                    textos[i].GetComponent<PestaniaControles>().Abrir();
                    PausarJuego();
                }
                else
                {
                    textos[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach (RawImage ri in textos) ri.gameObject.SetActive(false);

            int ultimoIndice = textos.Length - 1;
            textos[ultimoIndice].gameObject.SetActive(true);
            textos[ultimoIndice].GetComponent<PestaniaControles>().Abrir();
            PausarJuego();
        }
    }

    public void SetSection()
    {
        Debug.Log("[SECCION] SetSection() invocado (Ej. dinamita recogida o inicio).");
        SetText();
        sectionIndex++;
    }
}