using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze size (cells)")]
    public int width = 40;
    public int height = 40;
    public float cellSize = 1f;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject exitPrefab;

    [Header("Maze options")]
    public int seed = 0;
    public bool randomSeed = true;

    [HideInInspector] public List<Vector3> floorPositions = new List<Vector3>();
    [HideInInspector] public Vector2Int exitCell;

    private bool[,] visited;
    private System.Random rng;
    private Transform wallsParent;
    private Transform floorsParent;
    private NavMeshSurface navMeshSurface;

    void Start()
    {
        GenerateMaze();
    }

    public void GenerateMaze()
    {
        rng = randomSeed ? new System.Random() : new System.Random(seed);
        visited = new bool[width, height];

        // Limpiar objetos anteriores si existen
        if (wallsParent != null) Destroy(wallsParent.gameObject);
        if (floorsParent != null) Destroy(floorsParent.gameObject);

        wallsParent = new GameObject("Maze_Walls").transform;
        wallsParent.parent = transform;
        floorsParent = new GameObject("Maze_Floors").transform;
        floorsParent.parent = transform;

        floorPositions.Clear();

        // GENERAR PISOS
        if (floorPrefab != null)
        {
            Vector3 center = transform.position;
            GameObject f = Instantiate(floorPrefab, center, Quaternion.identity, transform);
            
            // El prefab tiene escala (0.1, 1, 0.1) = 1x1 unidad real
            // Necesitamos width × cellSize unidades
            float desiredWidth = width * cellSize;
            float desiredHeight = height * cellSize;
            
            f.transform.localScale = new Vector3(
                0.1f * desiredWidth, 
                1f, 
                0.1f * desiredHeight
            );
            
            // Asegurar que el Box Collider esté activo y configurado
            BoxCollider floorCollider = f.GetComponent<BoxCollider>();
            if (floorCollider == null)
            {
                floorCollider = f.AddComponent<BoxCollider>();
            }
            floorCollider.enabled = true;
            
            // Añadir NavMeshSurface al suelo
            navMeshSurface = f.GetComponent<NavMeshSurface>();
            if (navMeshSurface == null)
            {
                navMeshSurface = f.AddComponent<NavMeshSurface>();
            }
            navMeshSurface.collectObjects = CollectObjects.All;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            floorPositions.Clear();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 pos = CellToWorld(x, y);
                    floorPositions.Add(pos);
                }
            }
        }

        // CARVE MAZE INTERNO
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        int sx = rng.Next(width);
        int sy = rng.Next(height);
        visited[sx, sy] = true;
        stack.Push(new Vector2Int(sx, sy));

        HashSet<(int, int, int, int)> passages = new HashSet<(int, int, int, int)>();

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            int cx = current.x, cy = current.y;
            List<Vector2Int> neighbours = new List<Vector2Int>();

            if (cx > 0 && !visited[cx - 1, cy]) neighbours.Add(new Vector2Int(cx - 1, cy));
            if (cx < width - 1 && !visited[cx + 1, cy]) neighbours.Add(new Vector2Int(cx + 1, cy));
            if (cy > 0 && !visited[cx, cy - 1]) neighbours.Add(new Vector2Int(cx, cy - 1));
            if (cy < height - 1 && !visited[cx, cy + 1]) neighbours.Add(new Vector2Int(cx, cy + 1));

            if (neighbours.Count > 0)
            {
                var next = neighbours[rng.Next(neighbours.Count)];
                passages.Add((cx, cy, next.x, next.y));
                passages.Add((next.x, next.y, cx, cy));
                visited[next.x, next.y] = true;
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }

        // MUROS INTERIORES (sin incluir bordes externos)
        float half = cellSize / 2f;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 center = CellToWorld(x, y);

                // NORTH - solo muros internos
                if (y < height - 1 && !passages.Contains((x, y, x, y + 1)))
                {
                    Vector3 pos = center + new Vector3(0, 0.5f, half);
                    CreateWall(pos, Quaternion.identity, cellSize, wallsParent);
                }

                // EAST - solo muros internos
                if (x < width - 1 && !passages.Contains((x, y, x + 1, y)))
                {
                    Vector3 pos = center + new Vector3(half, 0.5f, 0);
                    CreateWall(pos, Quaternion.Euler(0f, 90f, 0f), cellSize, wallsParent);
                }
            }
        }

        // MUROS PERIMETRALES CON SALIDA
        CreatePerimeterWalls();
    }
    
    public void RebakeNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.RemoveData();
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh rebakeado correctamente!");
        }
        else
        {
            Debug.LogError("No se encontró NavMeshSurface. No se pudo rebakear.");
        }
    }

    void CreateWall(Vector3 pos, Quaternion rot, float length, Transform parent)
    {
        if (wallPrefab == null) return;
        GameObject w = Instantiate(wallPrefab, pos, rot, parent);
        w.transform.localScale = new Vector3(length, w.transform.localScale.y, w.transform.localScale.z);
    }

    void CreatePerimeterWalls()
    {
        float half = cellSize / 2f;
        
        // Elegir lado aleatorio: 0=Norte, 1=Este, 2=Sur, 3=Oeste
        int exitSide = rng.Next(4);
        int exitIndex = 0;
        
        // Elegir índice de salida ANTES de los bucles
        if (exitSide == 0 || exitSide == 2)
        {
            exitIndex = rng.Next(width); // para Norte o Sur
        }
        else
        {
            exitIndex = rng.Next(height); // para Este u Oeste
        }

        // NORTE (y = height - 1)
        for (int x = 0; x < width; x++)
        {
            Vector3 pos = CellToWorld(x, height - 1) + new Vector3(0, 0.5f, half);
            
            if (exitSide == 0 && x == exitIndex)
            {
                exitCell = new Vector2Int(x, height - 1);
                Instantiate(exitPrefab, pos, Quaternion.identity, wallsParent);
            }
            else
            {
                CreateWall(pos, Quaternion.identity, cellSize, wallsParent);
            }
        }

        // SUR (y = 0)
        for (int x = 0; x < width; x++)
        {
            Vector3 pos = CellToWorld(x, 0) + new Vector3(0, 0.5f, -half);
            
            if (exitSide == 2 && x == exitIndex)
            {
                exitCell = new Vector2Int(x, 0);
                Instantiate(exitPrefab, pos, Quaternion.Euler(0, 180, 0), wallsParent);
            }
            else
            {
                CreateWall(pos, Quaternion.identity, cellSize, wallsParent);
            }
        }

        // ESTE (x = width - 1)
        for (int y = 0; y < height; y++)
        {
            Vector3 pos = CellToWorld(width - 1, y) + new Vector3(half, 0.5f, 0);
            
            if (exitSide == 1 && y == exitIndex)
            {
                exitCell = new Vector2Int(width - 1, y);
                Instantiate(exitPrefab, pos, Quaternion.Euler(0, 90, 0), wallsParent);
            }
            else
            {
                CreateWall(pos, Quaternion.Euler(0, 90, 0), cellSize, wallsParent);
            }
        }

        // OESTE (x = 0)
        for (int y = 0; y < height; y++)
        {
            Vector3 pos = CellToWorld(0, y) + new Vector3(-half, 0.5f, 0);
            
            if (exitSide == 3 && y == exitIndex)
            {
                exitCell = new Vector2Int(0, y);
                Instantiate(exitPrefab, pos, Quaternion.Euler(0, 270, 0), wallsParent);
            }
            else
            {
                CreateWall(pos, Quaternion.Euler(0, 90, 0), cellSize, wallsParent);
            }
        }
    }

    public Vector3 CellToWorld(int x, int y)
    {
        Vector3 origin = transform.position;
        float worldX = (x - width / 2f + 0.5f) * cellSize;
        float worldZ = (y - height / 2f + 0.5f) * cellSize;
        return origin + new Vector3(worldX, 0f, worldZ);
    }

    public List<Vector3> GetAllFloorPositions()
    {
        return new List<Vector3>(floorPositions);
    }

    public Vector2Int GetExitCell()
    {
        return exitCell;
    }
}