using UnityEngine;
using TMPro;
 
/// <summary>
/// UI superior: muestra el número de ronda y la cuenta atrás de 30 segundos.
///
/// Setup en el Inspector:
///   - roundText:     TextMeshPro con "RONDA 1" (se actualiza al inicio de cada ronda)
///   - countdownText: TextMeshPro con "30"      (se actualiza cada frame)
/// </summary>
/// 
public class RoundUI : MonoBehaviour
{
    [Header("Referencias de Texto")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI countdownText;
 
    [Header("Color de Urgencia")]
    [Tooltip("El contador cambia a este color cuando quedan pocos segundos.")]
    [SerializeField] private Color normalColor  = Color.white;
    [SerializeField] private Color urgentColor  = Color.red;
    [SerializeField] private float urgentThreshold = 10f;
 
    // ── Ciclo de vida ─────────────────────────────────────────────────────────
 
    private void Start()
    {
        GameEvents.OnRoundStarted += OnRoundStarted;
 
        // Inicializamos el texto con la ronda 1 por si el evento ya pasó
        UpdateRoundText(1);
    }
 
    private void Update()
    {
        if (RoundManager.Instance == null) return;
 
        float t = RoundManager.Instance.RoundTimeRemaining;
 
        // Mostramos el techo del tiempo (29.9s → "30", 0.1s → "1")
        countdownText.text  = Mathf.CeilToInt(t).ToString();
        countdownText.color = t <= urgentThreshold ? urgentColor : normalColor;
    }
 
    // ── Eventos ───────────────────────────────────────────────────────────────
 
    private void OnRoundStarted(int round)
    {
        UpdateRoundText(round);
    }
 
    private void UpdateRoundText(int round)
    {
        if (roundText != null)
            roundText.text = $"RONDA {round}";
    }
 
    private void OnDestroy()
    {
        GameEvents.OnRoundStarted -= OnRoundStarted;
    }
}
 
