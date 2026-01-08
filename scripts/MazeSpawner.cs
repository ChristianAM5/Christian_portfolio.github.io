using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeSpawner : MonoBehaviour
{
    [Header("References")]
    public MazeGenerator mazeGenerator;
    public GameObject playerAPrefab;
    public GameObject slimePrefab;
    public int slimeCount = 4;
    public GameObject tntPrefab;

    [Header("Player spawn area")]
    public Vector2Int playerSpawnAreaSize = new Vector2Int(3, 3);

    [Header("Slime Settings")]
    public float slimeDistance = 5f;

    [Header("Flechas")]
    public int tntToSpawn = 5;

    private GameObject playerInstance;
    private List<GameObject> slimeInstances = new List<GameObject>();

    [Header("Slime Colors (Order)")]
	public Color[] slimeColors = new Color[]
	{
    	Color.green,
    	Color.blue,
    	new Color(0.6f, 0f, 1f), // morado
    	Color.red
	};

    void Start()
    {
        if (mazeGenerator == null)
        {
            mazeGenerator = FindObjectOfType<MazeGenerator>();
            if (mazeGenerator == null)
            {
                Debug.LogError("No se encontró MazeGenerator en la escena.");
                return;
            }
        }

        // Esperar a que todo esté generado
        StartCoroutine(WaitForMazeAndSpawn());
    }

    IEnumerator WaitForMazeAndSpawn()
    {
        // Esperar 2 frames para asegurar que el maze está completamente instanciado
        yield return null;
        yield return null;
        
        // Rebakear NavMesh
        mazeGenerator.RebakeNavMesh();
        
        // Esperar a que el NavMesh esté completamente generado
        yield return new WaitForSeconds(1f);
        
        // AHORA spawneamos todo
        yield return StartCoroutine(DoSpawnNextFrame());
    }

    IEnumerator DoSpawnNextFrame()
    {
        List<Vector3> floors = mazeGenerator.GetAllFloorPositions();
        if (floors.Count == 0)
        {
            Debug.LogError("No se encontraron posiciones de piso.");
            yield break;
        }

        // Spawn jugador
        Vector3 playerPos = FindPlayerSpawn(floors);
        playerInstance = Instantiate(playerAPrefab, playerPos + Vector3.up * 2f, Quaternion.identity);

        // Spawn slimes en círculo
        for (int i = 0; i < slimeCount; i++)
        {
            float angle = i * Mathf.PI * 2f / slimeCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * slimeDistance;
            Vector3 desiredPos = playerPos + offset;

            Vector3 nearestFloor = floors[0];
            float minDist = Vector3.Distance(desiredPos, nearestFloor);
            foreach (var f in floors)
            {
                float d = Vector3.Distance(desiredPos, f);
                if (d < minDist)
                {
                    minDist = d;
                    nearestFloor = f;
                }
            }

            GameObject slime = Instantiate(slimePrefab, nearestFloor + Vector3.up * 2f, Quaternion.identity);
slimeInstances.Add(slime);

// --- ASIGNAR COLOR SEGÚN ORDEN ---
SlimeVision v = slime.GetComponent<SlimeVision>();
if (v != null && slimeColors.Length > 0)
{
    // i es el índice del slime → 0,1,2,3
    v.lightColor = slimeColors[i % slimeColors.Length];
}
        }

        SpawnTnts(floors);
    }

    Vector3 FindPlayerSpawn(List<Vector3> floors)
    {
        int tries = 0;
        while (tries < 1000)
        {
            tries++;
            Vector3 candidate = floors[Random.Range(0, floors.Count)];
            bool ok = true;

            float halfX = (playerSpawnAreaSize.x / 2f) * mazeGenerator.cellSize;
            float halfY = (playerSpawnAreaSize.y / 2f) * mazeGenerator.cellSize;

            for (int dx = -playerSpawnAreaSize.x / 2; dx <= playerSpawnAreaSize.x / 2; dx++)
            {
                for (int dy = -playerSpawnAreaSize.y / 2; dy <= playerSpawnAreaSize.y / 2; dy++)
                {
                    Vector3 sample = candidate + new Vector3(dx * mazeGenerator.cellSize, 0f, dy * mazeGenerator.cellSize);
                    bool found = false;
                    foreach (var f in floors)
                    {
                        if (Vector3.Distance(f, sample) < mazeGenerator.cellSize * 0.1f)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        ok = false;
                        break;
                    }
                }
                if (!ok) break;
            }

            if (ok) return candidate;
        }

        return mazeGenerator.CellToWorld(mazeGenerator.width / 2, mazeGenerator.height / 2);
    }

    void SpawnTnts(List<Vector3> floors)
    {
        int spawned = 0;
        int attempts = 0;
        while (spawned < tntToSpawn && attempts < 5000)
        {
            attempts++;
            Vector3 pos = floors[Random.Range(0, floors.Count)];
            if (Vector3.Distance(pos, playerInstance.transform.position) < 3f) continue;

            bool nearSlime = false;
            foreach (var s in slimeInstances)
            {
                if (Vector3.Distance(pos, s.transform.position) < 3f)
                {
                    nearSlime = true;
                    break;
                }
            }
            if (nearSlime) continue;

            Instantiate(tntPrefab, pos + Vector3.up * 0.5f, Quaternion.identity);
            spawned++;
        }
    }
}