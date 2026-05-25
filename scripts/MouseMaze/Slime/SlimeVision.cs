using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeVision : MonoBehaviour
{
    [Header("Vision Settings")]
    public float visionRadius = 5f;
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

    [Tooltip("Nombre del objeto SpriteRenderer hijo del prefab Player_Raton")]
    public string mouseIconName = "cabezarata 1";

    private AudioSource audioSource;
    private Light visionLight;

    // Ratones actualmente visibles
    private List<SpriteRenderer> spritesActivos = new List<SpriteRenderer>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (cameraB == null)
            cameraB = GameObject.Find("MainCamera_B")?.GetComponent<Camera>();

        if (cameraB != null)
        {
            cameraB.backgroundColor = Color.black;
            cameraB.clearFlags = CameraClearFlags.SolidColor;
        }

        if (useLightVision)
            CreateVisionLight();
    }

    void CreateVisionLight()
    {
        GameObject lightObj = new GameObject("SlimeVisionLight");
        lightObj.transform.parent = transform;

        visionLight = lightObj.AddComponent<Light>();
        visionLight.type = LightType.Spot;
        visionLight.color = lightColor;
        visionLight.intensity = lightIntensity;
        visionLight.range = visionRadius * 1.5f;
        visionLight.spotAngle = spotAngle;
        visionLight.shadows = LightShadows.None;
        lightObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRadius, playerLayer);

        // Construir lista de sprites actualmente en rango
        List<SpriteRenderer> spritesEnRango = new List<SpriteRenderer>();
        foreach (Collider hit in hits)
        {
            SpriteRenderer sprite = BuscarMouseIcon(hit.gameObject);
            if (sprite != null)
                spritesEnRango.Add(sprite);
        }

        // Activar los que entran en rango
        foreach (SpriteRenderer sprite in spritesEnRango)
        {
            if (!spritesActivos.Contains(sprite))
            {
                sprite.enabled = true;
                spritesActivos.Add(sprite);
                PlayAlertSound();
            }
        }

        // Desactivar los que salen del rango
        List<SpriteRenderer> aSalir = new List<SpriteRenderer>();
        foreach (SpriteRenderer sprite in spritesActivos)
        {
            if (!spritesEnRango.Contains(sprite))
            {
                if (sprite != null) sprite.enabled = false;
                aSalir.Add(sprite);
            }
        }
        foreach (SpriteRenderer sprite in aSalir)
            spritesActivos.Remove(sprite);

        if (useLightVision)
            UpdateVisionLight();
    }

    SpriteRenderer BuscarMouseIcon(GameObject raton)
    {
        Transform iconTransform = raton.transform.Find(mouseIconName);
        if (iconTransform != null)
            return iconTransform.GetComponent<SpriteRenderer>();
        return raton.GetComponentInChildren<SpriteRenderer>(true);
    }

    void PlayAlertSound()
    {
        if (audioSource != null && alertSound != null)
            audioSource.PlayOneShot(alertSound);
    }

    void UpdateVisionLight()
    {
        if (visionLight == null) return;
        float height = 3f;
        visionLight.transform.localPosition = new Vector3(0f, height, 0f);
        float angleRad = Mathf.Atan(visionRadius / height);
        float angleDeg = angleRad * Mathf.Rad2Deg * 2f;
        visionLight.spotAngle = angleDeg;
        visionLight.range = visionRadius + height;
    }
}
