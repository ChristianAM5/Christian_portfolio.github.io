using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System.Linq.Expressions;

public class CtrlConexionCONAVATAR_NO_SE_USA : MonoBehaviourPunCallbacks
{
    [Header("Paneles")]
    [SerializeField] GameObject[] paneles;
    /*
    [SerializeField] GameObject panelConexion;
    [SerializeField] GameObject panelBienvenida;
    [SerializeField] GameObject panelCreacionSala;
    */
    [Header("Panel Conexión")]
    [SerializeField] TMP_InputField inputPlayerName;
    [SerializeField] Button btnConectar;

    [Header("Panel Bienvenida")]
    [SerializeField] TextMeshProUGUI txtBienvenida;
    [SerializeField] Button btnCrearSala;
    [SerializeField] Button btnUnirseSala;

    [Header("Panel Creación Sala")]
    [SerializeField] TMP_InputField ifNombreSala;
    [SerializeField] TMP_InputField ifMinPlayers;
    [SerializeField] TMP_InputField ifMaxPlayers;
    [SerializeField] Toggle check_SalaPrivada;


    [Header("Panel Unirse a SalaS")]
    [SerializeField] TMP_InputField ifUnirseASala;

    public GameObject elemSala;
    public GameObject contenedorSala;
    Dictionary<string, RoomInfo> listaSalas;

    [Header("Panel De Sala")]
    [SerializeField] TextMeshProUGUI txtNombreSala;
    [SerializeField] TextMeshProUGUI txtCapacidad;
    [SerializeField] Button btnComenzarJuego;

    public GameObject elemPlayer;
    public GameObject contenedor;


    [SerializeField] TextMeshProUGUI txtListaPlayers;

    [Header("Panel Barra de estado")]
    [SerializeField] TextMeshProUGUI txtBarraEstado;

    //-------------
    [Header("Otros")]
    static public CtrlConexionCONAVATAR_NO_SE_USA conexion;
    [HideInInspector] public int avatarSeleccionado = 0;
    ExitGames.Client.Photon.Hashtable propiedadesPlayer;
    public string nivel = "";

    private void Start()
    {
        listaSalas = new Dictionary<string, RoomInfo>();

        //ActivarPaneles(panelConexion);
        avatarSeleccionado = -1;
        propiedadesPlayer = new ExitGames.Client.Photon.Hashtable();

        conexion = this;

        ActivarPaneles(0);
    }

