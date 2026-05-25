using UnityEngine;
using TMPro;

public class BuffUIController : MonoBehaviour
{
    public GameObject buffPanel;
    public TMP_Text buffText;

    void Update()
    {
        var sys = TemporaryGlobalBuffSystem.Instance;
        if (sys == null) return;

        if (sys.HayBuffActivo)
        {
            buffPanel.SetActive(true);
            int minutos = Mathf.FloorToInt(sys.BuffTiempoRestante / 60f);
            int segundos = Mathf.FloorToInt(sys.BuffTiempoRestante % 60f);
            buffText.text = $"{sys.BuffNombreActivo}  {minutos:00}:{segundos:00}";
        }
        else
        {
            buffPanel.SetActive(false);
        }
    }
}
