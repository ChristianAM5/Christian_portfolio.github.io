using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectLibrary : MonoBehaviour
{
    // Array que se rellena desde el Inspector de Unity con los grupos de sonidos
    [SerializeField] private SoundEffectGroup[] soundEffectGroups;

    // Diccionario interno para buscar sonidos de forma ultra rápida usando su nombre
    private Dictionary<string, List<AudioClip>> soundDictionary;

    private void Awake()
    {
        // Al arrancar, preparamos el diccionario para que esté listo antes de que nadie pida sonidos
        InitializeDictionary();
    }

    /// <summary>
    /// Pasa los datos del Array (fácil de editar en Unity) al Diccionario (eficiente para programar).
    /// </summary>
    private void InitializeDictionary()
    {
        soundDictionary = new Dictionary<string, List<AudioClip>>();

        foreach (SoundEffectGroup soundEffectGroup in soundEffectGroups)
        {
            // Guarda la lista de clips usando el nombre del grupo como llave (Key)
            soundDictionary[soundEffectGroup.name] = soundEffectGroup.audioClips;
        }
    }

    /// <summary>
    /// Busca un grupo por nombre y devuelve un sonido aleatorio de ese grupo.
    /// </summary>
    /// <param name="name">El nombre del grupo de sonidos (ej: "Chest", "Jump").</param>
    /// <returns>Un AudioClip aleatorio o null si no se encuentra.</returns>
    public AudioClip GetRandomClip(string name)
    {
        // Comprobamos si el nombre existe en nuestra biblioteca
        if (soundDictionary.ContainsKey(name))
        {
            List<AudioClip> audioClips = soundDictionary[name];

            // Si el grupo tiene sonidos, elige uno al azar
            if (audioClips.Count > 0)
            {
                return audioClips[Random.Range(0, audioClips.Count)];
            }
        }

        // Si no existe el grupo o está vacío, avisa devolviendo nada
        return null;
    }

}

// Estructura de datos para organizar los sonidos en el Inspector de Unity
[System.Serializable]
public struct SoundEffectGroup
{
    public string name;              // Nombre del grupo (ej: "Explosiones")
    public List<AudioClip> audioClips; // Lista de archivos de audio .wav, .mp3, etc.
}

