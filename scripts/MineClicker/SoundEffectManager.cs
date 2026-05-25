using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Se ha añadido un nuevo Slider solo para la musica de fondo

public class SoundEffectManager : MonoBehaviour
{
    // Singleton: Permite que solo exista un Manager y que sea accesible globalmente
    private static SoundEffectManager Instance;

    // Referencias a los componentes de audio
    private static AudioSource audioSource;            // Para sonidos normales
    private static AudioSource randomPitchAudioSource; // Para sonidos con variación de tono
    private static AudioSource bgmAudioSource; // Música de fondo
    private static SoundEffectLibrary soundEffectLibrary; // La biblioteca que contiene los clips

    [SerializeField] private Slider sfxSlider; // Control deslizante de la interfaz para el volumen
    [SerializeField] private Slider sfxSliderBGM;	

    private void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance == null)
        {
            Instance = this;

            // Obtiene los dos AudioSources que deben estar en el mismo GameObject
            AudioSource[] audioSources = GetComponents<AudioSource>();
            audioSource = audioSources[0];
            randomPitchAudioSource = audioSources[1];
	    bgmAudioSource = audioSources[2];

            // Obtiene la biblioteca de sonidos del mismo objeto
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
        }
        else
        {
            // Si ya existe uno, destruye el duplicado
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Método estático para reproducir un sonido por su nombre.
    /// Puede llamarse desde otros scripts como: SoundEffectManager.Play("Nombre");
    /// </summary>
    /// <param name="soundName">Nombre del grupo de sonidos en la librería.</param>
    /// <param name="randomPitch">Si es true, el sonido variará su tono cada vez.</param>
    public static void Play(string soundName, bool randomPitch = false)
    {
        // Pide a la librería un clip aleatorio del grupo solicitado
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);

        if (audioClip != null)
        {
            if (randomPitch)
            {
                // Aplica una variación de tono aleatoria (entre 1 y 1.5)
                randomPitchAudioSource.pitch = Random.Range(1f, 1.5f);
                randomPitchAudioSource.PlayOneShot(audioClip);
            }
            else
            {
                // Reproduce el sonido de forma normal
                audioSource.PlayOneShot(audioClip);
            }
        }
    }

    void Start()
    {
        // Conecta el Slider de la UI para que llame a OnValueChanged cuando se mueva
        sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
	sfxSliderBGM.onValueChanged.AddListener(delegate { OnBGMValueChanged(); });
    }

    // Ajusta el volumen de ambos reproductores de audio.

    public static void SetVolume(float volume)
    {
        audioSource.volume = volume;
        randomPitchAudioSource.volume = volume;
    }

    // Método que responde al evento del Slider en la interfaz.

    public void OnValueChanged()
    {
        SetVolume(sfxSlider.value);
    }

    public static void SetBGMVolume(float volume)
    {
        bgmAudioSource.volume = volume;
    }

    public void OnBGMValueChanged()
    {
        SetBGMVolume(sfxSliderBGM.value);
    }

}


