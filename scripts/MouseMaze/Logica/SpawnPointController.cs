using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPointController : MonoBehaviourPunCallbacks
{
    public bool isOcupated = false;
    public float respawnTime = 30f; // Tiempo entre spawn de flechas

    [Header("Prefabs")]
    private string prefabFlechaOnline = "palillo_collectible Variant";
    public GameObject prefabFlechaOffline;
    
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
            InvokeRepeating("NewCollectableRound", respawnTime, respawnTime);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (isOcupated) isOcupated = false;
        }
    }
    
    void NewCollectableRound()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;
        
        if (!isOcupated)
        {
            isOcupated = true;
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Instantiate(prefabFlechaOnline, transform.position, Quaternion.identity);
            else
                Instantiate(prefabFlechaOffline, transform.position, Quaternion.identity);
        }
    }
}