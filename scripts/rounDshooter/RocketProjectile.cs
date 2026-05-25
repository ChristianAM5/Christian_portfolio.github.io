using UnityEngine;
using System.Collections;

/// <summary>
/// Componente que va en el prefab del cohete/proyectil.
/// WeaponController lo instancia y le pasa los parámetros desde WeaponData.
/// Se mueve en línea recta y al impactar ejecuta una explosión de área.
/// </summary>
/// 
public class RocketProjectile : MonoBehaviour
{
    // Inyectados por WeaponController al instanciar
    [HideInInspector] public float speed;
    [HideInInspector] public float explosionRadius;
    [HideInInspector] public float explosionDamage;
    [HideInInspector] public float explosionForce;
    [HideInInspector] public GameObject explosionVFXPrefab;
    [HideInInspector] public AudioClip explosionSound;
 
    [Header("Configuración")]
    [Tooltip("El cohete se autodestruye tras este tiempo si no impacta nada.")]
    [SerializeField] private float maxLifetime = 10f;
 
    [Tooltip("Radio del SphereCollider que sustituye al MeshCollider del prefab.")]
    [SerializeField] private float colliderRadius = 0.15f;
 
    [Tooltip("Segundos de gracia antes de activar el collider. " +
             "Evita que explote nada más salir del cañón. " +
             "Sube este valor si sigue explotando al disparar.")]
    [SerializeField] private float colliderDelay = 0.12f;
 
    private Vector3 _flyDirection;
    private bool _hasExploded = false;
    private SphereCollider _col;
 
    // ── Awake: configura físicas y collider ANTES de que Unity los valide ────
    private void Awake()
    {
        // Desactivar todos los MeshColliders (cóncavos no permitidos con Rigidbody dinámico)
        foreach (MeshCollider mc in GetComponentsInChildren<MeshCollider>())
            mc.enabled = false;
 
        // Añadir SphereCollider y dejarlo DESACTIVADO hasta el grace period
        _col = GetComponent<SphereCollider>();
        if (_col == null)
            _col = gameObject.AddComponent<SphereCollider>();
 
        _col.radius = colliderRadius;
        _col.isTrigger = true;
        _col.enabled = false; // <── clave: empieza desactivado
 
        // Rigidbody sin gravedad y sin rotaciones físicas
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
 
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
 
    private void Start()
    {
        // Dirección real desde la cámara (independiente de la rotación del modelo)
        Camera cam = Camera.main;
        _flyDirection = cam != null ? cam.transform.forward : transform.forward;
 
        // Orientamos el visual del cohete para que mire hacia donde vuela
        transform.rotation = Quaternion.LookRotation(_flyDirection);
 
        // Activamos el collider después del grace period
        StartCoroutine(EnableColliderAfterDelay());
 
        Destroy(gameObject, maxLifetime);
    }
 
    private IEnumerator EnableColliderAfterDelay()
    {
        yield return new WaitForSeconds(colliderDelay);
        if (_col != null) _col.enabled = true;
    }
 
    private void Update()
    {
        transform.Translate(_flyDirection * speed * Time.deltaTime, Space.World);
    }
 
    // ── Detección de impacto ─────────────────────────────────────────────────
 
    private void OnTriggerEnter(Collider other)
    {
        if (_hasExploded) return;
        if (other.CompareTag("Player")) return;
 
        Explode(transform.position);
    }
 
    // ── Explosión ────────────────────────────────────────────────────────────
 
    private void Explode(Vector3 point)
    {
        _hasExploded = true;
 
        if (explosionVFXPrefab != null)
        {
            GameObject vfx = Instantiate(explosionVFXPrefab, point, Quaternion.identity);
            Destroy(vfx, 4f);
        }
 
        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, point);
 
        if (explosionRadius > 0f)
        {
            Collider[] hitColliders = Physics.OverlapSphere(point, explosionRadius);
 
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("Player")) continue;
 
                float distance = Vector3.Distance(point, col.transform.position);
                float falloff = 1f - Mathf.InverseLerp(0f, explosionRadius, distance);
                float finalDamage = explosionDamage * falloff;
                if (finalDamage <= 0f) continue;
 
                HealthSystem health = col.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(finalDamage);
                }
                else
                {
                    DamageProxy proxy = col.GetComponent<DamageProxy>();
                    if (proxy != null) proxy.TakeDamage(finalDamage);
                }
 
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddExplosionForce(explosionForce, point, explosionRadius, 0.5f, ForceMode.Impulse);
            }
        }
 
        Destroy(gameObject);
    }
 
    private void OnDrawGizmosSelected()
    {
        if (explosionRadius > 0f)
        {
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}