using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;                          

// CANVAS SlimeKills ACTIVADO con RawImage DESACTIVADO como hijo en la escena para que funcione.

public class SlimeKills : MonoBehaviour
{
    private Animator slimeAttack;
    SlimeStatus slimeStatus;

    private void Awake()
    {
        slimeAttack = GetComponent<Animator>();
        slimeStatus = GetComponent<SlimeStatus>();
    }

    public void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag != "Player" || slimeStatus.isStunned) return;

        var player = collision.gameObject
            .GetComponent<UnityStandardAssets.Characters.FirstPerson
            .RigidbodyFirstPersonController>();

        if (player == null || player.isDead) return;

        slimeAttack.SetTrigger("Attack");

        if (player == null) return;

        if (PhotonNetwork.IsConnected)
        {
            // Le decimos al CLIENTE DEL RATÓN que ejecute su muerte
            PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();
            if (targetView != null)
                targetView.RPC("RPC_Morir", RpcTarget.All); // Decirle a todos que este raton a muerto
        }
        else
        {
            player.Morir(); // Offline: llamada directa
        }
    }
}