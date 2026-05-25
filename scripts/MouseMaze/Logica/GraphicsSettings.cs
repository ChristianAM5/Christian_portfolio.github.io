using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class GraphicsSettings : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Slider fpsSlider;
    [SerializeField] private TMP_Text fpsLabel;
    [SerializeField] private Slider sensSlider;
    [SerializeField] private TMP_Text sensLabel;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button applyButton;
    [SerializeField] private Toggle FPS_Counter;
    [SerializeField] public Text FPS_Text;


    private Resolution[] availableResolutions;
    private UnityEngine.Rendering.RenderPipelineAsset cachedPipeline;

    // Valores pendientes — no se aplican hasta pulsar Aplicar
    private int pendingQuality;
    private int pendingResolutionIndex;
    private int pendingFps;
    private int pendingSens;
    private bool pendingFullscreen;
    private bool pendingFPSCounter;

    // Valor especial del slider que significa "sin limite"
    private const int FPS_UNLIMITED = 241;
    private int SENS_MAXVALUE = 500;

    void OnEnable()
    {

        cachedPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        // Configurar sliders ANTES de LoadSettings
        fpsSlider.wholeNumbers = true;
        fpsSlider.minValue = 10;
        fpsSlider.maxValue = FPS_UNLIMITED;

        sensSlider.wholeNumbers = true;
        sensSlider.minValue = 1;
        sensSlider.maxValue = SENS_MAXVALUE;



        LoadSettings();
        PopulateQuality();
        PopulateResolutions();

        // Enlazar eventos
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fpsSlider.onValueChanged.AddListener(OnFpsChanged);
        sensSlider.onValueChanged.AddListener(OnSensChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        FPS_Counter.onValueChanged.AddListener(OnFPSCounterChanged);
        applyButton.onClick.AddListener(ApplyChanges);


    }

    void OnDisable()
    {
        qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        fpsSlider.onValueChanged.RemoveListener(OnFpsChanged);
        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        applyButton.onClick.RemoveListener(ApplyChanges);
        FPS_Counter.onValueChanged.RemoveListener(OnFPSCounterChanged);
    }

    // -------------------------
    // CALIDAD
    // -------------------------
    void PopulateQuality()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        pendingQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
        qualityDropdown.SetValueWithoutNotify(pendingQuality);
        qualityDropdown.RefreshShownValue();
    }

    void OnQualityChanged(int index)
    {
        pendingQuality = index;
    }

    // -------------------------
    // RESOLUCION (sin duplicados de Hz)
    // -------------------------
    void PopulateResolutions()
    {
        var allResolutions = Screen.resolutions;

        if (allResolutions == null || allResolutions.Length == 0)
        {
            Debug.LogWarning("Screen.resolutions vacio, usando resolucion actual como fallback.");
            availableResolutions = new Resolution[] { Screen.currentResolution };
        }
        else
        {
            // Queda solo la resolucion de mayor Hz por par ancho/alto
            var filtered = new Dictionary<(int, int), Resolution>();
            foreach (var r in allResolutions)
            {
                var key = (r.width, r.height);
                if (!filtered.ContainsKey(key) || r.refreshRateRatio.value > filtered[key].refreshRateRatio.value)
                    filtered[key] = r;
            }
            availableResolutions = filtered.Values
                .OrderBy(r => r.width).ThenBy(r => r.height).ToArray();
        }

        resolutionDropdown.ClearOptions();
        var options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            var r = availableResolutions[i];
            options.Add($"{r.width} x {r.height}");
            if (r.width == Screen.currentResolution.width &&
                r.height == Screen.currentResolution.height)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        pendingResolutionIndex = Mathf.Clamp(
            PlayerPrefs.GetInt("Resolution", currentIndex), 0, availableResolutions.Length - 1);
        resolutionDropdown.SetValueWithoutNotify(pendingResolutionIndex);
        resolutionDropdown.RefreshShownValue();

        Debug.Log($"Resoluciones cargadas: {availableResolutions.Length}");
    }

    void OnResolutionChanged(int index)
    {
        pendingResolutionIndex = index;
    }

    // -------------------------
    // FPS
    // -------------------------
    void OnFpsChanged(float value)
    {
        pendingFps = Mathf.RoundToInt(value);
        fpsLabel.text = pendingFps >= FPS_UNLIMITED ? "Sin limite" : $"{pendingFps} FPS";
    }
    // -------------------------
    // SENSIBILIDAD
    // -------------------------
    void OnSensChanged(float value)
    {
        pendingSens = Mathf.RoundToInt(value);
        sensLabel.text = pendingSens >= SENS_MAXVALUE ? $"{SENS_MAXVALUE} Sensibilidad" : $"{pendingSens} Sensibilidad";
    }

    // -------------------------
    // PANTALLA COMPLETA
    // -------------------------
    void OnFullscreenChanged(bool value)
    {
        pendingFullscreen = value;
    }
    void OnFPSCounterChanged(bool value)
    {
        pendingFPSCounter = value;
    }
    // -------------------------
    // RENDER SCALE (resolucion 3D)
    // -------------------------


    // -------------------------
    // APLICAR
    // -------------------------
    public void ApplyChanges()
    {
        // Calidad
        QualitySettings.SetQualityLevel(pendingQuality, false);
        QualitySettings.vSyncCount = 0;
        PlayerPrefs.SetInt("GraphicsQuality", pendingQuality);

        // FPS
        Application.targetFrameRate = pendingFps >= FPS_UNLIMITED ? -1 : pendingFps;
        PlayerPrefs.SetInt("TargetFPS", pendingFps);

        //ContadorFPS
        if (FPS_Text != null)
        {
            PlayerPrefs.SetInt("FPS_Counter", pendingFPSCounter ? 1 : 0);
            FPS_Text.gameObject.SetActive(pendingFPSCounter);
        }

        //Sensibilidad
        PlayerPrefs.SetInt("Sensibility", pendingSens);

        // Aplicar al jugador local de forma segura
        RigidbodyFirstPersonController[] todos = FindObjectsOfType<RigidbodyFirstPersonController>();
        foreach (var p in todos)
        {
            // Obtenemos el componente PhotonView una vez para evitar errores
            PhotonView pv = p.GetComponent<PhotonView>();

            // Si no hay red (Offline) o si es NUESTRO jugador (Online)
            if (!PhotonNetwork.IsConnected || (pv != null && pv.IsMine))
            {
                p.ActualizarSensibilidad();
                break;
            }
        }

        if (availableResolutions != null && availableResolutions.Length > pendingResolutionIndex)
        {
            var r = availableResolutions[pendingResolutionIndex];
            // Usamos SetResolution que es más estable que Screen.fullScreen solo
            Screen.SetResolution(r.width, r.height, pendingFullscreen);
        }

        PlayerPrefs.SetInt("Resolution", pendingResolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", pendingFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }



    // -------------------------
    // CARGAR AJUSTES GUARDADOS
    // -------------------------
    void LoadSettings()
    {
        QualitySettings.vSyncCount = 0;

        // 1. Cargar datos de PlayerPrefs (o valores por defecto)
        pendingQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
        pendingFps = PlayerPrefs.GetInt("TargetFPS", 60);
        pendingSens = PlayerPrefs.GetInt("Sensibility", 20); // Por defecto 20
        pendingResolutionIndex = PlayerPrefs.GetInt("Resolution", 0);
        pendingFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        pendingFPSCounter = PlayerPrefs.GetInt("FPS_Counter", 0) == 1;

        // 2. APLICAR LOS AJUSTES AL MOTOR DE UNITY
        QualitySettings.SetQualityLevel(pendingQuality);
        Application.targetFrameRate = (pendingFps >= FPS_UNLIMITED) ? -1 : pendingFps;
        Screen.fullScreen = pendingFullscreen;

        // 3. APLICAR SENSIBILIDAD AL JUGADOR QUE ESTÉ EN ESCENA
        // Buscamos al controlador y le pedimos que se actualice con lo que acabamos de cargar
        RigidbodyFirstPersonController jugador = FindObjectOfType<RigidbodyFirstPersonController>();
        if (jugador != null)
        {
            jugador.ActualizarSensibilidad();
        }

        // 4. ACTUALIZAR LA INTERFAZ (Sliders y textos)
        fpsSlider.SetValueWithoutNotify(pendingFps);
        sensSlider.SetValueWithoutNotify(pendingSens);
        fpsLabel.text = pendingFps >= FPS_UNLIMITED ? "Sin limite" : $"{pendingFps} FPS";
        sensLabel.text = $"{pendingSens} Sensibilidad";
        fullscreenToggle.SetIsOnWithoutNotify(pendingFullscreen);
        FPS_Counter.SetIsOnWithoutNotify(pendingFPSCounter);
        qualityDropdown.SetValueWithoutNotify(pendingQuality);
        qualityDropdown.RefreshShownValue();

        if (FPS_Text != null) FPS_Text.gameObject.SetActive(pendingFPSCounter);
    }
}