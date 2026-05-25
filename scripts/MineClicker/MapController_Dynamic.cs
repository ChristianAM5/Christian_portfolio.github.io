using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Se ha modificado para añadir un sprite que identifique a cada zona del mapa

public class MapController_Dynamic : MonoBehaviour
{
    [Header("Referencias UI")]
    public RectTransform mapParent;   // El contenedor donde se dibujarán las áreas del mapa
    public GameObject areaPrefab;     // Un Prefab (con un componente Image) que representará cada área
    public RectTransform playerIcon;  // El icono del jugador en la UI

    [Header("Colores")]
    public Color defaultColour = Color.gray;     // Color de las zonas donde no está el jugador
    public Color currentAreaColour = Color.green; // Color de la zona donde sí está el jugador

    [Header("Map Settings")]
    public GameObject mapBounds;          // Objeto padre en la escena que contiene los colisionadores de las zonas
    public PolygonCollider2D initialArea; // El área donde el jugador empieza la partida
    public float mapScale = 10f;          // Factor de conversión para pasar de unidades de mundo a píxeles de UI

    private PolygonCollider2D[] mapAreas; // Array con todos los colisionadores encontrados
    // Diccionario para encontrar rápidamente el objeto visual (RectTransform) usando el nombre del área
    private Dictionary<string, RectTransform> uiAreas = new Dictionary<string, RectTransform>();

    public static MapController_Dynamic Instance { get; private set; }

    // Clase para mapear zonas
    [System.Serializable]
    public class AreaSprite
    {
        public string areaName;
        public Sprite sprite;
    }

    [Header("Sprites por zona")]
    public List<AreaSprite> areaSprites;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Al arrancar, detecta todas las áreas (PolygonCollider2D) que son hijas de mapBounds
        mapAreas = mapBounds.GetComponentsInChildren<PolygonCollider2D>();
    }

    /// <summary>
    /// Crea visualmente todo el mapa en la interfaz.
    /// </summary>
    /// <param name="newCurrentArea">Opcional: el área donde empezar resaltado.</param>
    public void GenerateMap(PolygonCollider2D newCurrentArea = null)
    {
        // Si no se pasa un área, usa la inicial definida en el inspector
        PolygonCollider2D currentArea = newCurrentArea != null ? newCurrentArea : initialArea;

        // Limpia el mapa anterior para no duplicar imágenes
        ClearMap();

        foreach (PolygonCollider2D area in mapAreas)
        {
            // Crea un elemento visual por cada colisionador de área detectado
            CreateAreaUI(area, area == currentArea);
        }

        // Sitúa el icono del jugador al inicio
        MovePlayerIcon(currentArea.name);
    }

    // Destruye todos los hijos dentro del contenedor del mapa y limpia el diccionario
    private void ClearMap()
    {
        foreach (Transform child in mapParent)
        {
            //Destroy(child);
            Destroy(child.gameObject);
        }
        uiAreas.Clear();
    }

    private void CreateAreaUI(PolygonCollider2D area, bool isCurrent)
    {
        // 1. Instancia el prefab dentro del contenedor del mapa
        GameObject areaImage = Instantiate(areaPrefab, mapParent);
        RectTransform rectTransform = areaImage.GetComponent<RectTransform>();

        // 2. Obtiene los límites (Bounds) del colisionador en el mundo real
        Bounds bounds = area.bounds;

        // 3. Ajusta el tamaño y la posición en la UI multiplicando por la escala
        // Esto hace que si el área es grande en el juego, sea grande en el mapa
        rectTransform.sizeDelta = new Vector2(bounds.size.x * mapScale, bounds.size.y * mapScale);
        rectTransform.anchoredPosition = bounds.center * mapScale;

        // 4. Asigna el sprite a la zona correspondiente
        Image background = areaImage.GetComponent<Image>();
        Image icon = areaImage.transform.Find("Icon").GetComponent<Image>();

        Sprite sprite = GetSpriteForArea(area.name);

        if (sprite != null)
        {
            icon.sprite = sprite;
            icon.gameObject.SetActive(true);
        }
        else
        {
            icon.gameObject.SetActive(false);
        }

        // Fondo siempre gris
        background.color = isCurrent ? currentAreaColour : defaultColour;

        // 5. Guarda la referencia en el diccionario usando el nombre del objeto como clave
        uiAreas[area.name] = rectTransform;
    }

    /// <summary>
    /// Cambia visualmente qué zona está activa sin regenerar todo el mapa.
    /// </summary>
    public void UpdateCurrentArea(string newCurrentArea)
    {
        // Recorre el diccionario para actualizar los colores de las imágenes
        foreach (KeyValuePair<string, RectTransform> area in uiAreas)
        {
            Image background = area.Value.GetComponent<Image>();

            if (area.Key == newCurrentArea)
            {
                background.color = currentAreaColour;
            }
            else
            {
                background.color = defaultColour;
            }
        }

        // Mueve el icono del jugador a la nueva zona
        MovePlayerIcon(newCurrentArea);
    }

    private void MovePlayerIcon(string newCurrentArea)
    {
        // Busca en el diccionario si existe el nombre del área
        if (uiAreas.TryGetValue(newCurrentArea, out RectTransform areaUI))
        {
            // Coloca el icono del jugador exactamente en la misma posición que el área de la UI
            playerIcon.anchoredPosition = areaUI.anchoredPosition;
        }
    }

    // Obtener sprite para las zonas
    private Sprite GetSpriteForArea(string areaName)
    {
        foreach (var a in areaSprites)
        {
            if (a.areaName == areaName)
                return a.sprite;
        }

        return null; // fallback
    }
}
