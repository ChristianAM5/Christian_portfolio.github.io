using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPunCallbacks
{

    [Header("Prefabs Offline")]
    public GameObject prefabTNT_Offline;
    public GameObject prefabFlecha_Offline;

    public GameObject[] spawnPoint;

    [Header("Prefabs Online (Resources)")]
    private string prefabTNT = "Dinamita Variant";
    private string prefabFlecha = "palillo_collectible Variant";

    [Header("Configuracion")]
    public int cantidadTNTs = 8; // Numero de tnts para ganar

    public static int totalTNTsEnMapa = 0;

    void Start()
    {
        spawnPoint = GameObject.FindGameObjectsWithTag("SpwanPoint_Tnt");

	    // Online: solo el master spawnea y avisa a todos por RPC
        // Offline: este cliente es el único, spawnea directamente
        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
        {
            GetPosition();
        }
    }

    void GetPosition()
    {
        totalTNTsEnMapa = 0;

        // Resetear todos los puntos al inicio
        foreach (var sp in spawnPoint)
        {
            var ctrl = sp.GetComponent<SpawnPointController>();
            if (ctrl != null) ctrl.isOcupated = false;
        }

        // Spawnear TNTs
        SpawnItems(cantidadTNTs, (pos, rot) =>
        {
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Instantiate(prefabTNT, pos, Quaternion.identity);
            else // OFFLINE
                Instantiate(prefabTNT_Offline, pos, Quaternion.identity);
            totalTNTsEnMapa++;
        });

        // El resto de puntos libres se llenan de flechas
        int flechas = spawnPoint.Length - cantidadTNTs;
        SpawnItems(flechas, (pos, rot) =>
        {
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Instantiate(prefabFlecha, pos, Quaternion.identity);
            else // OFFLINE
                Instantiate(prefabFlecha_Offline, pos, Quaternion.identity);
        });

        // Online: avisar a todos por RPC
        // Offline: asignar directamente, no hay RPC
        if (PhotonNetwork.IsConnected)
            photonView.RPC("RPC_SetTotalTNTs", RpcTarget.AllBuffered, totalTNTsEnMapa);
        else
            RPC_SetTotalTNTs(totalTNTsEnMapa); // llamada directa
    }

    // Metodo generico para spawnear N items evitando bucle infinito
    void SpawnItems(int cantidad, System.Action<Vector3, Quaternion> spawnAction)
    {
        int spawneados = 0;
        int intentos = 0;
        int maxIntentos = spawnPoint.Length * 10; // limite de seguridad

        while (spawneados < cantidad && intentos < maxIntentos)
        {
            intentos++;
            int index = Random.Range(0, spawnPoint.Length);
            var controller = spawnPoint[index].GetComponent<SpawnPointController>();

            if (controller != null && !controller.isOcupated)
            {
                controller.isOcupated = true;
                spawnAction(spawnPoint[index].transform.position, spawnPoint[index].transform.rotation);
                spawneados++;
            }
        }

        if (spawneados < cantidad)
        {
            Debug.LogWarning($"[SpawnManager] Solo se pudieron spawnear {spawneados}/{cantidad} items. No habia suficientes puntos libres.");
        }
    }

    [PunRPC]
    void RPC_SetTotalTNTs(int cantidad)
    {
        totalTNTsEnMapa = cantidad;
        Debug.Log($"Total de TNTs en el mapa: {totalTNTsEnMapa}");
    }
    
    [PunRPC]
    public void RPC_DestruirObjeto(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
            PhotonNetwork.Destroy(pv.gameObject);
    }
}