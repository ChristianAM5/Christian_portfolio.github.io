using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Cuando la recompensa spawnerActivo esta desbloqueada.
// Este script genera NPCs enemigos cada cierto tiempo.
// Solo aparece uno a la vez y en zonas desbloqueadas.

public class DuelEnemySpawner : MonoBehaviour
{
    [Header("Prefab NPC enemigo")]
    public GameObject enemyPrefab;

    [Header("Tiempo spawn (segundos)")]
    public float tiempoMin = 300f;
    public float tiempoMax = 600f;

    private bool spawnerActivo = false;
    private GameObject enemigoActual;


    // Activa el script desde RiddleRewardManager
    public void ActivarSpawner()
    {
        if (spawnerActivo) return;
        spawnerActivo = true;
        StartCoroutine(LoopSpawn());
    }

    // Corrutina infinita que espera un tiempo, spawnea al enemigo y espera que desaparezca
    private IEnumerator LoopSpawn()
    {
        while (true)
        {
            float espera = Random.Range(tiempoMin, tiempoMax);
            yield return new WaitForSeconds(espera);

            yield return SpawnEnZonaDesbloqueada();

            // Esperar a que el NPC desaparezca
            while (enemigoActual != null)
                yield return null;
        }
    }

    // Busca las zonas desbloqueadas en las que spawnear al enemigo
    private IEnumerator SpawnEnZonaDesbloqueada()
    {
        // Obtenemos lista de zonas donde se puede spawnear
        List<PolygonCollider2D> zonasValidas = ObtenerZonasDesbloqueadas();

        if (zonasValidas.Count == 0)
            yield break;

        // Elegimos una zona al azar
        PolygonCollider2D zona = zonasValidas[Random.Range(0, zonasValidas.Count)];

        // Posición = centro de la zona
        Vector2 pos = zona.bounds.center;

        // Creamos el enemigo en el mundo
        enemigoActual = Instantiate(enemyPrefab, pos, Quaternion.identity);

        // Creamos notificacion en pantalla
        ItemPickupUIController.Instance?.ShowItemPickup(
            "Rata Salvaje Aparecio", null, 0);

        yield return null; // Espera 1 frame
    }

    // Busca todos los colliders de zonas y filtra
    // solo las que el jugador tiene desbloqueadas
    private List<PolygonCollider2D> ObtenerZonasDesbloqueadas()
    {
        List<PolygonCollider2D> lista = new List<PolygonCollider2D>();

        // Manager que sabe qué zonas están desbloqueadas
        ZoneUnlockManager zum = ZoneUnlockManager.Instance;

        // Buscamos todos los colliders del mapa
        PolygonCollider2D[] colliders = FindObjectsByType<PolygonCollider2D>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var col in colliders)
        {
            // Si la zona está desbloqueada, la añadimos
            if (zum.EstaDesbloqueada(col.gameObject.name))
                lista.Add(col);
        }

        return lista;
    }

    // Lo llama el NPC cuando muere o desaparece
    public void NotificarDespawn()
    {
        // Marcamos que ya no hay enemigo activo
        enemigoActual = null;
    }
}
