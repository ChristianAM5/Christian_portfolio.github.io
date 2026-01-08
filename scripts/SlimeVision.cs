using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeVision : MonoBehaviour
{
    [Header("Vision Settings")]
    public float visionRadius = 5f;
    public SpriteRenderer mouseIcon;
    public LayerMask playerLayer;
    
    [Header("Camera Settings")]
    public Camera cameraB;
    
    [Header("Light Settings")]
    public bool useLightVision = true;
    public Color lightColor = Color.yellow;
    public float lightIntensity;
    public float spotAngle;
    
    [Header("Audio")]
    public AudioClip alertSound;
    
    private bool playerVisible = false;
    private int playerVisualLayer;
    private AudioSource audioSource;
    private Light visionLight;

    void Start()
    {
        playerVisualLayer = LayerMask.NameToLayer("Player_Visual");
        audioSource = GetComponent<AudioSource>();
        
        if (cameraB == null)
        {
            cameraB = GameObject.Find("MainCamera_B")?.GetComponent<Camera>();
        }
        
        // Oscurecer la cámara B (solo lo hace el primer slime que se inicialice)
        if (cameraB != null)
        {
            cameraB.backgroundColor = Color.black;
            cameraB.clearFlags = CameraClearFlags.SolidColor;
        }
        
        // Crear luz de visión para este slime
        if (useLightVision)
        {
            CreateVisionLight();
        }
    }

    void CreateVisionLight()
    {
        GameObject lightObj = new GameObject("SlimeVisionLight");
        lightObj.transform.parent = transform;
        
        visionLight = lightObj.AddComponent<Light>();
        visionLight.type = LightType.Spot;
        visionLight.color = lightColor;
        visionLight.intensity = lightIntensity;
        visionLight.range = visionRadius * 1.5f; // Un poco más grande que el rango de visión
        visionLight.spotAngle = spotAngle;
        visionLight.shadows = LightShadows.None; // Sin sombras para mejor rendimiento
        
        // Apuntar hacia abajo
        lightObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        
        // Solo visible en cámara B (opcional: configura cullingMask si es necesario)
        // visionLight.cullingMask = ...; 
    }

    void Update()
    {
        // Comprobar si el jugador A está dentro del radio
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRadius, playerLayer);
        bool playerDetected = hits.Length > 0;
        
        if (playerDetected && !playerVisible)
        {
            ShowPlayerOnCamera();
            PlayAlertSound();
        }
        else if (!playerDetected && playerVisible)
        {
            HidePlayerOnCamera();
        }

	if (useLightVision)
            UpdateVisionLight();

    }

    void ShowPlayerOnCamera()
    {
        playerVisible = true;
        if (cameraB != null)
        {
            // Añadir la capa Player_Visual al culling mask
            //cameraB.cullingMask |= (1 << playerVisualLayer);
            mouseIcon.enabled = true;
        }
    }

    void HidePlayerOnCamera()
    {
        playerVisible = false;
        if (cameraB != null)
        {
            // Quitar la capa Player_Visual del culling mask
            //cameraB.cullingMask &= ~(1 << playerVisualLayer);
            mouseIcon.enabled = false;
        }
    }

    void PlayAlertSound()
    {
        if (audioSource != null && alertSound != null)
        {
            audioSource.PlayOneShot(alertSound);
        }
    }

void UpdateVisionLight()
{
    if (visionLight == null) return;

    // Altura fija que siempre funciona bien
    float height = 3f;

    // Recolocar la luz en esa altura
    visionLight.transform.localPosition = new Vector3(
        0f,
        height,
        0f
    );

    // Calcular el ángulo necesario para iluminar exactamente el radio
    float angleRad = Mathf.Atan(visionRadius / height);
    float angleDeg = angleRad * Mathf.Rad2Deg * 2f;

    // Ajustar ángulo automáticamente
    visionLight.spotAngle = angleDeg;

    // Asegurar que el rango cubre todo
    visionLight.range = visionRadius + height;
}

}