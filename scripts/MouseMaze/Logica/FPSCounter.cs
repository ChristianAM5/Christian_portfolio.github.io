using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private GameObject fpsPanel;
    private float deltaTime;
    private bool isVisible = false;

    void Start()
    {
        LoadVisibility(); // se carga solo al arrancar, sin depender de GraphicsSettings
    }

    void Update()
    {
        if (!isVisible) return;
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.color = fps >= 60 ? Color.green : fps >= 30 ? Color.yellow : Color.red;
        fpsText.text = $"{Mathf.RoundToInt(fps)} FPS";
    }

    public void Toggle(bool show)
    {
        isVisible = show;
        fpsPanel.SetActive(show);
        PlayerPrefs.SetInt("ShowFPS", show ? 1 : 0);
    }

    public void LoadVisibility()
    {
        bool saved = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        Toggle(saved);
    }
}