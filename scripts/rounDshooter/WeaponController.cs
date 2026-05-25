using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private Transform firePoint;
 
    [Header("Mejoras")]
    [Tooltip("ScriptableObject con los valores de mejora de esta arma. " +
             "Crea uno desde FPS Lab → Weapon Upgrade Data y asígnalo aquí.")]
    [SerializeField] private WeaponUpgradeData upgradeData;
 
    [Header("Referencias de UI")]
    [SerializeField] private HitmarkerUI hitmarkerUI;
 
    // ── Estado de munición ───────────────────────────────────────────────────
    private int currentAmmo;
 
    // ── Estado de recarga ─────────────────────────────────────────────────────
    // FIX: en vez de corrutina, guardamos el momento en que debe terminar la recarga.
    // Así funciona aunque el arma esté desactivada: cuando el jugador vuelva a esta
    // arma, el Update() verifica si ya ha pasado el tiempo y completa la recarga.
    private bool isReloading = false;
    private float reloadEndTime = -1f;
 
    // ── Estado de disparo ────────────────────────────────────────────────────
    private float nextFireTime;
    private bool isAttacking = false;
 
    // ── Niveles y bonos de mejora ─────────────────────────────────────────────
    private int damageUpgradeLevel = 0;
    private int ammoUpgradeLevel   = 0;
    private int reloadUpgradeLevel = 0;
 
    private float bonusDamage     = 0f;
    private int   bonusMaxAmmo    = 0;
    private float reloadReduction = 0f;
 
    // ── Stats finales (base + mejoras) ────────────────────────────────────────
    public float GetCurrentDamage()     => weaponData.damage + bonusDamage;
    public int   GetCurrentMaxAmmo()    => weaponData.maxAmmo + bonusMaxAmmo;
    public float GetCurrentReloadTime() => Mathf.Max(0.3f, weaponData.reloadTime - reloadReduction);
 
    // ── Getters para la UI ───────────────────────────────────────────────────
    public int  GetCurrentAmmo()    => currentAmmo;
    public bool GetIsReloading()    => isReloading;
    public int  GetMaxAmmo()        => GetCurrentMaxAmmo();
    public float GetZoomFOV()       => weaponData != null ? weaponData.adsZoomFOV : 0f;
    public string GetWeaponName()   => weaponData != null ? weaponData.weaponName : "";
 
    // Getters de nivel de mejora para que la UI sepa en qué nivel está cada stat
    public int DamageLevel  => damageUpgradeLevel;
    public int AmmoLevel    => ammoUpgradeLevel;
    public int ReloadLevel  => reloadUpgradeLevel;
 
    // ── Disponibilidad de mejoras ─────────────────────────────────────────────
    public bool CanUpgradeDamage()  => upgradeData != null && damageUpgradeLevel  < upgradeData.MaxDamageLevels;
    public bool CanUpgradeAmmo()    => upgradeData != null && ammoUpgradeLevel    < upgradeData.MaxAmmoLevels;
    public bool CanUpgradeReload()  => upgradeData != null && reloadUpgradeLevel  < upgradeData.MaxReloadLevels;
 
    // ── Aplicar mejoras (llamado por la UI entre rondas) ──────────────────────
 
    /// <summary>Aumenta el daño del arma. Devuelve false si ya está al máximo.</summary>
    public bool UpgradeDamage()
    {
        if (!CanUpgradeDamage()) return false;
        
        damageUpgradeLevel++;
        // Calculamos el total acumulado: Nivel 1 = 5, Nivel 2 = 10, Nivel 3 = 15...
        bonusDamage = damageUpgradeLevel * upgradeData.damagePerLevel;
        
        Debug.Log($"[{weaponData.weaponName}] Daño mejorado (Nivel {damageUpgradeLevel}) → Total: {GetCurrentDamage()}");
        return true;
    }
 
    /// <summary>Aumenta la capacidad del cargador y recarga inmediatamente.</summary>
    public bool UpgradeAmmo()
    {
        if (!CanUpgradeAmmo()) return false;
        
        ammoUpgradeLevel++;
        bonusMaxAmmo = ammoUpgradeLevel * upgradeData.ammoPerLevel;
        currentAmmo = GetCurrentMaxAmmo(); // Refill con el nuevo máximo
        
        Debug.Log($"[{weaponData.weaponName}] Munición mejorada (Nivel {ammoUpgradeLevel}) → Total: {GetCurrentMaxAmmo()}");
        return true;
    }
 
    public bool UpgradeReload()
    {
        if (!CanUpgradeReload()) return false;
        
        reloadUpgradeLevel++;
        reloadReduction = reloadUpgradeLevel * upgradeData.reloadReductionPerLevel;
        
        Debug.Log($"[{weaponData.weaponName}] Recarga mejorada (Nivel {reloadUpgradeLevel}) → Total: {GetCurrentReloadTime():F2}s");
        return true;
    }
 
    // ── Ciclo de vida ────────────────────────────────────────────────────────
 
    void Start()
    {
        currentAmmo = GetCurrentMaxAmmo();
    }
 
    void Update()
    {
        // FIX DE RECARGA: comprobamos por tiempo, no por corrutina.
        // Funciona aunque el GameObject estuviera desactivado mientras recargaba.
        if (isReloading && Time.time >= reloadEndTime)
            CompleteReload();

        // Disparo automático
        if (weaponData.isAutomatic && isAttacking && Time.time >= nextFireTime)
        {
            // Si mantiene el clic pero se quedó seco, inicia recarga y para el ataque
            if (currentAmmo <= 0 && !isReloading)
            {
                StartReload();
                isAttacking = false; // Detiene el bucle de disparo automático
            }
            else
            {
                Shoot();
                nextFireTime = Time.time + weaponData.fireRate;
            }
        }
    }
 
    // ── Input ────────────────────────────────────────────────────────────────
 
    private void OnAttack(InputValue value)
    {
        isAttacking = value.isPressed;

        // Si hace clic izquierdo, no está recargando y no le quedan balas -> ¡RECARGA!
        if (isAttacking && currentAmmo <= 0 && !isReloading)
        {
            StartReload();
            isAttacking = false; // Evitamos que intente disparar en este frame
            return;
        }

        // Semiautomático: un clic = un disparo (solo ocurre si SÍ tiene balas)
        if (!weaponData.isAutomatic && isAttacking && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + weaponData.fireRate;
        }
    }
 
    private void OnReload(InputValue value)
    {
        if (!isReloading && currentAmmo < GetCurrentMaxAmmo())
            StartReload();
    }
 
    // ── Recarga ──────────────────────────────────────────────────────────────
 
    private void StartReload()
    {
        isReloading  = true;
        reloadEndTime = Time.time + GetCurrentReloadTime();
        Debug.Log($"[{weaponData.weaponName}] Recargando... ({GetCurrentReloadTime():F2}s)");
    }
 
    private void CompleteReload()
    {
        currentAmmo  = GetCurrentMaxAmmo();
        isReloading  = false;
        reloadEndTime = -1f;
        Debug.Log($"[{weaponData.weaponName}] Recarga completa: {currentAmmo}/{GetCurrentMaxAmmo()}");
    }
 
    // ── Lógica de disparo ────────────────────────────────────────────────────
 
    private void Shoot()
    {
        if (currentAmmo <= 0) { Debug.Log($"[{weaponData.weaponName}] Cargador vacío."); return; }
        if (isReloading) return;
 
        currentAmmo--;
        SpawnMuzzleFlash();
        PlayShootSound();
 
        if (weaponData.projectilePrefab != null)
            ShootProjectile();
        else
            ShootRaycast();
    }
 
    private void ShootRaycast()
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(ray.origin, ray.direction * weaponData.range, Color.yellow, 0.5f);
 
        if (!Physics.Raycast(ray, out RaycastHit hit, weaponData.range)) return;
 
        if (weaponData.hitEffectPrefab != null)
        {
            GameObject vfx = Instantiate(weaponData.hitEffectPrefab, hit.point,
                                         Quaternion.LookRotation(hit.normal));
            Destroy(vfx, 2f);
        }
 
        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForceAtPosition(ray.direction * weaponData.impactForce, hit.point, ForceMode.Impulse);
 
        bool damageApplied = false;
 
        HealthSystem targetHealth = hit.collider.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(GetCurrentDamage()); // usa daño con mejoras
            damageApplied = true;
        }
        else
        {
            DamageProxy proxy = hit.collider.GetComponent<DamageProxy>();
            if (proxy != null) { proxy.TakeDamage(GetCurrentDamage()); damageApplied = true; }
        }
 
        if (damageApplied && hitmarkerUI != null)
            hitmarkerUI.ShowHitmarker();
    }
 
    private void ShootProjectile()
    {
        if (firePoint == null)
        {
            Debug.LogError($"[{weaponData.weaponName}] Falta el FirePoint en el Inspector.");
            return;
        }
 
        GameObject proj = Instantiate(weaponData.projectilePrefab,
                                      firePoint.position, firePoint.rotation);
        RocketProjectile rocket = proj.GetComponent<RocketProjectile>();
        if (rocket != null)
        {
            rocket.speed              = weaponData.projectileSpeed;
            rocket.explosionRadius    = weaponData.explosionRadius;
            rocket.explosionDamage    = weaponData.explosionDamage + bonusDamage; // mejoras afectan al cohete
            rocket.explosionForce     = weaponData.explosionForce;
            rocket.explosionVFXPrefab = weaponData.explosionVFXPrefab;
            rocket.explosionSound     = weaponData.explosionSound;
        }
    }
 
    private void SpawnMuzzleFlash()
    {
        if (weaponData.muzzleFlashPrefab == null || firePoint == null) return;
        GameObject flash = Instantiate(weaponData.muzzleFlashPrefab, firePoint.position, firePoint.rotation);
        Destroy(flash, 0.1f);
    }
 
    private void PlayShootSound()
    {
        if (weaponData.shootSound == null) return;
        AudioSource.PlayClipAtPoint(weaponData.shootSound, transform.position, 0.2f);
    }
 
    private void OnDisable()
    {
        isAttacking = false;
        // NO reseteamos isReloading ni reloadEndTime aquí.
        // La recarga sigue "en marcha" aunque el arma esté inactiva.
        // Cuando el jugador vuelva a esta arma, Update() detectará que ya terminó.
    }
}
 