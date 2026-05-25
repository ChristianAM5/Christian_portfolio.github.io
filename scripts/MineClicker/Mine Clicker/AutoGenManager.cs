using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// Gestiona los generadores automáticos de minerales por zona.
// Al comprar un generador instancia una criatura en la zona correspondiente.

public class AutoGenManager : MonoBehaviour
{
    // SINGLETON cualquiera puede leer el valor pero solo este script puede cambiarlo
    public static AutoGenManager Instance { get; private set; }

    [Header("Intervalo entre generaciones (segundos)")]
    public float intervaloBase = 30f;

    // Prefabs de criaturas (uno por zona)
    [Header("Prefabs de criaturas auto-generadoras")]
    public GameObject prefabCriaturaZonaCentral;
    public GameObject prefabCriaturaZonaCarbon;
    public GameObject prefabCriaturaZonaBauxita;
    public GameObject prefabCriaturaZonaHalita;
    public GameObject prefabCriaturaZonaCobre;

    // Prefabs de minerales (para que AutoGenChest los use)
    [Header("Prefabs de minerales")]
    public GameObject prefabCuarzo;
    public GameObject prefabCarbon;
    public GameObject prefabBauxita;
    public GameObject prefabHalita;
    public GameObject prefabCobre;

    // Cofres de cada zona
    [Header("Cofres de cada zona (arrastra los objetos de la escena)")]
    public AutoGenChest cofreCentral;
    public AutoGenChest cofreCarbon;
    public AutoGenChest cofreBauxita;
    public AutoGenChest cofreHalita;
    public AutoGenChest cofreCobre;

    // Criaturas activas por zona
    private Dictionary<MineralType, List<AutoGenCreature>> criaturasPorZona
        = new Dictionary<MineralType, List<AutoGenCreature>>();

    private void Awake()
    {
        // Si ya existe un manager, destruimos este para
        // evitar duplicados al cambiar de escena
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Creamos una lista vacía por cada tipo de mineral
        foreach (MineralType tipo in System.Enum.GetValues(typeof(MineralType)))
            criaturasPorZona[tipo] = new List<AutoGenCreature>();
    }


    // Llamado desde NPCMejorasController al comprar un auto-generador.
    // Instancia una nueva criatura en la zona correspondiente.

    public void ActualizarAutoGeneradores(MineralType tipo)
    {
        int numDeseado  = MineralUpgradeManager.Instance.GetNumAutoGeneradores(tipo);
        int numActuales = criaturasPorZona[tipo].Count;

        for (int i = numActuales; i < numDeseado; i++)
        {
            SpawnCriatura(tipo);
        }
    }

    // Creacion de la criatura se le asigna su mineral y cofre donde meterlo y se guarda a la lista

    private void SpawnCriatura(MineralType tipo)
    {
        GameObject prefab  = GetPrefabCriatura(tipo);
        AutoGenChest cofre = GetCofre(tipo);

        if (prefab == null)
        {
            Debug.LogWarning($"[AutoGen] No hay prefab de criatura para {tipo}");
            return;
        }

        // Obtenemos posición aleatoria dentro del PolygonCollider2D de la zona
        Vector3 posicion = GetPosicionAleatoriaDentroDeZona(tipo);

        GameObject obj = Instantiate(prefab, posicion, Quaternion.identity);

        AutoGenCreature criatura = obj.GetComponent<AutoGenCreature>();
        if (criatura != null)
        {
            criatura.tipoMineral   = tipo;
            criatura.cofreDeLaZona = cofre;
            criaturasPorZona[tipo].Add(criatura);
            Debug.Log($"[AutoGen] Criatura {criaturasPorZona[tipo].Count} " +
                    $"de {tipo} spawneada en {posicion}");
        }
    }


    // Devuelve una posición aleatoria dentro del PolygonCollider2D de la zona.
    // Usa el bounding box del collider para encontrar puntos válidos.

    private Vector3 GetPosicionAleatoriaDentroDeZona(MineralType tipo)
    {
        // Obtenemos el nombre de la zona desde ZoneManager
        string nombreZona = GetNombreZona(tipo);
        
        // Buscamos el PolygonCollider2D con ese nombre en MapBounds
        PolygonCollider2D[] colliders = FindObjectsByType<PolygonCollider2D>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        PolygonCollider2D zonaCollider = null;
        foreach (PolygonCollider2D col in colliders)
        {
            if (col.gameObject.name == nombreZona)
            {
                zonaCollider = col;
                break;
            }
        }

        if (zonaCollider == null)
        {
            Debug.LogWarning($"[AutoGen] No se encontró collider para zona {nombreZona}");
            return Vector3.zero;
        }

        // Intentamos hasta 30 veces encontrar un punto dentro del collider
        Bounds bounds = zonaCollider.bounds;
        for (int i = 0; i < 30; i++)
        {
            Vector2 puntoAleatorio = new Vector2(
                Random.Range(bounds.min.x + 1f, bounds.max.x - 1f),
                Random.Range(bounds.min.y + 1f, bounds.max.y - 1f)
            );

            // Comprobamos que el punto está dentro del polígono
            if (zonaCollider.OverlapPoint(puntoAleatorio))
                return new Vector3(puntoAleatorio.x, puntoAleatorio.y, 0f);
        }

        // Fallback: centro del collider
        return zonaCollider.bounds.center;
    }

    private string GetNombreZona(MineralType tipo)
    {
        ZoneManager zm = ZoneManager.Instance;
        switch (tipo)
        {
            case MineralType.Cuarzo:  return zm.zonaCentral;
            case MineralType.Carbon:  return zm.zonaIzquierda;
            case MineralType.Bauxita: return zm.zonaDerecha;
            case MineralType.Halita:  return zm.zonaArriba;
            case MineralType.Cobre:   return zm.zonaAbajo;
            default: return "";
        }
    }

    // Hace público GetPrefab para que AutoGenChest pueda usarlo.

    public GameObject GetPrefab(MineralType tipo)
    {
        switch (tipo)
        {
            case MineralType.Cuarzo:  return prefabCuarzo;
            case MineralType.Carbon:  return prefabCarbon;
            case MineralType.Bauxita: return prefabBauxita;
            case MineralType.Halita:  return prefabHalita;
            case MineralType.Cobre:   return prefabCobre;
            default: return null;
        }
    }

    // Restaura criaturas al cargar partida.
    public void RestaurarGeneradores()
    {
        foreach (MineralType tipo in System.Enum.GetValues(typeof(MineralType)))
            ActualizarAutoGeneradores(tipo);
    }

    // Helpers

    private GameObject GetPrefabCriatura(MineralType tipo)
    {
        switch (tipo)
        {
            case MineralType.Cuarzo:  return prefabCriaturaZonaCentral;
            case MineralType.Carbon:  return prefabCriaturaZonaCarbon;
            case MineralType.Bauxita: return prefabCriaturaZonaBauxita;
            case MineralType.Halita:  return prefabCriaturaZonaHalita;
            case MineralType.Cobre:   return prefabCriaturaZonaCobre;
            default: return null;
        }
    }

    public AutoGenChest GetCofre(MineralType tipo)
    {
        switch (tipo)
        {
            case MineralType.Cuarzo:  return cofreCentral;
            case MineralType.Carbon:  return cofreCarbon;
            case MineralType.Bauxita: return cofreBauxita;
            case MineralType.Halita:  return cofreHalita;
            case MineralType.Cobre:   return cofreCobre;
            default: return null;
        }
    }
}
