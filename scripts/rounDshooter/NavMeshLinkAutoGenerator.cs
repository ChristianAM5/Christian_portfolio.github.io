using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;
 
/// <summary>
/// Escanea la escena al inicio y genera NavMeshLinks automáticamente en los bordes
/// de superficies elevadas (pilares, plataformas, etc.) para que los NavMeshAgents
/// puedan subir y bajar sin configuración manual.
/// </summary>

public class NavMeshLinkAutoGenerator : MonoBehaviour
{
    [Header("Área de Escaneo")]
    [Tooltip("Centro del mapa a nivel del suelo (Y = 0 normalmente).")]
    [SerializeField] private Vector3 scanCenter = Vector3.zero;
    [Tooltip("Ancho y largo del área escaneada.")]
    [SerializeField] private Vector2 scanSize = new Vector2(60f, 60f);
    [Tooltip("Separación del grid. 0.5–1 m es lo ideal.")]
    [SerializeField] private float gridStep = 0.75f;
    [Tooltip("Y desde donde se lanzan raycasts. Debe superar todo el mapa.")]
    [SerializeField] private float raycastStartY = 30f;
 
    [Header("Detección de Bordes")]
    [Tooltip("Desnivel mínimo entre celdas vecinas para crear un link.")]
    [SerializeField] private float minDropHeight = 0.8f;
    [Tooltip("Radio de búsqueda NavMesh. Si ves zonas sin links, súbelo.")]
    [SerializeField] private float navMeshSampleRadius = 1.0f;
    [Tooltip("Distancia mínima entre links (evita duplicados).")]
    [SerializeField] private float minLinkSpacing = 1.0f;
 
    [Header("NavMeshLink")]
    [SerializeField] private float linkWidth = 0.8f;
    [Tooltip("Bidireccional = los agentes también pueden subir.")]
    [SerializeField] private bool bidirectional = true;
 
    [Header("Debug")]
    [SerializeField] private bool verboseDebug = false;
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color linkGizmoColor = new Color(0f, 1f, 1f, 1f);
 
    private readonly List<(Vector3 top, Vector3 bottom)> generatedLinks = new();
    private Dictionary<(int, int), float> heightCache = new();
 
    // 8 direcciones: cardinal + diagonal
    private static readonly Vector2Int[] Neighbors =
    {
        new( 1,  0), new(-1,  0),
        new( 0,  1), new( 0, -1),
        new( 1,  1), new(-1,  1),
        new( 1, -1), new(-1, -1),
    };
 
    void Start()
    {
        BuildHeightCache();
        int count = GenerateLinks();
        Debug.Log($"[NavMeshLinkAutoGenerator] Links generados: <b>{count}</b>" +
                  (count == 0 ? " — activa Verbose Debug para diagnosticar." : "."));
    }
 
    // ── Paso 1: altura de cada celda ─────────────────────────────────────────
 
    private void BuildHeightCache()
    {
        heightCache.Clear();
        int cols = Mathf.CeilToInt(scanSize.x / gridStep) + 1;
        int rows = Mathf.CeilToInt(scanSize.y / gridStep) + 1;
        float halfX = scanSize.x * 0.5f;
        float halfZ = scanSize.y * 0.5f;
 
        for (int ci = 0; ci < cols; ci++)
        {
            for (int ri = 0; ri < rows; ri++)
            {
                float wx = scanCenter.x - halfX + ci * gridStep;
                float wz = scanCenter.z - halfZ + ri * gridStep;
 
                float height = float.MinValue;
                if (Physics.Raycast(new Vector3(wx, raycastStartY, wz), Vector3.down,
                                    out RaycastHit hit, raycastStartY + 100f))
                    height = hit.point.y;
 
                heightCache[(ci, ri)] = height;
            }
        }
    }
 
    // ── Paso 2: detectar bordes → crear links ─────────────────────────────────
 
