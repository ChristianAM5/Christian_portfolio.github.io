using UnityEngine;
using Photon.Pun;

public class SlimeSpawner : MonoBehaviour
{
    public Transform[] slimeSpawnPoints;

    public void SpawnSlimes()
    {
        string[] slimePrefabs =
        {
            "Slime_Animation Azul Variant",
            "Slime_Animation Morado Variant",
            "Slime_Animation Rojo Variant",
            "Slime_Animation Verde Variant"
        };

        for (int i = 0; i < slimePrefabs.Length; i++)
        {
            PhotonNetwork.Instantiate(
                slimePrefabs[i],
                slimeSpawnPoints[i].position,
                slimeSpawnPoints[i].rotation
            );
        }
    }
}