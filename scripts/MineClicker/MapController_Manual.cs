using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapController_Manual : MonoBehaviour
{
    // Singleton: Permite acceder a este script desde cualquier otro (ej: MapController_Manual.Instance)
    public static MapController_Manual Instance { get; private set; }

    [Header("Configuración de UI")]
    public GameObject mapParent; // El objeto padre que contiene todas las imágenes de las zonas
    private List<Image> mapImages; // Lista interna para gestionar las imágenes de las zonas

    [Header("Colores")]
    public Color highlightColour = Color.yellow; // Color cuando la zona está activa
    public Color dimedColour = new Color(1f, 1f, 1f, 0.5f); // Color (con transparencia) para zonas inactivas

    [Header("Icono del Jugador")]
    public RectTransform playerIconTransform; // La imagen/icono que indica dónde está el jugador

    private void Awake()
    {
        // Implementación del patrón Singleton para evitar duplicados
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Busca automáticamente todos los componentes Image dentro de mapParent y los guarda en la lista
        mapImages = mapParent.GetComponentsInChildren<Image>().ToList();
    }

    /// <summary>
    /// Resalta un área específica por su nombre y mueve el icono del jugador allí.
    /// </summary>
    /// <param name="areaName">El nombre del GameObject que representa el área en la jerarquía</param>
    public void HighlightArea(string areaName)
    {
        // 1. Oscurecemos todas las áreas primero para "limpiar" el mapa
        foreach (Image area in mapImages)
        {
            area.color = dimedColour;
        }

        // 2. Buscamos el área que coincida con el nombre pasado por parámetro
        Image currentArea = mapImages.Find(x => x.name == areaName);

        if (currentArea != null)
        {
            // 3. Si la encuentra, le pone el color de resaltado
            currentArea.color = highlightColour;

            // 4. Mueve el icono del jugador a la posición de esa zona
            playerIconTransform.position = currentArea.GetComponent<RectTransform>().position;
        }
        else
        {
            // Aviso en consola por si escribiste mal el nombre de la zona
            UnityEngine.Debug.LogWarning("Area no encontrada: " + areaName);
        }
    }
}
