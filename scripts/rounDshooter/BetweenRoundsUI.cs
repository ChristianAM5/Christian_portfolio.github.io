using UnityEngine;

public class BetweenRoundsUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Textos")]
    [SerializeField] private TMPro.TextMeshProUGUI roundText;
    [SerializeField] private TMPro.TextMeshProUGUI killsText;
    [SerializeField] private TMPro.TextMeshProUGUI skillPointsText;

    [Header("Contenedor de armas")]
    [SerializeField] private Transform weaponsContainer;

    [Header("Prefab")]
    [SerializeField] private GameObject weaponPanelPrefab;

    [Header("Referencias")]
    [SerializeField] private WeaponManager weaponManager;

    private bool gameOver;

    private void OnEnable()
    {
        GameEvents.OnRoundEnded += ShowUI;
        GameEvents.OnSkillPointsChanged += UpdateSkillPoints;
        GameEvents.OnPlayerDeath += HandleGameOver;
    }

    private void HandleGameOver()
    {
        gameOver = true;
        panel.SetActive(false);
    }

    private void OnDisable()
    {
        GameEvents.OnRoundEnded -= ShowUI;
        GameEvents.OnSkillPointsChanged -= UpdateSkillPoints;
    }

    private void ShowUI(int round)
    {
        if (gameOver) return;
        panel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        roundText.text = $"RONDA {RoundManager.Instance.CurrentRound} COMPLETADA";
        killsText.text = $"Bajas totales: {RoundManager.Instance.TotalKills}";
        skillPointsText.text = $"Puntos de habilidad: {RoundManager.Instance.SkillPoints}";

        // Limpiar panels anteriores
        foreach (Transform child in weaponsContainer)
            Destroy(child.gameObject);

        int weaponCount = weaponManager.GetWeaponCount();

        for (int i = 0; i < weaponCount; i++)
        {
            if (!weaponManager.IsWeaponUnlocked(i))
                continue;

            WeaponController weapon = weaponManager.GetWeapon(i);
            if (weapon == null) continue;

            GameObject panelGO = Instantiate(weaponPanelPrefab, weaponsContainer);

            panelGO.transform.localScale = Vector3.one;
            panelGO.transform.localRotation = Quaternion.identity;

            WeaponUpgradePanel panelScript = panelGO.GetComponent<WeaponUpgradePanel>();
            panelScript.Setup(weapon);
        }
    }

    private void UpdateSkillPoints(int points)
    {
        if (skillPointsText != null)
            skillPointsText.text = $"Puntos de habilidad: {points}";
    }

    public void OnContinueButton()
    {
        panel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        RoundManager.Instance.StartNextRound();
    }
}
