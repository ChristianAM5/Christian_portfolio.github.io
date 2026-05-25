using UnityEngine;

/// <summary>
/// La interfaz IInteractable actúa como un "contrato". 
/// Cualquier objeto que quiera ser interactivo (puertas, cofres, interruptores) 
/// DEBE implementar estas reglas.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// El mensaje que se mostrará al jugador cuando su mirada o puntero 
    /// esté sobre el objeto (ej: "Presiona E para abrir").
    /// </summary>
    string InteractionPrompt { get; }

    /// <summary>
    /// Este método define la ACCIÓN. 
    /// Todo lo que deba ocurrir al interactuar se escribe dentro de este método 
    /// en la clase que implemente la interfaz.
    /// </summary>
    void Interact();
}

/* Conceptos: 
El Concepto del "Enchufe Universal"
Imagina que el Jugador es un enchufe de pared. El enchufe no sabe qué vas a conectar (una lámpara, una tostadora o una TV), 
pero sabe que cualquier cosa que conectes debe tener clavijas estándar.
IInteractable es ese estándar de clavijas.

Al Jugador no le importa si está frente a una Puerta o un Botón; solo pregunta: "¿Tienes las clavijas de IInteractable?". 
Si la respuesta es sí, el jugador puede llamar al método Interact().

¿Por qué es útil en vuestro FPS Lab?
Como ya tenéis un sistema desacoplado (usando eventos y etiquetas), la interfaz sigue esa misma filosofía:
    Desacoplamiento total: El script de detección del jugador (Raycast) no necesita saber cómo funciona una puerta. 
        Solo necesita saber que el objeto tiene un Interact().
    Escalabilidad: Si mañana queréis añadir una "Máquina de Vending" o un "Terminal de Hackeo", solo creáis un script 
        nuevo que herede de IInteractable. No tendréis que modificar ni una sola línea de código del Jugador.
*/
