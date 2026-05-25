using UnityEngine;

// Al ser 'static', no puedes arrastrar este script a un GameObject.
// Existe en "el aire" y cualquier otro script puede usarlo en cualquier momento.
public static class GlobalHelper
{
    /// <summary>
    /// Genera un identificador único basado en la ubicación y escena del objeto.
    /// </summary>
    /// <param name="obj">El GameObject que necesita el ID.</param>
    /// <returns>Un string con formato: NombreEscena_PosicionX_PosicionY</returns>
    public static string GenerateUniqueID(GameObject obj)
    {
        // Usa "Interpolación de strings" ($) para construir una cadena de texto.
        // Combina el nombre de la escena actual con las coordenadas X e Y.
        return $"{obj.scene.name}_{obj.transform.position.x}_{obj.transform.position.y}";
    }
}
