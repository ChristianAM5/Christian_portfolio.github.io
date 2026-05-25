using UnityEngine;
using System.Collections;

// Controla las mejoras temporales obtenidas al derrotar al dueler NPC

public class TemporaryGlobalBuffSystem : MonoBehaviour
{
    public static TemporaryGlobalBuffSystem Instance { get; private set; }

    public float duracionMin = 300f;
    public float duracionMax = 600f;

    private Coroutine buffActivo;

    // Gestionar bufos temporales en pantalla
    public string BuffNombreActivo { get; private set; } = "";
    public float BuffTiempoRestante { get; private set; } = 0f;
    public float BuffDuracionTotal { get; private set; } = 0f;
    public bool HayBuffActivo => buffActivo != null;

    private void Awake()
    {
        Instance = this;
    }

    public string AplicarBuffAleatorioConNombre()
    {
        
        if (buffActivo != null)
            StopCoroutine(buffActivo);

        int tipo = Random.Range(0, 3);
        float duracion = Random.Range(duracionMin, duracionMax);

        MineralUpgradeManager m = MineralUpgradeManager.Instance;
        string nombre = "";

        // Se elige una de las 3 mejoras globales y se llama a la corrutina correspondiente
        switch (tipo)
        {
            case 0:
                nombre = "x2 click";
                buffActivo = StartCoroutine(BuffCantidad(m, duracion));
                break;

            case 1:
                nombre = "x2 ganancias";
                buffActivo = StartCoroutine(BuffPrecio(m, duracion));
                break;

            case 2:
                nombre = "x2 capacidad";
                buffActivo = StartCoroutine(BuffCapacidad(m, duracion));
                break;
        }
        
        BuffNombreActivo = nombre;
        return nombre;
    }

    // Las corrutinas realizan la mejora durante un tiempo y luego la resetean
    private IEnumerator BuffCantidad(MineralUpgradeManager m, float duracion)
    {
        m.multiplicadorGlobalCantidad *= 2f;
        yield return ContarTiempo(duracion);
        m.multiplicadorGlobalCantidad /= 2f;
        buffActivo = null;
    }

    private IEnumerator BuffPrecio(MineralUpgradeManager m, float duracion)
    {
        m.multiplicadorGlobalPrecio *= 2f;
        yield return ContarTiempo(duracion);
        m.multiplicadorGlobalPrecio /= 2f;
        buffActivo = null;
    }

    private IEnumerator BuffCapacidad(MineralUpgradeManager m, float duracion)
    {
        m.multiplicadorGlobalCapacidad *= 2f;
        yield return ContarTiempo(duracion);
        m.multiplicadorGlobalCapacidad /= 2f;
        buffActivo = null;
    }

    // Tiempo del bufo
    private IEnumerator ContarTiempo(float duracion)
    {
        BuffDuracionTotal = duracion;
        BuffTiempoRestante = duracion;
        while (BuffTiempoRestante > 0f)
        {
            BuffTiempoRestante -= Time.deltaTime;
            yield return null;
        }
        BuffNombreActivo = "";
    }
}