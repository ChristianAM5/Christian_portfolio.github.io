using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System; // Necesario para usar 'Action' (Eventos)

public class PlayerInventory : MonoBehaviour
{
    // --- Atributos de Datos ---

    // Usamos HashSet en lugar de List porque las llaves son únicas.
    // Un HashSet no permite duplicados y es extremadamente rápido para buscar: HasKey()
    private HashSet<KeyType> _keys = new HashSet<KeyType>();

    // --- Comunicación por Eventos (Desacoplamiento) ---

    // Este evento es como una "emisora de radio". 
    // Cuando el jugador recoge una llave, "emite" un mensaje con el tipo de llave.
    // La UI o los efectos de sonido pueden "escuchar" este evento sin que este script sepa quiénes son.
    public event Action<KeyType> OnKeyCollected;

    // --- Lógica de Inventario ---

    /// <summary>
    /// Intenta añadir una llave al inventario.
    /// </summary>
    public void AddKey(KeyType type)
    {
        // Comprobamos si no tenemos ya esa llave para evitar procesos innecesarios
        if (!_keys.Contains(type))
        {
            _keys.Add(type);
            Debug.Log($"Llave {type} recogida.");

            // Disparamos el evento (el signo '?' verifica si hay alguien escuchando antes de llamar)
            // Esto permite que la UI se actualice automáticamente.
            OnKeyCollected?.Invoke(type);
        }
    }

    /// <summary>
    /// Verifica de forma rápida si el jugador tiene una llave específica.
    /// </summary>
    public bool HasKey(KeyType type) => _keys.Contains(type);

    /// <summary>
    /// Comprueba si se cumplen los requisitos para abrir una puerta o activar algo.
    /// </summary>
    public bool HasAllKeys(List<KeyType> requiredKeys)
    {
        // REGLA DE ORO: La Llave Maestra (Master) siempre da acceso total.
        // Esto es un ejemplo de "Early Return" (Retorno temprano) para optimizar el código.
        if (_keys.Contains(KeyType.Master))
        {
            Debug.Log("Acceso concedido por Llave Maestra");
            return true;
        }

        // Si no hay llave maestra, recorremos la lista de llaves requeridas.
        foreach (var key in requiredKeys)
        {
            // En el momento en que falte UNA sola llave, devolvemos 'false'.
            if (!_keys.Contains(key)) return false;
        }

        // Si el bucle termina y no faltó ninguna, el acceso es correcto.
        return true;
    }
}

/* Conceptos
El uso de HashSet<T> vs List<T>
Mientras una List es como una fila de personas donde puedes repetir nombres, un HashSet 
es como un club exclusivo: cada elemento es único. Para un sistema de llaves (donde no sueles 
tener "dos llaves rojas" iguales), el HashSet es mucho más eficiente para preguntar: "¿Tengo esta llave?".
*/