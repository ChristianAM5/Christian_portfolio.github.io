using System;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

[Serializable]
public class CrossbowOptions
{
    public GameObject crossbow;
    public GameObject arrowPrefab; // Se mantiene para el modo offline
    public float arrowSpeed = 70f;
    public TextMeshProUGUI flechasText;

    public int arrows;
    public bool canShoot;


    public void Disparo(CapsuleCollider capsule, Camera cam)
    {
        arrows--;
        flechasText.text = arrows.ToString();

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * 0.5f;
        Vector3 direction = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).direction;

        if (direction == Vector3.zero) direction = cam.transform.forward;

        GameObject projectile;

        // Si está conectado a una sala de Photon, modo online
        if (PhotonNetwork.InRoom)
        {
            projectile = PhotonNetwork.Instantiate(
                "palillo_projectil Variant", 
                spawnPos, 
                Quaternion.LookRotation(direction)
            );
        }
        else
        {
            // Modo offline → instancia local normal
            projectile = UnityEngine.Object.Instantiate(
                arrowPrefab, 
                spawnPos, 
                Quaternion.LookRotation(direction)
            );
        }

        Physics.IgnoreCollision(projectile.GetComponent<Collider>(), capsule);
        projectile.GetComponent<Rigidbody>().velocity = direction * arrowSpeed;
    }

    public void Recoger()
    {
        arrows++;
        flechasText.text = arrows.ToString();
    }

    // public void ActualizarVisibilidad() => crossbow.SetActive(arrows > 0);
}