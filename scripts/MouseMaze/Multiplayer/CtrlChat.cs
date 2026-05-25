using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class CtrlChat : MonoBehaviourPunCallbacks, IChatClientListener
{
    [SerializeField] private string[] amigos;

    [Header("Propiedades Chat")]
    [SerializeField] private PhotonView PV;
    [SerializeField] private TextMeshProUGUI txtContenidoChat;
    [SerializeField] private TextMeshProUGUI txtContenidoPops;
    [SerializeField] private TMP_InputField ifMensaje;
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private GameObject popsPanel;
    [SerializeField] private GameObject canalGeneral;
    [SerializeField] private GameObject canalRatones;

    private RolJugador miRol;
    private bool chatHabilitado = false;
    private ChatClient clienteChat;
    private string nombreCanalActual;
    private string nombreJugador;
    private List<string> canalesDisponibles = new List<string>();
    private List<string> canalesSuscritosActuales = new List<string>();
    private RigidbodyFirstPersonController playerController;
    private bool chatListo = false;   // Indica si el chat está conectado y suscrito

    private Coroutine corrutinaPop;
    [SerializeField] private float tiempoEsperaAntesFade = 3f;
    [SerializeField] private float duracionFade = 1f;

    void Awake()
    {
        GameObject objMensaje = GameObject.Find("IfMensajeAEnviar");
        GameObject objLista = GameObject.Find("TxtListaMensajes");
        GameObject objPops = GameObject.Find("TxtContenidoPops");
        chatPanel = GameObject.Find("Panel_Chat");

        if (objMensaje != null) ifMensaje = objMensaje.GetComponent<TMP_InputField>();
        if (objLista != null) txtContenidoChat = objLista.GetComponent<TextMeshProUGUI>();
        if (objPops != null) txtContenidoPops = objPops.GetComponent<TextMeshProUGUI>();
        if (chatPanel != null) chatPanel.SetActive(false);

        Application.runInBackground = true;
        nombreJugador = PV.Owner.NickName;
        playerController = GetComponent<RigidbodyFirstPersonController>();

    }

    void Start()
    {
        if (PV.IsMine)
        {
            if (PhotonNetwork.InRoom)
            {
                ReiniciarConfiguracion();
            }
            else
            {
                Debug.Log("⏳ [CtrlChat] Esperando entrar a una sala...");
            } 
        }
    }

    private void DeterminarMiRol()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("SlimePlayerActorID", out object slimeActorObj))
        {
            int slimeActorNumber = (int)slimeActorObj;
            int miActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            
            if (miActorNumber == slimeActorNumber)
            {
                miRol = RolJugador.Slime;
                
            }
            else
            {
                miRol = RolJugador.Raton;
                
            }
        }
        else
        {
            Debug.LogError("[CtrlChat] No se encontró 'SlimePlayerActorID' en la sala. Asignando Rol por defecto: Ratón.");
            miRol = RolJugador.Raton;
        }
    }

    private void ConfigurarCanalesPorRol()
    {
        canalesDisponibles.Clear();
        switch (miRol)
        {
            case RolJugador.Slime:
                canalesDisponibles.Add("General");
                if (canalGeneral != null) canalGeneral.SetActive(true);
                break;
            case RolJugador.Raton:
                canalesDisponibles.Add("General");
                canalesDisponibles.Add("Ratones");
                if (canalGeneral != null) canalGeneral.SetActive(true);
                if (canalRatones != null) canalRatones.SetActive(true);
                break;
            default:
                canalesDisponibles.Add("General");
                if (canalGeneral != null) canalGeneral.SetActive(true);
                break;
        }

        if (clienteChat != null && clienteChat.CanChat)
            LimpiarYSuscribirCanales();
    }

    void Update()
    {
        if (!PV.IsMine) return;

        if (Input.GetKeyDown(KeyCode.Tab) && chatListo)
        {
            chatHabilitado = !chatHabilitado;
            if (popsPanel != null) popsPanel.SetActive(!chatHabilitado);
            if (chatPanel != null) chatPanel.SetActive(chatHabilitado);

            if (playerController != null)
                playerController.SetChatOpen(chatHabilitado);
            else
            {
                Cursor.lockState = chatHabilitado ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = chatHabilitado;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) && chatHabilitado)
            PrepararMensaje();

        clienteChat?.Service();
    }

    public void CambiarCanal(string nombreCanal)
    {
        if (clienteChat == null) { Debug.LogWarning("⚠️ [CtrlChat] Chat no conectado"); return; }
        if (!canalesDisponibles.Contains(nombreCanal)) { Debug.LogWarning($"⛔ [CtrlChat] No tienes acceso al canal '{nombreCanal}'"); return; }
        if (nombreCanal == nombreCanalActual) { Debug.Log($"ℹ️ [CtrlChat] Ya estás en el canal '{nombreCanal}'"); return; }

        nombreCanalActual = nombreCanal;
        MostrarCanal(nombreCanalActual);
    }

    public List<string> ObtenerCanalesDisponibles() => canalesDisponibles;

    public void Pulsar_BtnEnviar() => PrepararMensaje();
    public void Enter_IfEnviarMensaje(string _datos) => PrepararMensaje();

    private void PrepararMensaje()
    {
        if (ifMensaje == null) { Debug.LogError("❌ [CtrlChat] ifMensaje es NULL"); return; }
        string mensaje = ifMensaje.text;
        if (!string.IsNullOrEmpty(mensaje))
        {
            EnviarMensajePun(mensaje);
            ifMensaje.text = string.Empty;
        }
    }


    private void LimpiarYSuscribirCanales()
    {
        if (clienteChat == null) return;

        // 1. Dar de baja de los canales actuales (si los hay)
        if (canalesSuscritosActuales.Count > 0)
        {
            clienteChat.Unsubscribe(canalesSuscritosActuales.ToArray());
            Debug.Log($"🧹 [CtrlChat] Limpiando canales antiguos: {string.Join(", ", canalesSuscritosActuales)}");
            canalesSuscritosActuales.Clear();
        }

        // 2. Suscribirse a los nuevos canales disponibles
        if (canalesDisponibles.Count > 0)
        {
            clienteChat.Subscribe(canalesDisponibles.ToArray());
            Debug.Log($"📡 [CtrlChat] Suscribiendo nuevos canales: {string.Join(", ", canalesDisponibles)}");
        }
    }

    private void EnviarMensajePun(string _mensaje)
    {
        if (clienteChat != null && clienteChat.CanChat)
            clienteChat.PublishMessage(nombreCanalActual, _mensaje);
        else
            Debug.LogWarning("⚠️ [CtrlChat] No se pudo enviar mensaje: Chat no listo.");
    }

    public void Conectarse()
    {
        clienteChat = new ChatClient(this);
        clienteChat.UseBackgroundWorkerForSending = true;
        clienteChat.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
                            PhotonNetwork.AppVersion,
                            new AuthenticationValues(nombreJugador));
    }

    private void Desconectarse() => clienteChat?.Disconnect();

    private void MostrarCanal(string _canal)
    {
        if (string.IsNullOrEmpty(_canal)) return;
        if (clienteChat.TryGetChannel(_canal, out ChatChannel canal))
        {
            txtContenidoChat.text = canal.ToStringMessages();
        }
        else
        {
            txtContenidoChat.text = $"--- Canal: {_canal} ---\n(Sin mensajes)";
        }
    }

    private void MostrarMensajePop(string texto)
    {
        if (txtContenidoPops == null) return;
        if (corrutinaPop != null)
            StopCoroutine(corrutinaPop);

        txtContenidoPops.text = texto;
        Color color = txtContenidoPops.color;
        color.a = 1f;
        txtContenidoPops.color = color;

        corrutinaPop = StartCoroutine(EsperarYDesvanecer());
    }

    private IEnumerator EsperarYDesvanecer()
    {
        yield return new WaitForSeconds(tiempoEsperaAntesFade);
        float tiempo = 0f;
        Color color = txtContenidoPops.color;
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempo / duracionFade);
            color.a = alpha;
            txtContenidoPops.color = color;
            yield return null;
        }
        color.a = 0f;
        txtContenidoPops.color = color;
        txtContenidoPops.text = "";
        corrutinaPop = null;
    }

    // ------------------------------------------------------------
    // IChatClientListener
    // ------------------------------------------------------------
    public void DebugReturn(DebugLevel level, string message)
    {
        switch (level)
        {
            case DebugLevel.ERROR: Debug.LogError($"🚨 [Photon Chat] {message}"); break;
            case DebugLevel.WARNING: Debug.LogWarning($"⚠️ [Photon Chat] {message}"); break;
            default: Debug.Log($"ℹ️ [Photon Chat] {message}"); break;
        }
    }

    public void OnDisconnected()
    {
        Debug.Log($"🔌 [CtrlChat] Desconectado del chat - {nombreJugador}");
        chatListo = false;
    }

    public void OnConnected()
    {
        Debug.Log($"✅ [CtrlChat] Conectado al chat.");
        if (canalesDisponibles.Count > 0)
            LimpiarYSuscribirCanales();
        else
            Debug.LogWarning("⚠️ [CtrlChat] No hay canales disponibles para suscribir.");

        if (amigos != null && amigos.Length > 0)
            clienteChat.AddFriends(amigos);
        clienteChat.SetOnlineStatus(ChatUserStatus.Online);
    }

    public void OnChatStateChange(ChatState state)
    {
        if (state == ChatState.ConnectedToFrontEnd)
        {
            Debug.Log("🔄 [CtrlChat] Reconectado al frontend. Limpiando y resuscribiendo...");
            LimpiarYSuscribirCanales();
        }
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (channelName == nombreCanalActual)
            MostrarCanal(channelName);

        if (messages != null && messages.Length > 0)
        {
            int ultimo = messages.Length - 1;
            string remitente = senders[ultimo];
            string mensaje = messages[ultimo].ToString();
            MostrarMensajePop($"[{channelName}] {remitente}: {mensaje}");
        }
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log($"✅ [CtrlChat] Suscrito a canales: {string.Join(", ", channels)}");
        for (int i = 0; i < channels.Length; i++)
        {
            if (results[i] && !canalesSuscritosActuales.Contains(channels[i]))
                canalesSuscritosActuales.Add(channels[i]);
        }

        if (channels.Length > 0 && results[0])
        {
            nombreCanalActual = canalesDisponibles[0];
            MostrarCanal(nombreCanalActual);
            chatListo = true;
        }
    }

    public void OnUnsubscribed(string[] channels)
    {
        Debug.Log($"📤 [CtrlChat] Dado de baja de canales: {string.Join(", ", channels)}");
        foreach (string ch in channels)
            canalesSuscritosActuales.Remove(ch);
    }
    public void OnUserSubscribed(string channel, string user) => Debug.Log($"👤 [CtrlChat] {user} se suscribió a {channel}");
    public void OnUserUnsubscribed(string channel, string user) => Debug.Log($"👤 [CtrlChat] {user} se dio de baja de {channel}");
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }


    private void OnEnable()
    {
        // Registrar este script para recibir callbacks de Photon (si no lo hace automáticamente)
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnJoinedRoom()
    {

        Debug.LogWarning("🏠 [CtrlChat] Entré a una sala. Reiniciando configuración del chat...");
        ReiniciarConfiguracion();
    }

    public override void OnLeftRoom()
    {

        Debug.LogWarning("🚪 [CtrlChat] Salí de la sala. Desconectando chat y limpiando canales...");
        Desconectarse();
        canalesDisponibles.Clear();
        canalesSuscritosActuales.Clear();
        nombreCanalActual = null;
        chatHabilitado = false;
        if (chatPanel != null) chatPanel.SetActive(false);
        if (popsPanel != null) popsPanel.SetActive(false);
    }

    private void ReiniciarConfiguracion()
    {
        // 1. Determinar rol otra vez (las propiedades de sala ya están actualizadas)
        DeterminarMiRol();

        // 2. Configurar canales según el nuevo rol
        ConfigurarCanalesPorRol();

        // 3. Si el cliente de chat ya existe y está conectado, limpiar y resuscribir
        if (clienteChat != null && clienteChat.CanChat)
        {
            LimpiarYSuscribirCanales();
        }
        // 4. Si no existe o no está conectado, conectar
        else if (clienteChat == null)
        {
            Conectarse();
        }
        // 5. Si está en un estado intermedio (conectando), no hacer nada, la conexión ya se completará
    }


    public void ReconfigurarPorRol(RolJugador nuevoRol)
    {
        if (!PV.IsMine) return;

        Debug.Log($"🔁 [CtrlChat] Reconfigurando para rol: {nuevoRol}");
        miRol = nuevoRol;
        ConfigurarCanalesPorRol();

        // Si el chat ya está conectado, actualizar suscripciones
        if (clienteChat != null && clienteChat.CanChat)
            LimpiarYSuscribirCanales();
    }

    void OnDestroy()
    {
        if (!PV.IsMine) return;
        Debug.Log("🧹 [CtrlChat] Destruyendo objeto, limpiando chat...");
        LimpiarCompletamente();
    }

    public void LimpiarCompletamente()
    {
        if (clienteChat != null)
        {
            // Dar de baja de todos los canales suscritos
            if (canalesSuscritosActuales.Count > 0)
            {
                clienteChat.Unsubscribe(canalesSuscritosActuales.ToArray());
                Debug.Log($"🧹 [CtrlChat] Dando de baja canales: {string.Join(", ", canalesSuscritosActuales)}");
            }
            clienteChat.Disconnect();
            clienteChat = null;
        }
        canalesDisponibles.Clear();
        canalesSuscritosActuales.Clear();
        nombreCanalActual = null;
        chatHabilitado = false;
        if (chatPanel != null) chatPanel.SetActive(false);
        if (popsPanel != null) popsPanel.SetActive(false);
        Debug.Log("✅ [CtrlChat] Chat limpiado por completo");
    }
}