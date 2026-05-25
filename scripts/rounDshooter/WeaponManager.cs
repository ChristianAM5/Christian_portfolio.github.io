using UnityEngine;
using UnityEngine.InputSystem; // Necesario para detectar las teclas con el nuevo sistema

public class WeaponManager : MonoBehaviour
{
    [Header("Configuración de Armas")]
    [Tooltip("Arrastra aquí los GameObjects de las armas (hijos del jugador), " +
             "en orden: 0=Pistola, 1=Rifle, 2=Francotirador, 3=Lanzacohetes")]
    [SerializeField] private GameObject[] weapons;
 
    [Header("Zoom por Defecto")]
    [Tooltip("FOV al apuntar para armas sin adsZoomFOV configurado en su WeaponData.")]
    [SerializeField] private float defaultZoomFOV = 40f;
 
    // ── Estado ────────────────────────────────────────────────────────────────
    private int    currentWeaponIndex = 0;
    private bool[] unlockedWeapons;   // qué armas están disponibles para el jugador
 
    // ── Inicialización ────────────────────────────────────────────────────────
 
    void Start()
    {
        // Comenzamos con todas las armas desactivadas y bloqueadas.
        // RoundManager llamará a UnlockWeapon(0) en su Start() para dar la pistola.
        unlockedWeapons = new bool[weapons.Length];
 
        foreach (var w in weapons)
            if (w != null) w.SetActive(false);
    }
 
    void Update()
    {
        if (Keyboard.current == null) return;
 
        if (Keyboard.current.digit1Key.wasPressedThisFrame) TrySelectWeapon(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) TrySelectWeapon(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) TrySelectWeapon(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) TrySelectWeapon(3);
    }
 
    // ── API pública ───────────────────────────────────────────────────────────
 
    /// <summary>
    /// Desbloquea el arma en el índice dado. Llamado por RoundManager.
    /// Si no hay ningún arma activa aún, activa esta como primera arma.
    /// </summary>
    public void UnlockWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;
        if (unlockedWeapons[index]) return; // ya desbloqueada
 
        unlockedWeapons[index] = true;
        GameEvents.WeaponUnlocked(index);
        Debug.Log($"[WeaponManager] Arma desbloqueada: {weapons[index]?.name}");
 
        // Si es la primera arma que se desbloquea, la activamos automáticamente
        bool anyActive = false;
        foreach (var w in weapons)
            if (w != null && w.activeSelf) { anyActive = true; break; }
 
        if (!anyActive)
        {
            currentWeaponIndex = index;
            weapons[index]?.SetActive(true);
        }
    }
 
    /// <summary>
    /// Devuelve si el arma en el índice dado está desbloqueada.
    /// Usado por la UI de mejoras para ocultar armas no disponibles.
    /// </summary>
    public bool IsWeaponUnlocked(int index)
    {
        if (index < 0 || index >= unlockedWeapons.Length) return false;
        return unlockedWeapons[index];
    }
 
    public int GetWeaponCount() => weapons != null ? weapons.Length : 0;
 
    /// <summary>Devuelve el WeaponController del arma activa.</summary>
    public WeaponController GetActiveWeapon()
    {
        if (weapons == null || weapons.Length == 0) return null;
        if (weapons[currentWeaponIndex] == null)    return null;
        return weapons[currentWeaponIndex].GetComponent<WeaponController>();
    }
 
    /// <summary>
    /// Devuelve el WeaponController del arma en el índice dado (para la UI de mejoras).
    /// </summary>
    public WeaponController GetWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return null;
        if (weapons[index] == null) return null;
        return weapons[index].GetComponent<WeaponController>();
    }
 
    /// <summary>FOV dinámico según el arma activa. Leído por PlayerMovementCC.</summary>
    public float GetActiveZoomFOV()
    {
        WeaponController active = GetActiveWeapon();
        if (active != null)
        {
            float fov = active.GetZoomFOV();
            if (fov > 0f) return fov;
        }
        return defaultZoomFOV;
    }
 
    // ── Cambio de arma ────────────────────────────────────────────────────────
 
    private void TrySelectWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;
        if (!unlockedWeapons[index])
        {
            Debug.Log($"[WeaponManager] Arma {index} bloqueada.");
            return;
        }
        SelectWeapon(index);
    }
 
    public void SelectWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;
        if (index == currentWeaponIndex) return;
        if (weapons[index] == null) return;
 
        weapons[currentWeaponIndex]?.SetActive(false);
        currentWeaponIndex = index;
        weapons[currentWeaponIndex].SetActive(true);
 
        Debug.Log($"[WeaponManager] Arma activa: {weapons[currentWeaponIndex].name}");
    }
}