using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class CtrlConexion : MonoBehaviourPunCallbacks
{
    [Header("Paneles UI")]
    [SerializeField] private GameObject[] paneles = new GameObject[5];

    [Header("Panel 1: Conexión")]
    [SerializeField] private TMP_InputField ifJugador;

    [Header("Panel 2: Bienvenida")]
    [SerializeField] private TextMeshProUGUI txtBienvenida;

    [Header("Panel 3: Creación Sala")]
    [SerializeField] private TMP_InputField ifNombreSala;
    [SerializeField] private Toggle tglPrivada;         // Toggle Sala Privada
    [SerializeField] private TMP_Dropdown ddMapa;       // Dropdown con escenas

    [Header("Panel 4: Unirse a Sala")]
    [SerializeField] private TMP_InputField ifUnirseASala;
    [SerializeField] private Transform contenedorListaSalas; // Content del ScrollRect de salas

    [Header("Panel 5: Dentro de Sala")]
    [SerializeField] private TextMeshProUGUI txtNombreSala;
    [SerializeField] private TextMeshProUGUI txtCapacidad;
    [SerializeField] private Transform contenedorJugadores;  // Content del ScrollRect de jugadores
    [SerializeField] private TextMeshProUGUI txtInfoRoles;
    [SerializeField] private Button btnIniciarPartida;

    [Header("Barra de estado")]
    [SerializeField] private TextMeshProUGUI txtBarraEstado;

    [Header("Fuente")]
    [SerializeField] private TMP_FontAsset fuentePersonalizada;

    // Propiedades de sala
    private const string PROP_SLIME_PLAYER    = "SlimePlayerActorID";
    private const string PROP_PARTIDA_INICIADA = "PartidaIniciada";
    private const string PROP_MAPA            = "Mapa";
    private const int    MIN_JUGADORES        = 2;
    private const int    MAX_JUGADORES        = 5;

    // Los nombres deben coincidir exactamente con los Build Settings de Unity
    private readonly string[] MAPAS = { "Mouse Maze", "Cloaca", "Mina" }; // índice 4 = Aleatorio

    private RolJugador miRol = RolJugador.SinAsignar;
    private List<RoomInfo> listaActualSalas = new List<RoomInfo>();

    // -1 = sin elegir (se asignará aleatoriamente al iniciar)
    // cualquier otro valor = ActorNumber del jugador elegido como Slime por el host
    private int slimeElegidoActorNumber = -1;

    // ==================== INICIO ====================

    void Start()
    {
        // Si venimos de otra escena ya estamos conectados: saltar directamente a Bienvenida
        if (PhotonNetwork.IsConnected)
        {
            txtBienvenida.text = $"¡Bienvenido {PhotonNetwork.NickName}!";
            ActivarPaneles(1);

            //if (!PhotonNetwork.InLobby)
            //    PhotonNetwork.JoinLobby(); Ya se encarga on connected to master
        }
        else
        {
            ActivarPaneles(0);
        }

        // fuente rat en dropdown
        if (fuentePersonalizada != null && ddMapa != null)
        {
            // Texto del item seleccionado
            ddMapa.captionText.font = fuentePersonalizada;
            // Texto de cada opción del desplegable
            ddMapa.itemText.font = fuentePersonalizada;
        }
    }

    // ==================== UTILIDADES ====================

    private void ActivarPaneles(int indice)
    {
        for (int i = 0; i < paneles.Length; i++)
            paneles[i].SetActive(i == indice);
    }

    // ==================== PANEL 1: CONEXIÓN ====================

    public void Pulsar_BtnConectar()
    {
        if (!string.IsNullOrEmpty(ifJugador.text) && !string.IsNullOrWhiteSpace(ifJugador.text))
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.NickName = ifJugador.text;
            PhotonNetwork.ConnectUsingSettings();
            txtBarraEstado.text = "Conectando a Photon...";
        }
        else
        {
            txtBarraEstado.text = "Introduce un nombre de jugador";
        }
    }

    public void Pulsar_BtnSalir()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Master_MainMenu");
    }

    // ==================== PANEL 2: BIENVENIDA ====================

    public void Pulsar_BtnCrearNuevaSala()
    {
        ActivarPaneles(2);
    }

    public void Pulsar_BtnUnirseASala()
    {
        ActivarPaneles(3);
        ActualizarListaSalas(); // Dibujar la lista con lo que ya tenemos en caché
    }

    public void Pulsar_BtnDesconectarBienvenida()
    {
        PhotonNetwork.Disconnect();
        ActivarPaneles(0);
    }

    // ==================== PANEL 3: CREAR SALA ====================

    public void Pulsar_BtnCrearSala()
    {
        if (string.IsNullOrEmpty(ifNombreSala.text) || string.IsNullOrWhiteSpace(ifNombreSala.text))
        {
            txtBarraEstado.text = "Introduce un nombre de sala";
            return;
        }

        bool esPrivada = tglPrivada != null && tglPrivada.isOn;

        // Determinar mapa
        int indiceMapa = ddMapa != null ? ddMapa.value : 0;
        // Si el índice supera el array es porque eligió Aleatorio:
        // guardamos el texto "Aleatorio" para no revelar el mapa hasta que empiece la partida
        string mapaElegido = (indiceMapa >= MAPAS.Length) ? "Aleatorio" : MAPAS[indiceMapa];

        RoomOptions opciones = new RoomOptions
        {
            MaxPlayers = MAX_JUGADORES,
            IsVisible  = !esPrivada,   // Las privadas no aparecen en el listado
            IsOpen     = true,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { PROP_SLIME_PLAYER,     -1    },
                { PROP_PARTIDA_INICIADA, false },
                { PROP_MAPA,             mapaElegido }
            },
            CustomRoomPropertiesForLobby = new[] { PROP_SLIME_PLAYER, PROP_PARTIDA_INICIADA, PROP_MAPA }
        };

        PhotonNetwork.CreateRoom(ifNombreSala.text, opciones);
        txtBarraEstado.text = "Creando sala...";
    }

    public void Pulsar_BtnVolverBienvenidaDesdeCrear()
    {
        ActivarPaneles(1);
    }

    // ==================== PANEL 4: UNIRSE A SALA ====================

    // Botón para unirse mediante nombre de sala privada
    public void Pulsar_BtnUnirse()
    {
        if (!string.IsNullOrEmpty(ifUnirseASala.text) && !string.IsNullOrWhiteSpace(ifUnirseASala.text))
        {
            PhotonNetwork.JoinRoom(ifUnirseASala.text);
            txtBarraEstado.text = "Uniéndose a sala privada...";
        }
        else
        {
            txtBarraEstado.text = "Introduce el nombre de la sala privada";
        }
    }

    public void Pulsar_BtnVolverBienvenidaDesdeUnirse()
    {
        ActivarPaneles(1);
    }

    // Reconstruye la lista de botones de salas públicas
    private void ActualizarListaSalas()
    {
        if (contenedorListaSalas == null) return;

        foreach (Transform hijo in contenedorListaSalas)
            Destroy(hijo.gameObject);

        foreach (RoomInfo sala in listaActualSalas)
        {
            // Filtrar salas no válidas
            if (sala.RemovedFromList || !sala.IsOpen || !sala.IsVisible) continue;

            object partidaObj;
            bool yaIniciada = sala.CustomProperties.TryGetValue(PROP_PARTIDA_INICIADA, out partidaObj)
                              && (bool)partidaObj;
            if (yaIniciada) continue;

            string mapa = "?";
            object mapaObj;
            if (sala.CustomProperties.TryGetValue(PROP_MAPA, out mapaObj))
                mapa = mapaObj.ToString();

            // — Crear botón dinámicamente —
            GameObject btnObj = new GameObject(
                sala.Name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)
            );
            btnObj.transform.SetParent(contenedorListaSalas, false);

            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
            btnObj.GetComponent<Image>().color = new Color(0.15f, 0.35f, 0.75f);

            // Texto interior
            GameObject txtObj = new GameObject("Txt",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(12, 0);
            txtRt.offsetMax = new Vector2(-12, 0);

            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text      = $"{sala.Name}  {mapa}  {sala.PlayerCount}/{sala.MaxPlayers}";
            tmp.fontSize = 20;
            if (fuentePersonalizada != null) tmp.font = fuentePersonalizada;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color     = Color.white;

            // Acción: unirse
            string nombreRef = sala.Name;
            btnObj.GetComponent<Button>().onClick.AddListener(() => UnirseASalaDesdeBoton(nombreRef));
        }
    }

    private void UnirseASalaDesdeBoton(string nombreSala)
    {
        PhotonNetwork.JoinRoom(nombreSala);
        txtBarraEstado.text = $"Uniéndose a {nombreSala}...";
    }

    // ==================== PANEL 5: DENTRO DE SALA ====================

    public void Pulsar_BtnSalirDeSala()
    {
        PhotonNetwork.LeaveRoom();
        txtBarraEstado.text = "Saliendo de la sala...";
    }

    public void Pulsar_BtnIniciarPartida()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            txtBarraEstado.text = "Solo el host puede iniciar la partida";
            return;
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount < MIN_JUGADORES)
        {
            txtBarraEstado.text = $"Se necesitan mínimo {MIN_JUGADORES} jugadores";
            return;
        }

        // Cerrar sala para que nadie más pueda entrar
        PhotonNetwork.CurrentRoom.IsOpen    = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        AsignarRolesAleatorios();
    }

    // Kickea a un jugador (solo el MasterClient puede llamarlo)
    private void KickJugador(Player jugador)
    {
        if (PhotonNetwork.IsMasterClient && jugador != PhotonNetwork.LocalPlayer)
        {
            txtBarraEstado.text = $"Expulsando a {jugador.NickName}...";
            // Enviamos el RPC solo al jugador objetivo
            photonView.RPC("RPC_SerExpulsado", jugador);
        }
    }

    // Reconstruye la lista de jugadores como botones
    // El host ve dos botones por jugador: uno para expulsar y otro para asignarle el rol de Slime
    // Los jugadores normales solo ven los nombres sin interacción
    private void ActualizarBotonesJugadores()
    {
        if (contenedorJugadores == null) return;

        foreach (Transform hijo in contenedorJugadores)
            Destroy(hijo.gameObject);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Contenedor horizontal para agrupar el botón de nombre y el botón de rol Slime
            GameObject fila = new GameObject("Fila_" + player.NickName,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                typeof(HorizontalLayoutGroup));
            fila.transform.SetParent(contenedorJugadores, false);
            fila.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
            fila.GetComponent<Image>().color = Color.clear;

            HorizontalLayoutGroup hlg = fila.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.padding = new RectOffset(4, 4, 4, 4);
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;
            hlg.childForceExpandHeight = true;

            // ── Botón principal (nombre / kick) ──
            GameObject btnObj = new GameObject("Btn_" + player.NickName,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button),
                typeof(LayoutElement));
            btnObj.transform.SetParent(fila.transform, false);
            btnObj.GetComponent<LayoutElement>().flexibleWidth = 1; // Ocupa todo el espacio sobrante

            // Color según estado del jugador en la sala
            bool esSlimeElegido = player.ActorNumber == slimeElegidoActorNumber;
            Color color;
            if      (esSlimeElegido)                      color = new Color(0.7f,  0.2f,  0.7f);  // Morado: elegido como Slime
            else if (player == PhotonNetwork.LocalPlayer) color = new Color(0.2f,  0.65f, 0.3f);  // Verde: yo
            else if (player.IsMasterClient)               color = new Color(0.75f, 0.55f, 0.1f);  // Dorado: host
            else                                          color = new Color(0.25f, 0.25f, 0.45f); // Azul: otro jugador
            btnObj.GetComponent<Image>().color = color;

            // Texto del botón principal
            GameObject txtObj = new GameObject("Txt", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(10, 0); txtRt.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            bool puedeKickear = PhotonNetwork.IsMasterClient && player != PhotonNetwork.LocalPlayer;
            string etiqueta   = player.IsMasterClient ? " (Host)" : "";
            string hint       = puedeKickear ? "  [Expulsar]" : "";
            tmp.text      = player.NickName + etiqueta + hint;
            tmp.fontSize  = 20;
            if (fuentePersonalizada != null) tmp.font = fuentePersonalizada;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color     = Color.white;

            Button btnPrincipal = btnObj.GetComponent<Button>();
            if (puedeKickear)
            {
                Player playerRef = player;
                btnPrincipal.onClick.AddListener(() => KickJugador(playerRef));
            }
            else
            {
                btnPrincipal.interactable = false;
            }

            // ── Botón de asignación de rol Slime (solo visible para el MasterClient) ──
            // Pulsarlo marca a ese jugador como Slime; pulsarlo de nuevo lo desmarca (vuelve a aleatorio)
            // Solo puede haber un Slime elegido a la vez: al elegir uno nuevo se sobreescribe el anterior
            if (PhotonNetwork.IsMasterClient)
            {
                GameObject btnSlimeObj = new GameObject("BtnSlime_" + player.NickName,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button),
                    typeof(LayoutElement));
                btnSlimeObj.transform.SetParent(fila.transform, false);
                btnSlimeObj.GetComponent<LayoutElement>().preferredWidth = 80; // Ancho fijo para el botón de rol

                // Color más claro si este jugador ya está marcado como Slime
                btnSlimeObj.GetComponent<Image>().color = esSlimeElegido
                    ? new Color(0.9f, 0.3f, 0.9f)  // Morado claro: marcado
                    : new Color(0.4f, 0.1f, 0.4f); // Morado oscuro: sin marcar

                GameObject txtSlime = new GameObject("Txt", typeof(RectTransform),
                    typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                txtSlime.transform.SetParent(btnSlimeObj.transform, false);
                RectTransform tsRt = txtSlime.GetComponent<RectTransform>();
                tsRt.anchorMin = Vector2.zero; tsRt.anchorMax = Vector2.one;
                tsRt.offsetMin = Vector2.zero; tsRt.offsetMax = Vector2.zero;

                TextMeshProUGUI tmpS = txtSlime.GetComponent<TextMeshProUGUI>();
                tmpS.text      = "Slime";
                tmpS.fontSize  = 16;
                if (fuentePersonalizada != null) tmpS.font = fuentePersonalizada;
                tmpS.alignment = TextAlignmentOptions.Center;
                tmpS.color     = Color.white;

                // Toggle: si ya estaba elegido se desmarca (-1 = aleatorio), si no se marca
                int actorRef = player.ActorNumber;
                btnSlimeObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    slimeElegidoActorNumber = (slimeElegidoActorNumber == actorRef) ? -1 : actorRef;
                    string nombre = PhotonNetwork.CurrentRoom.GetPlayer(slimeElegidoActorNumber)?.NickName ?? "Aleatorio";
                    txtBarraEstado.text = slimeElegidoActorNumber == -1
                        ? "Slime: Aleatorio"
                        : $"Slime elegido: {nombre}";
                    ActualizarBotonesJugadores(); // Redibujar para reflejar el cambio de color
                });
            }
        }
    }

    // ==================== LÓGICA DE JUEGO ====================

    private void AsignarRolesAleatorios()
    {
        txtBarraEstado.text = "Asignando roles...";

        List<int> actores = new List<int>();
        foreach (Player p in PhotonNetwork.PlayerList)
            actores.Add(p.ActorNumber);

        int slimeActorNumber;

        if (slimeElegidoActorNumber != -1 && actores.Contains(slimeElegidoActorNumber))
        {
            // El host ha elegido manualmente quién será el Slime
            slimeActorNumber = slimeElegidoActorNumber;
        }
        else
        {
            // Nadie elegido: Fisher-Yates shuffle y cogemos el primero
            System.Random rng = new System.Random();
            for (int i = actores.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int tmp = actores[j]; actores[j] = actores[i]; actores[i] = tmp;
            }
            slimeActorNumber = actores[0];
        }

        // Reordenar la lista para que el Slime quede siempre en el índice 0
        actores.Remove(slimeActorNumber);
        actores.Insert(0, slimeActorNumber);

        // Leer mapa guardado en la sala
        string mapaACargar = MAPAS[0];
        object mapaObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PROP_MAPA, out mapaObj))
        {
            string mapaGuardado = mapaObj.ToString();
            // Solo aquí, al iniciar, resolvemos el aleatorio
            mapaACargar = (mapaGuardado == "Aleatorio")
                ? MAPAS[Random.Range(0, MAPAS.Length)]
                : mapaGuardado;
        }

        // Marcar partida iniciada en propiedades
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { PROP_SLIME_PLAYER,     slimeActorNumber },
            { PROP_PARTIDA_INICIADA, true             }
        });

        // Distribuir roles vía RPC
        for (int i = 0; i < actores.Count; i++)
        {
            RolJugador rol = (i == 0) ? RolJugador.Slime : RolJugador.Raton;
            photonView.RPC("RPC_AsignarRol", RpcTarget.All, actores[i], (int)rol);
        }

        // Solo el MasterClient carga la escena (AutomaticallySyncScene la propaga a todos)
        StartCoroutine(EsperarYCargarEscena(mapaACargar));
    }

    [PunRPC]
    private void RPC_AsignarRol(int actorNumber, int rolInt)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            miRol = (RolJugador)rolInt;
            Debug.Log($"Mi rol: {miRol}");
        }
    }

    [PunRPC]
    private void RPC_SerExpulsado()
    {
        txtBarraEstado.text = "Has sido expulsado de la sala";
        PhotonNetwork.LeaveRoom();
    }

    private IEnumerator EsperarYCargarEscena(string mapa)
    {
        yield return new WaitForSeconds(1f);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(mapa);
    }

    private void ActualizarInfoSala()
    {
        if (!PhotonNetwork.InRoom) return;

        txtNombreSala.text = "Sala: " + PhotonNetwork.CurrentRoom.Name;
        txtCapacidad.text  = $"Jugadores: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";

        ActualizarBotonesJugadores();

        // Info roles / estado
        object slimeObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PROP_SLIME_PLAYER, out slimeObj))
            txtInfoRoles.text = ((int)slimeObj != -1) ? "Roles asignados" : "Esperando jugadores...";

        // Botón iniciar
        if (btnIniciarPartida != null)
        {
            object pObj;
            bool yaIniciada = PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PROP_PARTIDA_INICIADA, out pObj)
                              && (bool)pObj;
            btnIniciarPartida.interactable = PhotonNetwork.IsMasterClient
                                             && PhotonNetwork.CurrentRoom.PlayerCount >= MIN_JUGADORES
                                             && !yaIniciada;
        }
    }

    // ==================== CALLBACKS DE PHOTON ====================

    public override void OnConnectedToMaster()
    {
        txtBarraEstado.text = "Conectado a Photon";
        txtBienvenida.text  = $"¡Bienvenido {PhotonNetwork.NickName}!";
        ActivarPaneles(1);
        PhotonNetwork.JoinLobby(); // Imprescindible para recibir OnRoomListUpdate
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Lobby unido, esperando lista de salas");
    }

    // Photon nos envía actualizaciones incrementales de la lista de salas
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            listaActualSalas.RemoveAll(r => r.Name == info.Name);
            if (!info.RemovedFromList)
                listaActualSalas.Add(info);
        }

        if (paneles[3].activeSelf) // Sólo redibujar si el panel 4 está visible
            ActualizarListaSalas();
    }

    public override void OnJoinedRoom()
    {
        txtBarraEstado.text = "En sala: " + PhotonNetwork.CurrentRoom.Name;
        ActivarPaneles(4);
        ActualizarInfoSala();
    }

    public override void OnLeftRoom()
    {
        txtBarraEstado.text = "Has salido de la sala";
        miRol = RolJugador.SinAsignar;
        slimeElegidoActorNumber = -1; // Resetear elección de Slime al salir de la sala
        ActivarPaneles(1); // → Bienvenida (nombre ya guardado en PhotonNetwork.NickName)
        if (PhotonNetwork.IsConnected && !PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ActualizarInfoSala();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Si el jugador que se fue era el Slime elegido, resetear la selección
        if (otherPlayer.ActorNumber == slimeElegidoActorNumber)
        {
            slimeElegidoActorNumber = -1;
            txtBarraEstado.text = "El jugador elegido como Slime ha salido, se asignara aleatoriamente";
        }
        ActualizarInfoSala();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        ActualizarInfoSala();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Sala creada");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        txtBarraEstado.text = "Error al crear sala: " + message;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // 32758 = sala cerrada (partida iniciada) | 32765 = sala llena
        switch (returnCode)
        {
            case 32758: txtBarraEstado.text = "No puedes entrar: la partida ya ha empezado"; break;
            case 32765: txtBarraEstado.text = "No puedes entrar: sala llena";                break;
            default:    txtBarraEstado.text = "Error al unirse: " + message;                 break;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        txtBarraEstado.text = "Desconectado: " + cause;
        listaActualSalas.Clear();
        ActivarPaneles(0);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // El host original se fue, todos abandonan
        txtBarraEstado.text = "El host ha abandonado la sala";
        PhotonNetwork.LeaveRoom();
    }

    // ==================== PÚBLICO ====================

    public RolJugador ObtenerMiRol() => miRol;
}