    private int GenerateLinks()
    {
        int created = 0;
        int cols = Mathf.CeilToInt(scanSize.x / gridStep) + 1;
        int rows = Mathf.CeilToInt(scanSize.y / gridStep) + 1;
        float halfX = scanSize.x * 0.5f;
        float halfZ = scanSize.y * 0.5f;
 
        for (int ci = 0; ci < cols; ci++)
        {
            for (int ri = 0; ri < rows; ri++)
            {
                float hA = heightCache[(ci, ri)];
                if (hA == float.MinValue) continue;
 
                float wAx = scanCenter.x - halfX + ci * gridStep;
                float wAz = scanCenter.z - halfZ + ri * gridStep;
 
                foreach (var nb in Neighbors)
                {
                    int ni = ci + nb.x;
                    int nj = ri + nb.y;
                    if (ni < 0 || nj < 0 || ni >= cols || nj >= rows) continue;
 
                    float hB = heightCache[(ni, nj)];
                    if (hB == float.MinValue) continue;
 
                    float drop = hA - hB;
                    if (drop < minDropHeight) continue;   // A es la celda ALTA, B la BAJA
 
                    float wBx = scanCenter.x - halfX + ni * gridStep;
                    float wBz = scanCenter.z - halfZ + nj * gridStep;
 
                    // ── Clave: puntos separados horizontalmente ──────────────
                    //   topCandidate    → en la celda ALTA   (A), ligeramente hacia B
                    //   bottomCandidate → en la celda BAJA   (B), ligeramente hacia afuera
                    //   Esto hace el link DIAGONAL y evita que pase por el sólido.
 
                    float t = 0.35f; // Qué fracción nos movemos de A hacia B
 
                    Vector3 topCandidate = new Vector3(
                        Mathf.Lerp(wAx, wBx, t),
                        hA + 0.05f,
                        Mathf.Lerp(wAz, wBz, t)
                    );
 
                    Vector3 bottomCandidate = new Vector3(
                        Mathf.Lerp(wAx, wBx, 1f - t),  // Simétrico: hacia B
                        hB + 0.05f,
                        Mathf.Lerp(wAz, wBz, 1f - t)
                    );
 
                    // Validar NavMesh en la cima
                    if (!NavMesh.SamplePosition(topCandidate, out NavMeshHit navTop,
                                                navMeshSampleRadius, NavMesh.AllAreas))
                    {
                        if (verboseDebug)
                            Debug.Log($"Sin NavMesh CIMA {topCandidate:F1}");
                        continue;
                    }
 
                    // Validar NavMesh en la base
                    if (!NavMesh.SamplePosition(bottomCandidate, out NavMeshHit navBottom,
                                                navMeshSampleRadius, NavMesh.AllAreas))
                    {
                        if (verboseDebug)
                            Debug.Log($"Sin NavMesh BASE {bottomCandidate:F1}");
                        continue;
                    }
 
                    // Descartar si el SamplePosition colocó ambos puntos a la misma altura
                    // (ocurre cuando la plataforma absorbe el punto inferior)
                    if (navTop.position.y - navBottom.position.y < minDropHeight * 0.4f)
                    {
                        if (verboseDebug)
                            Debug.Log($"Puntos colapsaron a misma altura. Ignorado.");
                        continue;
                    }
 
                    // Descartar duplicados
                    if (IsTooClose(navTop.position)) continue;
 
                    CreateLink(navTop.position, navBottom.position);
                    generatedLinks.Add((navTop.position, navBottom.position));
                    created++;
                }
            }
        }
 
        if (verboseDebug)
            Debug.Log($"[NavMeshLinkAutoGenerator] Celdas: {heightCache.Count} | Links: {created}\n" +
                      "Si es 0: baja minDropHeight, sube navMeshSampleRadius o sube raycastStartY.");
 
        return created;
    }
 
    private void CreateLink(Vector3 worldStart, Vector3 worldEnd)
    {
        GameObject go = new GameObject("AutoLink");
        go.transform.SetParent(transform, worldPositionStays: true);
        go.transform.position = worldStart;
 
        NavMeshLink link   = go.AddComponent<NavMeshLink>();
        link.startPoint    = Vector3.zero;
        link.endPoint      = go.transform.InverseTransformPoint(worldEnd);
        link.width         = linkWidth;
        link.bidirectional = bidirectional;
        link.autoUpdate    = false;
        link.agentTypeID   = 0;
    }
 
    private bool IsTooClose(Vector3 point)
    {
        foreach (var (top, _) in generatedLinks)
            if (Vector3.Distance(top, point) < minLinkSpacing) return true;
        return false;
    }
 
    // ── Gizmos ────────────────────────────────────────────────────────────────
 
    private void OnDrawGizmos()
    {
        // Área de escaneo
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.05f);
        Gizmos.DrawCube(new Vector3(scanCenter.x, scanCenter.y, scanCenter.z),
                        new Vector3(scanSize.x, 0.2f, scanSize.y));
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.5f);
        Gizmos.DrawWireCube(new Vector3(scanCenter.x, scanCenter.y, scanCenter.z),
                            new Vector3(scanSize.x, 0.2f, scanSize.y));
 
        if (!showGizmos) return;
 
        foreach (var (top, bottom) in generatedLinks)
        {
            Gizmos.color = linkGizmoColor;
            Gizmos.DrawLine(top, bottom);
            Gizmos.DrawSphere(top, 0.12f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(bottom, 0.09f);
        }
    }
 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawWireCube(new Vector3(scanCenter.x, raycastStartY, scanCenter.z),
                            new Vector3(scanSize.x, 0.2f, scanSize.y));
    }
}