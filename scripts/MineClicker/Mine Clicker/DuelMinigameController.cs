using UnityEngine;
using TMPro;
using System.Collections;

// Minijuego contra el NPCDuelista
// a 3 vidas se cuenta hasta 3 y el que antes presione el click
// le resta una vida al contrario, si se gana se recibe una mejora
// temporal y si se pierde se destruye el npc

public class DuelMinigameController : MonoBehaviour
{
    public static DuelMinigameController Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;

    [Header("UI")]
    public TextMeshProUGUI textoCuentaAtras; // texto 1, 2, 3, 0
    public TextMeshProUGUI vidasJugadorTxt;
    public TextMeshProUGUI vidasNPCTxt;
    public TextMeshProUGUI textoResultado; // texto feedback

    private DuelEnemyNPC npcActual;

    private int vidasJugador;
    private int vidasNPC;

    // Variables para evitar spameo en el duelo
    private bool esperandoDisparo;
    private bool jugadorPuedeDisparar;
    private bool dueloActivo;
    private bool penalizacionEnCurso;
    private bool rondaCancelada;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    private void Update()
    {
        if (!dueloActivo) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Click antes de tiempo
            if (!jugadorPuedeDisparar && !penalizacionEnCurso)
            {
                PenalizarPorPronto();
            }
            // Click válido
            else if (jugadorPuedeDisparar && esperandoDisparo)
            {
                esperandoDisparo = false;
                ResolverRonda(true);
            }
        }
    }

    // Inicia el minijuego pone pausa por si no estaba pausado ya
    // y añade las vidas de los jugadores en la UI
    public void IniciarDuelo(DuelEnemyNPC npc)
    {
        npcActual = npc;
        panel.SetActive(true);
        PauseController.SetPause(true);

        vidasJugador = 3;
        vidasNPC = 3;
        ActualizarUI();

        dueloActivo = true;
        StartCoroutine(RutinaDuelo());
    }

    private IEnumerator RutinaDuelo()
    {
        while (vidasJugador > 0 && vidasNPC > 0)
        {
            rondaCancelada = false;

            yield return CuentaAtras();

            // Si se canceló por click temprano, saltar a siguiente ronda
            if (rondaCancelada)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            esperandoDisparo = true;

            float tiempoNPC = Random.Range(0.1f, 0.3f);
            float t = 0f;

            while (esperandoDisparo)
            {
                t += Time.deltaTime;

                // NPC dispara cuando le toque
                if (t >= tiempoNPC)
                {
                    esperandoDisparo = false;
                    ResolverRonda(false);
                }

                yield return null;
            }

            yield return new WaitForSeconds(1f);
        }

        FinalizarDuelo();
    }

    // Corrutina con la cuenta atras
    private IEnumerator CuentaAtras()
    {
        jugadorPuedeDisparar = false;

        textoCuentaAtras.text = "1";
        yield return new WaitForSeconds(0.6f);
        if (rondaCancelada) yield break;

        textoCuentaAtras.text = "2";
        yield return new WaitForSeconds(0.6f);
        if (rondaCancelada) yield break;

        textoCuentaAtras.text = "3";
        yield return new WaitForSeconds(0.6f);
        if (rondaCancelada) yield break;

        textoCuentaAtras.text = "0";
        jugadorPuedeDisparar = true;
    }

    // Se resetea la ronda y el jugador pierde una vida si dispara antes del 0
    private void PenalizarPorPronto()
    {
        penalizacionEnCurso = true;
        rondaCancelada = true;

        vidasJugador--;
        ActualizarUI();

        SoundEffectManager.Play("Incorrect");
        StartCoroutine(MensajeMuyPronto());

        esperandoDisparo = false;
        jugadorPuedeDisparar = false;

        StartCoroutine(ResetPenalizacion());
    }

    private IEnumerator ResetPenalizacion()
    {
        yield return new WaitForSeconds(0.3f);
        penalizacionEnCurso = false;
    }

    // Conteo de vidas al final de cada ronda
    private void ResolverRonda(bool jugadorDisparo)
    {
        if (jugadorDisparo)
        {
            SoundEffectManager.Play("Correct");
            vidasNPC--;
        }
        else
        {
            SoundEffectManager.Play("Incorrect");
            vidasJugador--;
        }

        ActualizarUI();
    }

    private void ActualizarUI()
    {
        vidasJugadorTxt.text = "Jugador: " + vidasJugador;
        vidasNPCTxt.text     = "NPC: " + vidasNPC;
    }

    private void FinalizarDuelo()
    {
        dueloActivo = false;
        StartCoroutine(MostrarResultadoYRellenar());
    }

    // Resultados al final del duelo y llamada a aplicar mejora temporal,
    // reanudacion del juego y panel desactivado.
    private IEnumerator MostrarResultadoYRellenar()
    {
        bool jugadorGano = vidasJugador > 0;

        if (jugadorGano)
        {
            string buff = TemporaryGlobalBuffSystem.Instance.AplicarBuffAleatorioConNombre();
            textoResultado.text = "<color=green>HAS GANADO\nPotenciador: " + buff;
        }
        else
        {
            textoResultado.text = "<color=red>JA   QUE   PARGUELA";
        }

        yield return new WaitForSeconds(2f);

        panel.SetActive(false);
        PauseController.SetPause(false);
        npcActual.ResultadoDuelo(jugadorGano);
    }

    // Feedback al clickar antes del 0
    private IEnumerator MensajeMuyPronto()
    {
        textoResultado.text = "<color=red>A  DONDE  VAS  CON  TANTA  PRISA  FITIPALDI";
        SoundEffectManager.Play("Incorrect");

        yield return new WaitForSeconds(0.8f);
        textoResultado.text = "";
    }
}