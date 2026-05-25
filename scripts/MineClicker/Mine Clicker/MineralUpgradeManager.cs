using UnityEngine;
using System.Collections.Generic;

// Gestiona todos los niveles de mejora de cada mineral. Centraliza capacidad, 
// precio de venta, cantidad por click y número de auto-generadores.

public class MineralUpgradeManager : MonoBehaviour
{
    public static MineralUpgradeManager Instance { get; private set; }

    // ─── Multiplicadores globales ──────────────────────────────────
    // Modificadores globales que afecten a todos los minerales a la vez.
    [Header("Multiplicadores globales (para eventos futuros)")]
    public float multiplicadorGlobalCantidad   = 1f;
    public float multiplicadorGlobalPrecio     = 1f;
    public float multiplicadorGlobalCapacidad  = 1f;

    // ─── Configuración base por mineral ───────────────────────────
    // Cada entrada del diccionario guarda los datos de mejora de un mineral.
    private Dictionary<MineralType, DatosMejora> datosPorMineral;

    // ─── Niveles máximos ──────────────────────────────────────────
    public const int MAX_NIVEL = 10;

    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InicializarDatos();
    }

    // Define los valores base y los costes de mejora para cada mineral.

    private void InicializarDatos()
    {
        datosPorMineral = new Dictionary<MineralType, DatosMejora>();

        // ── CUARZO ────────────────────────────────────────────────
        datosPorMineral[MineralType.Cuarzo] = new DatosMejora(
            mineralType: MineralType.Cuarzo,

            // Cantidad por click niveles 1-10
            cantidadesPorClick: new int[]    { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            costesMejoraCantidad: new int[]  { 10, 20, 35, 55, 80, 110, 150, 200, 260, 330 },

            // Precio de venta niveles 1-10
            preciosVenta: new float[]        { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f },
            costesMejoraPrecio: new int[]    { 15, 30, 50, 75, 110, 150, 200, 260, 330, 420 },

            // Capacidad por slot niveles 1-10
            capacidades: new int[]           { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500 },
            costesMejoraCapacidad: new int[] { 20, 45, 80, 125, 180, 250, 330, 420, 520, 650 },

            // Coste de añadir cada auto-generador (niveles 1-10)
            costesAutoGen: new int[]         { 50, 110, 190, 290, 420, 580, 770, 990, 1250, 1550 }
        );

        // ── CARBÓN ────────────────────────────────────────────────
        datosPorMineral[MineralType.Carbon] = new DatosMejora(
            mineralType: MineralType.Carbon,
            cantidadesPorClick: new int[]    { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            costesMejoraCantidad: new int[]  { 15, 30, 52, 82, 120, 165, 225, 300, 390, 495 },
            preciosVenta: new float[]        { 2f, 4f, 6f, 8f, 10f, 12f, 14f, 16f, 18f, 20f },
            costesMejoraPrecio: new int[]    { 22, 45, 75, 112, 165, 225, 300, 390, 495, 630 },
            capacidades: new int[]           { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500 },
            costesMejoraCapacidad: new int[] { 30, 67, 120, 187, 270, 375, 495, 630, 780, 975 },
            costesAutoGen: new int[]         { 75, 165, 285, 435, 630, 870, 1155, 1485, 1875, 2325 }
        );

        // ── BAUXITA ───────────────────────────────────────────────
        datosPorMineral[MineralType.Bauxita] = new DatosMejora(
            mineralType: MineralType.Bauxita,
            cantidadesPorClick: new int[]    { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            costesMejoraCantidad: new int[]  { 22, 45, 78, 123, 180, 247, 337, 450, 585, 742 },
            preciosVenta: new float[]        { 3f, 6f, 9f, 12f, 15f, 18f, 21f, 24f, 27f, 30f },
            costesMejoraPrecio: new int[]    { 33, 67, 112, 168, 247, 337, 450, 585, 742, 945 },
            capacidades: new int[]           { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500 },
            costesMejoraCapacidad: new int[] { 45, 101, 180, 281, 405, 562, 742, 945, 1170, 1462 },
            costesAutoGen: new int[]         { 112, 247, 427, 652, 945, 1305, 1732, 2227, 2812, 3487 }
        );

        // ── HALITA ────────────────────────────────────────────────
        datosPorMineral[MineralType.Halita] = new DatosMejora(
            mineralType: MineralType.Halita,
            cantidadesPorClick: new int[]    { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            costesMejoraCantidad: new int[]  { 33, 67, 117, 184, 270, 371, 506, 675, 877, 1113 },
            preciosVenta: new float[]        { 4f, 8f, 12f, 16f, 20f, 24f, 28f, 32f, 36f, 40f },
            costesMejoraPrecio: new int[]    { 49, 101, 168, 252, 371, 506, 675, 877, 1113, 1417 },
            capacidades: new int[]           { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500 },
            costesMejoraCapacidad: new int[] { 67, 151, 270, 421, 607, 843, 1113, 1417, 1755, 2194 },
            costesAutoGen: new int[]         { 168, 371, 641, 978, 1417, 1957, 2598, 3341, 4219, 5231 }
        );

        // ── COBRE ─────────────────────────────────────────────────
        datosPorMineral[MineralType.Cobre] = new DatosMejora(
            mineralType: MineralType.Cobre,
            cantidadesPorClick: new int[]    { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            costesMejoraCantidad: new int[]  { 49, 101, 175, 276, 405, 556, 759, 1012, 1316, 1669 },
            preciosVenta: new float[]        { 5f, 10f, 15f, 20f, 25f, 30f, 35f, 40f, 45f, 50f },
            costesMejoraPrecio: new int[]    { 73, 151, 252, 378, 556, 759, 1012, 1316, 1669, 2126 },
            capacidades: new int[]           { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500 },
            costesMejoraCapacidad: new int[] { 101, 227, 405, 632, 911, 1264, 1669, 2126, 2632, 3291 },
            costesAutoGen: new int[]         { 252, 556, 961, 1467, 2126, 2936, 3897, 5011, 6328, 7847 }
        );
    }

    // Capacidad máxima del slot para el mineral dado según su nivel de mejora actual.
    // Aplicando el multiplicador global.

    public int GetCapacidad(MineralType tipo)
    {
        DatosMejora datos = datosPorMineral[tipo];
        int valorBase = datos.capacidades[datos.nivelCapacidad];
        return Mathf.RoundToInt(valorBase * multiplicadorGlobalCapacidad);
    }

    // Cantidad de mineral obtenida por click según nivel de mejora
    public int GetCantidadPorClick(MineralType tipo)
    {
        DatosMejora datos = datosPorMineral[tipo];
        int valorBase = datos.cantidadesPorClick[datos.nivelCantidad];
        return Mathf.RoundToInt(valorBase * multiplicadorGlobalCantidad);
    }

    // Precio de venta por unidad según nivel de mejora
    public float GetPrecioVenta(MineralType tipo)
    {
        DatosMejora datos = datosPorMineral[tipo];
        float valorBase = datos.preciosVenta[datos.nivelPrecio];
        return valorBase * multiplicadorGlobalPrecio;
    }

    // Número de auto-generadores activos para este mineral
    public int GetNumAutoGeneradores(MineralType tipo)
    {
        return datosPorMineral[tipo].numAutoGeneradores;
    }

    // Getters de nivel actual (funciones simples)

    public int GetNivelCantidad(MineralType tipo)   => datosPorMineral[tipo].nivelCantidad;
    public int GetNivelPrecio(MineralType tipo)     => datosPorMineral[tipo].nivelPrecio;
    public int GetNivelCapacidad(MineralType tipo)  => datosPorMineral[tipo].nivelCapacidad;
    public int GetNivelAutoGen(MineralType tipo)    => datosPorMineral[tipo].numAutoGeneradores;

    // Getters de coste de siguiente mejora

    public int GetCosteSiguienteCantidad(MineralType tipo)
    {
        DatosMejora d = datosPorMineral[tipo];
        if (d.nivelCantidad >= MAX_NIVEL - 1) return -1; // ya está al máximo
        return d.costesMejoraCantidad[d.nivelCantidad];
    }

    public int GetCosteSiguientePrecio(MineralType tipo)
    {
        DatosMejora d = datosPorMineral[tipo];
        if (d.nivelPrecio >= MAX_NIVEL - 1) return -1;
        return d.costesMejoraPrecio[d.nivelPrecio];
    }

    public int GetCosteSiguienteCapacidad(MineralType tipo)
    {
        DatosMejora d = datosPorMineral[tipo];
        if (d.nivelCapacidad >= MAX_NIVEL - 1) return -1;
        return d.costesMejoraCapacidad[d.nivelCapacidad];
    }

    public int GetCosteSiguienteAutoGen(MineralType tipo)
    {
        DatosMejora d = datosPorMineral[tipo];
        if (d.numAutoGeneradores >= MAX_NIVEL) return -1;
        return d.costesAutoGen[d.numAutoGeneradores];
    }

    // Métodos de mejora

    public bool MejorarCantidad(MineralType tipo)
    {
        DatosMejora d = datosPorMineral[tipo];
        if (d.nivelCantidad >= MAX_NIVEL - 1) return false;
        d.nivelCantidad++;
        return true;
    }

    public bool MejorarPrecio(MineralType tipo)
    {
        DatosMejora d = datosPorMineral[tipo];
        if (d.nivelPrecio >= MAX_NIVEL - 1) return false;
        d.nivelPrecio++;
        return true;
    }

    public bool MejorarCapacidad(MineralType tipo)
    {
        DatosMejora d = datosPorMineral[tipo];
        if (d.nivelCapacidad >= MAX_NIVEL - 1) return false;
        d.nivelCapacidad++;
        return true;
    }

    public bool AnadirAutoGenerador(MineralType tipo)
    {
        DatosMejora d = datosPorMineral[tipo];
        if (d.numAutoGeneradores >= MAX_NIVEL) return false;
        d.numAutoGeneradores++;
        return true;
    }

    // Guardado / Carga

    //Devuelve los datos de mejora serializables para el SaveController
    public List<UpgradeSaveData> GetSaveData()
    {
        List<UpgradeSaveData> lista = new List<UpgradeSaveData>();
        foreach (var kvp in datosPorMineral)
        {
            lista.Add(new UpgradeSaveData
            {
                mineralType       = kvp.Key,
                nivelCantidad     = kvp.Value.nivelCantidad,
                nivelPrecio       = kvp.Value.nivelPrecio,
                nivelCapacidad    = kvp.Value.nivelCapacidad,
                numAutoGeneradores = kvp.Value.numAutoGeneradores
            });
        }
        return lista;
    }

    // Carga los datos de mejora desde el SaveController
    public void LoadSaveData(List<UpgradeSaveData> datos)
    {
        if (datos == null) return;
        foreach (UpgradeSaveData d in datos)
        {
            if (datosPorMineral.ContainsKey(d.mineralType))
            {
                datosPorMineral[d.mineralType].nivelCantidad      = d.nivelCantidad;
                datosPorMineral[d.mineralType].nivelPrecio        = d.nivelPrecio;
                datosPorMineral[d.mineralType].nivelCapacidad     = d.nivelCapacidad;
                datosPorMineral[d.mineralType].numAutoGeneradores = d.numAutoGeneradores;
            }
        }
    }
}

// Clase de datos internos por mineral 
// Contiene todos los niveles y costes de mejora de un mineral concreto.
public class DatosMejora
{
    public MineralType mineralType;

    // Arrays de valores por nivel (índice 0 = nivel 1, índice 9 = nivel 10)
    public int[]   cantidadesPorClick;
    public int[]   costesMejoraCantidad;
    public float[] preciosVenta;
    public int[]   costesMejoraPrecio;
    public int[]   capacidades;
    public int[]   costesMejoraCapacidad;
    public int[]   costesAutoGen;

    // Niveles actuales (empiezan en 0 = nivel 1)
    public int nivelCantidad      = 0;
    public int nivelPrecio        = 0;
    public int nivelCapacidad     = 0;
    public int numAutoGeneradores = 0;

    public DatosMejora(
        MineralType mineralType,
        int[]   cantidadesPorClick,
        int[]   costesMejoraCantidad,
        float[] preciosVenta,
        int[]   costesMejoraPrecio,
        int[]   capacidades,
        int[]   costesMejoraCapacidad,
        int[]   costesAutoGen)
    {
        this.mineralType            = mineralType;
        this.cantidadesPorClick     = cantidadesPorClick;
        this.costesMejoraCantidad   = costesMejoraCantidad;
        this.preciosVenta           = preciosVenta;
        this.costesMejoraPrecio     = costesMejoraPrecio;
        this.capacidades            = capacidades;
        this.costesMejoraCapacidad  = costesMejoraCapacidad;
        this.costesAutoGen          = costesAutoGen;
    }
}

// Clase serializable para guardado
[System.Serializable]
public class UpgradeSaveData
{
    public MineralType mineralType;
    public int nivelCantidad;
    public int nivelPrecio;
    public int nivelCapacidad;
    public int numAutoGeneradores;
}


