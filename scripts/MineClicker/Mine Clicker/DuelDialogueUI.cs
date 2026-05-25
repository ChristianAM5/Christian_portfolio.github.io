using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Gestiona la UI de dialogo del NPC dueler
// Este script se encuentra dentro del canvas de la NPC

public class DuelDialogueUI : MonoBehaviour
{
    public static DuelDialogueUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;

    [Header("Botones")]
    public Button botonSi;
    public Button botonNo;

    public TextMeshProUGUI textoDescripcion;

    private DuelEnemyNPC npcActual;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    // Conectar botones por script
    private void Start()
    {
        botonSi.onClick.AddListener(OnAceptar);
        botonNo.onClick.AddListener(OnRechazar);
    }

    // Se llama cuando se interactua con el npc desde DuelEnemyNPC
    // Active el panel de dialogo y la pausa
    public void MostrarDialogo(DuelEnemyNPC npc)
    {
        npcActual = npc;
        panel.SetActive(true);
        PauseController.SetPause(true);
    }

    // Si se acepta el duelo se cambia el texto con una pequeña descripcion del juego,
    // se desactivan los botones y espera con una corrutina 2 segundos antes de activar el canvas del minijuego.
    private void OnAceptar()
    {
        botonSi.interactable = false;
        botonNo.interactable = false;

        textoDescripcion.text =
            "<color=green>Cuenta  1, 2, 3.  Dispara  antes  que  yo.  A  3  vidas.";

        StartCoroutine(EmpezarDueloTrasExplicacion());
    }

    private System.Collections.IEnumerator EmpezarDueloTrasExplicacion()
    {
        yield return new WaitForSecondsRealtime(2f);

        panel.SetActive(false);
        PauseController.SetPause(false);
        npcActual.AceptarDuelo();
    }

    // Si se rechaza el duelo se cambia el texto, se desactivan los botones 
    // y se destruye el npc actual llamando a la corrutina correspondiente
    private void OnRechazar()
    {
        botonSi.interactable = false;
        botonNo.interactable = false;

        textoDescripcion.text = "<color=red>JA   PRINGAO";

        StartCoroutine(CerrarTrasInsulto());
    }

    private System.Collections.IEnumerator CerrarTrasInsulto()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        panel.SetActive(false);
        PauseController.SetPause(false);
        npcActual.RechazarDuelo();
    }

}
