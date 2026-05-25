using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Gestiona la UI de los acertijos
// Textos de feedback ante las respuestas
// Y normaliza el input de la respuesta para evitar mayusculas, tildes y demás.

public class RiddleUI : MonoBehaviour
{
    [Header("Input del jugador")]
    public TMP_InputField inputRespuesta;

    [Header("Botones")]
    public Button botonConfirmar;
    public Button botonCerrar;

    [Header("Feedback")]
    public TextMeshProUGUI textoFeedback;
    public float duracionFeedback = 3f;

    // Mensajes graciosos para respuestas incorrectas
    // Se eligen aleatoriamente cuando la respuesta es incorrecta
    private string[] mensajesIncorrectos = new string[]
    {
        "¡CASI CRACK! Pero no...",
        "Todavia no lo has resuelto? mi abuela ya tendria la casa recogia y el puchero puesto",
        "Casi lo consigues, estás cerquita",
        "Es eso, pero escrito de otra forma",
        "Me sorprende genuinamente que estes capacitado siquiera para escribir",
        "Pensar está sobrevalorado supongo",
        "Acaba de morir un gatito, y es tu culpa",
        "Ya verás cuando se enteren de que esto está automatizado y no lee tu respuesta al escribir este mensaje sin utilidad"
    };

    private RiddleSign cartelActual;
    private Coroutine corrutinaFeedback;

    // Conectar botones por codigo y ocultar UI al principio
    private void Awake()
    {
        botonConfirmar?.onClick.AddListener(ComprobarRespuesta);
        botonCerrar?.onClick.AddListener(Cerrar);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        // Confirmar con Enter
        if (Input.GetKeyDown(KeyCode.Return))
            ComprobarRespuesta();

        // Cerrar con Escape
        if (Input.GetKeyDown(KeyCode.Escape))
            Cerrar();
    }

    // Guarda la referencia del acertijo actual y limpia el input
    public void Inicializar(RiddleSign cartel)
    {
        cartelActual        = cartel;
        inputRespuesta.text = "";
        textoFeedback.gameObject.SetActive(false);
        inputRespuesta.Select();
        inputRespuesta.ActivateInputField();
    }

    // Normaliza el input mayusculas, tildes y espacios
    private void ComprobarRespuesta()
    {
        string respuesta = inputRespuesta.text.Trim().ToLower()
            .Replace("á","a").Replace("é","e").Replace("í","i")
            .Replace("ó","o").Replace("ú","u");

        if (string.IsNullOrEmpty(respuesta))
        {
            MostrarFeedback("Pon  algo  y  luego  confirmas  por  el  amor  de  dios", false);
            return;
        }

        // Comprobamos si la respuesta es válida
        bool esCorrecta = false;
        foreach (string valida in cartelActual.respuestasValidas)
        {
            string validaNorm = valida.Trim().ToLower()
                .Replace("á","a").Replace("é","e").Replace("í","i")
                .Replace("ó","o").Replace("ú","u");

            if (respuesta == validaNorm)
            {
                esCorrecta = true;
                break;
            }
        }

        // En caso correcto muestra el texto con la recompensa y se cierra
        if (esCorrecta)
        {
            SoundEffectManager.Play("Correct");

            string recompensa = ObtenerTextoRecompensa();
            string mensaje = "CORRECTO\nRecompensa: " + recompensa;

            MostrarFeedback(mensaje, true);
            StartCoroutine(ResolverYCerrar());
        }
        else // En caso incorrecto muestra un mensaje gracioso y limpia el input
        {
	    SoundEffectManager.Play("Incorrect");
            // Mensaje gracioso aleatorio
            string msg = mensajesIncorrectos[
                Random.Range(0, mensajesIncorrectos.Length)];
            MostrarFeedback(msg, false);
            inputRespuesta.text = "";
            inputRespuesta.Select();
            inputRespuesta.ActivateInputField();
        }
    }

    // Llamar al texto con la recompensa de RiddleRewardManager
    private string ObtenerTextoRecompensa()
    {
        if (cartelActual == null) return "";

        string id = cartelActual.gameObject.name;
        return RiddleRewardManager.Instance.GetDescripcionRecompensa(id);
    }

    // Espera 3 segundos antes de cerrar y destruir para que se pueda leer la mejora obtenida
    private IEnumerator ResolverYCerrar()
    {
        yield return new WaitForSeconds(3f);

        // Aplicamos el multiplicador/mejora
        RiddleRewardManager.Instance.AplicarRecompensa(cartelActual.gameObject.name);

        cartelActual.MarcarResuelto();
        Cerrar();
    }

    // Ocultar panel y reanudar el juego
    private void Cerrar()
    {
        gameObject.SetActive(false);
        PauseController.SetPause(false);
        cartelActual = null;
    }

    private void MostrarFeedback(string mensaje, bool exito)
    {
        if (textoFeedback == null) return;
        if (corrutinaFeedback != null) StopCoroutine(corrutinaFeedback);
        corrutinaFeedback = StartCoroutine(FeedbackTemporal(mensaje, exito));
    }

    private IEnumerator FeedbackTemporal(string mensaje, bool exito)
    {
        textoFeedback.color = exito
            ? new Color(0.1f, 0.6f, 0.1f)
            : new Color(0.8f, 0.1f, 0.1f);
        textoFeedback.text = mensaje;
        textoFeedback.gameObject.SetActive(true);

        if (!exito)
        {
            yield return new WaitForSeconds(duracionFeedback);
            textoFeedback.gameObject.SetActive(false);
        }
    }
}
