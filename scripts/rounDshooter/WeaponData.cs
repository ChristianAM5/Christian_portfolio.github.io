using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "FPS Lab/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identificaci�n")]
    public string weaponName;

    [Header("Estad�sticas de Disparo")]
    public float damage = 10f;
    public float range = 50f;
    public float fireRate = 0.2f; // Tiempo entre disparos

    // --- NUEVO CAMPO PARA EL MODO DE DISPARO ---
    [Tooltip("Si est� marcado, el arma dispara continuamente al mantener pulsado. Si no, requiere un clic por bala.")]
    public bool isAutomatic = true;

    // --- NUEVA VARIABLE PARA FUERZA F�SICA ---
    [Tooltip("Fuerza de impacto aplicada a objetos con Rigidbody")]
    public float impactForce = 150f;

    [Header("Munici�n")]
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    //[Tooltip("Munici�n m�xima que el jugador puede llevar en reserva para este arma.")]
    //public int maxReserveAmmo = 30;
    //[Tooltip("El tipo de munici�n que utiliza este arma.")]
    //public AmmoType ammoType;

    [Header("Efectos Visuales")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public AudioClip shootSound;

    // ─────────────────────────────────────────────────────────────────────────
    // NUEVO: ZOOM PERSONALIZADO POR ARMA
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Zoom ADS (Apuntar)")]
    [Tooltip("FOV de la cámara al apuntar con esta arma. " +
             "0 = usa el valor por defecto definido en PlayerMovementCC.\n" +
             "Recomendados: Pistola ~50, Rifle ~35, Francotirador ~10-15.")]
    public float adsZoomFOV = 0f;
 
    // ─────────────────────────────────────────────────────────────────────────
    // NUEVO: MODO PROYECTIL (en vez de raycast instantáneo)
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Proyectil (dejar vacío para hitscan)")]
    [Tooltip("Arrastra aquí el prefab del cohete/bala física. " +
             "Si está vacío, el arma funciona como hitscan (raycast instantáneo).")]
    public GameObject projectilePrefab;
 
    [Tooltip("Velocidad en m/s del proyectil. Ignorado si no hay prefab de proyectil.")]
    public float projectileSpeed = 25f;
 
    // ─────────────────────────────────────────────────────────────────────────
    // NUEVO: EXPLOSIÓN (para PCTAG)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Explosión (solo si hay proyectil)")]
    [Tooltip("Radio en metros del daño de área al explotar. 0 = sin explosión.")]
    public float explosionRadius = 0f;
 
    [Tooltip("Daño máximo en el centro de la explosión. " +
             "Se reduce proporcionalmente con la distancia hasta el borde del radio.")]
    public float explosionDamage = 0f;
 
    [Tooltip("Prefab de partículas/VFX que aparece en el punto de impacto.")]
    public GameObject explosionVFXPrefab;
 
    [Tooltip("Sonido de la explosión.")]
    public AudioClip explosionSound;
 
    [Tooltip("Fuerza que aplica la explosión a los Rigidbodies cercanos.")]
    public float explosionForce = 800f;
}

