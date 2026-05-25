using UnityEngine;
using System.Collections;


// Script que va en cada criatura auto-generadora.
// Genera minerales cada X segundos y los mete en el cofre de su zona correspondiente.

public class AutoGenCreature : MonoBehaviour
{
    [HideInInspector] public MineralType tipoMineral;
    [HideInInspector] public AutoGenChest cofreDeLaZona;

    // Intervalo sacado del AutoGenManager para poder modificarlo globalmente
    // Cada X segundos se genera una unidad de mineral
    private float intervalo => AutoGenManager.Instance.intervaloBase;

    // Movimiento para que las criaturas no esten estáticas
    [Header("Movimiento")]
    public float velocidadMovimiento = 0.5f;
    public float tiempoEntreMovimientos = 3f;

    private Vector3 destino;
    private PolygonCollider2D zonaCollider;

    // Animacion de movimiento
    private Animator animator;
    private Vector3 ultimaPosicion;


    private void Start()
    {
        animator = GetComponent<Animator>();
        ultimaPosicion = transform.position;

        StartCoroutine(GenerarLoop());
        StartCoroutine(MovimientoLoop());
    
        // Buscamos el collider de nuestra zona
        BuscarColliderZona();

    }

    // Solo para las animaciones de movimiento
    void Update()
    {
        Vector3 movimiento = (transform.position - ultimaPosicion).normalized;

        animator.SetFloat("MoveX", movimiento.x);
        animator.SetFloat("MoveY", movimiento.y);

        ultimaPosicion = transform.position;
    }

    // Corrutina para generar automaticamente minerales cada intervalo
    private IEnumerator GenerarLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalo);

            if (cofreDeLaZona != null)
            {
                cofreDeLaZona.AnadirMineral(tipoMineral, 1);
                Debug.Log($"[Criatura] Generado 1 de {tipoMineral} en el cofre.");
            }
        }
    }

    // Obtener colliders de zonas y nombres para que
    // no se salgan de su zona al desplazarse

    private void BuscarColliderZona()
    {
        string nombreZona = AutoGenManager.Instance != null 
            ? ObtenerNombreZona() : "";
        
        PolygonCollider2D[] cols = FindObjectsByType<PolygonCollider2D>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        foreach (PolygonCollider2D col in cols)
            if (col.gameObject.name == nombreZona)
            { zonaCollider = col; break; }
    }

    private string ObtenerNombreZona()
    {
        ZoneManager zm = ZoneManager.Instance;
        switch (tipoMineral)
        {
            case MineralType.Cuarzo:  return zm.zonaCentral;
            case MineralType.Carbon:  return zm.zonaIzquierda;
            case MineralType.Bauxita: return zm.zonaDerecha;
            case MineralType.Halita:  return zm.zonaArriba;
            case MineralType.Cobre:   return zm.zonaAbajo;
            default: return "";
        }
    }


    // Corrutina de movimiento "aleatorio"
    private IEnumerator MovimientoLoop()
    {
        while (true)
        {
            // Elegimos un nuevo destino aleatorio dentro de la zona
            destino = ObtenerDestinoAleatorio();
            
            // Nos movemos hacia el destino
            while (Vector3.Distance(transform.position, destino) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, destino, 
                    velocidadMovimiento * Time.deltaTime);
                yield return null;
            }
            
            // Esperamos antes de movernos de nuevo
            yield return new WaitForSeconds(tiempoEntreMovimientos);
        }
    }

    private Vector3 ObtenerDestinoAleatorio()
    {
        if (zonaCollider == null) return transform.position;
        
        Bounds b = zonaCollider.bounds;
        for (int i = 0; i < 20; i++)
        {
            Vector2 punto = new Vector2(
                Random.Range(b.min.x + 1f, b.max.x - 1f),
                Random.Range(b.min.y + 1f, b.max.y - 1f)
            );
            if (zonaCollider.OverlapPoint(punto))
                return new Vector3(punto.x, punto.y, 0f);
        }
        return transform.position;
    }

}
