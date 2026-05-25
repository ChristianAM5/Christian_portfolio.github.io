using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GameManager_Network : MonoBehaviourPunCallbacks
{
    public static GameManager_Network Instance;
    
    [Header("Puntos de Spawn")]
    [SerializeField] private Transform[] spawnsRatones; // 4 puntos de spawn para ratones
    [SerializeField] private Transform spawnSlime;      // 1 punto de spawn para control slime
    
    [Header("Prefabs en Resources")]
    // Estos prefabs DEBEN estar en carpeta Resources/
    private string prefabRaton = "RatonV6";
    private string prefabSlimeController = "SlimeController";
    
    [Header("Cámaras")]
    [SerializeField] private Camera camaraAereaSlime; // MainCamera_B
    // Usamos una Lista en lugar de un Array porque es más fácil añadir y quitar elementos dinámicamente
    public static List<Camera> camarasActivas = new List<Camera>();

    private RolJugador miRol;
    private GameObject miJugador;
    private static int contadorRatonesSpawneados = 0;

    public SlimeSpawner slimeSpawner;

    public int tntGlobal = 0; // Variable compartida, conteo de TNT

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        Debug.Log(" GameManager_Network iniciado");
       
        
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log($" Conectado a Photon. En sala: {PhotonNetwork.CurrentRoom.Name}");
            // Obtener mi rol de las propiedades de sala    
            DeterminarMiRol();
            // Cambios especificos para slimes o ratones
            ConfigurarSegunRol();
            // Esperar un frame para que todo esté listo
            Invoke(nameof(SpawnearJugador), 0.5f);
        }
        else
        {
            Debug.LogError(" No estás conectado a Photon o no estás en una sala");
        }

	    if (miRol == RolJugador.Slime)
        {
            slimeSpawner.SpawnSlimes();
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Camaras en la lista: " + camarasActivas.Count);
        }
    }

    private void DeterminarMiRol()
    {
        object slimeActorObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("SlimePlayerActorID", out slimeActorObj))
        {
            int slimeActorNumber = (int)slimeActorObj;
            if (PhotonNetwork.LocalPlayer.ActorNumber == slimeActorNumber)
            {
                miRol = RolJugador.Slime;
            }
            else
            {
                miRol = RolJugador.Raton;
            }
            Debug.Log($" Mi rol en la partida: {miRol}");
        }
        else
        {
            Debug.LogError(" No se encontró información de roles en la sala");
        }
    }

    private void ConfigurarSegunRol()
    {
       	if (miRol == RolJugador.Raton)
       	{
            // El raton apaga la camara aerea del slime
            GameObject cameraBRig = GameObject.Find("Camera_B_Rig");
            GameObject hudFantasma = GameObject.Find("HUD_FANTASMA");
            if (cameraBRig != null)
                cameraBRig.SetActive(false);
            else
                Debug.LogWarning("No se encontro Camera_B_Rig en la escena");
            if (hudFantasma != null)
                hudFantasma.SetActive(false);
            else
                Debug.LogWarning("No se encontro Camera_B_Rig en la escena");
        }
       	else if (miRol == RolJugador.Slime)
       	{
            // El slime apaga la HUD del raton
            GameObject hudRaton = GameObject.Find("HUD_RATON");
            if (hudRaton != null)
                hudRaton.SetActive(false);
            else
                Debug.LogWarning("No se encontro HUD_RATON en la escena");
       	}
    }

    
    private void SpawnearJugador()
    {
        Debug.Log($" SpawnearJugador llamado. Mi rol: {miRol}");
        
        if (miRol == RolJugador.Raton)
        {
            SpawnearRaton();
        }
        else if (miRol == RolJugador.Slime)
        {
            SpawnearSlimeController();
        }
        else
        {
            Debug.LogError($" Rol sin asignar: {miRol}");
        }
    }
    
    private void SpawnearRaton()
    {
        Debug.Log(" Intentando spawnear ratón...");
        
        //  VERIFICACIONES
        if (spawnsRatones == null || spawnsRatones.Length == 0)
        {
            Debug.LogError(" spawnsRatones está vacío o null. Asigna los spawn points en el Inspector!");
            return;
        }
        
        int indiceSpawn = contadorRatonesSpawneados % spawnsRatones.Length;
        
        if (spawnsRatones[indiceSpawn] == null)
        {
            Debug.LogError($" Spawn point {indiceSpawn} es null!");
            return;
        }
        
        contadorRatonesSpawneados++;
        Vector3 posicion = spawnsRatones[indiceSpawn].position;
        Quaternion rotacion = spawnsRatones[indiceSpawn].rotation;
        
        Debug.Log($" Spawneando en: {posicion}");
        Debug.Log($" Prefab: Resources/{prefabRaton}");
        
        try
        {
            miJugador = PhotonNetwork.Instantiate(prefabRaton, posicion, rotacion);
            
            if (miJugador != null)
            {
                Debug.Log($" Ratón spawneado exitosamente en posición {indiceSpawn}");
            }
            else
            {
                Debug.LogError(" PhotonNetwork.Instantiate devolvió null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($" ERROR al spawnear ratón: {e.Message}");
        }

        Debug.Log("Ratones spawneados: " + contadorRatonesSpawneados);
    }
    
    private void SpawnearSlimeController()
    {
        Debug.Log(" Intentando spawnear controlador slime...");
        
        if (spawnSlime == null)
        {
            Debug.LogError(" spawnSlime es null. Asígnalo en el Inspector!");
            return;
        }
        
        Vector3 posicion = spawnSlime.position;
        Quaternion rotacion = spawnSlime.rotation;
        
        Debug.Log($" Spawneando en: {posicion}");
        Debug.Log($" Prefab: Resources/{prefabSlimeController}");
        
        try
        {
            miJugador = PhotonNetwork.Instantiate(prefabSlimeController, posicion, rotacion);
            
            if (miJugador != null)
            {
                Debug.Log(" Controlador Slime spawneado exitosamente");
            }
            else
            {
                Debug.LogError(" PhotonNetwork.Instantiate devolvió null");
            }
            
            if (camaraAereaSlime != null)
            {
                camaraAereaSlime.gameObject.SetActive(true);
                Debug.Log(" Cámara aérea activada");
            }
            else
            {
                Debug.LogError(" camaraAereaSlime es null. Asígnala en el Inspector!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($" ERROR al spawnear slime controller: {e.Message}");
        }
    }
    
    public RolJugador ObtenerMiRol()
    {
        return miRol;
    }

    public static int BalanceoFlechas()
    {
        int arrows;
        if(contadorRatonesSpawneados < 2)
        {
            arrows = 3;
        }
        else if(contadorRatonesSpawneados == 2 && contadorRatonesSpawneados == 3)
        {
            arrows = 2;
        }
        else
        {
            arrows = 1;
        }

        return arrows;
    }

    /*[PunRPC]
    public void RPC_SumarTNTGlobal()
    {
        // Buscamos al jugador local en la escena para actualizar SU interfaz
        // Nota: Suponiendo que tu jugador tiene el tag "Player"
        GameObject jugadorLocal = null;
        foreach (var pv in PhotonNetwork.PhotonViewAndNetworkObjects)
        {
            if (pv.IsMine && pv.CompareTag("Player"))
            {
                jugadorLocal = pv.gameObject;
                break;
            }
        }

        if (jugadorLocal != null)
        {
            var controller = jugadorLocal.GetComponent<RigidbodyFirstPersonController>();
            controller.tnt++;
            controller.dinamitasText.text = controller.tnt + "/" + SpawnManager.Instance.totalTNTsEnMapa;
        }
    }*/

    [PunRPC]
    public void ActualizarTNTUI()
    {
        // Aumentamos el contador global en todos los ordenadores
        tntGlobal++;
        Debug.Log($"TNT recogida globalmente. Total: {tntGlobal}");

        // Ahora buscamos al jugador local de este ordenador para actualizar SU pantalla
        // Usamos FindObjectsOfType para encontrar a todos y filtrar por IsMine
        RigidbodyFirstPersonController[] todosLosJugadores = FindObjectsOfType<RigidbodyFirstPersonController>();

        foreach (var player in todosLosJugadores)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine) // Solo yo
            {
                // Actualizamos el texto de SU pantalla con el valor GLOBAL
                player.dinamitasText.text = tntGlobal + "/" + SpawnManager.totalTNTsEnMapa;
                break;
            }
        }
    }
}