    #region botones
    public void Pulsar_BtnConectar()
    {

        //Comprobamos si la caja del nickname está vacía
        if(
                !string.IsNullOrEmpty(inputPlayerName.text) &&
                !string.IsNullOrWhiteSpace(inputPlayerName.text)
        )
            //No está vacía, conectamos a Photon
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true;

            PhotonNetwork.NickName = inputPlayerName.text;

            txtBarraEstado.text = "Conecting to PHOTON";

            //ActivarPaneles(panelBienvenida);
            ActivarPaneles(1);
        }
        else 
            //Sí está vacía, pedimos que se meta un nickname
        {
            txtBarraEstado.text = "Please, enter a nickname";
        }
    }
    public void Pulsar_BtnSalir()
    {
        Application.Quit();
    }
    public void Pulsar_btnCrearNuevaSala()
    {
        //ActivarPaneles(panelCreacionSala);
        ActivarPaneles(2);
    }

    public void Pulsar_BtnUnirseASala_Bienvenida()
    {
        ActivarPaneles(3);
    }

    public void Pulsar_UnirseASala()
    {
        
        if (!string.IsNullOrEmpty(ifUnirseASala.text))
        {
            PhotonNetwork.JoinRoom(ifUnirseASala.text);
            Debug.Log("Conectando a sala");
            //PhotonNetwork.JoinOrCreateRoom
        }
        else
        {
            txtBarraEstado.text = "Introduzca un nombre correcto para la sala";
        }
    }

    public void Pulsar_BtnSeleccionarAvatar()
    {
        ActivarPaneles(5); //panel de seleccionar avatar
    }

    public void Pulsar_BtnDesconectarBienvenida()
    {
        PhotonNetwork.Disconnect();
        //ActivarPaneles(panelConexion);
        ActivarPaneles(0);
    }
    /// <summary>
    /// Desde el panel de seleccionar avatar, volvemos al panel debienvenida y guardamos en las propiedades de Photon el avatar seleccionado
    /// </summary>
    public void Pulsar_VolverPanelBienvenidaDesdeAvatar()
    {
        ActivarPaneles(1);

        if(avatarSeleccionado >= 0)
        {
            txtBarraEstado.text = " Avatar seleccionado " + avatarSeleccionado;

            // avtivar los botones de crear sala y unirse a sala
            paneles[1].transform.Find("BtnCrearSala").GetComponent<Button>().interactable = true;
            paneles[1].transform.Find("BtnUnirseSala").GetComponent<Button>().interactable = true;

            //guardar las propiedades de Photon qué avatar se ha seleccionado
            propiedadesPlayer["avatar"] = avatarSeleccionado;
            PhotonNetwork.LocalPlayer.SetCustomProperties(propiedadesPlayer);
        }
        else
        {
            Estado("No se ha seleccionado avatar");
        }
    }

    public void Pulsar_btnCrearSala()
    {
        byte minPlayers;
        byte maxPlayers;

        minPlayers = byte.Parse(ifMinPlayers.text);
        maxPlayers = byte.Parse(ifMaxPlayers.text);

        if(!string.IsNullOrEmpty(ifNombreSala.text))
        {
            if(!(minPlayers > maxPlayers || maxPlayers > 20)
                || minPlayers > 20 || maxPlayers < 2
                || minPlayers < 2
                )
            {
                RoomOptions opcionesSala = new RoomOptions();

                opcionesSala.MaxPlayers = maxPlayers;
                opcionesSala.IsVisible = check_SalaPrivada.isOn;

                PhotonNetwork.CreateRoom(ifNombreSala.text, 
                    opcionesSala, TypedLobby.Default);

                txtBarraEstado.text = "Creando sala " + ifNombreSala.text;
            }
            else
            {
                txtBarraEstado.text = "Valores de capacidad de ssala incorrecta";
            }
        }
        else
        {
            txtBarraEstado.text = "Introduzca nombre de sala";
        }
    }

    /// <summary>
    /// método para el botón "abandonar sala" del panelDeSala
    /// vuelve al panelBienvenida
    /// </summary>
    public void Pulsar_BtnAbandonarSala()
    {
        PhotonNetwork.LeaveRoom();
        ActivarPaneles(1); //panel de bienvenida
    }

    /// <summary>
    /// Abrir la escena del juego con Photon para poder sincronizar entre la gente
    /// </summary>
    public void Pulsar_BtnComenzarJuego()
    {
        PhotonNetwork.LoadLevel(1);
    }
    /// <summary>
    /// Método que se llama desde el botón que se pulsa en el ScrollView del panel de unirse a sala
    /// </summary>
    /// <param name="_nombreSala">sala seleccionada en la lista del panel Unirse a Sala</param>
    public void Pulsar_BtnUnirseASalaDesdeLista(string _nombreSala)
    {
        PhotonNetwork.JoinRoom(_nombreSala);
    }
    #endregion

    #region callback
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        txtBarraEstado.text = "Succesfully connected to PHOTON";
        txtBienvenida.text = "Welcome, " + PhotonNetwork.NickName;

        PhotonNetwork.JoinLobby(); //Conectarse al lobby por defecto;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //base.OnDisconnected(cause);
        txtBarraEstado.text = "Disconnectod from Photon " + cause;
    }

    public override void OnCreatedRoom()
    {
        //base.OnCreatedRoom();

        txtBarraEstado.text = PhotonNetwork.NickName + " se ha conectado a " + 
                            PhotonNetwork.CurrentRoom.Name;

        ActivarPaneles(4);
        ActualizarPanelDeSala();
    }
    public override void OnJoinedRoom()
    {
        //base.OnCreatedRoom();

        txtBarraEstado.text = PhotonNetwork.NickName + " se ha conectado a " +
                            PhotonNetwork.CurrentRoom.Name;

        ActivarPaneles(4);
        ActualizarPanelDeSala();
    }
    /// <summary>
    /// Se ejecuta cada vez que se entra en la sala
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //base.OnPlayerEnteredRoom(newPlayer);
        ActualizarPanelDeSala();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //borrar la sala de la lista que no se encuentra o no es visible por el momento
        foreach (RoomInfo sala in roomList)
        {
            if(sala.RemovedFromList || !sala.IsOpen ||  !sala.IsVisible)
            {
                listaSalas.Remove(sala.Name);
            }

            //comprobando que la sala se ha modificado
            if (listaSalas.ContainsKey(sala.Name))
            {
                if(sala.PlayerCount > 0)
                    listaSalas[sala.Name] = sala;
                else //si se ha quedado sin gente, la borramos
                    listaSalas.Remove(sala.Name) ;
            }
            else //es una nueva sala
                listaSalas.Add(sala.Name, sala);
        }
        ActualizarPanelUnirseASala();
    }

    #endregion

    void ActivarPaneles(int _panel)
    {
        for (int i = 0; i < paneles.Length; i++)
            paneles[i].SetActive(false);

        paneles[_panel].SetActive(true);
    }

    void ActualizarPanelDeSala()
    {
        string cadena = "";

        txtNombreSala.text = "Sala: " + PhotonNetwork.CurrentRoom.Name;
        txtCapacidad.text = "Capacidad: " + PhotonNetwork.CurrentRoom.PlayerCount
                            + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            cadena = cadena + player.NickName + "\n";
        }
        txtListaPlayers.text = cadena;

        while (contenedor.transform.childCount > 0)
        {
            DestroyImmediate(contenedor.transform.GetChild(0).gameObject);
        }

        foreach(Player p in PhotonNetwork.PlayerList)
        {
            GameObject nuevoElemento = Instantiate(elemPlayer);
            nuevoElemento.transform.SetParent(contenedor.transform);
            nuevoElemento.transform.Find("Txt_Player_Nickname").GetComponent<TextMeshProUGUI>().text = p.NickName;

            //Nombre del avatar - recuperamos la propiedad avatar
            object avatarPlayer = p.CustomProperties["avatar"];
            string avatar = "";

            switch((int)avatarPlayer)
            {
                case 0:
                    avatar = "Elvis";
                    break;
                case 1:
                    avatar = "Yaya";
                    break;
                case 2:
                    avatar = "Zombie";
                    break;
                default:
                    avatar = "No avatar selected";
                    break;
            }

            nuevoElemento.transform.Find("Txt_Avatar_Name").GetComponent<TextMeshProUGUI>().text = avatar;
        }

        //Comprobar que el número de personas en la sala es el mínimo para jugar
        if(PhotonNetwork.CurrentRoom.PlayerCount >= int.Parse(ifMinPlayers.text) && PhotonNetwork.IsMasterClient)
        {
            btnComenzarJuego.gameObject.SetActive(true);
        }
        else
        {
            btnComenzarJuego.gameObject.SetActive(false);
        }

    }

    public void ActualizarPanelUnirseASala()
    {
        //eliminar los prefabs que hacen referencia a las salas
        while (contenedorSala.transform.childCount > 0)
        {
            DestroyImmediate(contenedorSala.transform.GetChild(0).gameObject) ;
        }

        foreach(RoomInfo sala in listaSalas.Values)
        {
            GameObject nuevoElemento = Instantiate(elemSala);
            nuevoElemento.transform.SetParent (contenedorSala.transform, false);

            //Localizamos sus etiquetas y las actualizamos
            nuevoElemento.transform.Find("Txt_Sala_Nombre").GetComponent<TextMeshProUGUI>().text = sala.Name;

            nuevoElemento.transform.Find("Txt_Capacidad").GetComponent<TextMeshProUGUI>().text = sala.PlayerCount + "/" + sala.MaxPlayers;

            nuevoElemento.GetComponent<Button>().onClick.AddListener(()
                => { Pulsar_BtnUnirseASalaDesdeLista(sala.Name); });
        }
    }

    void Estado(string _mensaje)
    {
        txtBarraEstado.text = _mensaje;
        Debug.Log(_mensaje);
    }
}
