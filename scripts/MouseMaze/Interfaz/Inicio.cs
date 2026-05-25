using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inicio : MonoBehaviour
{

    [Header("Referencias Jugador 1")]
    public Button readyButton1;
    public TextMeshProUGUI statusText1;
    public TextMeshProUGUI playText1;
    public Image indicator1;
    public Image indicator12;


    [Header("Referencias Jugador 2")]
    public Button readyButton2;
    public TextMeshProUGUI statusText2;
    public TextMeshProUGUI playText2;
    public Image indicator2;
    public Image indicator21;

    [Header("Colores")]
    public Color notReadyColor = Color.red;
    public Color readyColor = Color.green;

    // BOOLEANOS para controlar estados
    private bool player1Ready = false;
    private bool player2Ready = false;
    private bool gameStarting = false;

    void Start()
    {

        UpdateAllUI();
    }

    void Update()
    {
        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            Debug.Log("Botón A presionado por Jugador 2");
            ToggleReadyStateRaton();
        }
    }

    public void OnMouseDown()
    {
        ToggleReadyStateSlime();
    }

    // Método para cambiar el estado de listo
    public void ToggleReadyStateRaton()
    {
        if (gameStarting) return;

        
        player2Ready = !player2Ready;
        

        UpdateAllUI();
        CheckIfBothReady();
    }


    public void ToggleReadyStateSlime()
    {
        if (gameStarting) return;

        
        player1Ready = !player1Ready;
        

        UpdateAllUI();
        CheckIfBothReady();
    }

    private void UpdateAllUI()
    {
        // VERIFICAR null antes de usar
        if (readyButton1 != null)
        {
            var text1 = readyButton1.GetComponentInChildren<TextMeshProUGUI>();
            if (text1 != null)
                text1.text = player1Ready ? "CANCELAR" : "EMPEZAR";
        }

        if (indicator1 != null) indicator1.color = player1Ready ? readyColor : notReadyColor;
        if (indicator12 != null) indicator12.color = player1Ready ? readyColor : notReadyColor;

        if (readyButton2 != null)
        {
            var text2 = readyButton2.GetComponentInChildren<TextMeshProUGUI>();
            if (text2 != null)
                text2.text = player2Ready ? "CANCELAR" : "EMPEZAR";
        }

        if (indicator2 != null) indicator2.color = player2Ready ? readyColor : notReadyColor;
        if (indicator21 != null) indicator21.color = player2Ready ? readyColor : notReadyColor;

        // Mostrar estado del otro jugador
        if (statusText1 != null)
            statusText1.text = $"Slime: {(player1Ready ? "LISTO" : "ESPERANDO")}\n(Clikea)";

        if (statusText2 != null)
            statusText2.text = $"Miguel: {(player2Ready ? "LISTO" : "ESPERANDO")}\n(Presiona A)";
    }

    // Verificar si ambos jugadores están listos
    private void CheckIfBothReady()
    {
        Debug.Log("Jugador Slime: " + player1Ready + " Jugador Rata: " + player2Ready);
        if (player1Ready && player2Ready && !gameStarting)
        {
            gameStarting = true;
            StartCoroutine(StartGameSequence());
        }
    }

    // Secuencia de inicio del juego
    private IEnumerator StartGameSequence()
    {
        // VERIFICAR null antes de desactivar
        if (readyButton1 != null) readyButton1.gameObject.SetActive(false);
        if (statusText1 != null) statusText1.gameObject.SetActive(false);
        if (indicator1 != null) indicator1.gameObject.SetActive(false);
        if (indicator12 != null) indicator12.gameObject.SetActive(false);

        if (readyButton2 != null) readyButton2.gameObject.SetActive(false);
        if (statusText2 != null) statusText2.gameObject.SetActive(false);
        if (indicator2 != null) indicator2.gameObject.SetActive(false);
        if (indicator21 != null) indicator21.gameObject.SetActive(false);

        // VERIFICAR textos de cuenta atrás
        if (playText1 != null)
        {
            playText1.gameObject.SetActive(true);
        }
        if (playText2 != null)
        {
            playText2.gameObject.SetActive(true);
        }

        // Cuenta atrás
        for (int i = 3; i > -1; i--)
        {
            if (playText1 != null) playText1.text = $"{i}";
            if (playText2 != null) playText2.text = $"{i}";
            yield return new WaitForSeconds(1f);
        }

        // Cargar escena
        UnityEngine.SceneManagement.SceneManager.LoadScene("Animacion_Inicial");
    }
}