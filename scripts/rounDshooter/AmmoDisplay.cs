using UnityEngine;
using TMPro; // Necesario para TextMeshPro

public class AmmoDisplay : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private WeaponManager weaponManager;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();

        // En lugar de buscar un arma fija, buscamos el gestor de armas del jugador
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            weaponManager = player.GetComponent<WeaponManager>();
        }
    }

    void Update()
    {
        if (weaponManager == null) return;

        // Le pedimos al WeaponManager el arma que el jugador tiene en la mano en este frame
        WeaponController activeWeapon = weaponManager.GetActiveWeapon();

        if (activeWeapon == null) return;

        // Actualizamos el texto con los datos del arma activa
        if (activeWeapon.GetIsReloading())
        {
            textMesh.text = "RECARGANDO...";
        }
        else
        {
            // Mostramos: Balas en cargador / Reserva de ese arma
            textMesh.text = $"{activeWeapon.GetCurrentAmmo()} / {activeWeapon.GetMaxAmmo()}";
        }
    }
}
