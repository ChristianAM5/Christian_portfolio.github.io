using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponUpgradePanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI weaponName;

    [SerializeField] private Button damageButton;
    [SerializeField] private Button ammoButton;
    [SerializeField] private Button reloadButton;

    private WeaponController weapon;

    public void Setup(WeaponController w)
    {
        weapon = w;

        weaponName.text = w.GetWeaponName();

        damageButton.onClick.AddListener(() => TryUpgradeDamage());
        ammoButton.onClick.AddListener(() => TryUpgradeAmmo());
        reloadButton.onClick.AddListener(() => TryUpgradeReload());
    }

    void TryUpgradeDamage()
    {
        if (!RoundManager.Instance.SpendSkillPoint()) return;

        if (!weapon.UpgradeDamage())
        {
            // devolver punto si está al máximo
            RoundManager.Instance.AddBonusSkillPoints(1);
        }
    }

    void TryUpgradeAmmo()
    {
        if (!RoundManager.Instance.SpendSkillPoint()) return;

        if (!weapon.UpgradeAmmo())
        {
            RoundManager.Instance.AddBonusSkillPoints(1);
        }
    }

    void TryUpgradeReload()
    {
        if (!RoundManager.Instance.SpendSkillPoint()) return;

        if (!weapon.UpgradeReload())
        {
            RoundManager.Instance.AddBonusSkillPoints(1);
        }
    }
}
